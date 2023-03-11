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

        private Crosshair _crosshair;

        protected override void OnEnabled()
        {
            base.OnEnabled();

            _crosshair = ObjectHolder.GetObject<Crosshair>();

            StateEngine.AddState(
                new FiringState(this),
                new ReloadingState(this),
                new IdleState(this));

            StateEngine.SwitchState<IdleState>();
        }

        protected override void Run()
        {
            base.Run();

            Quaternion target = Quaternion.LookRotation(_crosshair.RayHit());
            transform.rotation = Quaternion.Lerp(transform.rotation, target, Time.deltaTime * 20f);
        }

#region States
        private class FiringState : IState
        {
            private readonly Player _player;

            public FiringState(Player player) => _player = player;

            public void Enter()
            {
                //get up
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

            public void Enter() { }

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