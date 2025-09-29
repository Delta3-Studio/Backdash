using System.Numerics;
using Backdash.Core;
using Backdash.Data;
using Backdash.Network;

namespace Backdash.Tests.TestUtils;

static class Gen
{
    public static readonly Faker Faker = new();
    public static Randomizer Random => Faker.Random;
    public static Vector2 Vector2() => new(Random.Float(), Random.Float());
    public static Vector3 Vector3() => new(Random.Float(), Random.Float(), Random.Float());
    public static PeerAddress Peer() => Faker.Internet.IpEndPoint();
    public static Frame Frame() => new(Faker.Random.Int(0));
    public static FrameSpan FrameSpan() => new(Faker.Random.Int(0));
    public static ByteSize ByteSize() => new(Faker.Random.Long(0));
    public static ConnectionsState ConnectionsState() => new(Max.NumberOfPlayers);

    public static NetcodePlayer NetcodePlayer()
    {
        var type = Random.Enum<PlayerType>();

        return new(
            (sbyte)Random.Int(0, Max.NumberOfPlayers - 1),
            type,
            type is not PlayerType.Local ? Faker.Internet.IpEndPoint() : null
        );
    }

    public static GameInput GameInput(int frame, byte[] input)
    {
        if (input.Length > Max.CompressedBytes)
            throw new ArgumentOutOfRangeException(nameof(input));
        TestInput testInputBytes = new(input);
        GameInput result = new(testInputBytes)
        {
            Frame = new(frame),
        };
        return result;
    }

    public static GameInput[] GameInputRange(int count, int firstFrame = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        return Generator().ToArray();

        IEnumerable<GameInput> Generator()
        {
            for (var i = 0; i < count; i++)
            {
                var bytes = Random.ArrayElement(GoodInputBytes);
                yield return GameInput(firstFrame + i, [bytes]);
            }
        }
    }

    public static readonly byte[] GoodInputBytes =
    [
        1 << 0,
        1 << 1,
        1 << 2,
        1 << 3,
        1 << 4,
        1 << 5,
        1 << 6,
        1 << 7,
    ];
}
