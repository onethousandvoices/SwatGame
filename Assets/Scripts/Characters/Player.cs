using Controllers;
using SWAT.Behaviour;
using SWAT.Events;
using SWAT.Utility;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using Random = UnityEngine.Random;

namespace SWAT
{
    public class Player : BaseCharacter
    {
        [Config(Extras.Player, "A1")] private int _maxHealth;
        [Config(Extras.Player, "A2")] private int _maxArmour;
        [Config(Extras.Player, "A3")] private int _speed;

        [Config(Extras.PlayerWeapon, "A1")] private int _projectileDamage;
        [Config(Extras.PlayerWeapon, "A2")] private int _firingRate;
        [Config(Extras.PlayerWeapon, "A3")] private int _clipSize;
        [Config(Extras.PlayerWeapon, "A4")] private int _reloadTime;
        [Config(Extras.PlayerWeapon, "A5")] private int _totalAmmo;

        [SerializeField] private Animator _animator;
        [SerializeField] private RotationConstraint _rotationConstraint;
        [SerializeField] private HitPointsHolder _hitPointsHolder;

        private static readonly int _isSit = Animator.StringToHash("IsSit");
        
        private Crosshair _crosshair;
        
        protected override void OnEnabled()
        {
            base.OnEnabled();

            CurrentHealth = _maxHealth;
            CurrentArmour = _maxArmour;
            
            CurrentWeapon.Configure(
                _projectileDamage,
                _firingRate,
                _clipSize,
                _reloadTime,
                _totalAmmo);
            
            _crosshair = ObjectHolder.GetObject<Crosshair>();

            StateEngine.AddState(
                new FiringState(this),
                new PreIdleState(this),
                new PreReloadingState(this),
                new ReloadingState(this),
                new IdleState(this));

            StateEngine.SwitchState<IdleState>();
        }

        private HitPoint RandomHitPoint(int randomValue)
        {
            foreach (HitPoint hitPoint in _hitPointsHolder.HitPoints)
            {
                if (randomValue < hitPoint.Value) return hitPoint;
                randomValue -= hitPoint.Value;
            }
            return new HitPoint();
        }

        public async Task<HitPoint> GetTargetAsync()
        {
            int random = Random.Range(0, 100);
            return await Task.Run(() => RandomHitPoint(random));
        }

        private void GetUp()
        {
            _animator.SetBool(_isSit, false);
        }

        private void SitDown()
        {
            CurrentWeapon.SetFireState(false);
            _rotationConstraint.constraintActive = false;
            _rotationConstraint.weight = 0f;
            _animator.SetBool(_isSit, true);
        }

        public void UnityEvent_FirePoseAnimationEnd()
        {
            if (CurrentWeapon.FireState || StateEngine.CurrentState.GetType() == typeof(IdleState)) return;

            CurrentWeapon.SetFireState(true);
            _rotationConstraint.constraintActive = true;
        }

        protected override void Dead()
        {
            GameEvents.Call(new PlayerKilledEvent(this));
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
                _currentReloadingTime = _player.CurrentWeapon.ReloadTime;
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