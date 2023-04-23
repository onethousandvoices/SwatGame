using SWAT.LevelScripts.Navigation;
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
            if (!_events.ContainsKey(typeof(T)))
                return;

            List<Delegate> callbacks = _events[typeof(T)];
            for (int i = 0; i < callbacks.Count; i++)
                if (callbacks[i] is Action<T> callback)
                    callback(args);
        }

        public static void UnregisterAll()
            => _events.Clear();
    }

#region Events
    public class Event_TEST_BossSpawn
    {
        public Boss Boss { get; }
        public Path BossPath { get; }

        public Event_TEST_BossSpawn(Boss boss, Path path)
        {
            Boss = boss;
            BossPath = path;
        }
    }

    public class Event_CivilianLookEnded { }

    public class Event_CrosshairMoved { }

    public class Event_BossOnSecondaryWeaponShot { }

    public class Event_CharactersSpawned
    {
        public BaseCharacter[] Characters { get; }

        public Event_CharactersSpawned(BaseCharacter[] characters)
            => Characters = characters;
    }

    public class Event_WeaponFire
    {
        public BaseCharacter Carrier { get; }
        public float ClipSizeNormalized { get; }

        public Event_WeaponFire(BaseCharacter carrier, float clipSizeNormalized)
        {
            Carrier = carrier;
            ClipSizeNormalized = clipSizeNormalized;
        }
    }

    public class Event_KillStageEnemies { }

    public class Event_StageEnemiesDead { }

    public class Event_PlayerChangedPosition { }

    public class Event_PlayerRunStarted { }

    public class Event_CharacterKilled
    {
        public BaseCharacter Character { get; }

        public Event_CharacterKilled(BaseCharacter character)
            => Character = character;
    }

    public class Event_GameStart { }

    public class Event_GameOver
    {
        public string Reason { get; }
        public bool IsGoodEnd { get; }

        public Event_GameOver(string reason, bool isGoodEnd)
        {
            Reason = reason;
            IsGoodEnd = isGoodEnd;
        }
    }
#endregion
}