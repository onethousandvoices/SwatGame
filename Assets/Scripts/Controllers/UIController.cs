using NTC.Global.Cache;
using NTC.Global.Pool;
using SWAT;
using SWAT.Events;
using System.Collections.Generic;
using UnityEngine;

namespace Controllers
{
    public class UIController : MonoCache
    {
        [SerializeField] private Hud _hudPrefab;
        [SerializeField] private GameObject _hudHolder;

        private readonly Dictionary<Enemy, Hud> _enemiesHud = new Dictionary<Enemy, Hud>();
        
        protected override void OnEnabled()
        {
            GameEvents.Register<EnemiesSpawnedEvent>(OnEnemiesSpawned);
        }

        private void OnEnemiesSpawned(EnemiesSpawnedEvent obj)
        {
            return;
            _enemiesHud.Clear();
            
            foreach (Enemy enemy in obj.Enemies)
            {
                // Hud newHud = NightPool.Spawn(_hudPrefab, enemy.BarHolder.transform);
                // enemy.SetHud(newHud);
                // _enemiesHud.Add(enemy, newHud);
            }
        }
    }
}