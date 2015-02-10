using System;

namespace GraphClimber.Examples
{
    internal class IsEnumAttribute : Attribute, IGenericParameterFilter
    {
        public bool PassesFilter(Type type)
        {
            return type.IsEnum;
        }
    }
    
    internal class IsArrayAttribute : Attribute, IGenericParameterFilter
    {
        public bool PassesFilter(Type type)
        {
            return type.IsArray;
        }
    }
}