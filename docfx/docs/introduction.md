# Introduction

[Backdash](https://github.com/Delta3-Studio/Backdash) is a multiplayer networking SDK that helps you add
[rollback netcode](https://words.infil.net/w02-netcode.html) to a game.

Backdash began as a [.NET](https://dotnet.microsoft.com) port inspired by
[GGPO](https://github.com/pond3r/ggpo), a rollback netcode SDK written in C++. It can be used with game engines that
support .NET languages such as [C#](https://dotnet.microsoft.com/en-us/languages/csharp) and
[F#](https://dotnet.microsoft.com/en-us/languages/fsharp), including [Godot](https://godotengine.org/),
[MonoGame](https://monogame.net/), and [Stride](https://www.stride3d.net). This avoids the need to bind the game to
GGPO's native C++ library.

Backdash also provides configurable session options and extension points through
[`NetcodeOptions`](https://delta3-studio.github.io/Backdash/api/Backdash.NetcodeOptions.html) and
[`SessionServices`](https://delta3-studio.github.io/Backdash/api/Backdash.SessionServices-2.html).

> [!NOTE]
> Backdash currently targets .NET 10. Unity 6's documented API compatibility profiles target .NET Standard 2.1 or
> .NET Framework 4.8, so the Backdash package cannot be assumed to work in a Unity project as-is. Check
> [Unity's .NET profile support](https://docs.unity3d.com/6000.0/Documentation/Manual/dotnet-profile-support.html)
> and the requirements of your Unity version and build target.

## How Does It Work?

Rollback networking works best with a deterministic peer-to-peer game simulation. If every peer starts from the same
state and processes the same inputs in the same order, each peer should produce the same game state. A traditional
lockstep approach waits for every peer's input before simulating a frame. Network delay can make that approach feel
unresponsive.

With rollback, the game can simulate using local inputs and predictions for remote inputs that have not arrived yet.
When the actual inputs arrive, Backdash compares them with the predictions. If they differ, it restores an earlier game
state and re-simulates the affected frames. This reduces input delay, although incorrect predictions can cause visible
corrections and the simulation may still pause when its prediction limit is reached.

Don't worry if that sounds like a headache. [Backdash](https://github.com/Delta3-Studio/Backdash) was designed specifically
to implement the rollback algorithms and low-level networking logic in a way that's easy to integrate into your existing
game loop. If you simply implement the functionality to save your game state, load it back up, and execute a frame of
game state without rendering its outcome, [Backdash](https://github.com/Delta3-Studio/Backdash) can take care of the rest.

## Learning resources

More information about how it works and why it is good:

- [Analysis: Why Rollback Netcode Is Better](https://www.youtube.com/watch?v=0NLe4IpdS1w) _(video)_.
- [Infil's Netcode Article](https://words.infil.net/w02-netcode-p2.html).
- [Talking Rollback Netcode With Adam "Keits" Heart](https://www.youtube.com/watch?v=1RI5scXYhK0) _(video)_.
- [GDC Rollback Networking in Mortal Kombat and Injustice 2](https://www.youtube.com/watch?v=7jb0FOcImdg) _(video)_.
- [EVO 2017: GGPO panel](https://www.youtube.com/watch?v=k9JTIn1SVQ4) _(video)_.
- [Fight the Lag! The Trick Behind GGPO's Low Latency Netcode](https://drive.google.com/file/d/1cV0fY8e_SC1hIFF5E1rT8XRVRzPjU8W9/view)
- [Cross Counter LIVE feat. Mike Z](https://www.youtube.com/watch?v=Tu2kAdmUCaI&t=41m22s) _(video)_.
