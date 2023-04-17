using Cinemachine;
using NTC.Global.Cache;
using SWAT;
using SWAT.Events;
using System.Linq;
using UnityEngine;

namespace Controllers
{
    public class TutorialController : MonoCache
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private CinemachineStateDrivenCamera _camera;

        private static readonly int MeetFiring = Animator.StringToHash("MeetFiring");
        private static readonly int StopMeetFiring = Animator.StringToHash("StopMeetFiring");
        private static readonly int MeetCivilian = Animator.StringToHash("MeetCivilian");
        private static readonly int StopMeetCivilian = Animator.StringToHash("StopMeetCivilian");

        private Player _player;
        public bool IsTutorialDriven { get; private set; }
        public bool IsInputAllowed { get; private set; }
        public bool IsTutorialDone { get; private set;}
        private bool _isCivilianStage;

        protected override void OnEnabled()
        {
            _player = ObjectHolder.GetObject<Player>();
            GameEvents.Register<Event_CrosshairMoved>(OnCrosshairMoved);
            GameEvents.Register<Event_CharactersSpawned>(OnCharacterSpawned);

            IsTutorialDriven = true;
            IsInputAllowed = false;
            IsTutorialDone = false;
            _isCivilianStage = false;
            _animator.SetTrigger(MeetFiring);
        }

        private void OnCharacterSpawned(Event_CharactersSpawned obj)
        {
            Civilian civilian = (Civilian)obj.Characters.FirstOrDefault(c => c is Civilian);
            if (civilian == null)
                return;

            Time.timeScale = 0f;
            _isCivilianStage = true;
            IsInputAllowed = false;
            _animator.SetTrigger(MeetCivilian);
            _camera.LiveChild.LookAt = civilian.transform;
            GameEvents.Unregister<Event_CharactersSpawned>(OnCharacterSpawned);
            GameEvents.Call(new Event_CivilianLook());
        }

        private void OnCrosshairMoved(Event_CrosshairMoved obj)
        {
            IsTutorialDriven = false;
            _animator.SetTrigger(StopMeetFiring);

            GameEvents.Unregister<Event_CrosshairMoved>(OnCrosshairMoved);
        }

        public void UnityEvent_AllowInput() => IsInputAllowed = true;

        public void InputAfterCivilianMet()
        {
            if (!_isCivilianStage) return;
            
            Time.timeScale = 1f;
            
            if (_camera.LiveChild.LookAt != _player.transform)
            {
                _camera.LiveChild.LookAt = _player.transform;
                _animator.SetTrigger(StopMeetCivilian);
            }

            IsTutorialDone = true;
        }
    }
}