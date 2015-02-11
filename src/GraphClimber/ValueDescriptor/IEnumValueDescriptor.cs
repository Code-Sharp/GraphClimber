using System;

namespace GraphClimber
{
    public interface IEnumValueDescriptor
    {
        IStateMember UnderlyingValueStateMember { get; }
    }

    /// <remarks>
    /// Allows processing state member values based on their RuntimeType:
    /// Used mostly for serialization.
    /// </remarks>
    public interface IReadOnlyEnumExactValueDescriptor<TEnum, TUnderlying> : 
        IEnumValueDescriptor,
        IReadOnlyExactValueDescriptor<TEnum>
        where TEnum : IConvertible
        where TUnderlying : IConvertible
    {
        /// <summary>
        /// Gets the current value of the field
        /// </summary>
        /// <returns></returns>
        TUnderlying GetUnderlying();
    }

    /// <remarks>
    /// Allows processing state member values based on their static type:
    /// Used mostly for deserialization.
    /// </remarks>
    public interface IWriteOnlyEnumExactValueDescriptor<TEnum, TUnderlying> :
        IEnumValueDescriptor,
        IWriteOnlyExactValueDescriptor<TEnum>
        where TEnum : IConvertible
        where TUnderlying : IConvertible
    {
        /// <summary>
        /// Sets the field to a new value
        /// </summary>
        /// <param name="value"></param>
        void SetUnderlying(TUnderlying value);
    }
}