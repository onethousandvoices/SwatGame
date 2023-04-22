using Controllers;
using NaughtyAttributes;
using SWAT.Behaviour;
using SWAT.Events;
using SWAT.Utility;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;

namespace SWAT
{
    public class Player : BaseCharacter
    {
        [field: Config(Extras.Player, "A2")] protected override int BaseMaxArmour { get; }
        [field: Config(Extras.Player, "A1")] protected override int BaseMaxHealth { get; }
        [Config(Extras.Player, "A3")] private int _speed;

        [Config(Extras.PlayerWeapon, "A1")] private int _projectileDamage;
        [Config(Extras.PlayerWeapon, "A2")] private int _firingRate;
        [Config(Extras.PlayerWeapon, "A3")] private int _clipSize;
        [Config(Extras.PlayerWeapon, "A4")] private int _reloadTime;
        [Config(Extras.PlayerWeapon, "A5")] private int _totalAmmo;

        [SerializeField] private RotationConstraint _rotationConstraint;
        [field: SerializeField] public HitPointsHolder HitPointsHolder { get; private set; }

        private static readonly int _sitTrigger = Animator.StringToHash("Sit");
        private static readonly int _standUpTrigger = Animator.StringToHash("StandUp");
        private static readonly int _standUpRunTrigger = Animator.StringToHash("StandUpRun");
        private static readonly int _runTrigger = Animator.StringToHash("Run");
        private static readonly int _firingTrigger = Animator.StringToHash("Firing");
        private static readonly int _gameOver = Animator.StringToHash("GameOver");

        private Crosshair _crosshair;
        private TutorialController _tutorialController;
        private static readonly int GameStart = Animator.StringToHash("GameStart");
        private bool _isGameOver;

        public override CharacterType Type => CharacterType.Player;

        protected override void OnEnabled()
        {
            base.OnEnabled();

            Hud.PlayerCarry();

            CurrentHealth = BaseMaxHealth;
            CurrentArmour = BaseMaxArmour;

            Speed = _speed;

            CurrentWeapon.Configure(
                _projectileDamage,
                _firingRate,
                _clipSize,
                _reloadTime,
                _totalAmmo);

            _crosshair = ObjectHolder.GetObject<Crosshair>();
            _tutorialController = ObjectHolder.GetObject<TutorialController>();

            StateEngine.AddState(
                new PlayerFiringState(this),
                new PreIdleState(this),
                new PreReloadingState(this),
                new ReloadingState(this),
                new IdleState(this),
                new PlayerRunState(this));

            GameEvents.Register<Event_StageEnemiesDead>(OnStageEnemiesDeath);
            GameEvents.Register<Event_WeaponFire>(OnWeaponFire);
        }

        private void OnWeaponFire(Event_WeaponFire obj)
        {
            if (obj.Carrier != this)
                return;
            _crosshair.SetCrosshairProgression(obj.ClipSizeNormalized);
        }

        private async void OnStageEnemiesDeath(Event_StageEnemiesDead obj)
        {
            await Task.Delay(1000);
            StateEngine.SwitchState<PlayerRunState>();
        }

        private void SitDown()
        {
            CurrentWeapon.SetFireState(false);
            _rotationConstraint.constraintActive = false;
            _rotationConstraint.weight = 0f;
            Animator.SetTrigger(_sitTrigger);
        }

        public void UnityEvent_FirePoseAnimationEnd()
        {
            if (CurrentWeapon.FireState || StateEngine.CurrentState.GetType() == typeof(IdleState))
                return;

            CurrentWeapon.SetFireState(true);
            _rotationConstraint.constraintActive = true;
        }

        protected override void Dead(Vector3 hitPosition)
        {
            base.Dead(hitPosition);
            _rotationConstraint.constraintActive = false;
            GameEvents.Call(new Event_GameOver("Dead", false));
        }

        protected override void OnGameOver(Event_GameOver obj)
        {
            _isGameOver = true;
            Hud.gameObject.SetActive(false);
            Animator.SetTrigger(_gameOver);
        }

        protected override void OnGameStart(Event_GameStart obj)
        {
            base.OnGameStart(obj);
            _isGameOver = false;
            Hud.gameObject.SetActive(true);
            StateEngine.SwitchState<IdleState>();
            Animator.SetTrigger(GameStart);
            CurrentWeapon.Reload();
        }

