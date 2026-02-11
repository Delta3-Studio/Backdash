using System.Security.Cryptography;

namespace Backdash.Core;

interface IRandomNumberGenerator
{
    ushort SyncNumber();
    int NextInt();
    double NextGaussian();
}

sealed class DefaultRandomNumberGenerator(Random random) : IRandomNumberGenerator
{
    public ushort SyncNumber()
    {
        using var gen = RandomNumberGenerator.Create();
        Span<byte> buff = stackalloc byte[sizeof(ushort)];
        gen.GetBytes(buff);
        return BitConverter.ToUInt16(buff);
    }

    public int NextInt() => random.Next();

    public double NextGaussian() => random.NextGaussian();
}
