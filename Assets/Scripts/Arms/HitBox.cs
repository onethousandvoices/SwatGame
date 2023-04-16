using NTC.Global.Cache;
using UnityEngine;

namespace SWAT
{
    public class HitBox : MonoCache
    {
        [field: SerializeField] public BaseCharacter Character { get; private set; }
        [field: SerializeField] public Collider Collider { get; private set; }

        public void DoDamage(int damage, Vector3 position)
        {
            if (Character != null)
                Character.DoDamage(damage, position);
            else
                Debug.LogError($"{name} character is not assigned!");
        }

        private void OnValidate()
        {
            Collider = Get<Collider>();
        }
    }
}