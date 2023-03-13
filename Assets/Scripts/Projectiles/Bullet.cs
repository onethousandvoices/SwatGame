using UnityEngine;

namespace SWAT.Weapons
{
    public class Bullet : Projectile
    {
        public override void OnSpawn() { }

        public override void OnDespawn()
        {
            Debug.Log("Contact");

            Collider[] results = new Collider[1];
            // Physics.OverlapSphereNonAlloc(transform.position, Collider.radius, results);

            for (int i = 0; i < results.Length; i++)
            {
                Collider col = results[i];
                // Debug.Log(col);
            }
        }
    }
}