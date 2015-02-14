using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace GraphClimber
{
    internal class DescriptorWriter : IDescriptorWriter
    {
        private readonly IClimbStore _store;
        private ParameterExpression _descriptorReference;

        public DescriptorWriter(IClimbStore store)
        {
            _store = store;
        }

        public ParameterExpression DescriptorReference
        {
            get
            {
                return _descriptorReference;
            }
        }

        public Expression WriteDescriptorDeclaration(Expression processor, Expression owner, IStateMember member, Type runtimeType)
        {
            Type descriptorType =
                DescriptorExtensions.GetDescriptorType(member, runtimeType);

            Type memberLocalType = 
                typeof (MemberLocal<,>).MakeGenericType(member.MemberType, runtimeType);

            _descriptorReference = Expression.Variable(descriptorType, "descriptor");

            object memberLocal = Activator.CreateInstance(memberLocalType, _store, member);

            ConstructorInfo constructor = descriptorType.GetConstructors().FirstOrDefault();

            NewExpression creation =
                Expression.New(constructor,
                    processor,
                    owner,
                    Expression.Constant(memberLocal),
                    Expression.Constant(_store));

            BinaryExpression assign = Expression.Assign(_descriptorReference, creation);

            return assign;
        }
    }
}