using NTC.Global.Pool;
using SWAT.Behaviour;
using UnityEngine;

namespace SWAT.Weapons
{
    public class Bullet : Projectile
    {
        public override void OnSpawn() { }

        public override void OnDespawn() { }

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