        [Button("Run")]
        private void SetStateRun()
            => StateEngine.SwitchState<PlayerRunState>();

#region States
        private class PlayerRunState : RunState
        {
            private readonly Player _player;

            public PlayerRunState(IRunStateReady character) : base(character)
                => _player = (Player)character;

            public override void Enter()
            {
                base.Enter();
                _player.Animator.SetTrigger(_standUpRunTrigger);
                _player.Animator.SetTrigger(_runTrigger);
                TargetPathPoint = _player.Path.GetPoint();
                _player._rotationConstraint.weight = 0f;
                _player._rotationConstraint.constraintActive = false;
                _player.CurrentWeapon.RiseUp();

                GameEvents.Call(new Event_PlayerRunStarted());
            }

            protected override bool OnPathPointStop()
            {
                _player.StateEngine.SwitchState<IdleState>();
                return true;
            }

            public override void Exit()
            {
                _player.transform.rotation = TargetPathPoint.transform.rotation;
                _player.CurrentWeapon.Lower();
                GameEvents.Call(new Event_PlayerChangedPosition());
            }
        }

        private class PlayerFiringState : IState
        {
            private readonly Player _player;

            public PlayerFiringState(Player player)
                => _player = player;

            public void Enter()
            {
                _player.Animator.SetTrigger(_standUpTrigger);
                _player.Animator.SetTrigger(_firingTrigger);
                _player.IsVulnerable = true;
                _player._rotationConstraint.weight = 1f;
            }

            public void Run()
            {
                if (Input.GetMouseButton(0))
                    _player.CurrentWeapon.Fire();

                if (Input.GetMouseButtonUp(0))
                {
                    _player.CurrentWeapon.ResetFiringRate();
                    _player.StateEngine.SwitchState<PreIdleState>();
                }

                if (_player.CurrentWeapon.ClipIsEmpty)
                    _player.StateEngine.SwitchState<PreReloadingState>();
            }

            public void Exit()
                => _player.IsVulnerable = false;
        }

        private class PreReloadingState : IState
        {
            private readonly Player _player;

            public PreReloadingState(Player player)
                => _player = player;

            public void Enter()
                => _player.CurrentWeapon.SetFireState(false);

            public void Run()
            {
                _player._rotationConstraint.weight -= Time.deltaTime * 2f;

                if (_player._rotationConstraint.weight > 0.3f)
                    return;

                _player.StateEngine.SwitchState<ReloadingState>();
            }

            public void Exit()
                => _player.SitDown();
        }

        private class PreIdleState : IState
        {
            private readonly Player _player;

            public PreIdleState(Player player)
                => _player = player;

            public void Enter()
                => _player.CurrentWeapon.SetFireState(false);

            public void Run()
            {
                _player._rotationConstraint.weight -= Time.deltaTime * 2f;

                if (_player._rotationConstraint.weight > 0.3f)
                    return;

                _player.StateEngine.SwitchState<IdleState>();
            }

            public void Exit() { }
        }

        private class ReloadingState : IState
        {
            private readonly Player _player;

            private float _reloadingTimeNormalized;
            private float _currentReloadingTime;

            public ReloadingState(Player player)
                => _player = player;

            public void Enter()
            {
                _player._crosshair.EnableBar();
                _currentReloadingTime = _player.CurrentWeapon.ReloadTime;
                _reloadingTimeNormalized = 0f;
            }

            public void Run()
            {
                _currentReloadingTime -= Time.deltaTime;

                _reloadingTimeNormalized += Time.deltaTime;

                _player._crosshair.SetReloadProgression(_reloadingTimeNormalized / _player.CurrentWeapon.ReloadTime);

                if (_currentReloadingTime <= 0)
                    _player.StateEngine.SwitchState<IdleState>();
            }

            public void Exit()
            {
                _player._crosshair.ReloadReady();
                _player.CurrentWeapon.Reload();
            }
        }

        private class IdleState : IState
        {
            private readonly Player _player;

            public IdleState(Player player)
                => _player = player;

            public void Enter()
                => _player.SitDown();

            public void Run()
            {
                if (!Input.GetMouseButtonDown(0) || !_player._tutorialController.IsInputAllowed || _player._isGameOver)
                    return;
                if (_player._tutorialController.IsCivilianStage)
                {
                    _player._tutorialController.InputAfterCivilianMet();
                    return;
                }
                
                _player.StateEngine.SwitchState<PlayerFiringState>();
            }

            public void Exit() { }
        }
#endregion
    }
}