// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.View;
using EternalWar.Core.Helpers;
using EternalWar.Core.Model.Combat.Common;
using EternalWar.Core.Model.Combat.Definitions;
using EternalWar.Core.Model.Combat.Simulation.Events;
using System.Numerics;

namespace EternalWar.Client.Application.Run.Combat.Views.Actions
{
    public enum UnitTurnCellState { YetToTakeTurn, TakingTurnNow, TookTurn, Dead }

    public sealed record UnitOneShotAnimationRequest(UnitOneShotAnimationType AnimationType, ActionSequenceTimingContext ActionTimingContext);

    public sealed record UnitLoopAnimationRequest(UnitLoopAnimationType AnimationType, ActionSequenceTimingContext? ActionTimingContext, bool InterruptsCurrentOneShot = false);

    public interface ICombatViewActions
    {
        void Show();
        void Hide();

        void CreateUnit(CombatUnitUniqueIdentifier unitIdentifier, Vector3 position, EightDirection facingDirection);
        void SetUnitPosition(CombatUnitUniqueIdentifier unitIdentifier, Vector3 position);
        void SetUnitFacingDirection(CombatUnitUniqueIdentifier unitIdentifier, EightDirection facingDirection);
        IReadOnlyPresentationTask PlayUnitOneShotAnimation(CombatUnitUniqueIdentifier unitIdentifier, UnitLoopAnimationRequest activeLoopRequest, UnitOneShotAnimationRequest animationRequest);
        void SetUnitAnimationLoop(CombatUnitUniqueIdentifier unitIdentifier, UnitLoopAnimationRequest animationRequest);
        IReadOnlyPresentationTask SlideUnit(CombatUnitUniqueIdentifier unitIdentifier, Vector3 toPosition);
        void DeleteUnit(CombatUnitUniqueIdentifier unitIdentifier);
        void DeleteAllUnits();
        void CreateBlock(BlockInfo blockInfo, Vector3 position);
        void DeleteAllBlocks();

        void CreateActionButton(string combatActionSequenceId);
        void DeleteAllActionButtons();
        void ShowEndTurnButton();
        void HideEndTurnButton();
        void CreateUnitTurnCell(CombatUnitUniqueIdentifier unitIdentifier, TeamAffiliation unitTeamAffiliation, int unitCurrentHealth, int unitMaxHealth);
        void DeleteUnitTurnCell(CombatUnitUniqueIdentifier unitIdentifier);
        void DeleteAllUnitTurnCells();
        void SetUnitTurnCellState(CombatUnitUniqueIdentifier unitIdentifier, UnitTurnCellState state);
        void SetUnitHealthBar(CombatUnitUniqueIdentifier unitIdentifier, int unitCurrentHealth, int unitMaxHealth);

        void ShowDefeatScreen();
        void HideDefeatScreen();
        void ShowVictoryScreen(); // TODO: Give victory rewards as parameters
        void HideVictoryScreen();
    }
}
