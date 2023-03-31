using NTC.Global.Cache;
using SWAT;
using SWAT.Events;
using SWAT.LevelScripts;
using SWAT.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Controllers
{
    public class GameController : MonoCache
    {
#region CfgDictionaries
        private readonly Dictionary<string, int> _playerCfg = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _playerWeaponCfg = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _enemyThugCfg = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _enemyPistolCfg = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _enemySniperCfg = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _enemySniperRifleCfg = new Dictionary<string, int>();
#endregion

        protected override void OnEnabled()
        {
            ParseConfig();
            ConfigureObjects();
            
        }

        private IEnumerator Start() //todo kostbl
        {
            yield return new WaitForSeconds(1f);
            ObjectHolder.AddObject(new LevelController(FindObjectOfType<Level>()));
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

            for (int i = 0; i < config.Count; i++)
            {
                Dictionary<string, object> dict = config[i];
                string first = dict["ID"].ToString();

                foreach (string key in dict.Keys)
                {
                    if (key.Contains("ID")) continue;
                    if (string.IsNullOrEmpty(dict[key].ToString())) continue;

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
                    }
                }
            }
        }

        private void ConfigureObjects()
        {
            ObjectHolder.AddObject(this);

            MonoCache[] objects = FindObjectsOfType<MonoCache>();

            for (int i = 0; i < objects.Length; i++)
            {
                MonoCache obj = objects[i];

                ConfigObject(obj);

                if (obj.gameObject.TryGetComponent(out Camera cam)) ObjectHolder.AddObject(cam);
                if (obj.gameObject.GetComponent<Crosshair>() != null) ObjectHolder.AddObject(obj);
                if (obj.gameObject.GetComponent<Player>() != null) ObjectHolder.AddObject(obj);
            }
        }

        public void ConfigPrefabs()
        {
            MonoCache[] prefabs = Resources.LoadAll<MonoCache>($"Prefabs");

            foreach (MonoCache prefab in prefabs)
            {
                ConfigObject(prefab);
            }
        }

        public void ConfigObject(MonoCache obj)
        {
#if UNITY_EDITOR
            ParseConfig();
#endif
            
            FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            for (int j = 0; j < fields.Length; j++)
            {
                FieldInfo fieldInfo = fields[j];

                Config configRequest = fieldInfo.GetCustomAttribute<Config>();
                if (configRequest == null) continue;

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
                }
            }
        }
    }
}