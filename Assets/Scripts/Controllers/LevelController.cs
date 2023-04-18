using NTC.Global.Pool;
using SWAT;
using SWAT.Events;
using SWAT.LevelScripts;
using System.Collections.Generic;
using UnityEngine;

namespace Controllers
{
    public interface IGetStageCount
    {
        public int Count { get; }
    }

    public class LevelController : IGetStageCount
    {
        private List<BaseCharacter> _stageCharacters;
        private List<Enemy> _enemiesToKill;
        private readonly Transform _charactersHolder;
        private readonly Level _level;
        private int _index;
        private readonly bool _isDebug;

        public int Count => _level.Stages.Count;

        public LevelController(Level level, Transform charactersHolder, bool isDebug)
        {
            _level = level;
            _charactersHolder = charactersHolder;
            _isDebug = isDebug;
        }

        public void Init()
        {
            _index = 0;

            SpawnStageEnemies();

            GameEvents.Register<Event_CharacterKilled>(OnEnemyKilled);
            GameEvents.Register<Event_PlayerKilled>(OnPlayerKilled);
            GameEvents.Register<Event_PlayerChangedPosition>(OnPlayerMoved);
        }

        private void SpawnStageEnemies()
        {
            _stageCharacters ??= new List<BaseCharacter>();
            _enemiesToKill ??= new List<Enemy>();
            _stageCharacters.Clear();
            _enemiesToKill.Clear();

            if (_index >= _level.Stages.Count)
            {
                GameEvents.Call(new Event_LevelCompleted());
                return;
            }

            foreach (CharacterPathPair path in _level.Stages[_index].CharacterPathPairs)
            {
                if (_isDebug)
                    break;
                BaseCharacter character = NightPool.Spawn(path.Character, path.Path.Start.transform.position + new Vector3(0f, 0.4f, 0f));
                character.transform.parent = _charactersHolder;

                switch (character)
                {
                    case Enemy enemy:
                        enemy.SetPositions(path.Path);
                        _enemiesToKill.Add(enemy);
                        break;
                    case Civilian civ:
                        civ.SetPositions(path.Path);
                        break;
                }

                _stageCharacters.Add(character);
            }

            GameEvents.Call(new Event_CharactersSpawned(_stageCharacters.ToArray()));
        }

        private void OnPlayerMoved(Event_PlayerChangedPosition obj)
        {
            _index++;
            SpawnStageEnemies();
        }

        private void OnPlayerKilled(Event_PlayerKilled obj)
        {
            Debug.LogError("player is dead...");
        }

        private void OnEnemyKilled(Event_CharacterKilled killed)
        {
            if (killed.Character is not Enemy enemy)
                return;
            if (!_enemiesToKill.Contains(enemy))
                return;
            
            _enemiesToKill.Remove(enemy);

            if (_enemiesToKill.Count > 0)
                return;
            GameEvents.Call(new Event_StageEnemiesDead());
        }
    }
}