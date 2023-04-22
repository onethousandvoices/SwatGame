using Controllers;
using NaughtyAttributes;
using NTC.Global.Cache;
using NTC.Global.Pool;
using SWAT.Behaviour;
using SWAT.Events;
using SWAT.LevelScripts.Navigation;
using SWAT.Weapons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SWAT
{
    public enum CharacterType : byte
    {
        None,
        Player,
        Enemy,
        PeaceMan,
        Boss
    }

    public interface IRunStateReady
    {
        public Animator Animator { get; }
        public Path Path { get; }
        public Transform Transform { get; }
        public Rigidbody Rb { get; }
        public int Speed { get; }
    }

    public abstract class BaseCharacter : MonoCache, IRunStateReady
    {
        [field: SerializeField] public Path Path { get; protected set; }
        [HideIf("IsPeaceMan")]
        [SerializeField] protected Weapon CurrentWeapon;
        [field: SerializeField] public Animator Animator { get; protected set; }
        [SerializeField] protected Hud Hud;
        [HorizontalLine(color: EColor.Red)]
        [SerializeField] protected HitBox[] HitBoxes;
        [SerializeField] protected Rigidbody[] RagdollRbs;
        [SerializeField] protected Collider[] RagdollColliders;

        protected StateEngine StateEngine;

        protected bool IsVulnerable;
        protected int CurrentHealth;
        protected int CurrentArmour;

        public Transform Transform { get; private set; }
        public Rigidbody Rb { get; private set; }

        public abstract CharacterType Type { get; }

        public int Speed { get; protected set; }

        protected abstract int BaseMaxArmour { get; }
        protected abstract int BaseMaxHealth { get; }

        protected Action OnDamageTaken;

        private bool IsPeaceMan => Type == CharacterType.PeaceMan;

        protected override void OnEnabled()
        {
            Rb = Get<Rigidbody>();
            StateEngine = new StateEngine();
            Transform = transform;
            
            SetPhysicsState(true);

            GameEvents.Register<Event_GameOver>(OnGameOver);
            GameEvents.Register<Event_GameStart>(OnGameStart);
        }

        protected virtual void OnGameOver(Event_GameOver obj) { }

        protected virtual void OnGameStart(Event_GameStart obj)
        {
            if (CurrentWeapon != null)
                CurrentWeapon.Reload();

            if (Path != null)
                Path.ResetPath();
        }

        protected override void Run()
            => StateEngine.CurrentState?.Run();

        public void DoDamage(int damage, Vector3 position)
        {
            if (IsVulnerable == false || CurrentHealth <= 0)
                return;

            OnDamageTaken?.Invoke();

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
                Dead(position);
        }

        private void DamageArmour(float damage)
        {
            if (Hud == null)
                return;
            Hud.DamageArmour(damage);
        }

        private void DamageHealth(float damage)
        {
            if (Hud == null)
                return;
            Hud.DamageHealth(damage);
        }

        protected void SetPhysicsState(bool state)
        {
            Rb.isKinematic = !state;

            if (Animator != null)
                Animator.enabled = state;

            foreach (Rigidbody rb in RagdollRbs)
                rb.isKinematic = state;
            foreach (Collider col in RagdollColliders)
                col.isTrigger = state;
            foreach (HitBox box in HitBoxes)
                box.Collider.enabled = state;
        }

        protected virtual void Dead(Vector3 hitPosition)
        {
            GameEvents.Call(new Event_CharacterKilled(this));
            transform.parent = null;

            StateEngine.Stop();
            SetPhysicsState(false);

            Collider[] colliders = new Collider[5];
            Physics.OverlapSphereNonAlloc(hitPosition, 3f, colliders);

            foreach (Collider col in colliders)
                if (col != null && col.attachedRigidbody != null)
                    col.attachedRigidbody.AddExplosionForce(2888f, hitPosition, 3f);

            StartCoroutine(DeathRoutine());
        }

        [Button("Kill")]
        private void Kill()
            => Dead(transform.position);

        [Button("Update Params")]
        private void UpdateStats()
            => FindObjectOfType<GameController>().ConfigObject(this);

        private IEnumerator DeathRoutine()
        {
            yield return new WaitForSeconds(3f);

            foreach (Rigidbody rb in RagdollRbs)
                rb.isKinematic = true;
            foreach (Collider ragdollCollider in RagdollColliders)
                ragdollCollider.isTrigger = true;

            yield return new WaitForSeconds(4f);

            float t = 0f;
            while (t < 1f)
            {
                transform.position = Vector3.Lerp(transform.position, transform.position - new Vector3(0f, 5f, 0f), t);
                yield return null;
                t += Time.deltaTime;
            }

            NightPool.Despawn(this);
        }

        [Button("Config Ragdoll")]
        private void ConfigRagdoll()
        {
            FindRbs();
            FindRagdollColliders();
            FindHitBoxes();
        }

        private void FindRbs()
        {
            RagdollRbs = GetComponentsInChildren<Rigidbody>();
            RagdollRbs = RagdollRbs.Where(x => x != Get<Rigidbody>()).ToArray();
        }

        private void FindRagdollColliders()
        {
            RagdollColliders = GetComponentsInChildren<Collider>();

            List<Collider> ragdollColliders = new List<Collider>(RagdollColliders);
            List<Collider> collidersToRemove = new List<Collider>();

            foreach (Collider col in RagdollColliders)
                if (col.gameObject.TryGetComponent(out HitBox box))
                    collidersToRemove.Add(col);

            foreach (Collider col in collidersToRemove)
                ragdollColliders.Remove(col);

            RagdollColliders = ragdollColliders.ToArray();
        }

        private void FindHitBoxes()
            => HitBoxes = GetComponentsInChildren<HitBox>();
    }
}