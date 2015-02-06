using System;

namespace GraphClimber.Examples
{
    internal class IsArrayAttribute : Attribute, IGenericParameterFilter
    {
        public bool PassesFilter(Type type)
        {
            return type.IsArray;
        }
    }
}