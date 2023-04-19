using NTC.Global.Cache;
using SWAT.Events;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers
{
    public class StartMenuController : MonoCache
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _quitButton;

        private TutorialController _tutorial;

        protected override void OnEnabled()
        {
            _tutorial = ObjectHolder.GetObject<TutorialController>();

            _startButton.onClick.AddListener(() =>
            {
                GameEvents.Call(new Event_GameStart());
                gameObject.SetActive(false);
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

        protected override void OnDisabled()
        {
            _startButton.onClick.RemoveAllListeners();
            _quitButton.onClick.RemoveAllListeners();
        }
    }
}