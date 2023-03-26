using NTC.Global.Cache;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SWAT.LevelScripts
{
    public class Level : MonoCache
    {
        [field: SerializeField] public List<Stage> Stages { get; private set; }

        [MenuItem("GameObject/Level", false, 0)]
        private static void CreateNew()
        {
            GameObject go = new GameObject("Level", typeof(Level));
            Selection.activeGameObject = go;
        }
    }
}