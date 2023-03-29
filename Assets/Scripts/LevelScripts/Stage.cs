using System;
using UnityEngine;

namespace SWAT.LevelScripts
{
    [Serializable]
    public struct EnemyPath
    {
        [field: SerializeField] public Enemy Enemy { get; private set; }
        [field: SerializeField] public CharacterPositions CharacterPositions { get; private set; }
    }

    [Serializable]
    public class Stage
    {
        [field: SerializeField] public EnemyPath[] Enemies { get; private set; }
    }
}