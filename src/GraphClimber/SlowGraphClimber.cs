using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GraphClimber.ValueDescriptor;

namespace GraphClimber
{
    public class CachingStateMemberProvider : IStateMemberProvider
    {
        private readonly IStateMemberProvider _underlying;
        
        private readonly IDictionary<Type, IEnumerable<IStateMember>> _cache = new Dictionary<Type, IEnumerable<IStateMember>>();
        private readonly object _syncRoot = new object();

        public CachingStateMemberProvider(IStateMemberProvider underlying)
        {
            _underlying = underlying;
        }

        public IEnumerable<IStateMember> Provide(Type type)
        {
            IEnumerable<IStateMember> retVal;
            if (_cache.TryGetValue(type, out retVal))
            {
                return retVal;
            }

            lock (_syncRoot)
            {
                if (!_cache.TryGetValue(type, out retVal))
                {
                    // ToList() is here to cache yield return methods.
                    _cache[type] = retVal = _underlying.Provide(type).ToList();
                }
            }

            return retVal;
        }

        public IStateMember ProvideArrayMember(Type arrayType, int[] indices)
        {
            // TODO : May not be smart performance wise.
            return new ArrayStateMember(arrayType, arrayType.GetElementType(), indices);
        }
    }

    internal class ReflectionPropertyStateMemberProvider : IStateMemberProvider
    {
        public IEnumerable<IStateMember> Provide(Type type)
        {
            return type.GetRuntimeProperties()
                .Where(x => x.GetIndexParameters().Length == 0)
                .Select(x => new ReflectionPropertyStateMember(x));
        }

        public IStateMember ProvideArrayMember(Type arrayType, int[] indices)
        {
            return new ArrayStateMember(arrayType, arrayType.GetElementType(), indices);
        }
    }



    internal class ReflectionPropertyStateMember : IReflectionStateMember
    {
        private readonly PropertyInfo _property;

        public ReflectionPropertyStateMember(PropertyInfo property)
        {
            _property = property;
        }

        public string Name
        {
            get { return _property.Name; }
        }

        public Type OwnerType
        {
            get { return _property.ReflectedType; }
        }

        public virtual Type MemberType
        {
            get { return _property.PropertyType; }
        }

        public bool CanRead
        {
            get { return _property.CanRead; }
        }

        public bool CanWrite
        {
            get { return _property.CanWrite; }
        }

        public Expression GetGetExpression(Expression obj)
        {
            throw new NotImplementedException();
        }

        public Expression GetSetExpression(Expression obj, Expression value)
        {
            throw new NotImplementedException();
        }

        public Action<object, T> BuildSetterForBox<T>()
        {
            throw new NotImplementedException();
        }

        public bool IsArrayElement
        {
            get { return false; }
        }
        
        public int[] ElementIndex
        {
            get
            {
                // The empty index
                return new int[0];
            }
        }

        public MemberInfo UnderlyingMemberInfo
        {
            get
            {
                return _property;
            }
        }

        public object GetValue(object owner)
        {
            return _property.GetValue(owner);
        }

        public void SetValue(object owner, object value)
        {
            _property.SetValue(owner, value);
        }
    }

    public interface IReflectionStateMember : IStateMember
    {
        MemberInfo UnderlyingMemberInfo { get; }

        object GetValue(object owner);

        void SetValue(object owner, object value);
    }

