using Arms;
using Controllers;
using NaughtyAttributes;
using NTC.Global.Pool;
using SWAT.Behaviour;
using SWAT.Events;
using SWAT.LevelScripts;
using SWAT.LevelScripts.Navigation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using IState = SWAT.Behaviour.IState;

namespace SWAT
{
    public abstract class Enemy : BaseCharacter, IPoolItem
    {
        [SerializeField] private Hud _enemyHud;
        [SerializeField] private EnemyHudHolder _hudHolder;
        [SerializeField] private HitPointsValues _hitPointsValues;
        [SerializeField] private RotationConstraint _rotationConstraint;
        [field: SerializeField] public LineRenderer LaserBeam { get; private set; }
        [HorizontalLine(color: EColor.Red)]
        [SerializeField] private HitBox[] _hitBoxes;
        [SerializeField] private Rigidbody[] _ragdollRbs;
        [SerializeField] private Collider[] _ragdollColliders;

        private Rigidbody _mainRb;
        private Animator _animator;
        private Player _player;
        private Camera _camera;
        private Path _path;
        private List<HitPoint> _hitPoints;

        private bool _isFirePos;

        private static readonly int _fireTrigger = Animator.StringToHash("Fire");
        private static readonly int _runTrigger = Animator.StringToHash("Run");

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

            _hitPoints = new List<HitPoint>();

            for (int i = 0; i < 5; i++)
            {
                HitPoint point = new HitPoint(_player.HitPointsHolder.HitPoints[i], _hitPointsValues.Values[i]);
                _hitPoints.Add(point);
            }

            _mainRb = Get<Rigidbody>();
            _animator = Get<Animator>();

            SetRagdollState(true);

            CurrentWeapon.Configure(
                ProjectileDamage,
                FiringRate,
                ClipSize,
                ReloadTime,
                TotalAmmo);

            StateEngine.AddState(
                new FiringState(this),
                new RunState(this));
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

        [Button("Find Ragdoll Rbs")]
        private void FindRbs()
        {
            _ragdollRbs = GetComponentsInChildren<Rigidbody>();
            _ragdollRbs = _ragdollRbs.Where(x => x != Get<Rigidbody>()).ToArray();
        }

        [Button("Find Ragdoll Colliders")]
        private void FindRagdollColliders()
        {
            _ragdollColliders = GetComponentsInChildren<Collider>();

            List<Collider> ragdollColliders = new List<Collider>(_ragdollColliders);
            List<Collider> collidersToRemove = new List<Collider>();

            foreach (Collider col in _ragdollColliders)
                if (col.gameObject.TryGetComponent(out HitBox box))
                    collidersToRemove.Add(col);

            foreach (Collider col in collidersToRemove)
                ragdollColliders.Remove(col);

            _ragdollColliders = ragdollColliders.ToArray();
        }

        [Button("Find HitBoxes")]
        private void FindHitBoxes() => _hitBoxes = GetComponentsInChildren<HitBox>();

        private void SetRagdollState(bool state)
        {
            foreach (Rigidbody rb in _ragdollRbs)
                rb.isKinematic = state;
            foreach (Collider col in _ragdollColliders)
                col.isTrigger = state;
            foreach (HitBox box in _hitBoxes)
                box.Collider.isTrigger = !state;
        }

        public void SetPositions(Path path)
        {
            _path = path;
            SetFirstState();
        }

        protected virtual void SetFirstState() => StateEngine.SwitchState<RunState>();

        protected override void Dead(Vector3 hitPosition)
        {
            StateEngine.Stop();
            _hudHolder.gameObject.SetActive(false);
            SpawnDespawnActions(false);

            Collider[] colliders = new Collider[5];

            Physics.OverlapSphereNonAlloc(hitPosition, 3f, colliders);
            
            foreach (Collider col in colliders)
            {
                if (col != null && col.attachedRigidbody != null)
                    col.attachedRigidbody.AddExplosionForce(2888f, hitPosition, 3f);
            }
            
            GameEvents.Call(new EnemyKilledEvent(this));
            StartCoroutine(DeathRoutine());
        }

        private IEnumerator DeathRoutine()
        {
            yield return new WaitForSeconds(3f);

            foreach (Rigidbody rb in _ragdollRbs)
                rb.isKinematic = true;
            foreach (Collider ragdollCollider in _ragdollColliders)
                ragdollCollider.isTrigger = true;

            yield return new WaitForSeconds(7f);

            float t = 0f;
            while (t < 1f)
            {
                transform.position = Vector3.Lerp(transform.position, transform.position - new Vector3(0f, 5f, 0f), t);
                yield return null;
                t += Time.deltaTime;
            }

            NightPool.Despawn(this);
        }

        public void OnSpawn()
        {
            _enemyHud.OnSpawn();
            _hudHolder.gameObject.SetActive(true);
            SpawnDespawnActions(true);
        }

        private void SpawnDespawnActions(bool state)
        {
            SetRagdollState(state);
            _mainRb.isKinematic = !state;
            CurrentWeapon.gameObject.SetActive(state);
            if (_rotationConstraint != null)
                _rotationConstraint.constraintActive = state;
            if (_animator != null)
                _animator.enabled = state;
        }

        public void OnDespawn() { }

        protected override void LateRun() => _hudHolder.transform.LookAt(_camera.transform);

        public void UnityEvent_FirePoseReached() => CurrentWeapon.SetFireState(true);

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

                if (_currentFiringTime > 0)
                    return;

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

                _enemy._mainRb.AddForce(_enemy.transform.forward * (_enemy.Speed * 100 * Time.deltaTime), ForceMode.Force);

                if ((_targetPathPoint.transform.position - enemyPos).sqrMagnitude > 2f)
                    return;

                UpdatePathIndex();
            }

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