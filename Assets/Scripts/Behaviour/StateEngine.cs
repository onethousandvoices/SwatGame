using System;
using System.Collections.Generic;

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
            CurrentState = _states[typeof(T)];
            CurrentState.Enter();
        }
    }
}