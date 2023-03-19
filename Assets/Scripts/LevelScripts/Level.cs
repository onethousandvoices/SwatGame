using NTC.Global.Cache;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SWAT.LevelScripts
{
    public class Level : MonoCache
    {
        [SerializeField] private List<Stage> _stages;

        [MenuItem("GameObject/Level", false, 0)]
        private static void CreateNew()
        {
            GameObject go = new GameObject("Level", typeof(Level));
            Selection.activeGameObject = go;
        }

        public void CreateStage()
        {
            _stages ??= new List<Stage>();
            
            _stages.Add(new Stage());
        }
    }
}