using NTC.Global.Cache;
using System;
using UnityEngine;

namespace SWAT.Behaviour
{
    public class HitBox : MonoCache
    {
        [SerializeField] private BaseCharacter _character;

        public void DoDamage(int damage)
        {
            if (_character != null) 
                _character.DoDamage(damage);
            else
                Debug.LogError($"{name} character is not assigned!");
        }
    }
}