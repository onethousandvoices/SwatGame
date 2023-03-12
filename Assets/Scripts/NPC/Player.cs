using Controllers;
using SWAT.Behaviour;
using UnityEngine;

namespace SWAT
{
    public class Player : NPC
    {
        public bool IsVulnerable { get; private set; }

        [Config(Extras.Player, "A1")] private int _maxHealth;
        [Config(Extras.Player, "A2")] private int _maxArmour;
        [Config(Extras.Player, "A3")] private int _speed;

        private Animator  _animator;

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
        

#region States
        private class FiringState : IState
        {
            private readonly Player _player;

            public FiringState(Player player) => _player = player;

            public void Enter()
            {
                //get up
                _player.GetUp();
                _player.IsVulnerable = true;
            }

            public void Run()
            {
                // _player._spine.transform.eulerAngles = Vector3.Lerp(
                //     _player._spine.transform.eulerAngles, _player._crosshair.RayHit() - new Vector3(0f, 33f, 0f), Time.deltaTime * 20f);

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