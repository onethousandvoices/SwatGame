using Controllers;
using SWAT.Behaviour;
using SWAT.LevelScripts;
using SWAT.Utility;
using System;
using UnityEngine;

namespace SWAT
{
    public class Enemy : BaseCharacter
    {
        [SerializeField] private EnemyPositions _positions;

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

        protected override void OnEnabled()
        {
            base.OnEnabled();

            _rb = Get<Rigidbody>();
            
            CurrentWeapon.Configure(
                _projectileDamage,
                _firingRate,
                _clipSize,
                _reloadTime,
                _totalAmmo);

            StateEngine.AddState(
                new FiringState(this),
                new RunState(this));

            StateEngine.SwitchState<RunState>();
        }

#region States
        private class FiringState : IState
        {
            private readonly Enemy _enemy;
            private float _currentFiringTime;

            public FiringState(Enemy enemy) => _enemy = enemy;

            public void Enter()
            {
                _currentFiringTime = _enemy._firingTime;
                _enemy.CurrentWeapon.SetFireState(true);
                _enemy.CurrentWeapon.transform.LookAt(ObjectHolder.GetObject<Player>().transform.position);
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

            private Path _path;
            private Transform _targetPosition;
            private Vector3 _currentPathPoint;
            private int _pathIndex;
            private int _positionIndex;

            public RunState(Enemy enemy) => _enemy = enemy;

            public void Enter()
            {
                _enemy.CurrentWeapon.SetFireState(false);

                if (_positionIndex >= _enemy._positions.TargetPositions.Length)
                    _positionIndex = 0;
                _targetPosition = _enemy._positions.TargetPositions[_positionIndex];

                _path = _enemy._positions.GetPath(_targetPosition);
                _pathIndex = 0;
                
                if (_path == null)
                {
                    throw new Exception($"{_enemy.name} can't find path to {_targetPosition}");
                }

                UpdatePathIndex();
            }

            private void UpdatePathIndex()
            {
                if (_currentPathPoint == _path.End.position)
                {
                    _enemy.StateEngine.SwitchState<FiringState>();
                    return;
                }
                
                if (_pathIndex >= _path.PathPoints.Count)
                {
                    _currentPathPoint = _path.End.position;
                    return;
                }
                _currentPathPoint = _path.PathPoints[_pathIndex].transform.position;
            }

            public void Run()
            {
                Vector3 enemyPos = _enemy.transform.position;
                Vector3 direction = _currentPathPoint - enemyPos;
                direction.y = 0;
                Quaternion rotation = Quaternion.LookRotation(direction);
                _enemy.transform.rotation = Quaternion.Slerp(_enemy.transform.rotation, rotation, Time.deltaTime * 20f);
                
                //todo constrain velocty

                _enemy._rb.AddForce(_enemy.transform.forward * _enemy._speed, ForceMode.Force);
                
                if ((_currentPathPoint - enemyPos).sqrMagnitude > 2f) return;

                _pathIndex++;
                UpdatePathIndex();
            }

            public void Exit()
            {
                _positionIndex++;
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