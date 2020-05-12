using System;
using System.Linq.Expressions;

namespace GraphClimber
{
    internal class EnumConvert<TEnum, TUnderlying>
    {
        public static readonly Func<TEnum, TUnderlying> ToUnderlying = GetToUnderlying();
        public static readonly Func<TUnderlying, TEnum> ToEnum = GetToEnum();

        private static Func<TEnum, TUnderlying> GetToUnderlying()
        {
            return GetConvert<TEnum, TUnderlying>();
        }

        private static Func<TUnderlying, TEnum> GetToEnum()
        {
            return GetConvert<TUnderlying, TEnum>();
        }


        private static Func<TSource, TTarget> GetConvert<TSource, TTarget>()
        {
            var parameter = Expression.Parameter(typeof(TSource), "value");

            Expression<Func<TSource, TTarget>> lambda =
                Expression.Lambda<Func<TSource, TTarget>>
                    (Expression.Convert(parameter, typeof(TTarget)),
                     parameter);

            return lambda.Compile();
        }
    }
}