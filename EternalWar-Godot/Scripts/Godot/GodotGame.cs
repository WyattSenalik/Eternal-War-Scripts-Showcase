// Original Authors - Wyatt Senalik
using EternalWar.Client.Application;
using EternalWar.Engine.Godot.Lobby.Nodes;
using EternalWar.Engine.Godot.Run.Combat.Nodes;
using Godot;
using System;
using CustomDebug = EternalWar.Core.Helpers.Debug;

namespace EternalWar.Engine.Godot
{
    public sealed partial class GodotGame : Node
    {
        private const string DATA_FOLDER_RELATIVE_PATH = "../Data";

        [Export] private GodotGameNodeCollection? _gameNodeCollection;
        [Export] private InputManager? _inputManager;

        [Export] private GodotGameVisualDatabaseResource? _visualDatabaseResource;
        [Export] private ShaderMaterial? _sharedSolidMaterial; // TODO: Move to visual database

        private readonly GodotEngineRandomResolver _engineRandomResolver = new();
        private GameApplication? _gameApplication;
        private bool _isDisposed;
        private bool _isGameApplicationDisposed;

        public override void _Ready()
        {
            base._Ready();

            ArgumentNullException.ThrowIfNull(_gameNodeCollection, nameof(_gameNodeCollection));
            ArgumentNullException.ThrowIfNull(_inputManager, nameof(_inputManager));
            ArgumentNullException.ThrowIfNull(_visualDatabaseResource, nameof(_visualDatabaseResource));
            ArgumentNullException.ThrowIfNull(_sharedSolidMaterial, nameof(_sharedSolidMaterial));

            CustomDebug.Printed += OnPrinted;
            CustomDebug.PrintedError += OnPrintedError;

            _inputManager.Setup(); // TODO: Move to inside game?
            RenderUtil.Setup(_sharedSolidMaterial); // TODO: Probably want this to be part of the views or delete this entirely to get rid of shared state and put shared solid material in the database.

            // Load visual database
            GodotGameVisualDatabase visualDatabase = _visualDatabaseResource.CreateOptimizedDatabase();

            // References to nodes in tree
            GodotLobbyNodeCollection lobbyNodeCollection = _gameNodeCollection.LobbyNodeCollection;
            GodotCombatNodeCollection combatNodeCollection = _gameNodeCollection.CombatNodeCollection;
            GodotCombatUINodeCollection combatUINodeCollection = combatNodeCollection.UINodeCollection;
            GodotCombatActionsUINodeCollection combatActionsUINodeCollection = combatUINodeCollection.CombatActionsNodeCollection;

            // Create the view factory (the godot views are setup and linked here)
            GodotGameViewComposer viewComposer = new(_gameNodeCollection, visualDatabase, _engineRandomResolver);

            // Build the application
            try
            {
                _gameApplication = new GameApplication(DATA_FOLDER_RELATIVE_PATH, viewComposer);

                // Start the game
                _gameApplication.Start();
            }
            catch (Exception e)
            {
                GD.PrintErr($"Ran into an error trying to start the game application:\n{e}");
            }
        }

        public override void _ExitTree()
        {
            base._ExitTree();

            _gameApplication?.Stop();
            DisposeGameApplication();
            _inputManager?.Cleanup(); // TODO: Should be moved inside game?

            CustomDebug.Printed -= OnPrinted;
            CustomDebug.PrintedError -= OnPrintedError;
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
            ArgumentNullException.ThrowIfNull(_gameApplication, nameof(_gameApplication));

            _gameApplication.Update((float)delta);
        }

        private void DisposeGameApplication()
        {
            if (_isGameApplicationDisposed)
                return;
            _isGameApplicationDisposed = true;

            _gameApplication?.Dispose();
        }

        private static void OnPrinted(string msg) => GD.Print(msg);
        private static void OnPrintedError(string msg) => GD.PrintErr(msg);
    }
}
