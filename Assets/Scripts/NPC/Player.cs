using Controllers;
using SWAT.Behaviour;
using SWAT.Utility;
using System;
using UnityEngine;
using UnityEngine.Animations;

namespace SWAT
{
    public class Player : NPC
    {
        public bool IsVulnerable { get; private set; }

        [Config(Extras.Player, "A1")] private int _maxHealth;
        [Config(Extras.Player, "A2")] private int _maxArmour;
        [Config(Extras.Player, "A3")] private int _speed;

        [SerializeField] private RotationConstraint _rotationConstraint;

        private Animator  _animator;
        private Crosshair _crosshair;

        private static readonly int _isSit = Animator.StringToHash("IsSit");

        protected override void OnEnabled()
        {
            base.OnEnabled();

            _animator = Get<Animator>();

            _crosshair = ObjectHolder.GetObject<Crosshair>();

            StateEngine.AddState(
                new FiringState(this),
                new PreIdleState(this),
                new PreReloadingState(this),
                new ReloadingState(this),
                new IdleState(this));

            StateEngine.SwitchState<IdleState>();
        }

        private void GetUp()
        {
            _animator.SetBool(_isSit, false);
        }

        private void SitDown()
        {
            CurrentWeapon.SetFireState(false);
            _rotationConstraint.constraintActive = false;
            _rotationConstraint.weight           = 0f;
            _animator.SetBool(_isSit, true);
        }

        public void UnityEvent_FirePoseAnimationEnd()
        {
            if (CurrentWeapon.FireState || StateEngine.CurrentState.GetType() == typeof(IdleState)) return;

            CurrentWeapon.SetFireState(true);
            _rotationConstraint.constraintActive = true;
        }

#region States
        private class FiringState : IState
        {
            private readonly Player _player;

            public FiringState(Player player) => _player = player;

            public void Enter()
            {
                _player.GetUp();
                _player.IsVulnerable               = true;
                _player._rotationConstraint.weight = 1f;
            }

            public void Run()
            {
                if (Input.GetMouseButton(0))
                {
                    _player.CurrentWeapon.Fire();
                }

                if (Input.GetMouseButtonUp(0))
                {
                    _player.CurrentWeapon.ResetFiringRate();
                    _player.StateEngine.SwitchState<PreIdleState>();
                }

                if (_player.CurrentWeapon.ClipIsEmpty)
                    _player.StateEngine.SwitchState<PreReloadingState>();
            }

            public void Exit()
            {
                _player.IsVulnerable = false;
            }
        }

        private class PreReloadingState : IState
        {
            private readonly Player _player;

            public PreReloadingState(Player player) => _player = player;

            public void Enter()
            {
                _player.CurrentWeapon.SetFireState(false);
            }

            public void Run()
            {
                _player._rotationConstraint.weight -= Time.deltaTime * 2f;

                if (_player._rotationConstraint.weight > 0.3f) return;

                _player.StateEngine.SwitchState<ReloadingState>();
            }

            public void Exit()
            {
                _player.SitDown();
            }
        }

        private class PreIdleState : IState
        {
            private readonly Player _player;

            public PreIdleState(Player player) => _player = player;

            public void Enter()
            {
                _player.CurrentWeapon.SetFireState(false);
            }

            public void Run()
            {
                _player._rotationConstraint.weight -= Time.deltaTime * 2f;

                if (_player._rotationConstraint.weight > 0.3f) return;

                _player.StateEngine.SwitchState<IdleState>();
            }

            public void Exit() { }
        }

        private class ReloadingState : IState
        {
            private readonly Player _player;

            private float _reloadingTimeNormalized;
            private float _currentReloadingTime;

            public ReloadingState(Player player) => _player = player;

            public void Enter()
            {
                _player._crosshair.EnableBar();
                _currentReloadingTime    = _player.CurrentWeapon.ReloadTime;
                _reloadingTimeNormalized = 0f;
            }

            public void Run()
            {
                _currentReloadingTime -= Time.deltaTime;

                _reloadingTimeNormalized += Time.deltaTime;

                _player._crosshair.SetProgression(_reloadingTimeNormalized / _player.CurrentWeapon.ReloadTime);

                if (_currentReloadingTime <= 0)
                    _player.StateEngine.SwitchState<IdleState>();
            }

            public void Exit()
            {
                _player._crosshair.DisableBar();
                _player.CurrentWeapon.Reload();
            }
        }

        private class IdleState : IState
        {
            private readonly Player _player;

            public IdleState(Player player) => _player = player;

            public void Enter()
            {
                _player.SitDown();
            }

            public void Run()
            {
                if (Input.GetMouseButtonDown(0))
                {
                    _player.StateEngine.SwitchState<FiringState>();
                }
            }

            public void Exit() { }
        }
#endregion
    }
}