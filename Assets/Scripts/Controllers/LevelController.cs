using NTC.Global.Pool;
using SWAT;
using SWAT.Events;
using SWAT.LevelScripts;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Controllers
{
    public class LevelController
    {
        private List<Enemy> _enemiesToKill;
        private readonly Level _level;

        private int _index;
        
        public LevelController(Level level)
        {
            _level = level;
            Init();
        }

        private void Init()
        {
            _index = 0;

            SpawnStageEnemies();
            
            GameEvents.Register<EnemyKilledEvent>(OnEnemyKilled);
            GameEvents.Register<PlayerKilledEvent>(OnPlayerKilled);
            GameEvents.Register<PlayerChangedPositionEvent>(OnPlayerMoved);
        }

        private void SpawnStageEnemies()
        {
            _enemiesToKill ??= new List<Enemy>();
            _enemiesToKill.Clear();

            if (_index >= _level.Stages.Count)
            {
                GameEvents.Call(new LevelCompletedEvent());
                return;
            }
            
            foreach (EnemyPath path in _level.Stages[_index].Enemies)
            {
                break;
                Enemy enemy = NightPool.Spawn(path.Enemy, path.Path.Start.transform.position + new Vector3(0f, 0.4f, 0f));
                enemy.SetPositions(path.Path);
                
                _enemiesToKill.Add(enemy);
            }
            
            GameEvents.Call(new EnemiesSpawnedEvent(_enemiesToKill.ToArray()));
        }

        private void OnPlayerMoved(PlayerChangedPositionEvent obj)
        {
            _index++;
            SpawnStageEnemies();
        }

        private void OnPlayerKilled(PlayerKilledEvent obj)
        {
            Debug.LogError("player is dead...");
        }

        private void OnEnemyKilled(EnemyKilledEvent killedEvent)
        {
            if (_enemiesToKill.Contains(killedEvent.Enemy) == false) return;
            _enemiesToKill.Remove(killedEvent.Enemy);
            
            if (_enemiesToKill.Count > 0) return;
            GameEvents.Call(new StageEnemiesDeadEvent());
        }
    }
}