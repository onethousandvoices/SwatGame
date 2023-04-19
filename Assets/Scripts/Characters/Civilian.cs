using Controllers;
using NTC.Global.Pool;
using SWAT.Behaviour;
using SWAT.Events;
using SWAT.LevelScripts.Navigation;
using SWAT.Utility;
using UnityEngine;

namespace SWAT
{
    public class Civilian : BaseCharacter, IPoolItem
    {
        [SerializeField, Config(Extras.PeaceMan, "A1")] private int _maxHealth;
        [SerializeField, Config(Extras.PeaceMan, "A2")] private int _maxArmour;
        [SerializeField, Config(Extras.PeaceMan, "A3")] private int _speed;

        private Camera _camera;
        
        public override CharacterType Type => CharacterType.PeaceMan;

        protected override int BaseMaxArmour => _maxArmour;
        protected override int BaseMaxHealth => _maxHealth;

        private static readonly int RunTrigger = Animator.StringToHash("Run");

        protected override void OnEnabled()
        {
            base.OnEnabled();

            SetPhysicsState(true);
            IsVulnerable = true;

            _camera = ObjectHolder.GetObject<Camera>();
            
            CurrentArmour = BaseMaxArmour;
            CurrentHealth = BaseMaxHealth;
            Speed = _speed;

            OnDamageTaken += DamageTaken;

            StateEngine.AddState(
                new PeaceManRunState(this),
                new IdleState(this));

            StateEngine.SwitchState<IdleState>();
        }

        private void DamageTaken()
        {
            StateEngine.SwitchState<PeaceManRunState>();
        }

        public void SetPositions(Path path)
        {
            Path = path;
        }

        protected override void Dead(Vector3 hitPosition)
        {
            base.Dead(hitPosition);
            GameEvents.Call(new Event_GameOver("Civilian is dead", false));
        }
        
        protected override void LateRun() => Hud.transform.parent.transform.LookAt(_camera.transform);

        public void OnSpawn()
        {
            Hud.transform.parent.gameObject.SetActive(true);
            Hud.OnSpawn();
        }

        public void OnDespawn() => Hud.transform.parent.gameObject.SetActive(false);

#region States
        private class PeaceManRunState : RunState
        {
            private readonly Civilian _civilian;
            public PeaceManRunState(IRunStateReady character) : base(character) => _civilian = (Civilian)character;

            public override void Enter()
            {
                base.Enter();
                _civilian.Animator.SetTrigger(RunTrigger);
            }

            public override void Exit() { }
        }

        private class IdleState : IState
        {
            private readonly Civilian _character;
            public IdleState(Civilian character) => _character = character;

            public void Enter() { }

            public void Run() { }

            public void Exit() { }
        }
#endregion
    }
}