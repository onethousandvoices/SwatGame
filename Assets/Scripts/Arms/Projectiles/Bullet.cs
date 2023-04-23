using NTC.Global.Pool;
using UnityEngine;

namespace SWAT.Weapons
{
    public class Bullet : Projectile
    {
        [SerializeField] private TrailRenderer _trail;

        public override void OnSpawn()
            => _trail.enabled = true;

        public override void OnDespawn()
            => _trail.enabled = false;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out HitBox hitBox))
            {
                if (hitBox.Character.Type != Carrier)
                    hitBox.DoDamage(Damage, transform.position);
            }
            NightPool.Despawn(this);
        }
    }
}