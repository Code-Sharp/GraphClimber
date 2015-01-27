namespace GraphClimber
{
    /// <summary>
    /// Defines a processor that handles all the enumeration values.
    /// </summary>
    public interface IEnumProcessor
    {

        /// <summary>
        /// Process an enumeration value that placed inside 
        /// a field of the same type as the enum
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum</typeparam>
        /// <typeparam name="TUnderlying">The underlying type of the enum</typeparam>
        /// <remarks>
        /// It is safe to cast the given <paramref name="value"/> to <typeparamref name="TUnderlying"/>
        /// </remarks>
        /// <param name="value"></param>
        /// <param name="descriptor"></param>
        void Process<TEnum, TUnderlying>(TEnum value, IValueDescriptor<TEnum> descriptor);

        /// <summary>
        /// Processes an enumeration value that is boxed inside an object
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum</typeparam>
        /// <typeparam name="TUnderlying">The underlying type of the enum</typeparam>
        /// <remarks>
        /// It is safe to cast the given <paramref name="value"/> to <typeparamref name="TUnderlying"/>
        /// </remarks>
        /// <param name="value"></param>
        /// <param name="descriptor"></param>
        void Process<TEnum, TUnderlying>(TEnum value, IValueDescriptor<object> descriptor);

    }
}