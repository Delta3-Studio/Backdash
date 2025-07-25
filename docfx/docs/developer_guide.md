﻿## Developer Guide

> [!NOTE]
> adapted from [GGPO](https://github.com/pond3r/ggpo/edit/master/doc/DeveloperGuide.md)

### Installing

[NuGet package](https://www.nuget.org/packages/Backdash) available:

```ps
$ dotnet add package Backdash
```

## Game State and Inputs

Your game probably has many moving parts. [Backdash](https://github.com/lucasteles/Backdash) only depends on these two:

- **Game State** describes the current state of everything in your game. In a shooter, this would include the position
  of the ship and all the enemies on the screen, the location of all the bullets, how much health each opponent has, the
  current score, etc.

- **Game Inputs** are the set of things that modify the game state. These obviously include the joystick and button
  presses done by the player but can include other non-obvious inputs as well. For example, if your game uses the
  current time of day to calculate something in the game, the current time of day at the beginning of a frame is also an
  input.

There are many other things in your game engine that are neither game state nor inputs. For example, your audio and
video renderers are not game state since they don't have an effect on the outcome of the game. If you have a special
effects engine that's generating effects that do not have an impact on the game, they can be excluded from the game
state as well.

## Using State and Inputs for Synchronization

Each player in a [Backdash](https://github.com/lucasteles/Backdash) networked game has a complete copy of your game
running. [Backdash](https://github.com/lucasteles/Backdash) needs to keep both copies of the
game state in sync to ensure that both players are experiencing the same game. It would be much too expensive to send an
entire copy of the game state between players every frame. Instead, [Backdash](https://github.com/lucasteles/Backdash)
sends the players' inputs to each other and has
each player step the game forward. In order for this to work, your game engine must meet three criteria:

1. The game simulation must be fully deterministic. That is, for any given game state and inputs, advancing the game
   state by exactly 1 frame must result in identical game states for all players.
2. The game state must be fully encapsulated and serializable.
3. Your game engine must be able to load, save, and execute a single simulation frame without rendering the result of
   that frame. **This will be used to implement rollbacks**.

## Programming Guide

The following section contains a walk-through for porting your application
to [Backdash](https://github.com/lucasteles/Backdash).

For a detailed description of the
[Backdash](https://github.com/lucasteles/Backdash) API, please see
the [API Reference Docs](https://delta3-studio.github.io/Backdash/api/Backdash.html).

### Interfacing with [Backdash](https://github.com/lucasteles/Backdash)

[Backdash](https://github.com/lucasteles/Backdash) is designed to be easy to interface with new and existing game
engines. It handles most of the implementation of handling rollbacks by calling out to your application via
the [`INetcodeSessionHandler`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSessionHandler-1.html) hooks.

### Creating the [`INetcodeSession`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-2.html) Object

The [`INetcodeSession<TInput>`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-2.html) object is your interface to the [Backdash](https://github.com/lucasteles/Backdash) framework.

Create one with the `RollbackNetcode.WithInputType` builder:

For example, giving the user pre-defined types for the **Game State** and **Game Input**

```csharp
public class MyGameState {
    public MyGameState() {
        /* initialize state */
    }

    /* members */
}

[Flags]
public enum MyGameInput {
    /* members */
}
```

To create a new session bounded to port `9001`:

```csharp
using Backdash;

var session = RollbackNetcode
    .WithInputType<MyGameInput>()
    .Configure(options =>
    {
        options.LocalPort = 9001;
    })
    .Build();

```

> [!TIP]
> If you want to use an integer type as your input type:
>
> `RollbackNetcode.WithInputType(t => t.Integer<uint>())`

The session builder can be used to configure the session by setting [`NetcodeOptions`](https://delta3-studio.github.io/Backdash/api/Backdash.NetcodeOptions.html):
- passing an instance to `.WithOptions(..)`
- using a delegate function on `.Configure(options => {})`
- using the fluent api


```csharp
using Backdash;
using Backdash.Core;

var session = RollbackNetcode
    .WithInputType<MyGameInput>()
    .WithPort(9001)
    .WithInputDelayFrames(2)
    .WithLogLevel(LogLevel.Warning)
    .Build();

```

You should also define an implementation of
the [`INetcodeSessionHandler`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSessionHandler-1.html)
filled in with your game's callback functions for managing game state.

```csharp
public class MySessionHandler : INetcodeSessionHandler
{
    public void OnSessionStart() { /* ... */ }
    public void OnSessionClose() { /* ... */ }
    public void SaveState(in Frame frame, ref readonly BinaryBufferWriter writer) { /* ... */ }
    public void LoadState(in Frame frame, ref readonly BinaryBufferReader reader) { /* ... */ }
    public void AdvanceFrame() { /* ... */ }
    public void TimeSync(FrameSpan framesAhead) { /* ... */ }
    public void OnPeerEvent(PlayerHandle player, PeerEventInfo evt) { /* ... */ }
}
```

And then, set it into the session:

```csharp
session.SetHandler(new MySessionHandler());
```

The [`INetcodeSession`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-2.html) object should only
be used for a single game session. If you need to connect to another opponent, dispose your existing object using
the [`.Dispose`](https://learn.microsoft.com/pt-br/dotnet/api/system.idisposable.dispose?view=net-8.0)
method and start a new one:

```csharp
/* Close the current session to start a new one */
session.Dispose();
```

### Sending Player Locations

When you created the [`INetcodeSession`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-2.html)
you don't specify any information about the players participating in the game. To do so, call
the [`.AddPlayer()`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-2.html#Backdash_INetcodeSession_2_AddPlayers_System_Collections_Generic_IReadOnlyList_Backdash_Player__)
method function with an instance of [`Player`](https://delta3-studio.github.io/Backdash/api/Backdash.Player.html) for each
player. The following example shows how you might
use [`.AddPlayer()`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-2.html#Backdash_INetcodeSession_2_AddPlayers_System_Collections_Generic_IReadOnlyList_Backdash_Player__)
in a 2-player game:

```csharp
LocalPlayer player1 = new(1); // local player number 1

var player2Endpoint = IPEndPoint.Parse("192.168.0.100:8001"); // player 2 ip and port
RemotePlayer player2 = new(2, player2Endpoint); // remote player number 2

ResultCode result;
result = session.AddPlayer(player1);
// ...
result = session.AddPlayer(player2);
// ...
```

Check the [samples](https://github.com/lucasteles/Backdash/tree/master/samples) for more complete code.

### Starting session

After setting up players you must call the
session [.Start()](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-2.html#Backdash_INetcodeSession_2_Start_System_Threading_CancellationToken_)
method. This will start all the background work like socket receiver, input queue, peer synchronization, etc.

```csharp
session.Start();
```

### Synchronizing Local and Remote Inputs

Input synchronization happens on
the [session](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-1.html) at the top of each game frame.
This is done by
calling [`AddLocalInput`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-1.html#Backdash_INetcodeSession_1_AddLocalInput_Backdash_PlayerHandle__0_)
for each local player
and [`SynchronizeInputs`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-1.html#Backdash_INetcodeSession_1_SynchronizeInputs)
to fetch the inputs for remote players. Be sure to check the return value of `SynchronizeInputs`. If it returns a value
other than [ResultCode.Ok`](https://delta3-studio.github.io/Backdash/api/Backdash.ResultCode.html), you should
**not advance your game state**. This usually happens because [Backdash](https://github.com/lucasteles/Backdash) has not
received packets from the remote player in a while and has reached its internal prediction limit.

After synchronizing you can read the player's inputs using
the [`GetInput`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-1.html#Backdash_INetcodeSession_1_GetInput_System_Int32_)
method for a single-player
or [`GetInputs`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-1.html#Backdash_INetcodeSession_1_GetInputs_System_Span_Backdash_Data_SynchronizedInput__0___)
to load all player's inputs into a buffer.

For example, if your code looks like this currently for a local game:

```csharp
MyGameInput player1Input = GetControllerInput(0);
MyGameInput player2Input = GetControllerInput(1);

/* send p1 and p2 to the game */
AdvanceGameState(player1Input, player2Input, gameState);
```

You should change it to read as follows:

```csharp

// you must keep the local player handler reference or query it from the session.
var player1Handle = player1.Handle;

var localInput = GetControllerInput(0); // read the controller

// notify Backdash of the local player's inputs
var result = session.AddLocalInput(player1Handle, localInput);

if (result is ResultCode.Ok)
{
    result = session.SynchronizeInputs();
    if (result is ResultCode.Ok)
    {
        var gameInputs = session.CurrentSynchronizedInputs;
        AdvanceGameState(gameInputs[0], gameInputs[1], gameState);
    }
}
```

You should
call [`SynchronizeInputs`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-1.html#Backdash_INetcodeSession_1_SynchronizeInputs)
every frame, even those that happen during a rollback. Make sure you always use the values returned
from [`GetInputs`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-1.html#Backdash_INetcodeSession_1_GetInputs_System_Span_Backdash_Data_SynchronizedInput__0___)
rather than the values you've read from the local controllers to advance your game state. During a
rollback [`SynchronizeInputs`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-1.html#Backdash_INetcodeSession_1_SynchronizeInputs)
will replace the values passed
into [`AddLocalInput`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-1.html#Backdash_INetcodeSession_1_AddLocalInput_Backdash_PlayerHandle__0_)
with the values used for previous frames. Also, if you've manually added input delay for the local player to smooth out
the effect of rollbacks, the inputs you pass
into [`AddLocalInput`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-1.html#Backdash_INetcodeSession_1_AddLocalInput_Backdash_PlayerHandle__0_)
won't actually be returned
in [`GetInputs`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-1.html#Backdash_INetcodeSession_1_GetInputs_System_Span_Backdash_Data_SynchronizedInput__0___)
until after the frame delay.

### Implementing your `save` and `load` state handlers

[Backdash](https://github.com/lucasteles/Backdash) will call the [`LoadState`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSessionHandler-1.html#Backdash_INetcodeSessionHandler_1_LoadState_Backdash_Data_Frame___0__) and [`SaveState`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSessionHandler-1.html#Backdash_INetcodeSessionHandler_1_SaveState_Backdash_Data_Frame___0__) callbacks to periodically _save_ and _restore_ the state of your game.

The [`SaveState`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSessionHandler-1.html#Backdash_INetcodeSessionHandler_1_SaveState_Backdash_Data_Frame___0__) function is called with a plain binary buffer writer (`BinaryBufferReader`). You need to call `.Write` on each of the state members.

The [`LoadState`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSessionHandler-1.html#Backdash_INetcodeSessionHandler_1_LoadState_Backdash_Data_Frame___0__) function should restore the game state from a previously saved binary buffer using the `BinaryBufferReader`, reading each member.

> [!IMPORTANT]
> **⚠️:** You must read and write member in the **SAME ORDER** and also ensure the same size (like arrays or lists).

For example:

```csharp
using System.Numerics;
using Backdash.Serialization;
using Backdash.Serialization.Numerics;


public class MyGameState
{
    public int Value1;
    public Vector2 Value2;
}

public class MySessionHandler : INetcodeSessionHandler
{
    MyGameState currentGameState = new();

    public void SaveState(in Frame frame, ref readonly BinaryBufferWriter writer)
    {
        writer.Write(in currentGameState.Value1);
        writer.Write(in currentGameState.Value2);
    }

    public void LoadState(in Frame frame, ref readonly BinaryBufferReader reader)
    {
        reader.Read(ref currentGameState.Value1);
        reader.Read(ref currentGameState.Value2);

        /* or also:
        currentGameState.Value1 = reader.ReadInt32();
        currentGameState.Value2 = reader.ReadVector2();
        */
    }

    /* ... */
}
```

The saved **Game State** will have a calculated [checksum](https://en.wikipedia.org/wiki/Checksum) based on its binary representation. The default implementation is [Fletcher 32](http://en.wikipedia.org/wiki/Fletcher%27s_checksum) algorithm by the class `Fletcher32ChecksumProvider`.

You can also use you own checksum algorithm, for this just implement the interface
[`IChecksumProvider`](https://delta3-studio.github.io/Backdash/api/Backdash.Sync.State.IChecksumProvider-1.html).

### Custom State Serializer

If you don't want to write/read each member or just need to use other serialization method for the state, you is able to access the `IBufferWriter` for _save_ and the raw `ReadOnlySpan<byte>` for _load_:

Example for [MemoryPack](https://github.com/Cysharp/MemoryPack);

```csharp
using System.Numerics;
using Backdash.Data;
using Backdash.Serialization;
using MemoryPack;

[MemoryPackable]
public partial class MyGameState
{
    public int Value1;
    public Vector2 Value2;
}

public class MySessionHandler : INetcodeSessionHandler
{
    MyGameState currentGameState = new();

    public void SaveState(in Frame frame, ref readonly BinaryBufferWriter writer)
    {
        MemoryPackSerializer.Serialize(writer.Buffer, currentGameState);
    }

    public void LoadState(in Frame frame, ref readonly BinaryBufferReader reader)
    {
        MemoryPackSerializer.Deserialize(reader.Buffer, ref currentGameState!);
    }

    /* ... */
}


```


### Advance Frame Callback

This callback is called when a rollback occurs, just after
the [`SaveState`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSessionHandler-1.html#Backdash_INetcodeSessionHandler_1_SaveState_Backdash_Data_Frame___0__)
here you must synchronize inputs and advance the state.

Usually something like:
```csharp
// ...
public void AdvanceFrame()
{
    session.SynchronizeInputs();
    var gameInputs = session.CurrentSynchronizedInputs;
    AdvanceGameState(gameInputs[0], gameInputs[1], gameState);
}
// ...
```

### Remaining Callbacks

There are other callbacks in
the [`INetcodeSessionHandler`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSessionHandler-1.html) for
connection starting/closing the session, peer events, etc.

Check the
**[API Docs for more information](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSessionHandler-1.html)**.

### Frame Lifecycle

We're almost done. The last step is notify [Backdash](https://github.com/lucasteles/Backdash) every time your frame
starts and every time the **game state** finishes advancing by one frame.

Just call [`BeginFrame`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-1.html#Backdash_INetcodeSession_1_BeginFrame)
method on session at the beginning of each frame
and [`AdvanceFrame`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-1.html#Backdash_INetcodeSession_1_AdvanceFrame)
after you've finished one frame **but before you've started the next**.

So, the code for each frame should be something close to:

```csharp
public void Update(){
    session.BeginFrame();

    var localInput = GetControllerInput(localPlayer.Index);
    var result = session.AddLocalInput(localPlayer, localInput);

    if (result is not ResultCode.Ok)
        return;

    result = session.SynchronizeInputs();
    if (result is not ResultCode.Ok)
        return;

    var gameInputs = session.CurrentSynchronizedInputs;
    AdvanceGameState(gameInputs[0], gameInputs[1], gameState);

    session.AdvanceFrame();
}
```

## Input Type Encoding

We **heavily** recommend that you encode your game input inside
a [`Enum`](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/enum) with
[`FlagsAttribute`](https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-flagsattribute).

**Enum flags** are easy to compose and can represent a large number of inputs in a very low byte count, which is
important
because the inputs are what is transmitted over the network to other players.

For example, we can encode all usable digital buttons of a **XBox DPad** using only
[a `short` type](https://learn.microsoft.com/en-us/dotnet/api/system.int16) (_only 2 bytes_):

```csharp
[Flags]
public enum PadButtonInputs : short
{
    None = 0,
    Select = 1 << 0,
    Up = 1 << 1,
    Down = 1 << 2,
    Left = 1 << 3,
    Right = 1 << 4,
    X = 1 << 5,
    Y = 1 << 6,
    A = 1 << 7,
    B = 1 << 8,
    LeftBumper = 1 << 9,
    RightBumper = 1 << 10,
    LeftTrigger = 1 << 11,
    RightTrigger = 1 << 12,
    LeftStickButton = 1 << 13,
    RightStickButton = 1 << 14,
}
```

The serialization of enums and mostly of primitive types is automatically handled
by [Backdash](https://github.com/lucasteles/Backdash).


> [!CAUTION]
> We can also handle serialization of complex structs that **do not** contain any reference type member. for those
> no [`Endianess Convertion`](https://delta3-studio.github.io/Backdash/api/Backdash.NetcodeOptions.html#Backdash_NetcodeOptions_NetworkEndianness) is applied.

### Custom Serializer

If you need a more complex input type and
support [`Endianess convertion`](https://delta3-studio.github.io/Backdash/api/Backdash.NetcodeOptions.html#Backdash_NetcodeOptions_NetworkEndianness)
you must implement
an [`IBinarySerializer<TInput>`](https://delta3-studio.github.io/Backdash/api/Backdash.Serialization.IBinarySerializer-1.html)
for your input type.

> [!TIP]
> 💡 The easiest way to implement a binary serializer is by deriving
> from [`BinarySerializer<T>`](https://delta3-studio.github.io/Backdash/api/Backdash.Serialization.BinarySerializer-1.html)

**Example:**

Giving an input type composed as:

```csharp
[Flags]
public enum PadButtons : short
{
    None = 0,
    Select = 1 << 0,
    Up = 1 << 1,
    Down = 1 << 2,
    Left = 1 << 3,
    Right = 1 << 4,
    X = 1 << 5,
    Y = 1 << 6,
    A = 1 << 7,
    B = 1 << 8,

    LeftBumper = 1 << 9,
    RightBumper = 1 << 10,
    LeftStickButton = 1 << 11,
    RightStickButton = 1 << 12,
}

public record struct Axis
{
    public sbyte X;
    public sbyte Y;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct MyPadInputs
{
    public PadButtons Buttons;
    public byte LeftTrigger;
    public byte RightTrigger;
    public Axis LeftAxis;
    public Axis RightAxis;
}

```

You can implement the serializer as:

```csharp
public class MyPadInputsBinarySerializer : BinarySerializer<PadInputs>
{
    protected override void Serialize(in BinaryRawBufferWriter binaryWriter, in PadInputs data)
    {
        binaryWriter.Write((short)data.Buttons);
        binaryWriter.Write(data.LeftTrigger);
        binaryWriter.Write(data.RightTrigger);

        binaryWriter.Write(data.LeftAxis.X);
        binaryWriter.Write(data.LeftAxis.Y);

        binaryWriter.Write(data.RightAxis.X);
        binaryWriter.Write(data.RightAxis.Y);
    }

    protected override void Deserialize(in BinaryBufferReader binaryReader, ref PadInputs result)
    {
        result.Buttons = (PadButtons)binaryReader.ReadShort();
        result.LeftTrigger = binaryReader.ReadByte();
        result.RightTrigger = binaryReader.ReadByte();

        result.LeftAxis.X = binaryReader.ReadSByte();
        result.LeftAxis.Y = binaryReader.ReadSByte();

        result.RightAxis.X = binaryReader.ReadSByte();
        result.RightAxis.Y = binaryReader.ReadSByte();
    }
}
```
To use your custom serializer:

```csharp
MyPadInputsBinarySerializer mySerializer = new();

var session = RollbackNetcode.WithInputType(t => t.Custom(mySerializer));

```


## Tuning Your Application: Frame Delay vs. Speculative Execution

[Backdash](https://github.com/lucasteles/Backdash) uses both frame delay and speculative execution to hide latency. It
does so by allowing the application developer
the choice of how many frames they'd like to delay input by. If it takes more time to transmit a packet than the
number of frames specified by the game, [Backdash](https://github.com/lucasteles/Backdash) will use speculative
execution to hide the remaining latency. This number
can be tuned by the application mid-game if you so desire. Choosing a proper value for the frame delay depends very much
on your game. Here are some helpful hints.

In general, you should try to make your frame delay as high as possible without affecting the qualitative experience of
the game. For example, a fighting game requires pixel-perfect accuracy, excellent timing, and extremely tightly
controlled joystick motions. For this type of game, any frame delay larger than 1 can be noticed by most intermediate
players, and expert players may even notice a single frame of delay. On the other hand, board games or puzzle games
which do not have very strict timing requirements may get away with setting the frame latency as high as 4 or 5 before
users begin to notice.

Another reason to set the frame delay high is to eliminate the glitching that can occur during a rollback. The longer
the rollback, the more likely the user is to notice the discontinuities caused by temporarily executing the incorrect
prediction frames. For example, suppose your game has a feature where the entire screen will flash for exactly 2 frames
immediately after the user presses a button. Suppose further that you've chosen a value of 1 for the frame latency and
the time to transmit a packet is 4 frames. In this case, a rollback is likely to be around 3 frames (4 – 1 = 3). If the
flash occurs on the first frame of the rollback, your 2-second flash will be entirely consumed by the rollback, and the
remote player will never get to see it!  In this case, you're better off either specifying a higher frame latency value
or redesigning your video renderer to delay the flash until after the rollback occurs.

## Sample Applications

Check the samples on the [samples](https://github.com/lucasteles/Backdash/tree/master/samples) directory:

There are examples for up to 4 players:

- [Simple console game](https://github.com/lucasteles/Backdash/tree/master/samples/ConsoleGame)
- [Monogame SpaceWar](https://github.com/lucasteles/Backdash/tree/master/samples/SpaceWar)
- [Monogame SpaceWar with lobby over internet](https://github.com/lucasteles/Backdash/tree/master/samples/SpaceWar.Lobby)
- [Godot SpaceWar with lobby over internet](https://github.com/lucasteles/BackdashGodotSample)

> See the `.cmd`/`.sh` files in the `scripts` directory for examples on how to start 2, 3, and 4-player games.
