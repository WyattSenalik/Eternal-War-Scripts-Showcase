// Original Authors - Wyatt Senalik
using System;
using System.Collections.Generic;
using System.Linq;

namespace EternalWar.Core.Helpers.StateMachine
{
    public abstract class StateMachine<T> where T : IState
    {
        public T? PreviousState { get; private set; }
        public T? CurrentState { get; private set; }

        private readonly Dictionary<string, T> _states = new();
        private readonly List<StateConnection> _connections = new();

        public void AddState(string stateName, T state)
        {
            if (_states.ContainsKey(stateName))
                return;
            _states.Add(stateName, state);
        }

        public void AddConnection(T fromState, T toState, StateConnectionType connectionType)
        {
            _connections.Add(new StateConnection(fromState, toState));
            switch (connectionType)
            {
                case StateConnectionType.OneWay: break;
                case StateConnectionType.IncludeReverse:
                    _connections.Add(new StateConnection(toState, fromState));
                    break;
                default:
                    throw new NotImplementedException($"Unhandled {nameof(StateConnectionType)}: ({connectionType})");
            }
        }

        public bool TryChangeState(string toStateKey, object? arguments = null)
        {
            return _states.TryGetValue(toStateKey, out T? toState) && TryChangeState(toState, arguments);
        }

        public bool TryChangeState(T toState, object? arguments = null)
        {
            if (toState == null)
                return false;
            if (CompareStates(toState, CurrentState))
                return false;
            if (!DoesConnectionExist(toState))
                return false;
            SetState(toState, arguments);
            return true;
        }

        public void ChangeState(string toStateKey, object? arguments = null)
        {
            if (TryChangeState(toStateKey, arguments))
                return;
            Debug.PrintError($"Failed to change to state with key ({toStateKey}) from state of type ({CurrentState?.GetType().Name})");
        }

        public void ChangeState(T toState, object? arguments = null)
        {
            if (TryChangeState(toState, arguments))
                return;
            Debug.PrintError($"Failed to change to state of type ({toState.GetType().Name}) from state of type ({CurrentState?.GetType().Name})");
        }

        public IState? GetState(string key)
        {
            return !_states.TryGetValue(key, out T? state) ? null : state;
        }

        private void SetState(T toState, object? argumnets)
        {
            CurrentState?.Exit();
            PreviousState = CurrentState;
            CurrentState = toState;
            CurrentState.Enter(argumnets);
        }

        private bool DoesConnectionExist(T toState)
        {
            return CurrentState == null || _connections.Any(connection => CompareStates(connection.FromState, CurrentState) && CompareStates(connection.ToState, toState));
        }

        private static bool CompareStates(IState? a, IState? b)
        {
            return a == null ? b == null :
                b != null && a.Equals(b);
        }
    }
}