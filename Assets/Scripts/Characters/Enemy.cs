using Arms;
using Controllers;
using NTC.Global.Pool;
using SWAT.Behaviour;
using SWAT.LevelScripts.Navigation;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using IState = SWAT.Behaviour.IState;

namespace SWAT
{
    public abstract class Enemy : BaseCharacter, IPoolItem
    {
        [SerializeField] private HitPointsValues _hitPointsValues;
        [SerializeField] private RotationConstraint _rotationConstraint;
        [field: SerializeField] public LineRenderer LaserBeam { get; private set; }

        protected Player Player;
        private Camera _camera;
        private List<HitPoint> _hitPoints;

        private bool _isFirePos;

        protected static readonly int FireTrigger = Animator.StringToHash("Fire");
        protected static readonly int RunTrigger = Animator.StringToHash("Run");

        public override CharacterType Type => CharacterType.Enemy;

        protected override int BaseMaxArmour => ChildMaxArmour;
        protected override int BaseMaxHealth => ChildMaxHealth;

        protected abstract int ChildMaxArmour { get; }
        protected abstract int ChildMaxHealth { get; }
        protected abstract int ProjectileDamage { get; }
        protected abstract int FiringRate { get; }
        protected abstract int ClipSize { get; }
        protected abstract int ReloadTime { get; }
        protected abstract int TotalAmmo { get; }
        protected abstract int EnemySpeed { get; }
        protected abstract int FiringTime { get; }
        
        protected override void OnEnabled()
        {
            base.OnEnabled();

            IsVulnerable = true;
            
            CurrentHealth = ChildMaxHealth;
            CurrentArmour = ChildMaxArmour;
            Speed = EnemySpeed;

            Player = ObjectHolder.GetObject<Player>();
            _camera = ObjectHolder.GetObject<Camera>();

            _hitPoints = new List<HitPoint>();

            for (int i = 0; i < 5; i++)
            {
                HitPoint point = new HitPoint(Player.HitPointsHolder.HitPoints[i], _hitPointsValues.Values[i]);
                _hitPoints.Add(point);
            }
            
            CurrentWeapon.Configure(
                ProjectileDamage,
                FiringRate,
                ClipSize,
                ReloadTime,
                TotalAmmo);
            
            ConfigureStates();
        }

        protected virtual void ConfigureStates()
        {
            StateEngine.AddState(
                new EnemyFiringState(this),
                new EnemyRunState(this));
        }

        private HitPoint RandomHitPoint(int randomValue)
        {
            foreach (HitPoint hitPoint in _hitPoints)
            {
                if (randomValue < hitPoint.Value)
                    return hitPoint;
                randomValue -= hitPoint.Value;
            }
            Debug.LogError("Random hit point exception");
            return new HitPoint();
        }

        public async Task<HitPoint> GetTargetAsync()
        {
            int randomInt = Random.Range(0, 100);
            return await Task.Run(() => RandomHitPoint(randomInt));
        }

        public void SetPositions(Path path)
        {
            Path = path;
            SetFirstState();
        }

        protected virtual void SetFirstState() => StateEngine.SwitchState<EnemyRunState>();

        protected override void Dead(Vector3 hitPosition)
        {
            SpawnDespawnActions(false);
            base.Dead(hitPosition);
        }

        public void OnSpawn()
        {
            Hud.OnSpawn();
            SpawnDespawnActions(true);
            SetPhysicsState(true);
        }

        private void SpawnDespawnActions(bool state)
        {
            Hud.transform.parent.gameObject.SetActive(state);
            CurrentWeapon.SetActive(state);

            if (LaserBeam != null)
                LaserBeam.enabled = state;
            if (_rotationConstraint != null)
                _rotationConstraint.constraintActive = state;
        }

        public void OnDespawn() { }

        protected override void LateRun() => Hud.transform.parent.transform.LookAt(_camera.transform);

        public void UnityEvent_FirePoseReached() => CurrentWeapon.SetFireState(true);
        
#region States
        protected class EnemyFiringState : IState
        {
            private readonly Enemy _enemy;
            private float _currentFiringTime;

            public EnemyFiringState(Enemy enemy) => _enemy = enemy;

            public void Enter()
            {
                if (_enemy.Animator != null)
                    _enemy.Animator.SetTrigger(FireTrigger);

                _currentFiringTime = _enemy.FiringTime;
                _enemy.transform.LookAt(_enemy.Player.transform.position);
                _enemy.CurrentWeapon.SetFireState(true);
            }

            public void Run()
            {
                _enemy.CurrentWeapon.Fire();
                _currentFiringTime -= Time.deltaTime;

                if (_currentFiringTime > 0)
                    return;

                _enemy.StateEngine.SwitchState<EnemyRunState>();
            }

            public void Exit() { }
        }

        protected class EnemyRunState : RunState
        {
            private readonly Enemy _enemy;
            public EnemyRunState(IRunStateReady character) : base(character) => _enemy = (Enemy)character;

            public override void Enter()
            {
                base.Enter();
                if (_enemy.Animator != null)
                    _enemy.Animator.SetTrigger(RunTrigger);

                _enemy.CurrentWeapon.SetFireState(false);
            }
            
            protected override bool OnPathPointStop()
            {
                _enemy.StateEngine.SwitchState<EnemyFiringState>();
                return true;
            }

            public override void Exit() { }
        }
#endregion
    }
}