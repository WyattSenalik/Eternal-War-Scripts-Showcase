// Original Authors - Wyatt Senalik
using EternalWar.Engine.Godot.Lobby.Nodes;
using EternalWar.Engine.Godot.Run.Combat.Nodes;
using EternalWar.Engine.Godot.Run.Map.Nodes;
using Godot;
using System;

namespace EternalWar.Engine.Godot
{
    public sealed partial class GodotGameNodeCollection : Node
    {
        public GodotLobbyNodeCollection LobbyNodeCollection => _lobbyNodeCollection ?? throw new ArgumentNullException(nameof(_lobbyNodeCollection));
        public GodotCombatNodeCollection CombatNodeCollection => _combatNodeCollection ?? throw new ArgumentNullException(nameof(_combatNodeCollection));
        public GodotMapNodeCollection MapNodeCollection => _mapNodeCollection ?? throw new ArgumentNullException(nameof(_mapNodeCollection));

        [Export] private GodotLobbyNodeCollection? _lobbyNodeCollection;
        [Export] private GodotCombatNodeCollection? _combatNodeCollection;
        [Export] private GodotMapNodeCollection? _mapNodeCollection;
    }
}
