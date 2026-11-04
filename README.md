# Eternal War C# Code Showcase

This repository contains selected C# contributions I made to **Eternal War**, a multiplayer tactical roguelike built around increasingly difficult combat encounters.

The code is organized around a separation between the shared game simulation and the engine-specific presentation layer. The same combat rules can therefore be used by the client, the server, and enemy AI without making the simulation depend on Godot.

> **Scope:** This is a source-code showcase, not a complete game checkout. It includes selected application, simulation, server, networking, Godot-integration, and test code, but does not include the complete Godot project or the game's external data/assets needed to launch a playable build.

## What this code demonstrates

- **Engine-independent gameplay simulation.** Shared combat rules and state live outside the Godot project.
- **Simulation-driven enemy AI.** Goal-oriented AI evaluates possible actions by running them through the same combat simulator used by the game, rather than relying on a separate approximation of combat.
- **Command-based state changes and event recording.** Combat commands update state and produce events that can be consumed by gameplay systems and presentation code.
- **Separation of application and presentation.** Controllers and models describe game behavior while interfaces let the engine layer provide concrete views and input bindings.
- **Server-side request validation.** The server validates the identity and permissions associated with incoming player commands before applying and relaying them.
- **Replaceable networking transports.** Transport interfaces are separated from the client/server wrappers and their concrete backends.

## Architecture

The code is split into four main responsibilities:

| Layer | Responsibility |
| --- | --- |
| **Core simulation** | Shared data structures, game state, database definitions, combat rules, AI decision-making, and simulation. It does not depend on client, server, or Godot presentation code. |
| **Client application** | Client session state, game and interaction controllers, network message handling, and engine-agnostic views. |
| **Godot integration** | Concrete Godot views, UI bindings, interaction factories, visual resources, and animation playback. |
| **Server application** | Server session and controllers, request validation, command application, and communication with connected clients. |

### Typical interaction and presentation flow

1. A Godot UI binder or interaction factory connects a UI element to an `Interaction Action`.
2. The `Interaction Controller` handles that action. When it affects gameplay, it invokes the appropriate `Game Action`.
3. The client application creates a command request and sends it to the server.
4. The server checks that the request belongs to the transport-level client that sent it and that the requested change is allowed. Accepted commands are applied and relayed; rejected requests are denied.
5. The client session processes the resulting authoritative updates. The engine-agnostic `Game View` translates state changes into `Game View Actions`.
6. Godot implementations of those actions update the visible game. Presentation tasks allow visual changes and animations to finish before later presentation changes are consumed.

Network message handlers provide the corresponding entry point for messages received from the server. This keeps networking, interaction handling, gameplay rules, and rendering from becoming one tightly coupled system.

## Selected code

The following files are useful starting points for exploring the implementation. Links open the corresponding source in this repository.

### Combat simulation and AI

| File | What to look for |
| --- | --- |
| [`CombatSimulator.cs`](ExternalFiles/GameDataShared/Model/Combat/Simulation/CombatSimulator.cs) | Runs an action sequence through the combat command pipeline and returns the resulting state, random state, command log, and event log. |
| [`CombatCommandPipeline.cs`](ExternalFiles/GameDataShared/Model/Combat/Simulation/CombatCommandPipeline.cs) | Coordinates command processing, event recording, and event observers such as the passive resolver. |
| [`CombatCommandProcessor.cs`](ExternalFiles/GameDataShared/Model/Combat/Simulation/CombatCommandProcessor.cs) | Applies commands to the current combat state, records the commands and derived events, and notifies observers. |
| [`UnitDecisionControllerHandler_GoalOriented.cs`](ExternalFiles/GameDataShared/Model/Combat/Simulation/Handlers/UnitDecisionController/UnitDecisionControllerHandler_GoalOriented.cs) | Evaluates candidate action sequences and selection combinations by simulating them, then chooses actions according to configured goal priorities. |
| [`CombatAiHandlerTests.cs`](Tests/GameDataShared.Tests/CombatAiHandlerTests.cs) | Tests AI conditions and target-selection behavior. |

The AI's use of the shared simulator is a central architectural choice: evaluating an action uses the same combat rules that determine its actual result. The goal-oriented controller compounds selected actions across a turn and stops when it cannot find another useful action or when a simulated action makes no state progress.

### Client application and presentation

| File | What to look for |
| --- | --- |
| [`GameApplication.cs`](EternalWar-CSharp/GameApplication.cs) | Composes the client session, controllers, message handling, and game view; processes incoming messages and pending presentation changes. |
| [`GameController.cs`](EternalWar-CSharp/GameController.cs) | Composes the high-level game, lobby, and run controllers behind the application's game-action interfaces. |
| [`GameView.cs`](EternalWar-CSharp/View/GameView.cs) | Routes game-state and interaction changes to the lobby and run views. |
| [`PresentationTask.cs`](EternalWar-CSharp/View/PresentationTask.cs) | Provides a small task abstraction for coordinating asynchronous presentation work, including animations. |
| [`CombatUnitsAnimationController.cs`](EternalWar-CSharp/Run/Combat/Views/CombatUnitsAnimationController.cs) | Manages unit animation requests and coordinates loop and one-shot animations through the combat view actions. |

