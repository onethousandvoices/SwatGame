using Controllers;
using NTC.Global.Cache;
using NTC.Global.Pool;
using SWAT.Behaviour;
using System.Collections.Generic;
using UnityEngine;

namespace SWAT.Weapons
{
    // ReSharper disable once ConvertToAutoProperty

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
        public float ReloadTime => _reloadTime;

        [SerializeField] private Projectile _projectile;
        [SerializeField] private Transform _firePoint;
        [SerializeField, Range(1f, 500f)] private float _maxFireRange;
        [SerializeField] private Transform _rightHand;
        [SerializeField] private Transform _leftHand;
        [SerializeField] private WeaponCarrier _carrier;
        [SerializeField] private Vector3 _posOffset;

        private int _firingRate;
        private int _clipSize;
        private int _reloadTime;
        private int _totalAmmo;
        private int _lookSpeed;
        private int _projectileDamage;

        private List<Vector3> _aimingPoints;
        private Vector3 _currentAimingPoint;
        private ITarget _target;
        private RaycastHit _hit;
        private Player _player;

        private float _currentFiringRate;
        private float _currentClipSize;

        private bool _isRiseUp;

        public async void Configure(int projectileDamage, int firingRate, int clipSize, int reloadTime, int totalAmmo)
        {
            _projectileDamage = projectileDamage;
            _firingRate = firingRate;
            _clipSize = clipSize;
            _reloadTime = reloadTime;
            _totalAmmo = totalAmmo;

            switch (_carrier)
            {
                case WeaponCarrier.Player:
                    _lookSpeed = 20;
                    _target = ObjectHolder.GetObject<Crosshair>();
                    break;
                case WeaponCarrier.Enemy:
                    _lookSpeed = 50;
                    _player = ObjectHolder.GetObject<Player>();

                    const int count = 50;
                    _aimingPoints = new List<Vector3>();
                    for (int i = 0; i < count; i++)
                    {
                        HitPoint hitPoint = await _player.GetTargetAsync();
                        _aimingPoints.Add(hitPoint.Target.position);
                    }
                    break;
            }

            _currentClipSize = _clipSize;

            SetAimingPoint();
            ResetFiringRate();
        }

        public void Fire()
        {
            if (FireState == false) return;

            _currentFiringRate -= Time.deltaTime;

            if (_currentFiringRate >= 0) return;

            if (_carrier == WeaponCarrier.Enemy)
            {
                // RemoveAimingPoint();
                SetAimingPoint();
            }

            _currentFiringRate = 60f / _firingRate;
            _currentClipSize--;

            Projectile projectile = NightPool.Spawn(_projectile, _firePoint.position, transform.rotation);
            projectile.Configure(_projectileDamage);

            if (_currentClipSize <= 0)
            {
                ClipIsEmpty = true;
            }
            SetAimingPoint();
        }

        private async void SetAimingPoint()
        {
            // if (_aimingPoints.Count < 1) return;
            // _currentAimingPoint = _aimingPoints[0];
            HitPoint hitPoint = await _player.GetTargetAsync();
            _currentAimingPoint = hitPoint.Target.position;
        }

        private void RemoveAimingPoint()
        {
            if (_aimingPoints.Count < 1) return;
            _aimingPoints.RemoveAt(0);
        }

        protected override void Run()
        {
            if (_carrier == WeaponCarrier.Player)
                transform.position = _rightHand.position;

            if (FireState)
            {
                if (_carrier == WeaponCarrier.Player)
                    _currentAimingPoint = _target.GetTarget();
                else
                    transform.position = _rightHand.position + new Vector3(0f, 0.2f, 0f) + _posOffset;

                Vector3 direction = _currentAimingPoint - transform.position + _posOffset;
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * _lookSpeed);
            }
            else if (_carrier == WeaponCarrier.Player && _isRiseUp == false)
            {
                if (_leftHand != null)
                    transform.LookAt(_leftHand.transform.position);
                else
                    transform.rotation = _rightHand.rotation;
            }

#if UNITY_EDITOR
            Debug.DrawRay(_firePoint.position, transform.forward * 100f, Color.cyan);
#endif
        }

        public void RiseUp()
        {
            _isRiseUp = true;
            transform.localEulerAngles = new Vector3(-90f, 0f, 0f);
        }

        public void Lower()
        {
            _isRiseUp = false;
        }

        public void SetFireState(bool state) => FireState = state;

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