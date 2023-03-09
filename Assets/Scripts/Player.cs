using Controllers;
using NTC.Global.Cache;
using SWAT.Behaviour;
using SWAT.Weapons;
using UnityEngine;

namespace SWAT
{
    public class Player : MonoCache
    {
        public bool IsVulnerable { get; private set; }

        [Config(Extras.Player, "A1")] private int _maxHealth;
        [Config(Extras.Player, "A2")] private int _maxArmour;
        [Config(Extras.Player, "A3")] private int _speed;

        private StateEngine _stateEngine;
        private Weapon      _currentWeapon;
        private Crosshair   _crosshair;

        private int _currentHealth;
        private int _currentArmour;

        protected override void OnEnabled()
        {
            _crosshair = ObjectHolder.GetObject<Crosshair>();

            _currentWeapon = ChildrenGet<Weapon>();

            _stateEngine = new StateEngine();
            _stateEngine.AddState(
                new FiringState(this),
                new ReloadingState(this),
                new IdleState(this));

            _stateEngine.SwitchState<IdleState>();
        }

        protected override void Run()
        {
            _stateEngine.CurrentState?.Run();

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
                    _player._currentWeapon.Fire();
                }

                if (Input.GetMouseButtonUp(0))
                {
                    _player._currentWeapon.ResetFiringRate();
                }

                if (_player._currentWeapon.ClipIsEmpty) Exit();
            }

            public void Exit()
            {
                _player.IsVulnerable = false;
                _player._stateEngine.SwitchState<ReloadingState>();
            }
        }

        private class ReloadingState : IState
        {
            private readonly Player _player;

            private float _currentReloadingTime;

            public ReloadingState(Player player) => _player = player;

            public void Enter()
            {
                _currentReloadingTime = _player._currentWeapon.ReloadTime;
            }

            public void Run()
            {
                _currentReloadingTime -= Time.deltaTime;

                if (_currentReloadingTime <= 0) Exit();
            }

            public void Exit()
            {
                _player._currentWeapon.Reload();
                _player._stateEngine.SwitchState<IdleState>();
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
                    Exit();
                }
            }

            public void Exit()
            {
                _player._stateEngine.SwitchState<FiringState>();
            }
        }
#endregion
    }
}