    internal class ReflectionValueDescriptor<TField, TRuntime> :
        IReadOnlyValueDescriptor<TRuntime>,
        IReadOnlyExactValueDescriptor<TRuntime>,
        IWriteOnlyExactValueDescriptor<TField>,
        IWriteOnlyValueDescriptor<TField>,
        IReadWriteValueDescriptor<TField>,
        IReflectionValueDescriptor where
            TRuntime : TField
    {
        private readonly IStateMemberProvider _stateMemberProvider;
        private readonly IReflectionStateMember _stateMember;
        private readonly object _owner;
        private readonly object _processor;

        public ReflectionValueDescriptor(object processor, IStateMemberProvider stateMemberProvider,
            IReflectionStateMember stateMember, object owner)
        {
            _stateMemberProvider = stateMemberProvider;
            _stateMember = stateMember;
            _owner = owner;
            _processor = processor;
        }

        public TField Get()
        {
            return (TField) _stateMember.GetValue(_owner);
        }

        object IReflectionValueDescriptor.Get()
        {
            return Get();
        }

        TRuntime IReadOnlyValueDescriptor<TRuntime>.Get()
        {
            return (TRuntime) Get();
        }

        TRuntime IReadOnlyExactValueDescriptor<TRuntime>.Get()
        {
            IReadOnlyValueDescriptor<TRuntime> casted = this;
            return casted.Get();
        }

        public void Set(TField value)
        {
            _stateMember.SetValue(_owner, value);
        }

        public IStateMember StateMember
        {
            get { return _stateMember; }
        }

        public object Owner
        {
            get { return _owner; }
        }

        public void Climb()
        {
            TField value = Get();

            Type runtimeType;

            if (value != null)
            {
                runtimeType = value.GetType();
            }
            else
            {
                throw new NullReferenceException("Can't climb on null, you silly developer!");
            }

            if (runtimeType.IsArray)
            {
                ClimbArray((Array) (object) value);
            }
            else
            {
                IEnumerable<IStateMember> members =
                    _stateMemberProvider.Provide(runtimeType);

                object boxed = value;

                foreach (IReflectionStateMember member in 
                    members.Cast<IReflectionStateMember>())
                {
                    Type runtimeMemberType = GetRuntimeMemberType(member, value);
                    VisitMember(member, boxed, runtimeMemberType, false, false);
                }

                if (runtimeType.IsValueType)
                {
                    Set((TField) boxed);
                }
            }
        }

        private void ClimbArray(Array array)
        {
            int dimension = array.Rank;

            IEnumerable<IEnumerable<int>> indexesSets =
                Enumerable.Range(0, dimension)
                .Select(index => Enumerable.Range(array.GetLowerBound(index),
                    array.GetLength(index)));

            IEnumerable<IEnumerable<int>> allIndexes =
                Combinatorics.CartesianProduct(indexesSets.Reverse());

            foreach (IEnumerable<int> indexSet in allIndexes)
            {
                int[] indices = indexSet.Reverse().ToArray();

                IReflectionStateMember decorated = (IReflectionStateMember)
                    _stateMemberProvider.ProvideArrayMember(array.GetType(), indices);
                
                Type runtimeMemberType = GetRuntimeMemberType(decorated, array);
                
                VisitMember(decorated, array, runtimeMemberType, false, false);
            }
        }

        private Type GetRuntimeMemberType(IReflectionStateMember member, object value)
        {
            Type memberType = member.MemberType;

            object memberValue = member.GetValue(value);

            var result =
                memberValue != null ? memberValue.GetType() : memberType;

            return result;
        }

        private void VisitMember(IReflectionStateMember member,
            object owner,
            Type runtimeMemberType,
            bool skipSpecialMethod,
            bool routed)
        {
            IReflectionValueDescriptor descriptor =
                CreateDescriptor(member, owner, runtimeMemberType);

            CallProcess(descriptor, skipSpecialMethod, routed);
        }

        private IReflectionValueDescriptor CreateDescriptor(IReflectionStateMember member, object value,
            Type runtimeMemberType)
        {
            Type descriptorType;

            Type memberType = member.MemberType;
            if (memberType.IsEnum)
            {
                descriptorType = typeof (ReflectionEnumValueDescriptor<,,>)
                    .MakeGenericType(memberType, runtimeMemberType,
                        memberType.GetEnumUnderlyingType());
            }
            else
            {
                descriptorType = typeof(ReflectionValueDescriptor<,>)
                    .MakeGenericType(memberType, runtimeMemberType);                
            }

            IReflectionValueDescriptor descriptor =
                (IReflectionValueDescriptor) Activator.CreateInstance
                    (descriptorType,
                        _processor, _stateMemberProvider, member, value);

            return descriptor;
        }

        public void Route(IStateMember stateMember, object owner, bool skipSpecialMethod = true)
        {
            VisitMember((IReflectionStateMember) stateMember,
                owner,
                stateMember.MemberType,
                skipSpecialMethod, true);
        }

        public void Route(IStateMember stateMember, Type runtimeMemberType, object owner, bool skipSpecialMethod = true)
        {
            VisitMember((IReflectionStateMember) stateMember,
                owner,
                runtimeMemberType,
                skipSpecialMethod, true);
        }

        private void CallProcess(IReflectionValueDescriptor descriptor, bool skipSpecialMethod, bool routed)
        {
            Type fieldType = descriptor.StateMember.MemberType;

            bool methodCalled = false;

            if (!fieldType.IsValueType && !skipSpecialMethod)
            {
                methodCalled = TryCallSpecialMethod(descriptor, fieldType);
            }

            if (!methodCalled)
            {
                CallMatchedProcess(descriptor, routed);
            }
        }

        private bool TryCallSpecialMethod(IReflectionValueDescriptor descriptor, Type fieldType)
        {
            object value = descriptor.Get();

            INullProcessor processor = _processor as INullProcessor;
            IRevisitedProcessor revisitedProcessor = _processor as IRevisitedProcessor;

            if (processor != null && value == null)
            {
                CallGenericMethod(descriptor, typeof (INullProcessor), fieldType, "ProcessNull");
                return true;
            }
            else if ((value != null) &&
                     (revisitedProcessor != null) &&
                     revisitedProcessor.Visited(value))
            {
                CallGenericMethod(descriptor, typeof (IRevisitedProcessor), fieldType, "ProcessRevisited");
                return true;
            }

            return false;
        }

        private void CallGenericMethod(IReflectionValueDescriptor descriptor, Type processorType, Type genericType, string methodName)
        {
            MethodInfo methodToCall =
                processorType.GetMethod(methodName)
                    .MakeGenericMethod(genericType);

            methodToCall.Invoke(_processor, new object[] {descriptor});
        }

        private void CallMatchedProcess(IReflectionValueDescriptor descriptor, bool routed)
        {
            GenericArgumentBinder binder = new GenericArgumentBinder(new FallbackToFirstCandidateMethodSelector(new BinderMethodSelector(Type.DefaultBinder)));

            IEnumerable<MethodInfo> methods =
                _processor.GetType().GetMethods()
                    .Where(x => x.IsDefined(typeof (ProcessorMethodAttribute)))
                    .Where(x => !x.GetCustomAttribute<ProcessorMethodAttribute>().OnlyOnRoute || routed);

            Type descriptorType = descriptor.GetType();

            IEnumerable<IGrouping<int, MethodInfo>> candidates =
                methods.Select(method => Bind(binder, method, descriptorType))
                    .Where(x => x != null)
                    .GroupBy(x => x.GetCustomAttribute<ProcessorMethodAttribute>().Precedence)
                    .OrderBy(x => x.Key);

            IGrouping<int, MethodInfo> maximum = candidates.FirstOrDefault();

            if (maximum.Skip(1).Any())
            {
                throw new Exception("Too many candidates for the processor thingy. Use precedence.");
            }
            else
            {
                MethodInfo method = maximum.FirstOrDefault();

                if (method != null)
                {
                    method.Invoke(_processor, new object[] {descriptor});
                }
                else
                {
                    throw new Exception(
                        "No method found :(, this might be a bug in your code, or in the GraphClimber. Good luck.");
                }
            }
        }

        private MethodInfo Bind(GenericArgumentBinder binder, MethodInfo method, Type descriptorType)
        {
            MethodInfo bindedMethod;

            if (binder.TryBind(method, new[] {descriptorType}, out bindedMethod))
            {
                return bindedMethod;
            }

            return null;
        }
    }

