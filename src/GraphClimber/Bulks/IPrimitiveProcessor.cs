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

    /// <summary>
    /// Defines an actor that processes all the .net primitives.
    /// </summary>
    public interface IReadOnlyPrimitiveProcessor :
        IReadOnlyProcessor<byte>, IReadOnlyProcessor<sbyte>,
        IReadOnlyProcessor<short>, IReadOnlyProcessor<ushort>,
        IReadOnlyProcessor<int>, IReadOnlyProcessor<uint>,
        IReadOnlyProcessor<long>, IReadOnlyProcessor<ulong>,
        IReadOnlyProcessor<char>, IReadOnlyProcessor<double>,
        IReadOnlyProcessor<string>, IReadOnlyProcessor<DateTime>
    {
    }

    /// <summary>
    /// Defines an actor that processes all the .net primitives.
    /// </summary>
    public interface IReadOnlyExactPrimitiveProcessor :
        IReadOnlyExactProcessor<byte>, IReadOnlyExactProcessor<sbyte>,
        IReadOnlyExactProcessor<short>, IReadOnlyExactProcessor<ushort>,
        IReadOnlyExactProcessor<int>, IReadOnlyExactProcessor<uint>,
        IReadOnlyExactProcessor<long>, IReadOnlyExactProcessor<ulong>,
        IReadOnlyExactProcessor<char>, IReadOnlyExactProcessor<double>,
        IReadOnlyExactProcessor<string>, IReadOnlyExactProcessor<DateTime>
    {
    }

    /// <summary>
    /// Defines an actor that processes all the .net primitives.
    /// </summary>
    public interface IWriteOnlyExactPrimitiveProcessor :
        IWriteOnlyExactProcessor<byte>, IWriteOnlyExactProcessor<sbyte>,
        IWriteOnlyExactProcessor<short>, IWriteOnlyExactProcessor<ushort>,
        IWriteOnlyExactProcessor<int>, IWriteOnlyExactProcessor<uint>,
        IWriteOnlyExactProcessor<long>, IWriteOnlyExactProcessor<ulong>,
        IWriteOnlyExactProcessor<char>, IWriteOnlyExactProcessor<double>,
        IWriteOnlyExactProcessor<string>, IWriteOnlyExactProcessor<DateTime>
    {
    }
}