using System;
using System.Linq.Expressions;

namespace GraphClimber
{
    public static class DescriptorExtensionMethods
    {

        public static void RouteRuntimeType<TRuntime>(this IValueDescriptor descriptor, bool skipSpecialMethod = false)
        {
            RouteRuntimeType(descriptor, typeof (TRuntime), skipSpecialMethod);
        }

        public static void RouteRuntimeType(this IValueDescriptor descriptor, Type runtimeType,
            bool skipSpecialMethod = false)
        {
            descriptor.Route(descriptor.StateMember, runtimeType, descriptor.Owner, skipSpecialMethod);
        }

        public static void RouteMemberType<TMember>(this IValueDescriptor descriptor, bool skipSpecialMethod = false)
        {
            var memberType = typeof(TMember);
            RouteMemberType(descriptor, memberType, skipSpecialMethod);
        }

        public static void RouteMemberType(IValueDescriptor descriptor, Type memberType, bool skipSpecialMethod = false)
        {
            descriptor.Route(descriptor.StateMember.WithMemberType(memberType), memberType, descriptor.Owner, skipSpecialMethod);
        }

        public static void RouteMemberType<TMember>(this IValueDescriptor descriptor, string name, bool skipSpecialMethod = false)
        {
            var memberType = typeof(TMember);
            RouteMemberType(descriptor, name, memberType, skipSpecialMethod);
        }

        public static void RouteMemberType(IValueDescriptor descriptor, string name, Type memberType,
            bool skipSpecialMethod = false)
        {
            descriptor.Route(descriptor.StateMember.WithMemberType(memberType).WithName(name), memberType, descriptor.Owner,
                skipSpecialMethod);
        }
    }

    public static class StateMemberExtensions
    {

        public static IStateMember WithName(this IStateMember stateMember, string name)
        {
            return new NamedStateMember(stateMember, name);
        }

        public static IStateMember WithMemberType(this IStateMember stateMember, Type memberType)
        {
            return new MemberTypeStateMember(stateMember, memberType);
        }

        private class NamedStateMember : IStateMember
        {
            private readonly IStateMember _underlying;
            private readonly string _name;

            public NamedStateMember(IStateMember underlying, string name)
            {
                _underlying = underlying;
                _name = name;
            }

            public string Name
            {
                get { return _name; }
            }

            public Type OwnerType
            {
                get { return _underlying.OwnerType; }
            }

            public Type MemberType
            {
                get { return _underlying.MemberType; }
            }

            public bool CanRead
            {
                get { return _underlying.CanRead; }
            }

            public bool CanWrite
            {
                get { return _underlying.CanWrite; }
            }

            public Expression GetGetExpression(Expression obj)
            {
                return _underlying.GetGetExpression(obj);
            }

            public Expression GetSetExpression(Expression obj, Expression value)
            {
                return _underlying.GetSetExpression(obj, value);
            }

            public bool IsArrayElement
            {
                get { return _underlying.IsArrayElement; }
            }

            public int[] ElementIndex
            {
                get { return _underlying.ElementIndex; }
            }
        }

        private class MemberTypeStateMember : IStateMember
        {
            private readonly IStateMember _underlying;
            private readonly Type _memberType;

            public MemberTypeStateMember(IStateMember underlying, Type memberType)
            {
                _underlying = underlying;
                _memberType = memberType;
            }

            public string Name
            {
                get { return _underlying.Name; }
            }

            public Type OwnerType
            {
                get { return _underlying.OwnerType; }
            }

            public Type MemberType
            {
                get { return _memberType; }
            }

            public bool CanRead
            {
                get { return _underlying.CanRead; }
            }

            public bool CanWrite
            {
                get { return _underlying.CanWrite; }
            }

            public Expression GetGetExpression(Expression obj)
            {
                return _underlying.GetGetExpression(obj);
            }

            public Expression GetSetExpression(Expression obj, Expression value)
            {
                return _underlying.GetSetExpression(obj, value);
            }

            public bool IsArrayElement
            {
                get { return _underlying.IsArrayElement; }
            }

            public int[] ElementIndex
            {
                get { return _underlying.ElementIndex; }
            }
        }

    }


}
