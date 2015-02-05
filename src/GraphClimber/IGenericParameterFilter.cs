using System;

namespace GraphClimber
{
    public interface IGenericParameterFilter
    {

        bool PassesFilter(Type type);

    }
}