// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Run.Combat.Views.Actions;
using EternalWar.Engine.Godot.Run.Combat.Nodes;
using EternalWar.Engine.Godot.Run.Combat.Resources;
using EternalWar.Engine.Godot.View.Resources.Combat.Prefabs;
using EVLibrary.Godot;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using Godot;
using System;
using System.Collections.Generic;

namespace EternalWar.Engine.Godot.Run.Combat.View
{
    public sealed class GodotCombatView_UI_TurnOrder(GodotGameVisualDatabase visualDatabase, GodotTurnOrderUINodeCollection godotTurnOrderUINodeCollectionInstance)
    {
        private const string GROW_ON_SELECT = "GrowOnSelect"; // TODO: Temporary strings probably use an enum or change the animation names to match what is already used.
        private const string INTRO_SLIDE = "IntroSlide";
        private const string RESET = "RESET";
        private const string DEAD = "RevivableDead";

        private readonly Dictionary<CombatUnitUniqueIdentifier, GodotTurnOrderUnitUINodeCollection> _cellsMap = new();
        private readonly GodotGameVisualDatabase _visualDatabase = visualDatabase;
        private readonly GodotTurnOrderUINodeCollection _godotTurnOrderUINodeCollectionInstance = godotTurnOrderUINodeCollectionInstance;

        public void CreateUnitTurnCell(CombatUnitUniqueIdentifier unitIdentifier, TeamAffiliation unitTeamAffiliation, int unitCurrentHealthPoints, int unitMaxHealthPoints)
        {
            string unitDefinitionId = unitIdentifier.DefinitionId;
            Node unitCellParentNode = _godotTurnOrderUINodeCollectionInstance.UnitCellParentNode;
            PackedScene unitTurnCellUIPrefab = _visualDatabase.UnitTurnCellUIPrefab;

            GodotTurnOrderUnitUINodeCollection unitTurnCellVisualNode = unitTurnCellUIPrefab.Instantiate<GodotTurnOrderUnitUINodeCollection>();
            unitCellParentNode.AddChild(unitTurnCellVisualNode);

            // Image
            GodotUnitVisualResource unitVisualDefinition = _visualDatabase.GetUnit(unitDefinitionId);
            Texture2D unitIconTexture = unitVisualDefinition.Icon;
            TextureRect unitTurnCellVisualNodeImage = unitTurnCellVisualNode.Image;
            unitTurnCellVisualNodeImage.Texture = unitIconTexture;

            // Outline
            TextureRect outline = unitTurnCellVisualNode.Outline;
            outline.SelfModulate = DetermineUnitCellOutlineColor(unitTeamAffiliation);

            // Health bar
            ProgressBar healthBar = unitTurnCellVisualNode.HealthBar;
            healthBar.Value = unitCurrentHealthPoints;
            healthBar.MaxValue = unitMaxHealthPoints;

            // Anime
            AnimationPlayer animationPlayer = unitTurnCellVisualNode.AnimationPlayer;
            animationPlayer.Queue(INTRO_SLIDE);

            _cellsMap.Add(unitIdentifier, unitTurnCellVisualNode);
        }

        public void DeleteUnitTurnCell(CombatUnitUniqueIdentifier unitIdentifier)
        {
            if (!_cellsMap.TryGetValue(unitIdentifier, out GodotTurnOrderUnitUINodeCollection? visualNode))
            {
                GodotPrintHelper.PrintRich($"{nameof(unitIdentifier)} not found in {nameof(_cellsMap)}.", GodotPrintHelper.COLOR_RED);
                return;
            }
            visualNode.QueueFree();
            _cellsMap.Remove(unitIdentifier);
        }

        public void DeleteAllUnitTurnCells()
        {
            foreach (GodotTurnOrderUnitUINodeCollection visualNode in _cellsMap.Values)
            {
                visualNode.QueueFree();
            }
            _cellsMap.Clear();
        }

        public void SetUnitTurnCellState(CombatUnitUniqueIdentifier unitIdentifier, UnitTurnCellState state)
        {
            if (!_cellsMap.TryGetValue(unitIdentifier, out GodotTurnOrderUnitUINodeCollection? visualNode))
                return;

            AnimationPlayer animationPlayer = visualNode.AnimationPlayer;
            string animationName = DetermineAnimationName(state);
            Animation animation = animationPlayer.GetAnimation(animationName);
            if (animation == null)
                return;
            if (animationPlayer.CurrentAnimation == animationName)
                return;
            animationPlayer.Play(animationName);
        }

        public void SetUnitHealthBar(CombatUnitUniqueIdentifier unitIdentifier, int unitCurrentHealth, int unitMaxHealth)
        {
            if (!_cellsMap.TryGetValue(unitIdentifier, out GodotTurnOrderUnitUINodeCollection? visualNode))
                return;

            ProgressBar healthBar = visualNode.HealthBar;

            healthBar.Value = unitCurrentHealth;
            healthBar.MaxValue = unitMaxHealth;
        }

        private static Color DetermineUnitCellOutlineColor(TeamAffiliation teamAffiliation)
        {
            return teamAffiliation switch
            {
                TeamAffiliation.Enemy => Colors.Red,
                TeamAffiliation.Neutral => Colors.Gray,
                TeamAffiliation.Ally => Colors.Aqua,
                _ => throw new NotImplementedException($"Unhandled {nameof(TeamAffiliation)}: ({teamAffiliation})")
            };
        }

        private static string DetermineAnimationName(UnitTurnCellState state)
        {
            return state switch
            {
                UnitTurnCellState.YetToTakeTurn => RESET,
                UnitTurnCellState.TakingTurnNow => GROW_ON_SELECT,
                UnitTurnCellState.Dead => DEAD,
                _ => throw new NotImplementedException($"Unhandled {nameof(state)}: ({state})")
            };
        }
    }
}
