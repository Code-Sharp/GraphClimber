using System;
using System.Linq.Expressions;

namespace GraphClimber
{
    /// <summary>
    /// Defines a state member that can be climbed.
    /// </summary>
    public interface IStateMember
    {
        /// <summary>
        /// Gets the name of the state member
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the type that owns the state member.
        /// </summary>
        Type OwnerType { get; }

        /// <summary>
        /// Gets the compile-time type of this member.
        /// </summary>
        Type MemberType { get; }

        /// <summary>
        /// Gets the get expression to the state member
        /// </summary>
        /// <param name="obj">An expression of the object that owns the field</param>
        /// <returns></returns>
        Expression GetGetExpression(Expression obj);

        /// <summary>
        /// Gets the set expression to the state member
        /// </summary>
        /// <param name="obj">An expression of the object that owns the field</param>
        /// <param name="value">The value to be set</param>
        /// <returns></returns>
        Expression GetSetExpression(Expression obj, Expression value);
    }
}