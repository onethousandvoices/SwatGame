using Cinemachine;
using NTC.Global.Cache;
using SWAT;
using SWAT.Events;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Controllers
{
    public class TutorialController : MonoCache
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private CinemachineVirtualCamera _runCamera;

        private static readonly int MeetFiring = Animator.StringToHash("MeetFiring");
        private static readonly int StopMeetFiring = Animator.StringToHash("StopMeetFiring");
        private static readonly int MeetCivilian = Animator.StringToHash("MeetCivilian");
        private static readonly int StopMeetCivilian = Animator.StringToHash("StopMeetCivilian");

        private Player _player;
        private Crosshair _crosshair;
        private CinemachineComposer _composer;
        private Vector3 _baseOffset;
        public bool IsCivilianStage { get; private set; }
        public bool IsInputAllowed { get; private set; }
        public bool IsTutorialDone { get; private set; }

        protected override void OnEnabled()
        {
            _player = ObjectHolder.GetObject<Player>();
            _crosshair = ObjectHolder.GetObject<Crosshair>();

            //debug
            if (!ObjectHolder.GetObject<GameController>().IsTutorial)
            {
                IsInputAllowed = true;
                return;
            }

            GameEvents.Register<Event_GameStart>(OnGameStart);
            GameEvents.Register<Event_CrosshairMoved>(OnCrosshairMoved);
            GameEvents.Register<Event_CharactersSpawned>(OnCharacterSpawned);

            _composer = _runCamera.GetCinemachineComponent<CinemachineComposer>();
            _baseOffset = _composer.m_TrackedObjectOffset;
            IsCivilianStage = false;
            IsInputAllowed = false;
            IsTutorialDone = false;
        }

        private void OnGameStart(Event_GameStart obj)
        {
            if (IsTutorialDone)
                return;
            _animator.SetTrigger(MeetFiring);
        }

        private async void OnCharacterSpawned(Event_CharactersSpawned obj)
        {
            Civilian civilian = (Civilian)obj.Characters.FirstOrDefault(c => c is Civilian);
            if (civilian == null)
                return;

            Time.timeScale = 0f;
            IsCivilianStage = true;
            IsInputAllowed = false;
            _animator.SetTrigger(MeetCivilian);

            _runCamera.LookAt = civilian.transform;
            // _runCamera.Follow = civilian.transform;
            
            _composer.m_TrackedObjectOffset = new Vector3(_baseOffset.x, _baseOffset.y, 0);

            _runCamera.m_Lens.FieldOfView = 20;

            GameEvents.Unregister<Event_CharactersSpawned>(OnCharacterSpawned);
            await Task.Yield();
            _crosshair.gameObject.SetActive(false);
        }

        private void OnCrosshairMoved(Event_CrosshairMoved obj)
        {
            _animator.SetTrigger(StopMeetFiring);
            GameEvents.Unregister<Event_CrosshairMoved>(OnCrosshairMoved);
        }

        public void UnityEvent_AllowInput()
            => IsInputAllowed = true;

        public void InputAfterCivilianMet()
        {
            Time.timeScale = 1f;
            _runCamera.m_Lens.FieldOfView = 60;
            _composer.m_TrackedObjectOffset = _baseOffset;

            if (_runCamera.LookAt != _player.transform)
            {
                _runCamera.LookAt = _player.transform;
                // _runCamera.Follow = _player.transform;
                _animator.SetTrigger(StopMeetCivilian);
            }

            IsTutorialDone = true;
            IsCivilianStage = false;
            GameEvents.Call(new Event_CivilianLookEnded());
        }
    }
}