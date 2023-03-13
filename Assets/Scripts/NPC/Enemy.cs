using SWAT.Behaviour;
using SWAT.Weapons;
using System;
using UnityEngine;

namespace SWAT
{
    public class Enemy : NPC
    {
        [SerializeField] private Weapon _weapon;
        
        protected override void OnEnabled()
        {
        }

        protected override void Run()
        {
        }

#region States
        private class FiringState : IState
        {
            public void Enter()
            {
            }

            public void Run()
            {
            }

            public void Exit()
            {
            }
        }
#endregion
    }
}