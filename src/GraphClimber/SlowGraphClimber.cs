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
            get
            {
                return _property.Name;
            }
        }

        public Type OwnerType
        {
            get
            {
                return _property.ReflectedType;
            }
        }

        public Type MemberType
        {
            get
            {
                return _property.PropertyType;
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
        IReadWriteValueDescriptor<TField>,
        IReadOnlyValueDescriptor<TRuntime>,
        IReflectionValueDescriptor
    {
        private readonly IStateMemberProvider _stateMemberProvider;
        private readonly IReflectionStateMember _stateMember;
        private readonly object _owner;
        private readonly object _processor;

        public ReflectionValueDescriptor(object processor, IStateMemberProvider stateMemberProvider, IReflectionStateMember stateMember, object owner)
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
            return (TRuntime)_stateMember.GetValue(_owner);
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

            IEnumerable<IStateMember> members = 
                _stateMemberProvider.Provide(runtimeType);

            foreach (IReflectionStateMember member in 
                members.Cast<IReflectionStateMember>())
            {
                object memberValue = member.GetValue(value);

                Type memberType = 
                    memberValue != null ? memberValue.GetType() : member.MemberType;

                Type descriptorType = typeof (ReflectionValueDescriptor<,>)
                    .MakeGenericType(member.MemberType, memberType);

                IReflectionValueDescriptor descriptor =
                    (IReflectionValueDescriptor) Activator.CreateInstance
                        (descriptorType,
                            _processor, _stateMemberProvider, member, value);

                CallProcess(descriptor);
            }
        }

        private void CallProcess(IReflectionValueDescriptor descriptor)
        {
            Type fieldType = descriptor.StateMember.MemberType;

            bool methodCalled = false;

            if (!fieldType.IsValueType)
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
                CallGenericMethod(descriptor, fieldType, "ProcessNull");
                return true;
            }
            else if ((value != null) &&
                     (revisitedProcessor != null) &&
                     revisitedProcessor.Visited(value))
            {
                Type runtimeType = value.GetType();
                CallGenericMethod(descriptor, runtimeType, "ProcessRevisited");
                return true;
            }

            return false;
        }

        private void CallGenericMethod(IReflectionValueDescriptor descriptor, Type genericType, string methodName)
        {
            MethodInfo methodToCall =
                typeof (INullProcessor).GetMethod(methodName)
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
                    throw new Exception("No method found :(, this might be a bug in your code, or in the GraphClimber. Good luck.");
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
                new ReflectionValueDescriptor<object, object>(processor,
                    _stateMemberProvider,
                    new ReflectionPropertyStateMember(typeof(Box).GetProperty("Parent")),
                    new Box { Parent = parent });

            descriptor.Climb();
        }

        class Box
        {
            public object Parent { get; set; } 
        }
    }
}