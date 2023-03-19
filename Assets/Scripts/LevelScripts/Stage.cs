using NTC.Global.Cache;
using System;
using UnityEngine;

namespace SWAT.LevelScripts
{
    [Serializable]
    public struct EnemyPath
    {
        [field: SerializeField] public Enemy Enemy { get; private set; }
        [field: SerializeField] public EnemyPositions EnemyPositions { get; private set; }
    }

    public class Stage : MonoCache
    {
        [field: SerializeField] public EnemyPath[] Enemies { get; private set; }
    }
}