using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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

        public object GetValue(object owner)
        {
            return _property.GetValue(owner);
        }

        public void SetValue(object owner, object value)
        {
            _property.SetValue(owner, value);
        }
    }

    internal interface IReflectionStateMember : IStateMember
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

        public ReflectionValueDescriptor(IStateMemberProvider stateMemberProvider,
            IReflectionStateMember stateMember, object owner)
        {
            _stateMemberProvider = stateMemberProvider;
            _stateMember = stateMember;
            _owner = owner;
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

        public void Climb(object processor)
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

            IEnumerable<IStateMember> members =
                _stateMemberProvider.Provide(runtimeType);

            foreach (IReflectionStateMember member in 
                members.Cast<IReflectionStateMember>())
            {
                Type runtimeMemberType = GetRuntimeMemberType(member, member.MemberType, value);
                VisitMember(member, value, runtimeMemberType, false, processor);
            }
        }

        private Type GetRuntimeMemberType(IReflectionStateMember member, Type memberType, object value)
        {
            object memberValue = member.GetValue(value);

            var result =
                memberValue != null ? memberValue.GetType() : memberType;

            return result;
        }

        private void VisitMember(IReflectionStateMember member, object owner, Type runtimeMemberType,
            bool skipSpecialMethod, object processor)
        {
            IReflectionValueDescriptor descriptor =
                CreateDescriptor(member, owner, runtimeMemberType);

            CallProcess(descriptor, skipSpecialMethod, processor);
        }

        private IReflectionValueDescriptor CreateDescriptor(IReflectionStateMember member, object value,
            Type runtimeMemberType)
        {
            Type descriptorType = typeof (ReflectionValueDescriptor<,>)
                .MakeGenericType(member.MemberType, runtimeMemberType);

            IReflectionValueDescriptor descriptor =
                (IReflectionValueDescriptor) Activator.CreateInstance
                    (descriptorType,
                        _stateMemberProvider, member, value);

            return descriptor;
        }

        public void Route(IStateMember stateMember, object owner, object processor)
        {
            VisitMember((IReflectionStateMember) stateMember,
                owner,
                stateMember.MemberType,
                true, processor);
        }

        public void Route(IStateMember stateMember, Type runtimeMemberType, object owner, object processor)
        {
            VisitMember((IReflectionStateMember) stateMember,
                owner,
                runtimeMemberType,
                true, processor);
        }

        private void CallProcess(IReflectionValueDescriptor descriptor, bool skipSpecialMethod, object processor)
        {
            Type fieldType = descriptor.StateMember.MemberType;

            bool methodCalled = false;

            if (!fieldType.IsValueType && !skipSpecialMethod)
            {
                methodCalled = TryCallSpecialMethod(descriptor, fieldType, processor);
            }

            if (!methodCalled)
            {
                CallMatchedProcess(descriptor, processor);
            }
        }

        private bool TryCallSpecialMethod(IReflectionValueDescriptor descriptor, Type fieldType, object processor)
        {
            object value = descriptor.Get();

            INullProcessor nullProcessor = processor as INullProcessor;
            IRevisitedProcessor revisitedProcessor = processor as IRevisitedProcessor;

            if (nullProcessor != null && value == null)
            {
                CallGenericMethod(descriptor, fieldType, "ProcessNull", processor);
                return true;
            }
            else if ((value != null) &&
                     (revisitedProcessor != null) &&
                     revisitedProcessor.Visited(value))
            {
                Type runtimeType = value.GetType();
                CallGenericMethod(descriptor, runtimeType, "ProcessRevisited", processor);
                return true;
            }

            return false;
        }

        private void CallGenericMethod(IReflectionValueDescriptor descriptor, Type genericType, string methodName, object processor)
        {
            MethodInfo methodToCall =
                typeof (INullProcessor).GetMethod(methodName)
                    .MakeGenericMethod(genericType);

            methodToCall.Invoke(processor, new object[] {descriptor});
        }

        private void CallMatchedProcess(IReflectionValueDescriptor descriptor, object processor)
        {
            GenericArgumentBinder binder = new GenericArgumentBinder();

            IEnumerable<MethodInfo> methods =
                processor.GetType().GetMethods()
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
                    method.Invoke(processor, new object[] {descriptor});
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
            var descriptor =
                new ReflectionValueDescriptor<object, object>(_stateMemberProvider,
                    new ReflectionPropertyStateMember(typeof (Box).GetProperty("Parent")),
                    new Box {Parent = parent});

            descriptor.Climb(processor);
        }

        private class Box
        {
            public object Parent { get; set; }
        }
    }
}