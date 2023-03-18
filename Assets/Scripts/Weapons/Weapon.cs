using Controllers;
using NTC.Global.Cache;
using NTC.Global.Pool;
using SWAT.Behaviour;
using SWAT.Utility;
using System;
using UnityEngine;

namespace SWAT.Weapons
{
    public enum WeaponCarrier : byte
    {
        None,
        Player,
        Enemy
    }

    public class Weapon : MonoCache
    {
        public bool ClipIsEmpty { get; private set; }
        public bool FireState { get; private set; }
        // ReSharper disable once ConvertToAutoProperty
        public float ReloadTime => _reloadTime;

        [SerializeField] private Projectile _projectile;
        [SerializeField] private Transform _firePoint;
        [SerializeField, Range(1f, 500f)] private float _maxFireRange;
        [SerializeField] private Transform _rightHand;
        [SerializeField] private Transform _leftHand;
        [SerializeField] private WeaponCarrier _carrier;

        private int _firingRate;
        private int _clipSize;
        private int _reloadTime;
        private int _totalAmmo;

        private ITarget _target;
        private RaycastHit _hit;

        private float _currentFiringRate;
        private float _currentClipSize;

        private int _obstaclesLayer;

        public void Configure(int projectileDamage, int firingRate, int clipSize, int reloadTime, int totalAmmo)
        {
            _projectile.Configure(projectileDamage);
            _firingRate = firingRate;
            _clipSize = clipSize;
            _reloadTime = reloadTime;
            _totalAmmo = totalAmmo;
        }

        protected override void OnEnabled()
        {
            if (_carrier == WeaponCarrier.Player)
            {
                _target = ObjectHolder.GetObject<Crosshair>();
            }
            else if (_carrier == WeaponCarrier.Enemy)
            {
                _target = ObjectHolder.GetObject<Player>();
            }

            _currentClipSize = _clipSize;

            ResetFiringRate();
        }

        public void Fire()
        {
            if (FireState == false) return;

            _currentFiringRate -= Time.deltaTime;

            if (_currentFiringRate >= 0) return;

            _currentFiringRate = 60f / _firingRate;
            _currentClipSize--;

            NightPool.Despawn(NightPool.Spawn(_projectile, _firePoint.position, transform.rotation), FlyTime());

            if (_currentClipSize <= 0)
            {
                ClipIsEmpty = true;
            }
        }

        protected override void Run()
        {
            transform.position = _rightHand.position;

            if (FireState)
            {
                Vector3 direction = _target.GetTarget() - transform.position;
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 20f);
            }
            else
            {
                transform.LookAt(_leftHand.transform.position);
            }

#if UNITY_EDITOR
            Debug.DrawRay(transform.position, transform.forward * 100f, Color.cyan);
#endif
        }

        public void SetFireState(bool state) => FireState = state;

        private float FlyTime()
        {
            if (_obstaclesLayer == 0)
                _obstaclesLayer = LayerMask.GetMask("Obstacle");

            Physics.Raycast(transform.position, transform.forward, out _hit, _maxFireRange, _obstaclesLayer);

            float distance = _hit.transform == null
                ? _maxFireRange
                : Vector3.Distance(_firePoint.position, _hit.point);

            return distance / _projectile.FlySpeed;
        }

        public void ResetFiringRate()
        {
            _currentFiringRate = 60f / _firingRate;
        }

        public void Reload()
        {
            _totalAmmo -= _clipSize;
            _currentClipSize = _clipSize;
            ClipIsEmpty = false;
            ResetFiringRate();
        }
    }
}