using System.Reflection;

namespace GraphClimber
{
    public static class GenericParameterAttributePolyfills
    {

        public static bool HasFlag(this GenericParameterAttributes value, GenericParameterAttributes flag)
        {
            return (value & flag) == flag;
        }

    }
}
