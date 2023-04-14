using NaughtyAttributes;
using SWAT.Utility;
using UnityEngine;

namespace SWAT
{
    public class EnemyThug : Enemy
    {
        [HorizontalLine(color: EColor.Red)]
        [SerializeField, Config(Extras.Enemy, "A1")] private int _maxHealth;
        [SerializeField, Config(Extras.Enemy, "A2")] private int _maxArmour;
        [SerializeField, Config(Extras.Enemy, "A3")] private int _speed;
        [SerializeField, Config(Extras.Enemy, "A4")] private int _firingTime;
        [HorizontalLine(color: EColor.Red)]
        [SerializeField, Config(Extras.EnemyWeapon_Pistol, "A1")] private int _projectileDamage;
        [SerializeField, Config(Extras.EnemyWeapon_Pistol, "A2")] private int _firingRate;
        [SerializeField, Config(Extras.EnemyWeapon_Pistol, "A3")] private int _clipSize;
        [SerializeField, Config(Extras.EnemyWeapon_Pistol, "A4")] private int _reloadTime;
        [SerializeField, Config(Extras.EnemyWeapon_Pistol, "A5")] private int _totalAmmo;
        
        protected override int ChildMaxArmour => _maxArmour;
        protected override int ChildMaxHealth => _maxHealth;
        protected override int ProjectileDamage => _projectileDamage;
        protected override int FiringRate => _firingRate;
        protected override int ClipSize => _clipSize;
        protected override int ReloadTime => _reloadTime;
        protected override int TotalAmmo => _totalAmmo;
        protected override int EnemySpeed => _speed;
        protected override int FiringTime => _firingTime;
    }
}