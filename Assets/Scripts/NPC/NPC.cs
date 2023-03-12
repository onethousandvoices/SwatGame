using NTC.Global.Cache;
using SWAT.Behaviour;
using SWAT.Weapons;
using UnityEngine;

namespace SWAT
{
    public abstract class NPC : MonoCache
    {
        [SerializeField] protected Weapon CurrentWeapon;

        protected StateEngine StateEngine;

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
    }
}