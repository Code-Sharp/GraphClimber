using System;
using System.Linq.Expressions;

namespace GraphClimber
{
    interface IDescriptorWriter
    {
        Expression WriteDescriptorDeclaration(Expression processor, Expression owner, IStateMember member, Type runtimeType);
    }
}