using Backdash.Core;
using Backdash.Synchronizing.Input;

namespace Backdash.Tests.Specs.Unit.Sync.Input;

public sealed class InputQueueTests
{
    static InputQueue<int> CreateInputQueue(int size = 8, int frameDelay = 0)
    {
        var logger = Logger.Create(LogLevel.None);
        InputQueue<int> sut = new(0, size, logger)
        {
            LocalFrameDelay = frameDelay,
        };
        return sut;
    }

    [Fact]
    public void ShouldStoreAndReturnAddedInputAsConfirmed()
    {
        var sut = CreateInputQueue();
        GameInput<int> added = new(42, Frame.Zero);

        sut.AddInput(ref added);

        added.Frame.Should().Be(Frame.Zero);
        sut.GetInput(Frame.Zero, out var returned).Should().BeTrue();
        returned.Should().Be(new GameInput<int>(42, Frame.Zero));
    }

    [Fact]
    public void ShouldPredictDefaultInputForFrameZero()
    {
        var sut = CreateInputQueue();
        sut.GetInput(Frame.Zero, out var predicted).Should().BeFalse();

        predicted.Should().Be(new GameInput<int>(0, Frame.Zero));
        sut.FirstIncorrectFrame.Should().Be(Frame.Null);
    }

    [Fact]
    public void ShouldExitPredictionWhenActualInputMatches()
    {
        var sut = CreateInputQueue();
        sut.GetInput(Frame.Zero, out _).Should().BeFalse();
        GameInput<int> actual = new(0, Frame.Zero);

        sut.AddInput(ref actual);

        sut.FirstIncorrectFrame.Should().Be(Frame.Null);
        sut.GetInput(Frame.Zero, out var confirmed).Should().BeTrue();
        confirmed.Data.Should().Be(0);
    }

    [Fact]
    public void ShouldRecordFirstIncorrectPredictedFrame()
    {
        var sut = CreateInputQueue();
        sut.GetInput(Frame.Zero, out _).Should().BeFalse();
        GameInput<int> actual = new(7, Frame.Zero);

        sut.AddInput(ref actual);

        sut.FirstIncorrectFrame.Should().Be(Frame.Zero);
    }

    [Fact]
    public void ShouldPadFramesWhenLocalDelayIncreases()
    {
        var sut = CreateInputQueue(frameDelay: 2);
        GameInput<int> added = new(15, Frame.Zero);

        sut.AddInput(ref added);

        added.Frame.Should().Be((Frame)2);
        sut.GetInput(Frame.Zero, out var firstPadding).Should().BeTrue();
        sut.GetInput(Frame.One, out var secondPadding).Should().BeTrue();
        sut.GetInput((Frame)2, out var delayed).Should().BeTrue();
        firstPadding.Data.Should().Be(0);
        secondPadding.Data.Should().Be(0);
        delayed.Data.Should().Be(15);
    }

    [Fact]
    public void ShouldDiscardInputsAfterRollbackFrameAndAcceptNextInput()
    {
        var sut = CreateInputQueue();
        GameInput<int> first = new(10, Frame.Zero);
        GameInput<int> second = new(20, Frame.One);
        GameInput<int> third = new(30, (Frame)2);
        sut.AddInput(ref first);
        sut.AddInput(ref second);
        sut.AddInput(ref third);

        sut.DiscardInputsAfter((Frame)1);

        sut.GetInput(Frame.One, out var retained).Should().BeTrue();
        retained.Data.Should().Be(20);
        GameInput<int> replacement = new(25, (Frame)2);
        sut.AddInput(ref replacement);
        sut.GetInput((Frame)2, out var added).Should().BeTrue();
        added.Data.Should().Be(25);
    }

    [Fact]
    public void ShouldAllowDiscardingAlreadyDiscardedConfirmedFrame()
    {
        var sut = CreateInputQueue();
        GameInput<int> first = new(10, Frame.Zero);
        GameInput<int> second = new(20, Frame.One);
        GameInput<int> third = new(30, (Frame)2);
        sut.AddInput(ref first);
        sut.AddInput(ref second);
        sut.AddInput(ref third);

        sut.DiscardConfirmedFrames(Frame.One);
        var discardAgain = () => sut.DiscardConfirmedFrames(Frame.Zero);

        discardAgain.Should().NotThrow();
        sut.GetInput((Frame)2, out var retained).Should().BeTrue();
        retained.Data.Should().Be(30);
    }

    [Fact]
    public void ShouldPreserveFramesAfterMostRecentlyRequestedFrame()
    {
        var sut = CreateInputQueue();
        GameInput<int> first = new(10, Frame.Zero);
        GameInput<int> second = new(20, Frame.One);
        GameInput<int> third = new(30, (Frame)2);
        sut.AddInput(ref first);
        sut.AddInput(ref second);
        sut.AddInput(ref third);
        sut.GetInput(Frame.One, out _).Should().BeTrue();

        sut.DiscardConfirmedFrames((Frame)2);

        sut.GetInput((Frame)2, out var retained).Should().BeTrue();
        retained.Data.Should().Be(30);
    }

    [Fact]
    public void ShouldDropInputsUntilQueueCatchesUpAfterLocalDelayDecreases()
    {
        using var logger = Logger.Create(LogLevel.None);
        InputQueue<int> sut = new(0, 8, logger) { LocalFrameDelay = 2 };
        GameInput<int> first = new(10, Frame.Zero);
        sut.AddInput(ref first);
        first.Frame.Should().Be((Frame)2);

        sut.LocalFrameDelay = 0;
        GameInput<int> dropped = new(20, Frame.One);
        sut.AddInput(ref dropped);
        dropped.Frame.Should().Be(Frame.Null);
        GameInput<int> droppedAgain = new(25, (Frame)2);
        sut.AddInput(ref droppedAgain);
        droppedAgain.Frame.Should().Be(Frame.Null);
        GameInput<int> caughtUp = new(30, (Frame)3);
        sut.AddInput(ref caughtUp);
        caughtUp.Frame.Should().Be((Frame)3);

        sut.GetInput((Frame)2, out var previous).Should().BeTrue();
        sut.GetInput((Frame)3, out var current).Should().BeTrue();
        previous.Data.Should().Be(10);
        current.Data.Should().Be(30);
    }

    [Fact]
    public void ShouldRollbackAcrossPaddingFramesAfterDelayIncrease()
    {
        var sut = CreateInputQueue();
        GameInput<int> first = new(10, Frame.Zero);
        GameInput<int> second = new(20, Frame.One);
        sut.AddInput(ref first);
        sut.AddInput(ref second);

        sut.LocalFrameDelay = 2;
        GameInput<int> original = new(30, (Frame)2);
        sut.AddInput(ref original);
        original.Frame.Should().Be((Frame)4);

        sut.DiscardInputsAfter(Frame.One);

        GameInput<int> replacement = new(35, (Frame)2);
        sut.AddInput(ref replacement);
        replacement.Frame.Should().Be((Frame)4);
        sut.GetInput((Frame)2, out var padding2).Should().BeTrue();
        sut.GetInput((Frame)3, out var padding3).Should().BeTrue();
        sut.GetInput((Frame)4, out var replayed).Should().BeTrue();
        padding2.Data.Should().Be(20);
        padding3.Data.Should().Be(20);
        replayed.Data.Should().Be(35);
    }
}
