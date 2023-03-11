using NTC.Global.Cache;
using SWAT.Behaviour;
using SWAT.Weapons;

namespace SWAT
{
    public abstract class NPC : MonoCache
    {
        protected StateEngine StateEngine;
        protected Weapon      CurrentWeapon;

        protected int CurrentHealth;
        protected int CurrentArmour;

        protected override void OnEnabled()
        {
            CurrentWeapon = ChildrenGet<Weapon>();
            StateEngine   = new StateEngine();
        }

        protected override void Run()
        {
            StateEngine.CurrentState?.Run();
        }
    }
}