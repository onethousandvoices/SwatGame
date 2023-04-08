using Controllers;
using SWAT.Behaviour;
using SWAT.Events;
using SWAT.LevelScripts;
using SWAT.LevelScripts.Navigation;
using SWAT.Utility;
using UnityEngine;
using UnityEngine.Animations;

namespace SWAT
{
    public class Player : BaseCharacter
    {
        [SerializeField] private Path _path;
        
        [Config(Extras.Player, "A1")] private int _maxHealth;
        [Config(Extras.Player, "A2")] private int _maxArmour;
        [Config(Extras.Player, "A3")] private int _speed;

        [Config(Extras.PlayerWeapon, "A1")] private int _projectileDamage;
        [Config(Extras.PlayerWeapon, "A2")] private int _firingRate;
        [Config(Extras.PlayerWeapon, "A3")] private int _clipSize;
        [Config(Extras.PlayerWeapon, "A4")] private int _reloadTime;
        [Config(Extras.PlayerWeapon, "A5")] private int _totalAmmo;

        [SerializeField] private Hud _playerHud;
        [SerializeField] private Animator _animator;
        [SerializeField] private RotationConstraint _rotationConstraint;
        [field: SerializeField] public HitPointsHolder HitPointsHolder { get; private set; }

        private static readonly int _isSit = Animator.StringToHash("IsSit");
        private static readonly int _runTrigger = Animator.StringToHash("Run");
        private static readonly int _firingTrigger = Animator.StringToHash("Firing");

        private Rigidbody _rb;
        private Crosshair _crosshair;

        public override CharacterType Type => CharacterType.Player;
        
        protected override int BaseMaxArmour => _maxArmour;
        protected override int BaseMaxHealth => _maxHealth;
        
        protected override void OnEnabled()
        {
            base.OnEnabled();
            
            SetHud(_playerHud);
            _playerHud.PlayerCarry();
            
            CurrentHealth = _maxHealth;
            CurrentArmour = _maxArmour;

            _rb = Get<Rigidbody>();
            
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
                new IdleState(this),
                new RunState(this));

            StateEngine.SwitchState<IdleState>();
            
            GameEvents.Register<StageEnemiesDeadEvent>(OnStageEnemiesDeath);
            GameEvents.Register<WeaponFireEvent>(OnWeaponFire);
        }

        private void OnWeaponFire(WeaponFireEvent @event)
        {
            if (@event.Carrier != this) return;
            _crosshair.SetCrosshairProgression(@event.ClipSizeNormalized);
        }

        private void OnStageEnemiesDeath(StageEnemiesDeadEvent obj)
        {
            StateEngine.SwitchState<RunState>();
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

        protected override void Dead(Vector3 position) => GameEvents.Call(new PlayerKilledEvent(this));

#region States
        private class RunState : IState
        {
            private readonly Player _player;
            private PathPoint _targetPathPoint;
            public RunState(Player player) => _player = player;

            public void Enter()
            {
                _player.GetUp();
                _player._animator.SetTrigger(_runTrigger);
                _player.CurrentWeapon.SetFireState(false);
                _targetPathPoint = _player._path.GetPoint();
                _player._rotationConstraint.weight = 0f;
                _player._rotationConstraint.constraintActive = false;
                _player.CurrentWeapon.RiseUp();
            }

            private void UpdatePathIndex()
            {
                if (_targetPathPoint.IsStopPoint)
                {
                    _player.StateEngine.SwitchState<IdleState>();
                    return;
                }
                _targetPathPoint = _player._path.GetPoint();
            }

            public void Run()
            {
                Vector3 playerPos = _player.transform.position;
                Vector3 direction = _targetPathPoint.transform.position - playerPos;
                direction.y = 0;
                Quaternion rotation = Quaternion.LookRotation(direction);
                _player.transform.rotation = Quaternion.Slerp(_player.transform.rotation, rotation, Time.deltaTime * 20f);
                
                //todo constrain velocty

                _player._rb.AddForce(_player.transform.forward * (_player._speed * 100 * Time.deltaTime), ForceMode.Force);
                
                if ((_targetPathPoint.transform.position - playerPos).sqrMagnitude > 2f) return;

                UpdatePathIndex();
            }

            public void Exit()
            {
                _player.transform.rotation = _targetPathPoint.transform.rotation;
                _player.CurrentWeapon.Lower();
                GameEvents.Call(new PlayerChangedPositionEvent());
            }
        }
        
        private class FiringState : IState
        {
            private readonly Player _player;

            public FiringState(Player player) => _player = player;

            public void Enter()
            {
                _player.GetUp();
                _player._animator.SetTrigger(_firingTrigger);
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

                _player._crosshair.SetReloadProgression(_reloadingTimeNormalized / _player.CurrentWeapon.ReloadTime);

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