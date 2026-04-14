namespace Backdash.Synchronizing.State;

sealed class ChecksumStore
{
    readonly Entry[] data;

    public ChecksumStore(int size)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);
        data = new Entry[size];
    }

    public void Add(Frame frame, uint checksum)
    {
        ref var entry = ref data[frame.Number % data.Length];
        entry.Frame = frame;
        entry.Checksum = checksum;
    }

    public uint Get(Frame frame)
    {
        var entry = data[frame.Number % data.Length];
        return entry.Frame == frame ? entry.Checksum : 0;
    }

    struct Entry
    {
        public Frame Frame;
        public uint Checksum;
    }
}
