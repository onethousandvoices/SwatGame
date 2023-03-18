using SWAT.Behaviour;
using SWAT.Utility;
using UnityEngine;

namespace SWAT
{
    public class Enemy : BaseCharacter
    {
        [SerializeField, Config(Extras.Enemy, "A1")] private int _maxHealth;
        [SerializeField, Config(Extras.Enemy, "A2")] private int _maxArmour;
        [SerializeField, Config(Extras.Enemy, "A3")] private int _speed;
        [SerializeField, Config(Extras.Enemy, "A4")] private int _firingTime;

        [SerializeField, Config(Extras.EnemyWeapon_Pistol, "A1")] private int _projectileDamage;
        [SerializeField, Config(Extras.EnemyWeapon_Pistol, "A2")] private int _firingRate;
        [SerializeField, Config(Extras.EnemyWeapon_Pistol, "A3")] private int _clipSize;
        [SerializeField, Config(Extras.EnemyWeapon_Pistol, "A4")] private int _reloadTime;
        [SerializeField, Config(Extras.EnemyWeapon_Pistol, "A5")] private int _totalAmmo;

        protected override void OnEnabled()
        {
            base.OnEnabled();
            
            CurrentWeapon.Configure(
                _projectileDamage,
                _firingRate,
                _clipSize,
                _reloadTime,
                _totalAmmo);

            StateEngine.AddState(
                new FiringState(),
                new RunState());
        }

#region States
        private class FiringState : IState
        {
            public void Enter() { }

            public void Run() { }

            public void Exit() { }
        }

        private class RunState : IState
        {
            public void Enter() { }

            public void Run() { }

            public void Exit() { }
        }
#endregion
    }
}