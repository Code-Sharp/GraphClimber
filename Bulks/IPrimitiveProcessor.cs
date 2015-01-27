using System;

namespace GraphClimber.Bulks
{
    /// <summary>
    /// Defines an actor that processes all the .net primitives.
    /// </summary>
    public interface IPrimitiveProcessor : 
        IProcessor<byte>, IProcessor<sbyte>,
        IProcessor<short>, IProcessor<ushort>,
        IProcessor<int>, IProcessor<uint>,
        IProcessor<long>, IProcessor<ulong>,
        IProcessor<char>, IProcessor<double>,
        IProcessor<string>, IProcessor<DateTime>
    {

    }
}