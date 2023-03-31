using Controllers;
using SWAT.Events;
using SWAT.LevelScripts;
using SWAT.LevelScripts.Navigation;
using SWAT.Utility;
using System;
using UnityEngine;
using IState = SWAT.Behaviour.IState;

namespace SWAT
{
    public class Enemy : BaseCharacter
    {
        [SerializeField] private Path _path;

        [SerializeField, Config(Extras.Enemy, "A1")] private int _maxHealth;
        [SerializeField, Config(Extras.Enemy, "A2")] private int _maxArmour;
        [SerializeField, Config(Extras.Enemy, "A3")] private int _speed;
        [SerializeField, Config(Extras.Enemy, "A4")] private int _firingTime;

        [SerializeField, Config(Extras.EnemyWeapon_Pistol, "A1")] private int _projectileDamage;
        [SerializeField, Config(Extras.EnemyWeapon_Pistol, "A2")] private int _firingRate;
        [SerializeField, Config(Extras.EnemyWeapon_Pistol, "A3")] private int _clipSize;
        [SerializeField, Config(Extras.EnemyWeapon_Pistol, "A4")] private int _reloadTime;
        [SerializeField, Config(Extras.EnemyWeapon_Pistol, "A5")] private int _totalAmmo;

        private Rigidbody _rb;
        private Animator _animator;
        private Player _player;

        private bool _isFirePos;
        
        private static readonly int _fireTrigger = Animator.StringToHash("Fire");
        private static readonly int _runTrigger = Animator.StringToHash("Run");

        protected override void OnEnabled()
        {
            base.OnEnabled();

            IsVulnerable = true;
            
            CurrentHealth = _maxHealth;
            CurrentArmour = _maxArmour;

            _player = ObjectHolder.GetObject<Player>();
            
            _rb = Get<Rigidbody>();
            _animator = Get<Animator>();
            
            CurrentWeapon.Configure(
                _projectileDamage,
                _firingRate,
                _clipSize,
                _reloadTime,
                _totalAmmo);

            StateEngine.AddState(
                new FiringState(this),
                new RunState(this),
                new DeadState());
        }

        public void SetPositions(Path path)
        {
            _path = path;
            StateEngine.SwitchState<RunState>();
        }

        protected override void Dead()
        {
            GameEvents.Call(new EnemyKilledEvent(this));

            StateEngine.SwitchState<DeadState>();
            //todo death animation
        }

        public void UnityEvent_FirePoseReached()
        {
            CurrentWeapon.SetFireState(true);
        }

#region States
        private class FiringState : IState
        {
            private readonly Enemy _enemy;
            private float _currentFiringTime;

            public FiringState(Enemy enemy) => _enemy = enemy;

            public void Enter()
            {
                _enemy._animator.SetTrigger(_fireTrigger);
                _currentFiringTime = _enemy._firingTime;
                _enemy.transform.LookAt(_enemy._player.transform.position);
            }

            public void Run()
            {
                _enemy.CurrentWeapon.Fire();
                _currentFiringTime -= Time.deltaTime;

                if (_currentFiringTime > 0) return;

                _enemy.StateEngine.SwitchState<RunState>();
            }

            public void Exit()
            { }
        }

        private class RunState : IState
        {
            private readonly Enemy _enemy;
            private PathPoint _targetPathPoint;
            public RunState(Enemy enemy) => _enemy = enemy;

            public void Enter()
            {
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

                _enemy._rb.AddForce(_enemy.transform.forward * _enemy._speed, ForceMode.Force);
                
                if ((_targetPathPoint.transform.position - enemyPos).sqrMagnitude > 2f) return;

                UpdatePathIndex();
            }

            public void Exit()
            {
            }
        }

        private class DeadState : IState
        {
            public void Enter()
            {
            }

            public void Run()
            {
            }

            public void Exit()
            {
            }
        }
#endregion

#if UNITY_EDITOR
        public void UpdateConfigValues()
        {
            GameController controller = FindObjectOfType<GameController>();
            controller.ConfigObject(this);
            Debug.LogError($"{name} parameters updated!");
        }
#endif
    }
}