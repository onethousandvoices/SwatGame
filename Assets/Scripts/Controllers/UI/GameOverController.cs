using NTC.Global.Cache;
using SWAT.Events;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Controllers
{
    public class GameOverController : MonoCache
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private TextMeshProUGUI _gameOverText;
        
        private static readonly int GoodEnd = Animator.StringToHash("GoodEnd");
        private static readonly int BadEnd = Animator.StringToHash("BadEnd");

        protected override void OnEnabled()
        {
            GameEvents.Register<Event_GameOver>(OnGameOver);
        }

        private void OnGameOver(Event_GameOver obj)
        {
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

            StartCoroutine(GameOverTimeFade());
        }

        private static IEnumerator GameOverTimeFade()
        {
            float t = 0f;
            const float target = 0f;
            float start = Time.timeScale;
            
            while (t < 1f)
            {
                Time.timeScale = Mathf.Lerp(start, target, t);
                t += Time.deltaTime / 3f;
                yield return null;
            }
        }
    }
}