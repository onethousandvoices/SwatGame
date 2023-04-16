using System;
using System.Collections.Generic;
using UnityEngine;

namespace SWAT.Behaviour
{
    public class StateEngine
    {
        public IState CurrentState { get; private set; }

        private readonly Dictionary<Type, IState> _states = new Dictionary<Type, IState>();

        public void AddState(params IState[] states)
        {
            for (int i = 0; i < states.Length; i++)
            {
                IState state = states[i];
                _states.Add(state.GetType(), state);
            }
        }

        public void SwitchState<T>()
        {
            CurrentState?.Exit();
            CurrentState = _states[typeof(T)];
            CurrentState.Enter();

            // Debug.LogError($"Current state is {CurrentState}");
        }

        public void Stop()
        {
            CurrentState?.Exit();
            CurrentState = null;
        }
    }
}