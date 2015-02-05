using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace GraphClimber
{
    internal class ReflectionPropertyStateMemberProvider : IStateMemberProvider
    {
        public IEnumerable<IStateMember> Provide(Type type)
        {
            return type.GetRuntimeProperties()
                .Where(x => x.GetIndexParameters().Length == 0)
                .Select(x => new ReflectionPropertyStateMember(x));
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

        public Type MemberType
        {
            get { return _property.PropertyType; }
        }

        public Expression GetGetExpression(Expression obj)
        {
            throw new NotImplementedException();
        }

        public Expression GetSetExpression(Expression obj, Expression value)
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

            foreach (IReflectionStateMember member in 
                members.Cast<IReflectionStateMember>())
            {
                    Type runtimeMemberType = GetRuntimeMemberType(member, value);
                VisitMember(member, value, runtimeMemberType, false);
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

            Type arrayElementType = array.GetType().GetElementType();

            foreach (IEnumerable<int> indexSet in allIndexes)
            {
                int[] indices = indexSet.Reverse().ToArray();

                var member = new ArrayStateMember(array.GetType(), arrayElementType ,indices);
                Type runtimeMemberType = GetRuntimeMemberType(member, array);
                VisitMember(member, array, runtimeMemberType, false);
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

        private void VisitMember(IReflectionStateMember member, object owner, Type runtimeMemberType,
            bool skipSpecialMethod)
        {
            IReflectionValueDescriptor descriptor =
                CreateDescriptor(member, owner, runtimeMemberType);

            CallProcess(descriptor, skipSpecialMethod);
        }

        private IReflectionValueDescriptor CreateDescriptor(IReflectionStateMember member, object value,
            Type runtimeMemberType)
        {
            Type descriptorType = typeof (ReflectionValueDescriptor<,>)
                .MakeGenericType(member.MemberType, runtimeMemberType);

            IReflectionValueDescriptor descriptor =
                (IReflectionValueDescriptor) Activator.CreateInstance
                    (descriptorType,
                        _processor, _stateMemberProvider, member, value);

            return descriptor;
        }

        public void Route(IStateMember stateMember, object owner)
        {
            VisitMember((IReflectionStateMember) stateMember,
                owner,
                stateMember.MemberType,
                true);
        }

        public void Route(IStateMember stateMember, Type runtimeMemberType, object owner)
        {
            VisitMember((IReflectionStateMember) stateMember,
                owner,
                runtimeMemberType,
                true);
        }

        private void CallProcess(IReflectionValueDescriptor descriptor, bool skipSpecialMethod)
        {
            Type fieldType = descriptor.StateMember.MemberType;

            bool methodCalled = false;

            if (!fieldType.IsValueType && !skipSpecialMethod)
            {
                methodCalled = TryCallSpecialMethod(descriptor, fieldType);
            }

            if (!methodCalled)
            {
                CallMatchedProcess(descriptor);
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
                Type runtimeType = value.GetType();
                CallGenericMethod(descriptor, typeof (IRevisitedProcessor), runtimeType, "ProcessRevisited");
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

        private void CallMatchedProcess(IReflectionValueDescriptor descriptor)
        {
            GenericArgumentBinder binder = new GenericArgumentBinder();

            IEnumerable<MethodInfo> methods =
                _processor.GetType().GetMethods()
                    .Where(x => x.IsDefined(typeof (ProcessorMethodAttribute)));

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

        public Expression GetGetExpression(Expression obj)
        {
            throw new NotImplementedException();
        }

        public Expression GetSetExpression(Expression obj, Expression value)
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

    public class SlowGraphClimber<TProcessor>
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

        public void Route(object current, TProcessor processor)
        {
            var descriptor = GetDescriptor(current, processor);

            descriptor.Route(descriptor.StateMember, current.GetType(), new Box { Parent = current });
        }

        private ReflectionValueDescriptor<object, object> GetDescriptor(object current, TProcessor processor)
        {
            var descriptor =
                new ReflectionValueDescriptor<object, object>(processor,
                    _stateMemberProvider,
                    new ReflectionPropertyStateMember(typeof (Box).GetProperty("Parent")),
                    new Box {Parent = current});
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