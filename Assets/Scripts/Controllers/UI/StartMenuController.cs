using NTC.Global.Cache;
using SWAT.Events;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers
{
    public class StartMenuController : MonoCache
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _quitButton;

        private TutorialController _tutorial;
        private static readonly int Hide = Animator.StringToHash("Hide");

        protected override void OnEnabled()
        {
            _tutorial = ObjectHolder.GetObject<TutorialController>();

            _startButton.onClick.AddListener(() =>
            {
                _animator.SetTrigger(Hide);
            });
            _quitButton.onClick.AddListener(() =>
            {
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
        }

        public void UnityEvent_FadeEnd()
        {
            GameEvents.Call(new Event_GameStart());
            gameObject.SetActive(false);
        }

        protected override void OnDisabled()
        {
            _startButton.onClick.RemoveAllListeners();
            _quitButton.onClick.RemoveAllListeners();
        }
    }
}