    internal class ReflectionEnumValueDescriptor<TField, TRuntime, TUnderlying> :
        ReflectionValueDescriptor<TField, TRuntime>,
        IReadOnlyEnumExactValueDescriptor<TField, TUnderlying>,
        IWriteOnlyEnumExactValueDescriptor<TField, TUnderlying>
        where TRuntime : TField
        where TField : IConvertible
        where TUnderlying : IConvertible
    {
        private readonly IStateMember _underlyingValueStateMember;

        public ReflectionEnumValueDescriptor(object processor, IStateMemberProvider stateMemberProvider, IReflectionStateMember stateMember, object owner) : 
            base(processor, stateMemberProvider, stateMember, owner)
        {
            _underlyingValueStateMember = new EnumUnderlyingValueStateMember(stateMember);
        }

        public TUnderlying GetUnderlying()
        {
            TField result = Get();
            
            TUnderlying value = 
                (TUnderlying) Convert.ChangeType(result, typeof (TUnderlying));
            
            return value;
        }

        public void SetUnderlying(TUnderlying value)
        {
            long asLong =
                (long) Convert.ChangeType(value, typeof (long));

            TField result =
                (TField) Enum.ToObject(typeof (TField), asLong);

            Set(result);
        }

        public IStateMember UnderlyingValueStateMember
        {
            get
            {
                return _underlyingValueStateMember;
            }
        }
    }

