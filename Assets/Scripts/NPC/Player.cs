using Controllers;
using SWAT.Behaviour;
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

        private Animator _animator;

        private static readonly int _isSit = Animator.StringToHash("IsSit");

        protected override void OnEnabled()
        {
            base.OnEnabled();

            _animator = Get<Animator>();

            StateEngine.AddState(
                new FiringState(this),
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
            _animator.SetBool(_isSit, true);
        }

        public void UnityEvent_FirePoseAnimationEnd()
        {
            if (CurrentWeapon.FireState == false)
            {
                CurrentWeapon.SetFireState(true);
                _rotationConstraint.constraintActive = true;
            }
            else
            {
                CurrentWeapon.SetFireState(false);
                _rotationConstraint.constraintActive = false;
            }
        }

#region States
        private class FiringState : IState
        {
            private readonly Player _player;

            public FiringState(Player player) => _player = player;

            public void Enter()
            {
                _player.GetUp();
                _player.IsVulnerable = true;
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
                    _player.StateEngine.SwitchState<IdleState>();
                }

                if (_player.CurrentWeapon.ClipIsEmpty)
                    _player.StateEngine.SwitchState<ReloadingState>();
            }

            public void Exit()
            {
                _player.IsVulnerable = false;
            }
        }

        private class ReloadingState : IState
        {
            private readonly Player _player;

            private float _currentReloadingTime;

            public ReloadingState(Player player) => _player = player;

            public void Enter()
            {
                _player.SitDown();
                _currentReloadingTime = _player.CurrentWeapon.ReloadTime;
            }

            public void Run()
            {
                _currentReloadingTime -= Time.deltaTime;

                if (_currentReloadingTime <= 0)
                    _player.StateEngine.SwitchState<IdleState>();
            }

            public void Exit()
            {
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