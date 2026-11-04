// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.Lobby.Views;
using EternalWar.Client.Application.Lobby.Views.Actions;
using EternalWar.Client.Application.Model;
using EternalWar.Client.Application.Model.Interaction;
using EternalWar.Client.Application.Run.Combat.Views.Actions;
using EternalWar.Client.Application.Run.Map.Views.Actions;
using EternalWar.Client.Application.Run.Views;
using EternalWar.Core.Model;
using EternalWar.Core.Model.Lobby.Runtime;
using EternalWar.Core.Model.Run.Runtime;
using System;

namespace EternalWar.Client.Application.View
{
    internal sealed record ViewHandleChangeContext(ClientGameState CurrentClientGameState, ClientGameState AfterApplyClientGameState, InteractionState InteractionState)
    {
        public NetworkGameState AfterApplyNetworkGameState => AfterApplyClientGameState.NetworkGameState;
    }

    internal sealed record ViewHandleInteractionChangeContext(InteractionState CurrentInteractionState, InteractionState AfterApplyInteractionState);

    internal sealed class GameView
    {
        private readonly LobbyView _lobbyView;
        private readonly RunView _runView;
        private Type? _previousActivePhaseType;

        public GameView(GameDatabase database, GameViewActions gameViewActions)
        {
            ILobbyViewActions lobbyViewActions = gameViewActions.LobbyViewActions;
            ICombatViewActions combatViewActions = gameViewActions.CombatViewActions;
            IMapViewActions mapViewActions = gameViewActions.MapViewActions;

            _lobbyView = new LobbyView(lobbyViewActions);
            _runView = new RunView(database, combatViewActions, mapViewActions);
        }

        public ChangeHandledResult HandleChange(ClientGameStateChange change, ViewHandleChangeContext context)
        {
            HandlePhaseChange(context.AfterApplyClientGameState.GetGamePhase());

            ChangeHandledResult lobbyResult = _lobbyView.HandleChange(change, context);
            ChangeHandledResult runResult = change switch
            {
                ClientGameStateChange_NetworkGameEvent networkEvent => _runView.HandleEvent(networkEvent.Event, context),
                ClientGameStateChange_ConnectionConnected or ClientGameStateChange_NetworkGameStateReset => _runView.HandleStateSnapshot(context.AfterApplyNetworkGameState, context),
                ClientGameStateChange_ConnectionConnecting or ClientGameStateChange_ConnectionDisconnected => ChangeHandledResult_Finished.Instance,
                _ => throw new NotImplementedException($"Unhandled {nameof(ClientGameStateChange)}: ({change?.GetType()})")
            };
            return lobbyResult.Combine(runResult);
        }

        public ChangeHandledResult HandleInteractionChange(InteractionStateChange change, ViewHandleInteractionChangeContext context)
        {
            ChangeHandledResult lobbyResult = _lobbyView.HandleInteractionChange(change, context);
            ChangeHandledResult runResult = _runView.HandleInteractionChange(change, context);
            return lobbyResult.Combine(runResult);
        }

        private void HandlePhaseChange(GamePhase phase)
        {
            Type phaseType = phase.GetType();

            if (_previousActivePhaseType != phaseType)
            {
                switch (phase)
                {
                    case LobbyRuntime lobbyRuntime:
                        _runView.Deactivate();
                        _lobbyView.Activate(lobbyRuntime);
                        break;

                    case RunRuntime runRuntime:
                        _lobbyView.Deactivate();
                        break;

                    default:
                        throw new NotImplementedException($"Unhandled {nameof(GamePhase)}: {phase.GetType()}");
                }
            }
            _previousActivePhaseType = phaseType;
        }
    }
}