    internal class EnumUnderlyingValueStateMember : IReflectionStateMember
    {
        private readonly IReflectionStateMember _stateMember;
        private readonly Type _enumType;
        private readonly Type _underlyingType;

        public EnumUnderlyingValueStateMember(IReflectionStateMember stateMember)
        {
            _stateMember = stateMember;
            _enumType = _stateMember.MemberType;
            _underlyingType = _enumType.GetEnumUnderlyingType();
        }

        public string Name
        {
            get { return _stateMember.Name; }
        }

        public Type OwnerType
        {
            get { return _stateMember.OwnerType; }
        }

        public Type MemberType
        {
            get { return _underlyingType; }
        }

        public bool CanRead
        {
            get { return _stateMember.CanRead; }
        }

        public bool CanWrite
        {
            get { return _stateMember.CanWrite; }
        }

        public Expression GetGetExpression(Expression obj)
        {
            return _stateMember.GetGetExpression(obj);
        }

        public Expression GetSetExpression(Expression obj, Expression value)
        {
            return _stateMember.GetSetExpression(obj, value);
        }

        public Action<object, T> BuildSetterForBox<T>()
        {
            return _stateMember.BuildSetterForBox<T>();
        }

        public bool IsArrayElement
        {
            get { return _stateMember.IsArrayElement; }
        }

        public int[] ElementIndex
        {
            get { return _stateMember.ElementIndex; }
        }

        public MemberInfo UnderlyingMemberInfo
        {
            get { return _stateMember.UnderlyingMemberInfo; }
        }

        public object GetValue(object owner)
        {
            return Convert.ChangeType(_stateMember.GetValue(owner), _underlyingType);
        }

        public void SetValue(object owner, object value)
        {
            // Thats how they do it :(
            object casted =
                Enum.ToObject(_enumType, Convert.ToInt64(value));

            _stateMember.SetValue(owner, casted);
        }
    }

    internal class ArrayStateMember : IReflectionStateMember
    {
        private readonly Type _arrayType;
        private readonly Type _arrayElementType;
        private readonly int[] _indices;

        public ArrayStateMember(Type arrayType, Type arrayElementType, int[] indices)
        {
            _arrayType = arrayType;
            _arrayElementType = arrayElementType;
            _indices = indices;
        }

        public string Name
        {
            get
            {
                return string.Format("[{0}]",
                    string.Join(", ", _indices));
            }
        }

        public Type OwnerType
        {
            get
            {
                return _arrayType;
            }
        }

