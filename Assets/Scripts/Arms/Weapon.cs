using Controllers;
using NaughtyAttributes;
using NTC.Global.Cache;
using NTC.Global.Pool;
using SWAT.Behaviour;
using SWAT.Events;
using System.Threading.Tasks;
using UnityEngine;

namespace SWAT.Weapons
{
    public class Weapon : MonoCache
    {
        public bool ClipIsEmpty { get; private set; }
        public bool FireState { get; private set; }
        public float ReloadTime => _reloadTime;

        [SerializeField] private Weapon _weaponPair;
        [HorizontalLine(color: EColor.Red)]
        [SerializeField] private Projectile _projectile;
        [SerializeField] private Transform _firePoint;
        [SerializeField, Range(1f, 500f)] private float _maxFireRange;
        [SerializeField] private Transform _rightHand;
        [SerializeField] private Transform _leftHand;
        [SerializeField] private Vector3 _posOffset;
        [SerializeField] private BaseCharacter _carrier;

        private int _firingRate;
        private int _clipSize;
        private int _reloadTime;
        private int _totalAmmo;
        private int _lookSpeed;
        private int _projectileDamage;

        private Vector3 _currentAimingPoint;
        private ITarget _target;
        private RaycastHit _hit;

        private float _currentFiringRate;
        private float _currentClipSize;

        private bool _isRiseUp;

        public void Configure(int projectileDamage, int firingRate, int clipSize, int reloadTime, int totalAmmo)
        {
            _projectileDamage = projectileDamage;
            _firingRate = firingRate;
            _clipSize = clipSize;
            _reloadTime = reloadTime;
            _totalAmmo = totalAmmo;
            
            if (_carrier is Player)
            {
                _lookSpeed = 20;
                _target = ObjectHolder.GetObject<Crosshair>();
            }
            else
            {
                _lookSpeed = 150;
                SetAimingPoint();
            }

            if (_carrier is Boss)
            {
                GameEvents.Register<Event_BossOnSecondaryWeaponShot>(OnSecondaryWeaponShot);
            }

            _currentClipSize = _clipSize;

            ResetFiringRate();
        }

        private async void OnSecondaryWeaponShot(Event_BossOnSecondaryWeaponShot obj)
        {
            await _weaponPair.SetAimingPoint();
            _weaponPair.FireInner();
        }

        public async void Fire()
        {
            if (FireState == false)
                return;
            _currentFiringRate -= Time.deltaTime;

            if (_currentFiringRate >= 0)
                return;
            _currentFiringRate = 60f / _firingRate;

            if (_carrier is Enemy)
            {
                await SetAimingPoint();
                FireInner();
                return;
            }

            FireInner();
        }

        private void FireInner()
        {
            if (!gameObject.activeSelf)
                return;

            _currentClipSize--;

            Projectile projectile = NightPool.Spawn(_projectile, _firePoint.position, transform.rotation);
            projectile.Configure(_projectileDamage, _carrier.Type);

            GameEvents.Call(new Event_WeaponFire(_carrier, _currentClipSize / _clipSize));

            if (_currentClipSize <= 0)
                ClipIsEmpty = true;
        }

        private async Task SetAimingPoint()
        {
            Enemy enemy = (Enemy)_carrier;
            HitPoint hitPoint = await enemy.GetTargetAsync();
            _currentAimingPoint = hitPoint.Target.position;
        }

        protected override void Run()
        {
            if (_carrier is Player)
                transform.position = _rightHand.position;

            if (FireState)
            {
                switch (_carrier)
                {
                    case Player:
                        _currentAimingPoint = _target.GetTarget();
                        break;
                    case Boss boss:
                        break;
                    case Enemy enemy:
                        transform.position = _rightHand.position + new Vector3(0f, 0.2f, 0f) + _posOffset;

                        if (_currentAimingPoint != Vector3.zero && enemy.LaserBeam != null)
                        {
                            enemy.LaserBeam.SetPosition(1, _firePoint.position);
                            enemy.LaserBeam.SetPosition(0, _currentAimingPoint);
                        }
                        break;
                }

                Vector3 direction = _currentAimingPoint - transform.position + _posOffset;
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * _lookSpeed);
            }
            else if (_carrier is Player && _isRiseUp == false)
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

        public void SetActive(bool state)
        {
            gameObject.SetActive(state);
            if (_weaponPair != null)
                _weaponPair.gameObject.SetActive(state);
        }
        
        public void RiseUp()
        {
            _isRiseUp = true;
            transform.localEulerAngles = new Vector3(-90f, 0f, 0f);
        }

        public void Lower() => _isRiseUp = false;

        public void SetFireState(bool state) => FireState = state;

        public void ResetFiringRate() => _currentFiringRate = 60f / _firingRate;

        public void Reload()
        {
            _totalAmmo -= _clipSize;
            _currentClipSize = _clipSize;
            ClipIsEmpty = false;
            ResetFiringRate();
        }
    }
}