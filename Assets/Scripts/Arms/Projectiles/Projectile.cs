using NTC.Global.Cache;
using NTC.Global.Pool;
using UnityEngine;

namespace SWAT.Weapons
{
    public abstract class Projectile : MonoCache, IPoolItem
    {
        [field: SerializeField] public BoxCollider Collider { get; private set; }
        [field: SerializeField] public float FlySpeed { get; private set; }

        protected CharacterType Carrier { get; private set; }
        protected int Damage { get; private set; }

        public abstract void OnSpawn();
        public abstract void OnDespawn();

        protected override void Run()
        {
            transform.Translate(Vector3.forward * (FlySpeed * Time.deltaTime));
        }

        public void Configure(int damage, CharacterType type)
        {
            Damage = damage;
            Carrier = type;
        }
    }
}