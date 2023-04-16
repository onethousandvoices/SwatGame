using SWAT.LevelScripts.Navigation;
using System;
using UnityEngine;

namespace SWAT.LevelScripts
{
    [Serializable]
    public struct CharacterPathPair
    {
        [field: SerializeField] public BaseCharacter Character { get; private set; }
        [field: SerializeField] public Path Path { get; private set; }
    }

    [Serializable]
    public class Stage
    {
        [field: SerializeField] public CharacterPathPair[] CharacterPathPairs { get; private set; }
    }
}