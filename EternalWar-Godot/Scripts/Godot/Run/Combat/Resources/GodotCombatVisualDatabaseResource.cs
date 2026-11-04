// Original Authors - Wyatt Senalik
using Godot;
using System;
using System.Collections.Generic;

namespace EternalWar.Engine.Godot.Run.Combat.Resources
{
    [GlobalClass]
    public sealed partial class GodotCombatVisualDatabaseResource : Resource
    {
        public PackedScene CombatNodeCollectionPrefab => _combatNodeCollectionPrefab ?? throw new ArgumentNullException(nameof(_combatNodeCollectionPrefab));
        public PackedScene UnitTurnCellUIPrefab => _unitTurnCellUIPrefab ?? throw new ArgumentNullException(nameof(_unitTurnCellUIPrefab));
        public PackedScene CombatActionButtonPrefab => _combatActionButtonPrefab ?? throw new ArgumentNullException(nameof(_combatActionButtonPrefab));
        public IReadOnlyList<GodotBlockVisualResource> Blocks => _blocks;
        public IReadOnlyList<GodotUnitVisualResource> Units => _units;
        public IReadOnlyList<GodotArtifactVisualResource> Artifacts => _artifacts;

        [Export] private PackedScene? _combatNodeCollectionPrefab;
        [Export] private PackedScene? _unitTurnCellUIPrefab;
        [Export] private PackedScene? _combatActionButtonPrefab;
        [Export] private GodotBlockVisualResource[] _blocks = Array.Empty<GodotBlockVisualResource>();
        [Export] private GodotUnitVisualResource[] _units = Array.Empty<GodotUnitVisualResource>();
        [Export] private GodotArtifactVisualResource[] _artifacts = Array.Empty<GodotArtifactVisualResource>();
    }
}
