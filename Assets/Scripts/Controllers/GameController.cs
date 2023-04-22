using NaughtyAttributes;
using NTC.Global.Cache;
using SWAT;
using SWAT.Events;
using SWAT.LevelScripts;
using SWAT.LevelScripts.Navigation;
using SWAT.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Controllers
{
    public class GameController : MonoCache
    {
        [SerializeField] private Transform _charactersHolder;
        [field: SerializeField, HideInInspector] public bool IsDebug { get; private set; }
        [field: SerializeField, HideInInspector] public bool IsTutorial { get; private set; }
        [SerializeField, HideInInspector] private bool _tutorialShown;
        [SerializeField, HideInInspector] private bool _debugMode;
#region CfgDictionaries
        [SerializeField, ShowIf("_showCfg")] private SerializableDictionary<string, int> _playerCfg = new SerializableDictionary<string, int>();
        [SerializeField, ShowIf("_showCfg")] private SerializableDictionary<string, int> _playerWeaponCfg = new SerializableDictionary<string, int>();
        [SerializeField, ShowIf("_showCfg")] private SerializableDictionary<string, int> _enemyThugCfg = new SerializableDictionary<string, int>();
        [SerializeField, ShowIf("_showCfg")] private SerializableDictionary<string, int> _enemyPistolCfg = new SerializableDictionary<string, int>();
        [SerializeField, ShowIf("_showCfg")] private SerializableDictionary<string, int> _enemySniperCfg = new SerializableDictionary<string, int>();
        [SerializeField, ShowIf("_showCfg")] private SerializableDictionary<string, int> _enemySniperRifleCfg = new SerializableDictionary<string, int>();
        [SerializeField, ShowIf("_showCfg")] private SerializableDictionary<string, int> _peaceManCfg = new SerializableDictionary<string, int>();
        [SerializeField, ShowIf("_showCfg")] private SerializableDictionary<string, int> _bossCfg = new SerializableDictionary<string, int>();
        [SerializeField, ShowIf("_showCfg")] private SerializableDictionary<string, int> _bossWeaponCfg = new SerializableDictionary<string, int>();
#endregion
        private LevelController _levelController;
        private TutorialController _tutorialController;

        [SerializeField] private Boss _boss;
        [SerializeField] private Path _bossPath;

        protected override void OnEnabled()
        {
            Application.targetFrameRate = 60;
            ConfigureObjects();
            GameEvents.Register<Event_GameStart>(OnGameStart);
        }

        private void OnGameStart(Event_GameStart obj)
        {
            Time.timeScale = 1f;
            if (IsDebug)
                GameEvents.Call(new Event_TEST_BossSpawn(_boss, _bossPath));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.UnregisterAll();
        }

        private void ParseConfig()
        {
            List<Dictionary<string, object>> config = CSVReader.Read("BasicEntityCfg");

            _playerCfg.Clear();
            _playerWeaponCfg.Clear();
            _enemyThugCfg.Clear();
            _enemyPistolCfg.Clear();
            _enemySniperCfg.Clear();
            _enemySniperRifleCfg.Clear();
            _peaceManCfg.Clear();
            _bossCfg.Clear();
            _bossWeaponCfg.Clear();

            for (int i = 0; i < config.Count; i++)
            {
                Dictionary<string, object> dict = config[i];
                string first = dict["ID"].ToString();

                foreach (string key in dict.Keys)
                {
                    if (key.Contains("ID"))
                        continue;
                    if (string.IsNullOrEmpty(dict[key].ToString()))
                        continue;

                    switch (first)
                    {
                        case Extras.Player:
                            _playerCfg.Add(key, (int)dict[key]);
                            break;
                        case Extras.PlayerWeapon:
                            _playerWeaponCfg.Add(key, (int)dict[key]);
                            break;
                        case Extras.Enemy:
                            _enemyThugCfg.Add(key, (int)dict[key]);
                            break;
                        case Extras.EnemyWeapon_Pistol:
                            _enemyPistolCfg.Add(key, (int)dict[key]);
                            break;
                        case Extras.EnemySniper:
                            _enemySniperCfg.Add(key, (int)dict[key]);
                            break;
                        case Extras.EnemyWeapon_SniperRifle:
                            _enemySniperRifleCfg.Add(key, (int)dict[key]);
                            break;
                        case Extras.PeaceMan:
                            _peaceManCfg.Add(key, (int)dict[key]);
                            break;
                        case Extras.Boss:
                            _bossCfg.Add(key, (int)dict[key]);
                            break;
                        case Extras.Boss_Weapons:
                            _bossWeaponCfg.Add(key, (int)dict[key]);
                            break;
                    }
                }
            }
        }

        private void ConfigureObjects()
        {
            _levelController = new LevelController(FindObjectOfType<Level>(), _charactersHolder, IsDebug);
            ObjectHolder.AddObject(_levelController, typeof(IGetStageCount));
            ObjectHolder.AddObject(this);

            MonoCache[] objects = FindObjectsOfType<MonoCache>();

            for (int i = 0; i < objects.Length; i++)
            {
                MonoCache obj = objects[i];

                ConfigObject(obj);

                if (obj.gameObject.TryGetComponent(out Camera cam))
                    ObjectHolder.AddObject(cam);
                if (obj.gameObject.GetComponent<Crosshair>() != null)
                    ObjectHolder.AddObject(obj);
                if (obj.gameObject.GetComponent<Player>() != null)
                    ObjectHolder.AddObject(obj);
                if (obj.gameObject.GetComponent<TutorialController>() == null)
                    continue;
                ObjectHolder.AddObject(obj);
                _tutorialController = (TutorialController)obj;
            }
        }

        public void ConfigPrefabs()
        {
            ParseConfig();

            MonoCache[] sceneObjects = FindObjectsOfType<MonoCache>();
            MonoCache[] prefabs = Resources.LoadAll<MonoCache>($"Prefabs");

            foreach (MonoCache sceneObject in sceneObjects)
                ConfigObject(sceneObject);
            foreach (MonoCache prefab in prefabs)
                ConfigObject(prefab);
        }

        public void ConfigObject(MonoCache obj)
        {
            FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            for (int j = 0; j < fields.Length; j++)
            {
                FieldInfo fieldInfo = fields[j];

                Config configRequest = fieldInfo.GetCustomAttribute<Config>();
                if (configRequest == null)
                    continue;

                string id = configRequest.Id;
                string param = configRequest.Param;

                switch (id)
                {
                    case Extras.Player:
                        fieldInfo.SetValue(obj, _playerCfg[param]);
                        break;
                    case Extras.PlayerWeapon:
                        fieldInfo.SetValue(obj, _playerWeaponCfg[param]);
                        break;
                    case Extras.Enemy:
                        fieldInfo.SetValue(obj, _enemyThugCfg[param]);
                        break;
                    case Extras.EnemyWeapon_Pistol:
                        fieldInfo.SetValue(obj, _enemyPistolCfg[param]);
                        break;
                    case Extras.EnemySniper:
                        fieldInfo.SetValue(obj, _enemySniperCfg[param]);
                        break;
                    case Extras.EnemyWeapon_SniperRifle:
                        fieldInfo.SetValue(obj, _enemySniperRifleCfg[param]);
                        break;
                    case Extras.PeaceMan:
                        fieldInfo.SetValue(obj, _peaceManCfg[param]);
                        break;
                    case Extras.Boss:
                        fieldInfo.SetValue(obj, _bossCfg[param]);
                        break;
                    case Extras.Boss_Weapons:
                        fieldInfo.SetValue(obj, _bossWeaponCfg[param]);
                        break;
                }
            }
        }

        [Button("Set Debug"), HideIf("_debugMode")]
        private void SetDebug()
        {
            IsDebug = true;
            _debugMode = true;
        }

        [Button("Unset Debug"), ShowIf("_debugMode")]
        private void UnsetDebug()
        {
            IsDebug = false;
            _debugMode = false;
        }

        [Button("Enable Tutorial"), HideIf("_tutorialShown")]
        private void EnableTutorial()
        {
            IsTutorial = true;
            _tutorialShown = true;
        }

        [Button("Disable Tutorial"), ShowIf("_tutorialShown")]
        private void DisableTutorial()
        {
            IsTutorial = false;
            _tutorialShown = false;
        }

        [Button("Game Over Bad")]
        private void TestBad()
            => GameEvents.Call(new Event_GameOver("test bad", false));

        [Button("Game Over Good")]
        private void TestGood()
            => GameEvents.Call(new Event_GameOver("test good", true));

        [Button("Game Start")]
        private void StartGame()
            => GameEvents.Call(new Event_GameStart());
    }

    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<TKey> _keys = new List<TKey>();
        [SerializeField] private List<TValue> _values = new List<TValue>();

        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                _keys.Add(pair.Key);
                _values.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();
            for (int i = 0; i < _keys.Count; i++)
                Add(_keys[i], _values[i]);
        }
    }
}