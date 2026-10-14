# Best Practices and Troubleshooting

Below is a list of recommended best practices you should consider while porting your application
to [Backdash](https://github.com/Delta3-Studio/Backdash). Many of these recommendations are easy to follow, even if you're
not starting a game from scratch. Most applications will already conform to most of the recommendations below.

## Isolate **Game State** from **Non-Game State**

[Backdash](https://github.com/Delta3-Studio/Backdash) will periodically request that you save and load the entire state of
your game. For most games, the state that needs to be saved is a tiny fraction of the entire game. Usually, the video
and
audio renderers, look-up tables, textures, sound data, and your code segments are either constant from frame to frame or
not involved in the calculation of the game state. These do not need to be saved or restored.

You should isolate **non-game state** from the **game state** as **much as possible**. For example, you may consider
encapsulating all your game state into a single `class`/`record` type. This both clearly delineates what is
**game state** and what is not, and makes it trivial to implement the save and load callbacks.

## Advance Your Game State in Fixed Time Steps

[Backdash](https://github.com/Delta3-Studio/Backdash) will occasionally need to rollback and single-step your application
frame by frame. This is difficult if the simulation advances by a variable time step. Advance the game state by a fixed
time step for each simulation frame, even if the render loop runs at a variable rate.

## Separate Updating Game State from Rendering in Your Game Loop

[Backdash](https://github.com/Delta3-Studio/Backdash) will call your advance frame callback many times during a rollback.
Any effects or sounds that are generated
during the rollback need to be deferred until after the rollback is finished. This is most easily accomplished by
separating your game state from your render state. When you're finished, your game loop may look something like this:

```csharp
bool finished = false;
GameState state;
Inputs inputs;

do
{
    GetControllerInputs(ref inputs);
    finished = AdvanceGameState(inputs, state);
    if (!finished)
        RenderCurrentFrame(state);
} while (!finished);
```

In other words, your game state should be determined solely by the inputs, and your rendering code should be driven by the
current game state, and you should have a way to easily advance the game state forward using a set of inputs without
rendering.

## Make Sure Your Game State Advances Deterministically

Once you have your game state identified, make sure the next game state is computed solely from your game inputs. This
should happen naturally if you have correctly identified all the game states and inputs, but it can be tricky sometimes.
Here are some things that are easy to overlook:

### Beware of Random Number Generators

Many games use random numbers when computing the next game state. The random sequence must be identical on every peer
and repeat correctly after loading an earlier frame. Prefer Backdash's session random generator. If you use your own
generator, save and restore its state along with the game state.

Luckily, *Backdash* already gives you a deterministic random number generator in the [`INetcodeSession<>`](https://delta3.studio/Backdash/api/Backdash.INetcodeSession-1.html#Backdash_INetcodeSession_1_Random) class:

```csharp
session.Random.NextInt();
```

### Avoid External Time and Environment Inputs

Be careful if you use the current time of day in your game state calculation. This may be used for an effect on the game
or to derive another game state (e.g., using the timer as a seed to the random number generator). The time on two
computers or game consoles are almost never in sync, and using time in your game-state calculations can lead to synchronization
issues. You should either eliminate the use of time in your game state or include the current time for one of the
players as part of the input to a frame and always use that time in your calculations.

The use of external time sources in **non-game state** calculations is fine (_e.g, computing the duration of effects on
screen, or the attenuation of audio samples_).

> [!TIP]
> We provide an implementation of a
_[Deterministic Random](https://delta3-studio.github.io/Backdash/api/Backdash.Synchronizing.Random.IDeterministicRandom.html)_
> out of the box
> which can be accessed directly from the
_[Rollback Session](https://delta3-studio.github.io/Backdash/api/Backdash.INetcodeSession-1.html#Backdash_INetcodeSession_1_Random)_

## Beware of Dangling References

If your game state contains reference types, serialize the values needed to restore the simulation. Do not rely on
copying object references: mutable objects may be shared between a saved snapshot and the live game state.

## Beware of Static Variables or Other Hidden State

The language your game is written in may have features that make it difficult to track down all your state. [Static
automatic variables in `C`](https://www.javatpoint.com/auto-and-static-variable-in-c)
or [static members in
`C#`](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/static-classes-and-static-class-members)
are examples of this behavior. You need to track down all these locations and convert them to
a form that can be saved. For example, compare:

```csharp
using Backdash.Data;
using Backdash.Serialization;

// This will totally get you into trouble.
public record MyGameState
{
    private static int Counter;

    public int NextCounter => Counter++;
}
```

To:

```csharp
// If you must, this is better
public static class GlobalState
{
    public static int Counter; // move counter to a global static class

    public static int GetNextCounter() => Counter++;
}

public record MyGameState
{
    public int CurrentCounter; // keeps track of the counter value
}

public class MySessionHandler : INetcodeSessionHandler
{
    MyGameState currentGameState = new();

    public void SaveState(Frame frame, ref readonly BinaryBufferWriter writer)
    {
        writer.Write(in currentGameState.CurrentCounter);
    }

    public void LoadState(Frame frame, ref readonly BinaryBufferReader reader)
    {
        currentGameState.CurrentCounter = reader.ReadInt32();
        GlobalState.Counter = currentGameState.CurrentCounter;
    }

    /* ...  */
}
```

## Use SyncTest to Find Desynchronization

Once you've integrated [Backdash](https://github.com/Delta3-Studio/Backdash), use a `SyncTest` session to help find
simulation state that was omitted from save/load or otherwise causes desynchronization.

To create a sync test session use:

```csharp
using Backdash;

var session = RollbackNetcode
    .WithInputType<MyGameInput>()
    .Configure(options =>
    {
        // ...
    })
    .ForSyncTest(options =>
    {
        options.CheckDistance(1);
    })
    .Build();
```

The sync test session is a special session that is designed to find errors in your simulation's
determinism.

By default, a **sync-test session** checks each frame by replaying it after a one-frame rollback and comparing the
resulting state checksum with the original. A mismatch is logged and, by default, throws an exception. When state
logging is enabled (the default), Backdash logs both state representations at error level. `UseJsonStateParser()` formats
an object returned by `INetcodeSessionHandler.CreateState` as JSON; if that method returns `null`, the parser falls back
to a hexadecimal representation of the saved bytes. Use `.LogState(false)` to disable state logging.

By running **sync-test** on developer systems continuously when writing game code, you can identify **desync** causing
bugs immediately after they're introduced.

### Configuring

You can configure a wide range of options to help debug your state, like:

```csharp
using Backdash;

var session = RollbackNetcode
    .WithInputType<MyGameInput>()
    .ForSyncTest(options =>
    {
        options.UseJsonStateParser();
        options.UseDesyncHandler<YourDesyncHandler>();
        options.UseRandomInputProvider();
        options.CheckDistance(4);
    })
    .Build();
```

If you need a better, meaningful string representation of your state in the sync-test, it is recommended to implement
the optional method `INetcodeSessionHandler.CreateState`. This will be used to materialize previously saved states
before passing them to the `DesyncHandler`.

> [!NOTE]
> `.UseJsonStateParser()` produces JSON when `INetcodeSessionHandler.CreateState` returns a JSON-serializable object.
> If it returns `null`, the parser displays the saved bytes as hexadecimal instead.

#### Implement a DesyncHandler

A desynchronization handler lets you inspect a mismatch in a `SyncTest` session.

For example, this handler uses [DiffPlex](https://github.com/mmanela/diffplex) to print a state diff to the console:

```csharp
using Backdash;
using Backdash.Synchronizing.State;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

sealed class DiffPlexDesyncHandler : IStateDesyncHandler
{
    public void Handle(INetcodeSession session, in DesyncState previous, in DesyncState current)
    {
        var diff = InlineDiffBuilder.Diff(previous.Value, current.Value);

        var savedColor = Console.ForegroundColor;

        foreach (var line in diff.Lines)
        {
            switch (line.Type)
            {
                case ChangeType.Inserted:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("+ ");
                    break;
                case ChangeType.Deleted:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("- ");
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("  ");
                    break;
            }

            Console.WriteLine(line.Text);
        }

        Console.ForegroundColor = savedColor;
    }
}
```

> [!TIP]
> You can see this implementation working with JSON in
> the [SpaceWar](https://github.com/Delta3-Studio/Backdash/tree/master/samples/SpaceWar) sample.

## Where to Go from Here

This document describes the most basic features of [Backdash](https://github.com/Delta3-Studio/Backdash).
To learn more, I recommend starting with reading
the [API Docs](https://delta3-studio.github.io/Backdash/docs/introduction.html) and exploring
the [examples](https://github.com/Delta3-Studio/Backdash/tree/master/samples).

Good luck!