### Godot integration

| File | What to look for |
| --- | --- |
| [`GodotGameViewComposer.cs`](EternalWar-Godot/Scripts/Godot/GodotGameViewComposer.cs) | Binds Godot UI elements to interaction actions, creates interaction factories, and composes the concrete game view. |
| [`GodotCombatView_World_Units.cs`](EternalWar-Godot/Scripts/Godot/Run/Combat/View/GodotCombatView_World_Units.cs) | Creates unit visuals, positions and orients them in the world, and plays animations using Godot animation resources. |
| [`GodotCombatView.cs`](EternalWar-Godot/Scripts/Godot/Run/Combat/View/GodotCombatView.cs) | Implements the Godot-side combat view and connects the engine's combat UI and world views. |

The application layer defines *what* should happen; this layer defines *how* it is represented in Godot.

### Server and networking

| File | What to look for |
| --- | --- |
| [`ServerGameApplication.cs`](ExternalFiles/ServerSource/GameLogic/NetworkGameServer/ServerGameApplication.cs) | Loads the game database, creates the server session and controllers, starts the transport, and runs the server update loop. |
| [`HighLevelGameController.cs`](ExternalFiles/ServerSource/GameLogic/NetworkGameServer/Controllers/HighLevelGameController.cs) | Validates a requester's identity and checks whether a requested command is permitted before applying it. |
| [`INetworkClientBackend.cs`](Networking/Networking.Core/INetworkClientBackend.cs) / [`INetworkServerBackend.cs`](Networking/Networking.Core/INetworkServerBackend.cs) | Define the transport-independent client and server contracts. |
| [`LiteNetLibServerBackend.cs`](Networking/Networking.LiteNetLib/LiteNetLibServerBackend.cs) | Implements the server transport using LiteNetLib. |
| [`SteamNetworkingServerBackend.cs`](Networking/Networking.Steam/SteamNetworkingServerBackend.cs) | Contains the alternative Steam networking backend. |
| [`HighLevelGameControllerTests.cs`](Tests/NetworkGameServer.Tests/HighLevelGameControllerTests.cs) | Tests that the server uses the transport sender's identity when validating command requests. |
| [`LiteNetLibLoopbackTests.cs`](Tests/Networking.Tests/LiteNetLibLoopbackTests.cs) | Exercises client/server connection, message delivery, and disconnection over a local loopback connection. |

**Networking status:** `Networking.LiteNetLib` is the transport currently used by the game, which connects clients to the server by IP address and port. The `Networking.Steam` project contains a Facepunch Steamworks backend, but the game does not currently select it as its active transport. See [`Networking/README.md`](Networking/README.md) for the transport-specific notes and remaining integration work.

## Repository layout

```text
Eternal-War-Scripts-Showcase/
├── EternalWar-CSharp/              # Client application, controllers, models, and views
├── EternalWar-Godot/               # Godot-specific views, bindings, resources, and animation code
├── ExternalFiles/
│   ├── GameDataShared/              # Shared core data, game state, combat simulation, and AI
│   └── ServerSource/                 # Server application and network-game logic
├── Networking/
│   ├── Networking.Core/              # Transport interfaces and shared types
│   ├── Networking.Client/            # Transport-independent client wrapper
│   ├── Networking.Server/            # Transport-independent server wrapper
│   ├── Networking.LiteNetLib/        # LiteNetLib transport implementation
│   └── Networking.Steam/             # Steamworks transport implementation
├── EternalWar.Server.ConsoleApp/   # Dedicated-server console entry point
└── Tests/                            # NUnit tests for core, client, server, Godot utilities, and networking
```

## Technologies and project configuration

- **C# / .NET 8** for the shared core, client application, server application, console host, and test projects.
- **Godot .NET SDK 4.3.0** for the Godot integration project.
- **LiteNetLib** for the active UDP/IP networking transport.
- **Facepunch.Steamworks** for the separate Steam networking backend.
- **NUnit** for automated tests.

Shared build settings are defined in [`Directory.Build.props`](Directory.Build.props), and centrally managed NuGet versions are defined in [`Directory.Packages.props`](Directory.Packages.props).

## Tests

The test solution is [`Tests/EternalWar.Tests.sln`](Tests/EternalWar.Tests.sln). With the required .NET SDK and project dependencies available, run it from the repository root with:

```bash
dotnet test Tests/EternalWar.Tests.sln
```

The test projects cover core simulation and AI behavior, game-state and presentation sessions, server command handling, serialization, helper utilities, and networking backends.

Some session and server tests load the game's database from an external `Data` directory. That data directory is not included in this showcase, so those tests require the original game data to be available at the paths expected by the test fixtures.

## Notes

The source here is intended to make the design and implementation of selected systems easy to inspect. It is not a standalone distribution of Eternal War and does not include everything needed to build, launch, or play the game.
