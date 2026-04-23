namespace Backdash.Synchronizing.State;

sealed class ChecksumStore
{
    readonly Entry[] data;

    public ChecksumStore(int size)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);
        data = new Entry[size];
    }

    public void Add(Frame frame, Checksum checksum)
    {
        ref var entry = ref data[frame.Number % data.Length];
        entry.Frame = frame;
        entry.Checksum = checksum;
    }

    public Checksum Get(Frame frame)
    {
        var entry = data[frame.Number % data.Length];
        return entry.Frame == frame ? entry.Checksum : Checksum.Empty;
    }

    public Frame LastFrame() => data is [] ? Frame.Null : data.Max(x => x.Frame);

    struct Entry
    {
        public Frame Frame;
        public Checksum Checksum;
    }
}
