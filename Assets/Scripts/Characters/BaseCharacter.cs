using NTC.Global.Cache;
using SWAT.Behaviour;
using SWAT.Weapons;
using UnityEngine;

namespace SWAT
{
    public abstract class BaseCharacter : MonoCache
    {
        [SerializeField] protected Weapon CurrentWeapon;

        protected StateEngine StateEngine;

        protected bool IsVulnerable;
        protected int CurrentHealth;
        protected int CurrentArmour;

        protected override void OnEnabled()
        {
            StateEngine = new StateEngine();
        }

        protected override void Run()
        {
            StateEngine.CurrentState?.Run();
        }

        public void DoDamage(int damage)
        {
            if (IsVulnerable == false) return;
            
            if (CurrentArmour > 0)
            {
                CurrentArmour -= damage;

                if (CurrentArmour < 0)
                    CurrentHealth -= Mathf.Abs(CurrentArmour);
                return;
            }

            if (CurrentHealth > 0)
            {
                CurrentHealth -= damage;
            }

            if (CurrentHealth <= 0)
            {
                Dead();
            }
        }

        protected abstract void Dead();
    }
}