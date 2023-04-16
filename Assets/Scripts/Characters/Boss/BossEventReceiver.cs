using NTC.Global.Cache;
using SWAT.Events;

namespace SWAT
{
    public class BossEventReceiver : MonoCache
    {
        public void AnimatorEvent_SecondaryWeaponShot()
        {
            GameEvents.Call(new Event_BossOnSecondaryWeaponShot());
        }
    }
}