This is a repo used to show my script contributions to the game Eternal War. Eternal War is a multiplayer rougelike where you advance through tactical combats, growing stronger as the combat scenarios get more difficult.

The architecture of the game is built around a separation between the engine (concrete views and interactions) and the game simulation (engine-agnostic). This both allows an easy separation for the server to simulate the game without needing engine logic and for the enemy AI to simulate real results of its turns instead of using a separate simulation / choice evaluation engine.
Roughly, this is how things are broken up:
Engine Game: Simply sets up the Game Application and engine structure.
├── Engine Game View Composer (Implements Game View Composer): Concretely creating the views that implement Game View Actions. Also responsible for binding ui inputs to Interaction Actions.
│ ├── Engine UI Binders (Invokes Interaction Actions): Binds the non-dynamically created engine UI elements to their corresponding Interaction Actions.
│ ├── Engine Interaction Factories (Invokes Interaction Actions): Creates dynamic UI elements and binds them as they are created.
│ └── Engine Game Views (Implements Game View Actions; Invokes Engine Interaction Factories): Concrete engine views that implement Game View Actions. The actual visuals.
└── Game Application: Simply sets up the engine-agnostic structure.
├── Network Message Handlers (Invokes Game Actions) [Exists only if the game is networked]: After a message is received from the server, it arrives here, where it is then passed on to the appropriate Game Action.
├── Game Model: State of the game.
├── Game Controller (Modifies Game Model; Implements Game Actions): The only place the model is allowed to be modified. This is where "Game Logic" begins.
├── Game Actions: Conceptually represents different things that have an effect on the game. Such as a unit ending its turn. These are interfaces implemented by controllers and act as the glue between the network messages/player-interaction and the game controllers.
├── Interaction Model: State of the interaction.
├── Interaction Controller (Modifies Interaction Model; Invokes Game Actions; Implements Interaction Actions): Concrete implementation of what to do when an Interaction Action is invoked. It is the only thing allowed to modify the interaction model. It also invokes Game Actions when relevant.a
├── Interaction Actions: Collection of "events" like a button press that describe what happens when the player interacts with UI elements of Input. Interfaces, no concrete implementations.
├── Game View (Reads Game Model and Interaction Model; Invokes Game View Actions): Tells the view what to look like based on the game state.
├── Game View Actions: Collection of "events" that could modify how the game looks. Interfaces, no concrete implementation.
└── Game View Composer (Creates Game View Actions): Interface about creating view actions. Passed in from the Engine Game.

EternalWar.Core contains the game's data structures, logic, and simulation that describes gameplay that will be shared across the network. Does not contain anything that is client or server specific.
EternalWar.Client.Application contains extra data structures and logic specific to the client and also contains GameApplication.cs which runs the client session's simulation.
EternalWar.Server.Application contains extra data structures and logic specific to the server and also contains ServerGameApplication.cs which runs the server session's simulation.
EternalWar.Engine.Godot is the godot-specific implementation that defines the concrete views and interactions (input from the player)

The various Networking libraries are a generic solution created for communicating across the server using either LiteNetLib (for development) or Steamworks (for release).
