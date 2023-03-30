using NTC.Global.Pool;
using SWAT;
using SWAT.Events;
using SWAT.LevelScripts;
using System.Collections.Generic;
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
            // return;
            _index = 0;
            _enemiesToKill = new List<Enemy>();

            foreach (EnemyPath path in _level.Stages[_index].Enemies)
            {
                Enemy enemy = NightPool.Spawn(path.Enemy, path.Path.Start.transform.position + new Vector3(0f, 2f, 0f));
                enemy.SetPositions(path.Path);
                
                _enemiesToKill.Add(enemy);
            }
            
            GameEvents.Register<EnemyKilledEvent>(OnEnemyKilled);
            GameEvents.Register<PlayerKilledEvent>(OnPlayerKilled);
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
            Debug.LogError("change stage. all dead...");
            GameEvents.Call(new StageEnemiesDeadEvent());
        }
    }
}