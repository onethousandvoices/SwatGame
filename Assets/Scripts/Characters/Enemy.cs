using Controllers;
using NaughtyAttributes;
using NTC.Global.Pool;
using SWAT.Events;
using SWAT.LevelScripts;
using SWAT.LevelScripts.Navigation;
using UnityEngine;
using IState = SWAT.Behaviour.IState;

namespace SWAT
{
    public abstract class Enemy : BaseCharacter, IPoolItem
    {
        [SerializeField] private Hud _enemyHud;
        [SerializeField] private EnemyHudHolder _hudHolder;
        [SerializeField] private Path _path;

        private Rigidbody _rb;
        private Animator _animator;
        private Player _player;
        private Camera _camera;

        private bool _isFirePos;

        private static readonly int _fireTrigger = Animator.StringToHash("Fire");
        private static readonly int _runTrigger = Animator.StringToHash("Run");

        protected override int BaseMaxArmour => ChildMaxArmour;
        protected override int BaseMaxHealth => ChildMaxHealth;

        protected abstract int ChildMaxArmour { get; }
        protected abstract int ChildMaxHealth { get; }
        protected abstract int ProjectileDamage { get; }
        protected abstract int FiringRate { get; }
        protected abstract int ClipSize { get; }
        protected abstract int ReloadTime { get; }
        protected abstract int TotalAmmo { get; }
        protected abstract int Speed { get; }
        protected abstract int FiringTime { get; }
        
        protected override void OnEnabled()
        {
            base.OnEnabled();

            IsVulnerable = true;

            SetHud(_enemyHud);

            CurrentHealth = ChildMaxHealth;
            CurrentArmour = ChildMaxArmour;

            _player = ObjectHolder.GetObject<Player>();
            _camera = ObjectHolder.GetObject<Camera>();

            _rb = Get<Rigidbody>();
            _animator = Get<Animator>();

            CurrentWeapon.Configure(
                ProjectileDamage,
                FiringRate,
                ClipSize,
                ReloadTime,
                TotalAmmo);

            StateEngine.AddState(
                new FiringState(this),
                new RunState(this),
                new DeadState());
        }

        public void SetPositions(Path path)
        {
            _path = path;
            SetFirstState();
        }

        protected virtual void SetFirstState()
        {
            StateEngine.SwitchState<RunState>();
        }

        protected override void Dead()
        {
            GameEvents.Call(new EnemyKilledEvent(this));
            NightPool.Despawn(this);
        }

        protected override void LateRun()
        {
            _hudHolder.transform.LookAt(_camera.transform);
        }

        public void OnSpawn()
        {
            _enemyHud.OnSpawn();
        }

        public void OnDespawn() { }

        public void UnityEvent_FirePoseReached()
        {
            CurrentWeapon.SetFireState(true);
        }

#region States
        protected class FiringState : IState
        {
            private readonly Enemy _enemy;
            private float _currentFiringTime;

            public FiringState(Enemy enemy) => _enemy = enemy;

            public void Enter()
            {
                if (_enemy._animator != null)
                    _enemy._animator.SetTrigger(_fireTrigger);

                _currentFiringTime = _enemy.FiringTime;
                _enemy.transform.LookAt(_enemy._player.transform.position);
                _enemy.CurrentWeapon.SetFireState(true);
            }

            public void Run()
            {
                _enemy.CurrentWeapon.Fire();
                _currentFiringTime -= Time.deltaTime;

                if (_currentFiringTime > 0) return;

                _enemy.StateEngine.SwitchState<RunState>();
            }

            public void Exit() { }
        }

        private class RunState : IState
        {
            private readonly Enemy _enemy;
            private PathPoint _targetPathPoint;
            public RunState(Enemy enemy) => _enemy = enemy;

            public void Enter()
            {
                if (_enemy._animator != null)
                    _enemy._animator.SetTrigger(_runTrigger);
                
                _enemy.CurrentWeapon.SetFireState(false);
                _targetPathPoint = _enemy._path.GetPoint();
            }

            private void UpdatePathIndex()
            {
                if (_targetPathPoint.IsStopPoint)
                {
                    _enemy.StateEngine.SwitchState<FiringState>();
                    return;
                }

                _targetPathPoint = _enemy._path.GetPoint();
            }

            public void Run()
            {
                Vector3 enemyPos = _enemy.transform.position;
                Vector3 direction = _targetPathPoint.transform.position - enemyPos;
                direction.y = 0;
                Quaternion rotation = Quaternion.LookRotation(direction);
                _enemy.transform.rotation = Quaternion.Slerp(_enemy.transform.rotation, rotation, Time.deltaTime * 20f);

                //todo constrain velocty

                _enemy._rb.AddForce(_enemy.transform.forward * _enemy.Speed, ForceMode.Force);

                if ((_targetPathPoint.transform.position - enemyPos).sqrMagnitude > 2f) return;

                UpdatePathIndex();
            }

            public void Exit() { }
        }

        private class DeadState : IState
        {
            public void Enter() { }

            public void Run() { }

            public void Exit() { }
        }
#endregion

        [Button("Update Parameters")]
        public void UpdateConfigValues()
        {
            GameController controller = FindObjectOfType<GameController>();
            controller.ConfigObject(this);
            Debug.LogError($"{name} parameters updated!");
        }
    }
}