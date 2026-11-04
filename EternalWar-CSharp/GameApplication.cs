// Original Authors - Wyatt Senalik
using EternalWar.Client.Application.HighLevelNetwork.Actions;
using EternalWar.Client.Application.Model;
using EternalWar.Client.Application.Model.Interaction;
using EternalWar.Client.Application.Network;
using EternalWar.Client.Application.Network.Messages;
using EternalWar.Client.Application.View;
using EternalWar.Core.Helpers;
using EternalWar.Core.Model;
using System;

namespace EternalWar.Client.Application
{
    public sealed class GameApplication : IDisposable
    {
        private readonly NetworkMessageReceiver _messageReceiver = new();
        private readonly GameDatabase _database;
        private readonly ClientGameSession _session;
        private readonly GameController _gameController;
        private readonly GameView _gameView;
        private readonly GameInteractionController _interactionController;

        private bool _wasNotDisconnected;
        private ChangeHandledResult? _previousChangeResult;
        private ChangeHandledResult? _previousInterationChangeResult;

        public GameApplication(string dataFolderRelativePath, IGameViewComposer viewComposer)
        {
            string dataFolderAbsolutePath = IOPathHelpers.GetAbsolutePath(dataFolderRelativePath);

            try
            {
                _database = DatabaseLoader.LoadDatabase(dataFolderAbsolutePath, sanityCheck: true);
            } catch (Exception e)
            {
                throw new Exception($"Failed to load the database:\n{e}");
            }
            _session = new ClientGameSession(_database);
            _gameController = new GameController(_session, dataFolderAbsolutePath);

            GameActions gameActions = _gameController.GameActions;
            _interactionController = new GameInteractionController(_session, gameActions);

            GameInteractionActions interactionActions = _interactionController.GameInteractionActions;
            GameViewActions gameViewActions = viewComposer.Compose(interactionActions);
            _gameView = new GameView(_database, gameViewActions);

            MessageHandlerRegistration.Setup(gameActions);
        }

        public void Start()
        {
            _session.TellCurrentConnectionState();
            _wasNotDisconnected = false;
        }

#pragma warning disable CA1822 // Mark members as static
        public void Stop()
#pragma warning restore CA1822 // Mark members as static
        {
            MessageHandlerRegistration.Cleanup();
        }

        public void Restart()
        {
            Stop();
            Start();
        }

        public void Update(float deltaTime)
        {
            NetworkHelpers.Poll();

            // Handle server disconnect
            bool isDisconnected = !NetworkHelpers.IsConnected && !NetworkHelpers.IsConnecting;
            if (_wasNotDisconnected && isDisconnected)
            {
                IHighLevelNetworkActions networkActions = _gameController.HighLevelNetworkActions;
                networkActions.SelfClientDisconnected();
            }
            _wasNotDisconnected = !isDisconnected;

            // Receive messages from server
            _messageReceiver.Update();

            // Try applying the next presentation change
            TryApplyingNextPresentationChange();

            // Try applying the next interaction presentation change
            TryApplyNextInteractionPresentationChange();

        }

        public void Dispose()
        {
            _gameController.Dispose();
        }

        private void TryApplyingNextPresentationChange()
        {
            // Waiting for the previous change to finish
            if (_previousChangeResult != null)
            {
                if (!_previousChangeResult.CanBeConsumed)
                    return;
                _previousChangeResult = null;
            }

            // This won't change since we handled the changes separately.
            InteractionState currentInteractionState = _session.InteractionSession.CurrentVisualizedState;

            while (_previousChangeResult == null || _previousChangeResult.CanBeConsumed)
            {
                if (_session.TryPeekClientGameStateChange(out ClientGameStateChange? nextClientGameStateChange))
                {
                    ViewHandleChangeContext clientChangeContext = new(
                        nextClientGameStateChange.CurrentClientGameState,
                        nextClientGameStateChange.AfterApplyClientGameState,
                        currentInteractionState);
                    _previousChangeResult = _gameView.HandleChange(nextClientGameStateChange, clientChangeContext);
                    _session.ConsumeClientGameStateChange();
                    continue;
                }

                if (!_session.TryPeekVisualizationEvent(out NetworkGameEvent? nextVisualizationEvent))
                    break;

                ClientGameState currentClientGameState = _session.GetCurrentPresentationClientGameState();

                // Consume the change right away so that it gets marked as visualized in case we receive another change while visualizing this one.
                _session.ConsumeVisualizationEvent();

                ClientGameState afterApplyClientGameState = _session.GetCurrentPresentationClientGameState();
                ViewHandleChangeContext handleChangeContext = new(currentClientGameState, afterApplyClientGameState, currentInteractionState);
                ClientGameStateChange_NetworkGameEvent clientGameStateChange = new(nextVisualizationEvent, currentClientGameState, afterApplyClientGameState);
                _previousChangeResult = _gameView.HandleChange(clientGameStateChange, handleChangeContext);
            }
        }

        private void TryApplyNextInteractionPresentationChange()
        {
            // Waiting for the previous change to finish
            if (_previousInterationChangeResult != null)
            {
                if (!_previousInterationChangeResult.CanBeConsumed)
                    return;
                _previousInterationChangeResult = null;
            }

            PresentationInteractionSession interactionSession = _session.InteractionSession;
            while (_previousInterationChangeResult == null || _previousInterationChangeResult.CanBeConsumed)
            {
                if (!interactionSession.TryPeekVisualizationChange(out InteractionStateChange? nextVisualizationChange))
                    break;

                InteractionState currentInteractionState = interactionSession.CurrentVisualizedState;
                InteractionState applyApplyState = nextVisualizationChange.Apply(currentInteractionState);

                ViewHandleInteractionChangeContext handleChangeContext = new(currentInteractionState, applyApplyState);
                _previousInterationChangeResult = _gameView.HandleInteractionChange(nextVisualizationChange, handleChangeContext);

                // Consume the change right away so that it gets marked as visualized in case we receive another change while visualizing this one.
                interactionSession.ConsumeVisualizationChange();
            }
        }
    }
}
