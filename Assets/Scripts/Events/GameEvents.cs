using System;
using System.Collections.Generic;
using UnityEngine;

namespace SWAT.Events
{
    public static class GameEvents
    {
        private static readonly Dictionary<Type, List<Delegate>> _events = new Dictionary<Type, List<Delegate>>();

        public static void Register<T>(Action<T> callback)
        {
            if (!_events.ContainsKey(typeof(T)))
                _events.Add(typeof(T), new List<Delegate>());

            _events[typeof(T)].Add(callback);
        }

        public static void Unregister<T>(Action<T> callback)
        {
            if (_events.ContainsKey(typeof(T)))
            {
                _events[typeof(T)].Remove(callback);

                if (_events[typeof(T)].Count == 0)
                    _events.Remove(typeof(T));
            }
            else
                Debug.LogError($"Event type of {typeof(T)} not registered");
        }

        public static void Call<T>(T args)
        {
            if (_events.ContainsKey(typeof(T)))
            {
                List<Delegate> callbacks = _events[typeof(T)];
                for (int i = 0; i < callbacks.Count; i++)
                {
                    if (callbacks[i] is Action<T> callback)
                        callback(args);
                }
            }
            else
                Debug.LogError($"Event type of {typeof(T)} not registered");
        }

        public static void UnregisterAll()
        {
            _events.Clear();
        }
    }

#region Events
    public class EnemiesSpawnedEvent
    {
        public Enemy[] Enemies { get; }

        public EnemiesSpawnedEvent(Enemy[] enemies)
        {
            Enemies = enemies;
        }
    }
    
    public class StageEnemiesDeadEvent { }
    
    public class PlayerChangedPositionEvent { }
    
    public class LevelCompletedEvent { }

    public class EnemyKilledEvent
    {
        public Enemy Enemy { get; }

        public EnemyKilledEvent(Enemy enemy)
        {
            Enemy = enemy;
        }
    }

    public class PlayerKilledEvent
    {
        public Player Player { get; }

        public PlayerKilledEvent(Player player)
        {
            Player = player;
        }
    }
#endregion
}