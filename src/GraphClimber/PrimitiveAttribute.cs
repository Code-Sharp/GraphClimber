using System;

namespace GraphClimber
{
    [AttributeUsage(AttributeTargets.GenericParameter)]
    public class PrimitiveAttribute : Attribute, IGenericParameterFilter
    {
        public bool PassesFilter(Type type)
        {
            return type.IsPrimitive || type == typeof(string);
        }
    }
}