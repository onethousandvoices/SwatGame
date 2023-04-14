using NTC.Global.Cache;
using SWAT.Behaviour;
using SWAT.LevelScripts.Navigation;
using SWAT.Weapons;
using UnityEngine;

namespace SWAT
{
    public enum CharacterType : byte
    {
        None,
        Player,
        Enemy,
        Boss
    }

    public interface IRunStateReady
    {
        public StateEngine StateEngine { get; }
        public Path Path { get; }
        public Transform Transform { get; }
        public Rigidbody Rb { get; }
        public int Speed { get; }
    }

    public abstract class BaseCharacter : MonoCache, IRunStateReady
    {
        [SerializeField] protected Weapon CurrentWeapon;
        [SerializeField] protected Animator Animator;

        public abstract CharacterType Type { get; }

        protected abstract int BaseMaxArmour { get; }
        protected abstract int BaseMaxHealth { get; }

        protected bool IsVulnerable;
        protected int CurrentHealth;
        protected int CurrentArmour;

        private Hud _hud;
        private IRunStateReady _runStateReadyImplementation;

        public StateEngine StateEngine { get; private set; }
        public Path Path { get; protected set; }
        public Transform Transform { get; private set; }
        public Rigidbody Rb { get; private set; }
        public int Speed { get; protected set; }

        protected override void OnEnabled()
        {
            Rb = Get<Rigidbody>();
            StateEngine = new StateEngine();
            Transform = transform;
        }

        protected override void Run()
        {
            StateEngine.CurrentState?.Run();
        }

        public void DoDamage(int damage, Vector3 position)
        {
            if (IsVulnerable == false)
                return;

            if (CurrentArmour > 0)
            {
                float armourBeforeHit = (float)CurrentArmour / BaseMaxArmour;
                CurrentArmour -= damage;
                float armourAfterHit = (float)CurrentArmour / BaseMaxArmour;
                DamageArmour(armourBeforeHit - armourAfterHit);

                if (CurrentArmour >= 0)
                    return;

                float healthBeforeHit = (float)CurrentHealth / BaseMaxHealth;
                CurrentHealth -= Mathf.Abs(CurrentArmour);
                float healthAfterHit = (float)CurrentHealth / BaseMaxHealth;
                DamageHealth(healthBeforeHit - healthAfterHit);
            }

            if (CurrentHealth > 0)
            {
                float healthBeforeHit = (float)CurrentHealth / BaseMaxHealth;
                CurrentHealth -= damage;
                float healthAfterHit = (float)CurrentHealth / BaseMaxHealth;
                DamageHealth(healthBeforeHit - healthAfterHit);
            }

            if (CurrentHealth <= 0)
            {
                Dead(position);
            }
        }

        private void DamageArmour(float damage)
        {
            if (_hud == null)
                return;
            _hud.DamageArmour(damage);
        }

        private void DamageHealth(float damage)
        {
            if (_hud == null)
                return;
            _hud.DamageHealth(damage);
        }

        protected void SetHud(Hud hud) => _hud = hud;

        protected abstract void Dead(Vector3 hitPosition);
    }
}