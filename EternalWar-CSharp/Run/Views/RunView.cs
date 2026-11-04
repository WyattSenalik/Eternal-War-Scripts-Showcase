// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Model;
using EternalWar.Client.Application.Model.Interaction;
using EternalWar.Client.Application.Run.Combat.Views;
using EternalWar.Client.Application.Run.Combat.Views.Actions;
using EternalWar.Client.Application.Run.Map.Views;
using EternalWar.Client.Application.Run.Map.Views.Actions;
using EternalWar.Client.Application.View;
using EternalWar.Core.Model;
using EternalWar.Core.Model.Combat.Runtime;
using EternalWar.Core.Model.Combat.Simulation.Events;
using EternalWar.Core.Model.Run.Runtime;
using EternalWar.Core.Network;
using System;

namespace EternalWar.Client.Application.Run.Views
{
    // TODO: Add more to this class when we have more in a run than just combat
    internal sealed class RunView(GameDatabase database, ICombatViewActions combatViewActions, IMapViewActions mapViewActions)
    {
        private readonly CombatView _combatView = new(database, combatViewActions);
        private readonly MapView _mapView = new(mapViewActions);

        public void Deactivate()
        {
            _combatView.Deactivate();
        }

        public ChangeHandledResult HandleEvent(NetworkGameEvent gameEvent, ViewHandleChangeContext context)
        {
            return gameEvent switch
            {
                NetworkGameEvent_Run_CombatStarted => HandleChange_StartCombat(context),
                CombatEvent or NetworkGameEvent_Run_Combat => _combatView.HandleEvent(gameEvent, context),
                NetworkGameEvent_Run_Map mapEvent => _mapView.HandleEvent(mapEvent, context),
                _ => ChangeHandledResult_Finished.Instance
            };
        }

        public ChangeHandledResult HandleStateSnapshot(NetworkGameState networkGameState, ViewHandleChangeContext context)
        {
            _combatView.Deactivate();
            if (!networkGameState.TryGetCombatRuntime(out CombatRuntime? combatRuntime))
                return ChangeHandledResult_Finished.Instance;

            ClientInfo myClientInfo = context.AfterApplyClientGameState.GetMyClientInfo();
            _combatView.Activate(myClientInfo, combatRuntime);
            return ChangeHandledResult_Finished.Instance;
        }

#pragma warning disable CA1822 // Mark members as static
        public ChangeHandledResult HandleInteractionChange(InteractionStateChange change, ViewHandleInteractionChangeContext context)
#pragma warning restore CA1822 // Mark members as static
        {
            return change switch
            {
                // TODO: Add some as needed
                _ => ChangeHandledResult_Finished.Instance
            };
        }

        private ChangeHandledResult_Finished HandleChange_StartCombat(ViewHandleChangeContext context)
        {
            ClientGameState currentClientGameState = context.CurrentClientGameState;
            RunPhase currentRunPhase = currentClientGameState.GetRunPhase();
            Type currentRunPhaseType = currentRunPhase.GetType();

            NetworkGameState nextNetworkGameState = context.AfterApplyNetworkGameState;
            RunPhase nextRunPhase = nextNetworkGameState.GetRunPhase();
            Type nextRunPhaseType = nextRunPhase.GetType();

            // Activate/Deactivate
            if (currentRunPhaseType != nextRunPhaseType)
            {
                switch (nextRunPhase)
                {
                    case TEMP_NOTHING_RUN_PHASE:
                        _combatView.Deactivate();
                        break;

                    case CombatRuntime combatRuntime:
                        ClientInfo myClientInfo = currentClientGameState.GetMyClientInfo();
                        _combatView.Activate(myClientInfo, combatRuntime);
                        break;

                    default:
                        throw new NotImplementedException($"Unhandled {nameof(RunPhase)}: ({nextRunPhaseType})");
                }
            }

            return ChangeHandledResult_Finished.Instance;
        }
    }
}
