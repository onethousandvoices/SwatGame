using System;
using System.Collections.Generic;
using UnityEngine;

namespace SWAT.Behaviour
{
    public class StateEngine
    {
        public IState CurrentState { get; private set; }
        
        public void SwitchState(IState state)
        {
            CurrentState?.Exit();
            CurrentState = state;
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