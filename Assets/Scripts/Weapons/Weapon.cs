using Controllers;
using NTC.Global.Cache;
using NTC.Global.Pool;
using UnityEngine;

namespace SWAT.Weapons
{
    public class Weapon : MonoCache
    {
        public bool ClipIsEmpty { get; private set; }
        // ReSharper disable once ConvertToAutoProperty
        public float ReloadTime => _reloadTime;

        [SerializeField]                  private Projectile _projectile;
        [SerializeField]                  private Transform  _firePoint;
        [SerializeField, Range(1f, 500f)] private float      _maxFireRange;
        [SerializeField]                  private Transform  _rightHand;

        [Config(Extras.PlayerWeapon, "A1")] private int _projectileDamage;
        [Config(Extras.PlayerWeapon, "A2")] private int _firingRate;
        [Config(Extras.PlayerWeapon, "A3")] private int _clipSize;
        [Config(Extras.PlayerWeapon, "A4")] private int _reloadTime;
        [Config(Extras.PlayerWeapon, "A5")] private int _totalAmmo;

        private RaycastHit _hit;

        private float _currentFiringRate;
        private float _currentClipSize;

        private int _obstaclesLayer;

        private Crosshair _crosshair;

        protected override void OnEnabled()
        {
            _crosshair = ObjectHolder.GetObject<Crosshair>();
            
            _projectile.Configure(_projectileDamage);
            _currentClipSize = _clipSize;
        }

        public void Fire()
        {
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

            Vector3 direction = _crosshair.RayHit() - transform.position;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction),Time.deltaTime * 20f);
#if UNITY_EDITOR
            Debug.DrawRay(transform.position, transform.forward * 100f, Color.cyan);
#endif
        }

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
            _totalAmmo       -= _clipSize;
            _currentClipSize =  _clipSize;
            ClipIsEmpty      =  false;
            ResetFiringRate();
        }
    }
}