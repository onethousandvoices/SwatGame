using NTC.Global.Cache;
using System;
using UnityEngine;

namespace SWAT.Behaviour
{
    public class HitBox : MonoCache
    {
        [field: SerializeField] public BaseCharacter Character { get; private set; }

        public void DoDamage(int damage)
        {
            if (Character != null) 
                Character.DoDamage(damage);
            else
                Debug.LogError($"{name} character is not assigned!");
        }
    }
}