using NaughtyAttributes;
using SWAT.Utility;
using UnityEngine;

namespace SWAT
{
    public class EnemySniper : Enemy
    {
        [HorizontalLine(color: EColor.Red)]
        [SerializeField, Config(Extras.EnemySniper, "A1")] private int _maxHealth;
        [SerializeField, Config(Extras.EnemySniper, "A2")] private int _maxArmour;
        [SerializeField, Config(Extras.EnemySniper, "A3")] private int _speed;
        [SerializeField, Config(Extras.EnemySniper, "A4")] private int _firingTime;
        [HorizontalLine(color: EColor.Red)]
        [SerializeField, Config(Extras.EnemyWeapon_SniperRifle, "A1")] private int _projectileDamage;
        [SerializeField, Config(Extras.EnemyWeapon_SniperRifle, "A2")] private int _firingRate;
        [SerializeField, Config(Extras.EnemyWeapon_SniperRifle, "A3")] private int _clipSize;
        [SerializeField, Config(Extras.EnemyWeapon_SniperRifle, "A4")] private int _reloadTime;
        [SerializeField, Config(Extras.EnemyWeapon_SniperRifle, "A5")] private int _totalAmmo;

        protected override int ChildMaxArmour => _maxArmour;
        protected override int ChildMaxHealth => _maxHealth;
        protected override int ProjectileDamage => _projectileDamage;
        protected override int FiringRate => _firingRate;
        protected override int ClipSize => _clipSize;
        protected override int ReloadTime => _reloadTime;
        protected override int TotalAmmo => _totalAmmo;
        protected override int Speed => _speed;
        protected override int FiringTime => _firingTime;

        protected override void SetFirstState()
        {
            StateEngine.SwitchState<FiringState>();
        }
    }
}