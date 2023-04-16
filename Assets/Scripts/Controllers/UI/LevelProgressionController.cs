using NaughtyAttributes;
using NTC.Global.Cache;
using SWAT;
using SWAT.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers
{
    public class LevelProgressionController : MonoCache
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private Checkpoint _checkpointPrefab;
        [SerializeField] private RectTransform _lowerPanel;
        [SerializeField] private RectTransform _lowerPanelBg;
        [SerializeField] private ProgressBarCharacter _charactersImage;
        [SerializeField] private Animator _animator;

        private Checkpoint[] _checkpoints;
        private ProgressBarCharacter[] _progressBarCharacters;
        private Dictionary<BaseCharacter, ProgressBarCharacter> _characterPairs;
        private int _checkpointIndex;
        private int _stagesCount;
        private float _step;

        private static readonly int Show = Animator.StringToHash("Show");
        private static readonly int ShowLower = Animator.StringToHash("ShowLower");
        private static readonly int HideLower = Animator.StringToHash("HideLower");

        protected override void OnEnabled()
        {
            _characterPairs = new Dictionary<BaseCharacter, ProgressBarCharacter>();

            IGetStageCount stages = ObjectHolder.GetObject<IGetStageCount>();
            _stagesCount = stages.Count;

            RectTransform rect = _slider.GetComponent<RectTransform>();

            _step = 100f / (_stagesCount - 1);

            float sliderHeight = rect.anchoredPosition.y;
            float sliderLeft = -rect.rect.width / 2;
            float singlePercent = rect.rect.width / 100;
            float currentPercent = 0f;

            _checkpoints = new Checkpoint[_stagesCount];

            for (int i = 0; i < _checkpoints.Length; i++)
            {
                Checkpoint point = Instantiate(_checkpointPrefab, transform);
                point.Rect.anchoredPosition = new Vector2(sliderLeft + singlePercent * currentPercent, sliderHeight);
                _checkpoints[i] = point;

                currentPercent += _step;
            }

            _checkpoints[0].SetState(CheckpointState.Current);

            GameEvents.Register<Event_CharactersSpawned>(ConfigureLowerBar);
            GameEvents.Register<Event_CharacterKilled>(OnCharacterKilled);
            GameEvents.Register<Event_StageEnemiesDead>(OnStageEnemiesDeath);

            _animator.SetTrigger(Show);
        }

        private void ConfigureLowerBar(Event_CharactersSpawned obj)
        {
            int count = obj.Characters.Length;
            float step = 100f / (count - 1);
            float width = _charactersImage.Image.rectTransform.rect.width * count;
            float leftSide = -width / 2;
            float singlePercent = width / 100;
            float currentPercent = 0f;

            if (_progressBarCharacters != null)
                foreach (ProgressBarCharacter progressBarCharacter in _progressBarCharacters)
                    Destroy(progressBarCharacter.gameObject);

            _progressBarCharacters = new ProgressBarCharacter[count];
            _characterPairs.Clear();

            for (int i = 0; i < count; i++)
            {
                ProgressBarCharacter progressBarCharacter = Instantiate(_charactersImage, _lowerPanelBg);
                progressBarCharacter.Image.rectTransform.anchoredPosition =
                    new Vector2(count < 2 ? leftSide + Mathf.Abs(leftSide) : leftSide + singlePercent * currentPercent, _lowerPanelBg.anchoredPosition.y);

                BaseCharacter spawnedCharacter = obj.Characters[i];

                switch (spawnedCharacter)
                {
                    case Enemy:
                        progressBarCharacter.SetType(ProgressCharacterType.Enemy);
                        break;
                    case Civilian:
                        progressBarCharacter.SetType(ProgressCharacterType.Civilian);
                        break;
                    case Boss:
                        progressBarCharacter.SetType(ProgressCharacterType.Boss);
                        break;
                }

                _characterPairs.Add(spawnedCharacter, progressBarCharacter);
                _progressBarCharacters[i] = progressBarCharacter;

                currentPercent += step;
            }

            Image first = _progressBarCharacters[0].Image;
            float panelLeftSide = first.rectTransform.anchoredPosition.x - first.rectTransform.rect.width;
            _lowerPanel.sizeDelta = new Vector2(Mathf.Abs(panelLeftSide) * 2f, _lowerPanel.sizeDelta.y);

            _animator.SetTrigger(ShowLower);
        }

        private void OnCharacterKilled(Event_CharacterKilled obj)
        {
            if (!_characterPairs.ContainsKey(obj.Character))
                return;
            _characterPairs[obj.Character].CrossOut();
        }

        private void OnStageEnemiesDeath(Event_StageEnemiesDead obj)
        {
            NextCheckpoint();
            _animator.SetTrigger(HideLower);
        }

        [Button("next")]
        private void NextCheckpoint()
        {
            _checkpoints[_checkpointIndex].SetState(CheckpointState.Completed);
            if (_checkpointIndex + 1 >= _stagesCount)
                return;
            _checkpointIndex++;

            StartCoroutine(LerpSlider(_slider.value + _step / 100));

            _checkpoints[_checkpointIndex].SetState(CheckpointState.Current);
        }

        private IEnumerator LerpSlider(float target)
        {
            float t = 0f;
            float start = _slider.value;

            while (t < 1f)
            {
                _slider.value = Mathf.Lerp(start, target, t);
                t += Time.deltaTime;
                yield return null;
            }
        }
    }
}