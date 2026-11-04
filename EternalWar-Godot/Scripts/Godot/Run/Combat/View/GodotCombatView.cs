// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Run.Combat.Views.Actions;
using EternalWar.Client.Application.View;
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Engine.Godot.Run.Combat.Nodes;
using GodotExtensions;
using NumericsVector3 = System.Numerics.Vector3;

namespace EternalWar.Engine.Godot.Run.Combat.View
{
    public sealed class GodotCombatView(GodotGameVisualDatabase visualDatabase, GodotEngineRandomResolver engineRandomResolver, GodotCombatNodeCollection combatNodeCollectionInstance, ButtonFactory<string> combatActionButtonsFactory) : ICombatViewActions
    {
        private readonly GodotCombatNodeCollection _combatNodeCollectionInstance = combatNodeCollectionInstance;
        private readonly GodotCombatView_World _worldView = new(visualDatabase, combatNodeCollectionInstance, engineRandomResolver);
        private readonly GodotCombatView_UI _uiView = new(visualDatabase, combatNodeCollectionInstance.UINodeCollection, combatActionButtonsFactory);

        public void Show()
        {
            _combatNodeCollectionInstance.CombatViewport.Show();
            _combatNodeCollectionInstance.LightsParentNode.Show();
        }
        public void Hide()
        {
            _combatNodeCollectionInstance.CombatViewport.Hide();
            _combatNodeCollectionInstance.LightsParentNode.Hide();
        }

        public void CreateUnit(CombatUnitUniqueIdentifier unitIdentifier, NumericsVector3 position, EightDirection facingDirection) => _worldView.UnitsView.CreateUnit(unitIdentifier, position, facingDirection);
        public void SetUnitPosition(CombatUnitUniqueIdentifier unitIdentifier, NumericsVector3 position) => _worldView.UnitsView.SetUnitPosition(unitIdentifier, position);
        public void SetUnitFacingDirection(CombatUnitUniqueIdentifier unitIdentifier, EightDirection facingDirection) => _worldView.UnitsView.SetUnitFacingDirection(unitIdentifier, facingDirection);
        public IReadOnlyPresentationTask PlayUnitOneShotAnimation(CombatUnitUniqueIdentifier unitIdentifier, UnitLoopAnimationRequest activeLoopRequest, UnitOneShotAnimationRequest animationRequest) => _worldView.UnitsView.PlayUnitOneShotAnimation(unitIdentifier, activeLoopRequest, animationRequest);
        public void SetUnitAnimationLoop(CombatUnitUniqueIdentifier unitIdentifier, UnitLoopAnimationRequest animationRequest) => _worldView.UnitsView.SetUnitAnimationLoop(unitIdentifier, animationRequest);
        public IReadOnlyPresentationTask SlideUnit(CombatUnitUniqueIdentifier unitIdentifier, NumericsVector3 toPosition) => _worldView.UnitsView.SlideUnit(unitIdentifier, toPosition);
        public void DeleteUnit(CombatUnitUniqueIdentifier unitIdentifier) => _worldView.UnitsView.DeleteUnit(unitIdentifier);
        public void DeleteAllUnits() => _worldView.UnitsView.DeleteAllUnits();

        public void CreateBlock(BlockInfo blockInfo, NumericsVector3 position) => _worldView.BlocksView.CreateBlock(blockInfo, position);
        public void DeleteAllBlocks() => _worldView.BlocksView.DeleteAllBlocks();

        public void CreateActionButton(string combatActionSequenceId) => _uiView.CombatActionsView.CreateActionButton(combatActionSequenceId);
        public void DeleteAllActionButtons() => _uiView.CombatActionsView.DeleteAllActionButtons();
        public void ShowEndTurnButton() => _uiView.CombatActionsView.ShowEndTurnButton();
        public void HideEndTurnButton() => _uiView.CombatActionsView.HideEndTurnButton();

        public void CreateUnitTurnCell(CombatUnitUniqueIdentifier unitIdentifier, TeamAffiliation unitTeamAffiliation, int unitCurrentHealth, int unitMaxHealth) => _uiView.TurnOrderView.CreateUnitTurnCell(unitIdentifier, unitTeamAffiliation, unitCurrentHealth, unitMaxHealth);
        public void DeleteUnitTurnCell(CombatUnitUniqueIdentifier unitIdentifier) => _uiView.TurnOrderView.DeleteUnitTurnCell(unitIdentifier);
        public void DeleteAllUnitTurnCells() => _uiView.TurnOrderView.DeleteAllUnitTurnCells();
        public void SetUnitTurnCellState(CombatUnitUniqueIdentifier unitIdentifier, UnitTurnCellState state) => _uiView.TurnOrderView.SetUnitTurnCellState(unitIdentifier, state);
        public void SetUnitHealthBar(CombatUnitUniqueIdentifier unitIdentifier, int unitCurrentHealth, int unitMaxHealth) => _uiView.TurnOrderView.SetUnitHealthBar(unitIdentifier, unitCurrentHealth, unitMaxHealth);

        public void ShowDefeatScreen() => _uiView.EndView.ShowDefeatScreen();
        public void HideDefeatScreen() => _uiView.EndView.HideDefeatScreen();
        public void ShowVictoryScreen() => _uiView.EndView.ShowVictoryScreen(); // TODO: Give victory rewards as parameters
        public void HideVictoryScreen() => _uiView.EndView.HideVictoryScreen();
    }
}
