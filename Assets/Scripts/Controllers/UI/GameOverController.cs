using NTC.Global.Cache;
using SWAT.Events;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers
{
    public class GameOverController : MonoCache
    {
        [SerializeField] private Button _restartButton;
        [SerializeField] private Animator _animator;
        [SerializeField] private TextMeshProUGUI _gameOverText;

        private static readonly int GoodEnd = Animator.StringToHash("GoodEnd");
        private static readonly int BadEnd = Animator.StringToHash("BadEnd");

        protected override void OnEnabled()
        {
            GameEvents.Register<Event_GameOver>(OnGameOver);
            
            _restartButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
                GameEvents.Call(new Event_GameStart());
            });
        }

        private void OnGameOver(Event_GameOver obj)
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
            
            StartCoroutine(GameOverTimeFade(obj));
        }

        private IEnumerator GameOverTimeFade(Event_GameOver obj)
        {
            float t = 0f;
            const float target = 0.1f;
            float start = Time.timeScale;

            while (t < 1f)
            {
                Time.timeScale = Mathf.Lerp(start, target, t);
                t += Time.unscaledDeltaTime / 3f;
                yield return null;
            }
            
            _gameOverText.text = obj.Reason + "...";
            switch (obj.IsGoodEnd)
            {
                case true:
                    _animator.SetTrigger(GoodEnd);
                    break;
                case false:
                    _animator.SetTrigger(BadEnd);
                    break;
            }
        }

        protected override void OnDisabled() 
            => _restartButton.onClick.RemoveAllListeners();
    }
}