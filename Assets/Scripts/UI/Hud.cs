using Controllers;
using NaughtyAttributes;
using NTC.Global.Cache;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SWAT
{
    public class Hud : MonoCache
    {
        [field: SerializeField, HorizontalLine(color: EColor.Red)] public RectTransform Rect { get; private set; }
        [HorizontalLine(color: EColor.Red)]
        [ProgressBar("Timer", _baseTimerValue, EColor.Red)]
        [SerializeField] private float _currentTimerValue;

        [HorizontalLine(color: EColor.Red)]
        [SerializeField] private GameObject _armourHolder;
        [SerializeField] private GameObject _hpHolder;
        [SerializeField] private Image _armourFill;
        [SerializeField] private Image _hpFill;
        [HorizontalLine(color: EColor.Red)]
        [SerializeField] private Image[] _allImages;

        private const float _speed = 5f;
        private const float _baseTimerValue = 1.5f;

        private Coroutine _fadeRoutine;
        private bool _playerIsCarrier;

        [Button("Show")]
        private void Show()
        {
            gameObject.SetActive(true);

            if (_fadeRoutine != null)
                StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(Fade(1f));
        }

        public void Hide(Action callback = null)
        {
            gameObject.SetActive(true);

            if (_fadeRoutine != null)
                StopCoroutine(_fadeRoutine);

            _fadeRoutine = StartCoroutine(Fade(0f, callback));
        }

        public void DamageArmour(float value)
        {
            if (gameObject.activeSelf == false)
                Show();

            if (_armourHolder.activeSelf == false)
            {
                _armourHolder.SetActive(true);
                _hpHolder.SetActive(false);
            }

            _armourFill.fillAmount -= value;

            if (_armourFill.fillAmount == 0)
            {
                if (!_playerIsCarrier)
                    _armourHolder.SetActive(false);
                _hpHolder.SetActive(true);
            }

            ResetTimer();
        }

        public void DamageHealth(float value)
        {
            if (gameObject.activeSelf == false)
                Show();

            if (_hpHolder.activeSelf == false)
            {
                _hpHolder.SetActive(true);
                if (!_playerIsCarrier)
                    _armourHolder.SetActive(false);
            }

            _hpFill.fillAmount -= value;

            if (_hpFill.fillAmount == 0)
            {
                _hpHolder.SetActive(false);
                Hide();
            }

            ResetTimer();
        }

        protected override void Run()
        {
            if (_fadeRoutine != null || _playerIsCarrier)
                return;

            _currentTimerValue -= Time.deltaTime;

            if (_currentTimerValue > 0)
                return;

            Hide();
        }

        private IEnumerator Fade(float to, Action callback = null)
        {
            float t = 0f;
            Color start = _allImages[0].color;
            Color target = start;
            target.a = to;

            if (_playerIsCarrier)
            {
                SetTargetColor();
                yield break;
            }

            while (t < 1f)
            {
                foreach (Image image in _allImages)
                {
                    image.color = Color.Lerp(start, target, t);
                    yield return null;
                    t += Time.deltaTime * _speed;
                }
            }

            SetTargetColor();
            callback?.Invoke();

            void SetTargetColor()
            {
                foreach (Image image in _allImages)
                    image.color = target;

                ResetTimer();
                _fadeRoutine = null;

                if (to == 0 && _playerIsCarrier == false)
                    gameObject.SetActive(false);
            }
        }

        private void ResetTimer()
        {
            _currentTimerValue = _baseTimerValue;
        }

        [Button("Editor_Armour_Damage")]
        private void DamageArmour1()
        {
            DamageArmour(0.1f);
        }

        [Button("Editor_Hide")]
        private void TurnAllOff()
        {
            Color target = _allImages[0].color;
            target.a = 0f;

            foreach (Image image in _allImages)
                image.color = target;
        }

        [Button("Editor_Show")]
        private void TurnAllOn()
        {
            Color target = _allImages[0].color;
            target.a = 1f;

            foreach (Image image in _allImages)
                image.color = target;
        }

        public void PlayerCarry() => _playerIsCarrier = true;

        public void OnSpawn()
        {
            gameObject.SetActive(false);
            _hpFill.fillAmount = 1;
            _armourFill.fillAmount = 1;
        }
    }
}