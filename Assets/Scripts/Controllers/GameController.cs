using NTC.Global.Cache;
using SWAT;
using SWAT.Utility;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Controllers
{
    public class GameController : MonoCache
    {
        
#region CfgDictionaries
        private readonly Dictionary<string, int> _playerCfg       = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _playerWeaponCfg = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _enemyCfg        = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _enemyWeaponCfg  = new Dictionary<string, int>();
#endregion
        
        protected override void OnEnabled()
        {
            ParseConfig();
            ConfigureObjects();
        }

        private void ParseConfig()
        {
            List<Dictionary<string, object>> config = CSVReader.Read("BasicEntityCfg");

            for (int i = 0; i < config.Count; i++)
            {
                Dictionary<string, object> dict  = config[i];
                string                     first = dict["ID"].ToString();

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
                            _enemyCfg.Add(key, (int)dict[key]);
                            break;
                        case Extras.EnemyWeapon:
                            _enemyWeaponCfg.Add(key, (int)dict[key]);
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
                if (obj.gameObject.GetComponent<Player>()    != null) ObjectHolder.AddObject(obj);
            }
        }

        private void ConfigObject(MonoCache obj)
        {
            FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            for (int j = 0; j < fields.Length; j++)
            {
                FieldInfo fieldInfo = fields[j];

                Config configRequest = fieldInfo.GetCustomAttribute<Config>();
                if (configRequest == null) continue;

                string id    = configRequest.Id;
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
                        fieldInfo.SetValue(obj, _enemyCfg[param]);
                        break;
                    case Extras.EnemyWeapon:
                        fieldInfo.SetValue(obj, _enemyWeaponCfg[param]);
                        break;
                }
            }
        }
    }
}