        public Type MemberType
        {
            get
            {
                return _arrayElementType;
            }
        }

        public bool CanRead
        {
            get { return true; }
        }

        public bool CanWrite
        {
            get { return true; }
        }

        public Expression GetGetExpression(Expression obj)
        {
            throw new NotImplementedException();
        }

        public Expression GetSetExpression(Expression obj, Expression value)
        {
            throw new NotImplementedException();
        }

        public Action<object, T> BuildSetterForBox<T>()
        {
            throw new NotImplementedException();
        }

        public bool IsArrayElement
        {
            get
            {
                return true;
            }
        }

        public int[] ElementIndex
        {
            get
            {
                return _indices;
            }
        }

        public MemberInfo UnderlyingMemberInfo
        {
            get { throw new NotImplementedException(); }
        }

        public object GetValue(object owner)
        {
            Array array = owner as Array;
            return array.GetValue(_indices);
        }

        public void SetValue(object owner, object value)
        {
            Array array = owner as Array;
            array.SetValue(value, _indices);
        }
    }

    internal interface IReflectionValueDescriptor : IValueDescriptor
    {
        object Get();
    }


    public class SlowGraphClimber<TProcessor> : IGraphClimber<TProcessor>
    {
        private readonly IStateMemberProvider _stateMemberProvider;

        public SlowGraphClimber(IStateMemberProvider stateMemberProvider)
        {
            _stateMemberProvider = stateMemberProvider;
        }

        public void Climb(object parent, TProcessor processor)
        {
            var descriptor = GetDescriptor(parent, processor);

            descriptor.Climb();
        }

        public void Route(IStateMember stateMember, object current, TProcessor processor, bool skipSpecialMethod)
        {
            throw new NotImplementedException();
        }

        public void Route(object current, TProcessor processor, bool skipSpecialMethod)
        {
            var descriptor = GetDescriptor(current, processor);

            descriptor.Route(descriptor.StateMember, current.GetType(), new Box { Parent = current }, skipSpecialMethod);
        }


        private ReflectionValueDescriptor<object, object> GetDescriptor(object current, TProcessor processor)
        {
            var descriptor =
                new ReflectionValueDescriptor<object, object>(processor,
                    _stateMemberProvider,
                    new StaticStateMember(current), 
                    null);
            return descriptor;
        }


        private class Box
        {
            public object Parent { get; set; }
        }
    }

    /*
    public class StaticValueDescriptor : IReadOnlyValueDescriptor<object>,
        IReadOnlyExactValueDescriptor<object>,
        IReadWriteValueDescriptor<object>
    {
        private readonly object _current;
        private readonly object _processor;

        public StaticValueDescriptor(object current, object processor)
        {
            _current = current;
            _processor = processor;
            throw new NotImplementedException();
        }

        public void Set(object value)
        {
            throw new NotImplementedException();
        }

        public object Get()
        {
            return _current;
        }

        public IStateMember StateMember
        {
            get { return StaticStateMember.Empty; }
    }

        public class StaticStateMember : IStateMember
        {
            public static readonly IStateMember Empty = new StaticStateMember();

            private StaticStateMember()
            {
                
            }


            public string Name
            {
                get { return "InitialValue"; }
            }

            public Type OwnerType
            {
                get { return typeof (object); }
            }

            public Type MemberType
            {
                get { return typeof(object); }
            }

            public Expression GetGetExpression(Expression obj)
            {
                throw new NotImplementedException();
            }

            public Expression GetSetExpression(Expression obj, Expression value)
            {
                throw new NotImplementedException();
            }
        }

        public object Owner
        {
            get { throw new NotImplementedException(); }
        }

        public void Climb()
        {
            throw new NotImplementedException();
        }

        public void Route(IStateMember stateMember, Type runtimeMemberType, object owner)
        {
            throw new NotImplementedException();
        }

        public void Route(IStateMember stateMember, object owner)
        {
            throw new NotImplementedException();
        }
    }*/
}