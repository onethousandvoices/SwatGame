using NaughtyAttributes;
using SWAT.Behaviour;
using SWAT.Events;
using SWAT.Utility;
using UnityEngine;

namespace SWAT
{
    public class Boss : Enemy
    {
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

        private static readonly int ShootTrigger = Animator.StringToHash("Shoot");

        public override CharacterType Type => CharacterType.Boss;
        protected override int ChildMaxArmour => _maxArmour;
        protected override int ChildMaxHealth => _maxHealth;
        protected override int ProjectileDamage => _projectileDamage;
        protected override int FiringRate => _firingRate;
        protected override int ClipSize => _clipSize;
        protected override int ReloadTime => _reloadTime;
        protected override int TotalAmmo => _totalAmmo;
        protected override int EnemySpeed => _speed;
        protected override int FiringTime => _firingTime;

        protected override void OnEnabled()
        {
            base.OnEnabled();

            GameEvents.Register<Event_WeaponFire>(OnWeaponFire);
        }

        protected override void ConfigureStates()
        {
            StateEngine.AddState(
                new BossRunState(this),
                new BossFireState(this));
        }

        protected override void SetFirstState()
            => StateEngine.SwitchState<BossRunState>();

        private void OnWeaponFire(Event_WeaponFire obj)
        {
            if (obj.Carrier != this)
                return;
            Animator.SetTrigger(ShootTrigger);
        }

        private class BossFireState : IState
        {
            private readonly Boss _boss;
            private float _currentFiringTime;

            public BossFireState(Boss boss)
                => _boss = boss;

            public void Enter()
            {
                if (_boss.Animator != null)
                    _boss.Animator.SetTrigger(FireTrigger);
                _currentFiringTime = _boss.FiringTime;
                _boss.transform.LookAt(_boss.Player.transform.position);
                _boss.CurrentWeapon.SetFireState(true);
            }

            public void Run()
            {
                if (_boss.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 || _boss.Animator.IsInTransition(0))
                    return;

                _boss.CurrentWeapon.Fire();
                _currentFiringTime -= Time.deltaTime;

                if (_currentFiringTime > 0)
                    return;

                if (_boss.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !_boss.Animator.IsInTransition(0))
                    _boss.StateEngine.SwitchState<BossRunState>();
            }

            public void Exit() { }
        }

        private class BossRunState : EnemyRunState
        {
            private readonly Boss _boss;

            public BossRunState(IRunStateReady character) : base(character)
                => _boss = (Boss)character;

            protected override bool OnPathPointStop()
            {
                _boss.StateEngine.SwitchState<BossFireState>();
                return true;
            }
        }
    }
}