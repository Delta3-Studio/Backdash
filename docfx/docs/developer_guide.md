## Developer Guide

> [!NOTE]
> Adapted from the [GGPO Developer Guide](https://github.com/pond3r/ggpo/blob/master/doc/DeveloperGuide.md).

### Installing

[NuGet package](https://www.nuget.org/packages/Backdash) available:

```ps
$ dotnet add package Backdash
```

## Game State and Inputs

Your game probably has many moving parts. [Backdash](https://github.com/Delta3-Studio/Backdash) only depends on these
two:

- **Game State** describes the current state of everything in your game. In a shooter, this would include the position
  of the ship and all the enemies on the screen, the location of all the bullets, how much health each opponent has, the
  current score, etc.

- **Game Inputs** are the set of things that modify the game state. These obviously include the joystick and button
  presses done by the player, but can also include other non-obvious inputs as well. For example, if your game uses the
  current time of day to calculate something in the game, the current time of day at the beginning of a frame is also an
  input.

There are many other things in your game engine that are neither game state nor inputs. For example, your audio and
video renderers are not game state since they don't affect the outcome of the game. If you have a special
effects engine that's generating effects that do not have an impact on the game, they can be excluded from the game
state as well.

## Using State and Inputs for Synchronization

Each player in a [Backdash](https://github.com/Delta3-Studio/Backdash) networked game has a complete copy of the game
running. [Backdash](https://github.com/Delta3-Studio/Backdash) needs to keep both copies of the
game state in sync to ensure that both players are experiencing the same game. It would be much too expensive to send an
entire copy of the game state between players every frame.
Instead, [Backdash](https://github.com/Delta3-Studio/Backdash)
sends the players' inputs to each other and has
each player step the game forward. For this to work, your game engine must meet three criteria:

1. The game simulation must be fully deterministic. That is, for any given game state and inputs, advancing the game
   state by exactly 1 frame must result in identical game states for all players.
2. The game state must be fully encapsulated and serializable.
3. Your game engine must be able to load, save, and execute a single simulation frame without rendering the result of
   that frame. **This will be used to implement rollbacks**.

## Programming Guide

The following section contains a walk-through for porting your application
to [Backdash](https://github.com/Delta3-Studio/Backdash).

For a detailed description of the
[Backdash](https://github.com/Delta3-Studio/Backdash) API, please see
the [API Reference Docs](https://delta3-studio.github.io/Backdash/api/Backdash.html).

### Interfacing with [Backdash](https://github.com/Delta3-Studio/Backdash)

[Backdash](https://github.com/Delta3-Studio/Backdash) is designed to be easy to interface with new and existing game
engines. It handles most of the implementation of handling rollbacks by calling out to your application via
the [`INetcodeSessionHandler`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSessionHandler-1.html)
hooks.

### Creating the [
`INetcodeSession`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-2.html) Object

The [`INetcodeSession<TInput>`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-2.html) object is
your interface to the [Backdash](https://github.com/Delta3-Studio/Backdash) framework.

Create one with the `RollbackNetcode.WithInputType` builder:

The input type must be unmanaged. A flags enum or a struct made up of unmanaged fields is a common choice.

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

To create a new session bound to port `9001`:

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

The session builder can be used to configure the session by setting [
`NetcodeOptions`](https://delta3-studio.github.io/Backdash/api/Backdash.NetcodeOptions.html):

- passing an instance to `.WithOptions(..)`
- using a delegate function on `.Configure(options => {})`
- using the fluent API

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

### Network and protocol options

Network-specific settings are grouped under `NetcodeOptions.Protocol`. You can set them in the `Configure` callback or
pass a configured `NetcodeOptions` to the builder. For example:

```csharp
using System;
using Backdash;

var session = RollbackNetcode
    .WithInputType<MyGameInput>()
    .Configure(options =>
    {
        options.Protocol.DisconnectTimeout = TimeSpan.FromSeconds(5);
        options.Protocol.ConsistencyCheckEnabled = true;
        options.Protocol.ConsistencyCheckInterval = TimeSpan.FromSeconds(1);
    })
    .Build();
```

The default protocol configuration sends keep-alives every 200 ms, resends pending inputs after 200 ms without a
recent input exchange, and checks state consistency every second. Set `ConsistencyCheckInterval` to `TimeSpan.Zero` to
disable periodic consistency checks. `MaxPendingInputs` limits queued local inputs; when full, adding input can return
`ResultCode.InputDropped`. `MaxPackageQueue` bounds outgoing UDP messages and drops the oldest queued message when it
fills. Use the [
`ProtocolOptions` API reference](https://delta3-studio.github.io/Backdash/api/Backdash.Options.ProtocolOptions.html)
for the complete list of settings.

You should also define an implementation of
the [`INetcodeSessionHandler`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSessionHandler-1.html)
filled in with your game's callback functions for managing game state.

```csharp
using Backdash.Data;
using Backdash.Serialization;

public class MySessionHandler : INetcodeSessionHandler
{
    public void OnSessionStart() { /* ... */ }
    public void OnSessionClose() { /* ... */ }
    public void SaveState(Frame frame, ref readonly BinaryBufferWriter writer) { /* ... */ }
    public void LoadState(Frame frame, ref readonly BinaryBufferReader reader) { /* ... */ }
    public void AdvanceFrame() { /* ... */ }
    public void TimeSync(FrameSpan framesAhead) { /* ... */ }
    public void OnPeerEvent(NetcodePlayer player, in PeerEventInfo evt) { /* ... */ }
}
```

And then, set it into the session:

```csharp
session.SetHandler(new MySessionHandler());
```

The [`INetcodeSession`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-2.html) object should only
be used for a single game session. If you need to connect to another opponent, you must close the existing session
object using
[`.Dispose`](https://learn.microsoft.com/pt-br/dotnet/api/system.idisposable.dispose?view=net-8.0) method first, and
then start a new one:

```csharp
/* Close the current session to start a new one */
session.Dispose();
```

### Setting Players

When you created the [`INetcodeSession`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-2.html),
you didn't specify any information about the players participating in the game.
To do so, add a [`NetcodePlayer`](https://delta3-studio.github.io/Backdash/api/Backdash.NetcodePlayer.html) for each
player with `AddPlayer`, or provide the players to the builder with `WithPlayers`. The following example shows a
2-player remote session:

```csharp
var player1 = NetcodePlayer.CreateLocal(); // local player number 1

var player2Endpoint = IPEndPoint.Parse("192.168.0.100:8001"); // player 2 ip and port
var player2 = NetcodePlayer.CreateRemote(player2Endpoint); // remote player number 2

// add players in session builder
var session = RollbackNetcode
    .WithInputType<MyGameInput>()
    .WithPort(9001)
    .WithPlayers(player1, player2) // player list
    .ForRemote()
    .Build();

// ...
```

You can also add players after building the session and before starting it:

```csharp

var session = RollbackNetcode
    .WithInputType<MyGameInput>()
    .WithPort(9001)
    .ForRemote()
    .Build();

ResultCode result;
result = session.AddPlayer(player1);
// ...
result = session.AddPlayer(player2);
// ...
```

Every instance of [`NetcodePlayer`](https://delta3.studio/Backdash/api/Backdash.NetcodePlayer) will have an unique [**
`Guid` Id**](https://delta3.studio/Backdash/api/Backdash.NetcodePlayer.html#Backdash_NetcodePlayer_Id) _(It can be set
when you are instantiating the player)_

When a [`NetcodePlayer`](https://delta3.studio/Backdash/api/Backdash.NetcodePlayer) is added to a session, it will
receive a unique [`Index`](https://delta3.studio/Backdash/api/Backdash.NetcodePlayer#Backdash_NetcodePlayer_Index) and [
`Number`](https://delta3.studio/Backdash/api/Backdash.NetcodePlayer#Backdash_NetcodePlayer_Number)

`CustomId` is an application-defined identifier you can use to look up a player. It does not change Backdash's
session-assigned `Index` or `Number`.

```csharp
var player1 = NetcodePlayer.CreateLocal(); // local player number 1
player1.CustomId = 10;
var player2 = NetcodePlayer.CreateRemote(player2Endpoint); // remote player number 2
player2.CustomId = 20;

if (session.TryGetPlayerByCustomId(20, out var foundPlayer))
    Console.WriteLine($"Player 20 found: {foundPlayer.Id}")

// ...
```

Another useful option is to inherit the class `NetcodePlayer` and customize it as needed:

```csharp
public enum PlayerSide
{
    Left = -1,
    Right = 1,
}

public sealed class SessionPlayer : NetcodePlayer
{
    public SessionPlayer(
        int playerNumber, PlayerSide side,
        PlayerType type, IPEndPoint? endPoint = null
    ) : base(type, endPoint)
    {
        PlayerSide = side;
        CustomId = playerNumber;
    }

    public readonly PlayerSide PlayerSide;

    // ...
}

```

> For further, more complex setups, check the [samples](https://github.com/Delta3-Studio/Backdash/tree/master/samples).

### Starting session

After setting up the players, you must call the
session [.Start ()](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-2.html#Backdash_INetcodeSession_2_Start_System_Threading_CancellationToken_)
method. This will start all the background work, like the socket receiver, input queue, peer synchronization, etc.

```csharp
session.Start();
```

### Synchronizing Local and Remote Inputs

Input synchronization happens on
the [session](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-1.html) at the top of each game
frame.
This is done by
calling [`AddLocalInput`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-2.html)
for each local player
and [
`SynchronizeInputs`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-1.html#Backdash_INetcodeSession_1_SynchronizeInputs)
to collect the inputs for all players. Check the return value of `SynchronizeInputs`. If it is not
[`ResultCode.Ok`](https://delta3-studio.github.io/Backdash/api/Backdash.ResultCode.html), do not advance the game state.
For a remote session, this can happen when Backdash reaches its prediction limit while waiting for remote input.

After synchronizing, read one player's value from `CurrentInputs[index]`, or copy all values into a caller-provided
buffer with `GetInputs`. `GetInput(player)` returns a `SynchronizedInput<TInput>`; its `Input` property contains the
game input and its `Disconnected` property indicates the player's connection state.

For example, if your code looks like this currently for a local game:

```csharp
MyGameInput player1Input = GetControllerInput(0);
MyGameInput player2Input = GetControllerInput(1);

/* send p1 and p2 to the game */
AdvanceGameState(player1Input, player2Input, gameState);
```

You should change it to read as follows:

```csharp

// Keep the local player's NetcodePlayer reference.
var localPlayer = player1;

var localInput = GetControllerInput(0); // read the controller

// notify Backdash of the local player's inputs
var result = session.AddLocalInput(localPlayer, localInput);

if (result is ResultCode.Ok)
{
    result = session.SynchronizeInputs();
    if (result is ResultCode.Ok)
    {
        var gameInputs = session.CurrentInputs;
        AdvanceGameState(gameInputs[0], gameInputs[1], gameState);
    }
}
```

Call [`SynchronizeInputs`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-2.html) every frame,
including during a rollback. Advance the game with synchronized inputs, not directly with newly read controller values.
During rollback, Backdash replays the inputs associated with the resimulated frames. If you configure an input delay,
newly submitted local input is applied to a later frame.

### Implementing your `save` and `load` state handlers

[Backdash](https://github.com/Delta3-Studio/Backdash) will call the [
`LoadState`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSessionHandler-1.html#Backdash_INetcodeSessionHandler_1_LoadState_Backdash_Data_Frame___0__)
and [
`SaveState`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSessionHandler-1.html#Backdash_INetcodeSessionHandler_1_SaveState_Backdash_Data_Frame___0__)
callbacks to periodically _save_ and _restore_ the state of your game.

The [
`SaveState`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSessionHandler-1.html#Backdash_INetcodeSessionHandler_1_SaveState_Backdash_Data_Frame___0__)
callback receives a `BinaryBufferWriter`. Write each game-state member to it.

The [
`LoadState`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSessionHandler-1.html#Backdash_INetcodeSessionHandler_1_LoadState_Backdash_Data_Frame___0__)
function should restore the game state from a previously saved binary buffer using the `BinaryBufferReader`, reading
each member.

> [!IMPORTANT]
> **⚠️:** You must read and write members in the **SAME ORDER** and also ensure the same size (like arrays or lists).

For example:

```csharp
using System.Numerics;
using Backdash.Data;
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

    public void SaveState(Frame frame, ref readonly BinaryBufferWriter writer)
    {
        writer.Write(in currentGameState.Value1);
        writer.Write(in currentGameState.Value2);
    }

    public void LoadState(Frame frame, ref readonly BinaryBufferReader reader)
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

The saved **Game State** will have a calculated [checksum](https://en.wikipedia.org/wiki/Checksum) based on its binary
representation. The default implementation is [Fletcher 32](http://en.wikipedia.org/wiki/Fletcher%27s_checksum)
algorithm by the class `Fletcher32ChecksumProvider`.

You can also use your own checksum algorithm; for this, just implement the interface [
`IChecksumProvider`](https://delta3-studio.github.io/Backdash/api/Backdash.Sync.State.IChecksumProvider-1.html).

### Custom State Serializer

If you don't want to write/read each member or just need to use another serialization method for the state, you can
access the `IBufferWriter` for _save_ and the raw `ReadOnlySpan<byte>` for _load_:

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

    public void SaveState(Frame frame, ref readonly BinaryBufferWriter writer)
    {
        MemoryPackSerializer.Serialize(writer.Buffer, currentGameState);
    }

    public void LoadState(Frame frame, ref readonly BinaryBufferReader reader)
    {
        MemoryPackSerializer.Deserialize(reader.Buffer, ref currentGameState!);
    }

    /* ... */
}


```

### Advance Frame Callback

This callback is called when a rollback occurs, just after
the [
`SaveState`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSessionHandler-1.html#Backdash_INetcodeSessionHandler_1_SaveState_Backdash_Data_Frame___0__)
here, you must synchronize inputs and advance the state.

Usually something like:

```csharp
// ...
public void AdvanceFrame()
{
    session.SynchronizeInputs();
    var gameInputs = session.CurrentInputs;
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

We're almost done. The last step is notify [Backdash](https://github.com/Delta3-Studio/Backdash) every time your frame
starts and every time the **game state** finishes advancing by one frame.

Just call [
`BeginFrame`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-1.html#Backdash_INetcodeSession_1_BeginFrame)
method in the session at the beginning of each frame
and [
`AdvanceFrame`](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-1.html#Backdash_INetcodeSession_1_AdvanceFrame)
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

    var gameInputs = session.CurrentInputs;
    AdvanceGameState(gameInputs[0], gameInputs[1], gameState);

    session.AdvanceFrame();
}
```

## Input Type Encoding

We recommend encoding digital game inputs as an
[`enum`](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/enum) marked with
[`FlagsAttribute`](https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-flagsattribute).

**Enum flags** are easy to compose and can represent a large number of inputs in a very low byte count, which is
important
because the inputs are what is transmitted over the network to other players.

For example, a game's digital buttons can be represented using only
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
by [Backdash](https://github.com/Delta3-Studio/Backdash).

> [!CAUTION]
> Backdash can serialize unmanaged structs that contain no reference-type fields. This uses their in-memory
> representation, so the struct's fields do not receive per-field endianness conversion. For portable network input,
> provide an explicit serializer.

### Custom Input Serializer

If you need a custom input layout or explicit endianness handling, implement
an [
`IBinarySerializer<TInput>`](https://delta3-studio.github.io/Backdash/api/Backdash.Serialization.IBinarySerializer-1.html)
for your input type.

Every peer must use a compatible input format and endianness so that the same input bytes decode to the same value.

> [!TIP]
> 💡 Backdash provides `BinarySpanWriter` and `BinaryBufferReader` to help implement the serializer.

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
using Backdash.Network;
using Backdash.Serialization;

public sealed class MyPadInputsBinarySerializer(Endianness endianness) : IBinarySerializer<MyPadInputs>
{
    public Endianness Endianness { get; } = endianness;

    public int Serialize(in MyPadInputs data, Span<byte> buffer)
    {
        var offset = 0;
        BinarySpanWriter writer = new(buffer, ref offset, Endianness);
        writer.Write((short)data.Buttons);
        writer.Write(data.LeftTrigger);
        writer.Write(data.RightTrigger);

        writer.Write(data.LeftAxis.X);
        writer.Write(data.LeftAxis.Y);

        writer.Write(data.RightAxis.X);
        writer.Write(data.RightAxis.Y);
        return writer.WrittenCount;
    }

    public int Deserialize(ReadOnlySpan<byte> buffer, ref MyPadInputs result)
    {
        var offset = 0;
        BinaryBufferReader reader = new(buffer, ref offset, Endianness);
        result.Buttons = (PadButtons)reader.ReadInt16();
        result.LeftTrigger = reader.ReadByte();
        result.RightTrigger = reader.ReadByte();

        result.LeftAxis.X = reader.ReadSByte();
        result.LeftAxis.Y = reader.ReadSByte();

        result.RightAxis.X = reader.ReadSByte();
        result.RightAxis.Y = reader.ReadSByte();
        return reader.ReadCount;
    }
}
```

To use your custom serializer:

```csharp
var mySerializer = new MyPadInputsBinarySerializer(Endianness.BigEndian);

var session = RollbackNetcode
    .WithInputType(t => t.Custom(mySerializer))
    .Build();

```

## Tuning Your Application: Input Delay and Prediction

[Backdash](https://github.com/Delta3-Studio/Backdash) lets you configure how many frames to delay local input. If remote
input takes longer to arrive than this delay covers, Backdash predicts the missing input and may roll back when the
actual input arrives. You can adjust the delay during a session. The right value depends on how much input latency your
game can tolerate and how disruptive visible rollbacks are.

Increasing input delay can reduce the number of predicted frames, but it also adds latency to local controls. Start with
a low delay and tune it against real network conditions and playtesting. Games with precise timing often need less input
delay than turn-based or slower-paced games.

Longer rollbacks can make corrections more noticeable. For effects that should only be presented once, separate their
presentation from deterministic simulation so rollback re-simulation does not replay sounds, particles, or other visual
effects multiple times.

## Sample Applications

Check the samples on the [samples](https://github.com/Delta3-Studio/Backdash/tree/master/samples) directory:

There are examples for up to 4 players:

- [Simple console game](https://github.com/Delta3-Studio/Backdash/tree/master/samples/ConsoleGame)
- [Monogame SpaceWar](https://github.com/Delta3-Studio/Backdash/tree/master/samples/SpaceWar)
- [Monogame SpaceWar with lobby over the internet](https://github.com/Delta3-Studio/Backdash/tree/master/samples/SpaceWar.Lobby)
- [Godot SpaceWar with lobby over the internet](https://github.com/Delta3-Studio/BackdashGodotSample)

> See the `.cmd`/`.sh` files in the `scripts` directory for examples on how to start 2, 3, and 4-player games.
