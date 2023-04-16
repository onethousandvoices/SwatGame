using NaughtyAttributes;
using NTC.Global.Pool;
using SWAT.Behaviour;
using SWAT.Events;
using SWAT.Utility;
using SWAT.Weapons;
using UnityEngine;

namespace SWAT
{
    public class Boss : Enemy, IPoolItem
    {
        [SerializeField] private Weapon _weapon;
        [HorizontalLine(color: EColor.Red)]
        [SerializeField, Config(Extras.Boss, "A1")] private int _maxHealth;
        [SerializeField, Config(Extras.Boss, "A2")] private int _maxArmour;
        [SerializeField, Config(Extras.Boss, "A3")] private int _speed;
        [SerializeField, Config(Extras.Boss, "A4")] private int _firingTime;
        [HorizontalLine(color: EColor.Red)]
        [SerializeField, Config(Extras.Boss_Weapons, "A1")] private int _projectileDamage;
        [SerializeField, Config(Extras.Boss_Weapons, "A2")] private int _firingRate;
        [SerializeField, Config(Extras.Boss_Weapons, "A3")] private int _clipSize;
        [SerializeField, Config(Extras.Boss_Weapons, "A4")] private int _reloadTime;
        [SerializeField, Config(Extras.Boss_Weapons, "A5")] private int _totalAmmo;
        
        public override CharacterType Type { get; }
        protected override int ChildMaxArmour { get; }
        protected override int ChildMaxHealth { get; }
        protected override int ProjectileDamage { get; }
        protected override int FiringRate { get; }
        protected override int ClipSize { get; }
        protected override int ReloadTime { get; }
        protected override int TotalAmmo { get; }
        protected override int EnemySpeed { get; }
        protected override int FiringTime { get; }

        protected override void OnEnabled()
        {
            base.OnEnabled();
        }

        protected override void ConfigureStates()
        {
            StateEngine.AddState(
                new BossRunState(this));
        }

        private class BossRunState : EnemyRunState
        {
            public BossRunState(IRunStateReady character) : base(character) { }

            protected override bool OnPathPointStop()
            {
                //todo
                return true;
            }
        }
    }
}