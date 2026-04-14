using Backdash.Data;
using Backdash.Options;

namespace Backdash.Synchronizing.State;

sealed class ChecksumStore
{
    readonly CircularBuffer<uint> data;

    public ChecksumStore(int size)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);
        data = new(size);
    }

    public ChecksumStore(NetcodeOptions options) : this(Math.Max(options.TotalSavedFramesAllowed,
        options.Protocol.ConsistencyCheckStoreSize)) { }

    public void Add(Frame frame, uint checksum) { }


    public uint Get(Frame frame) => 0;
}
