using NaughtyAttributes;
using NTC.Global.Cache;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace SWAT
{
    public enum ProgressCharacterType : byte
    {
        Enemy,
        Civilian,
        Boss
    }
    
    public class ProgressBarCharacter : MonoCache
    {
        [field: SerializeField] public Image Image { get; private set; }
        [SerializeField] private Image _crossOutImage;
        [HorizontalLine(color: EColor.Red)]
        [SerializeField] private Sprite _enemy;
        [SerializeField] private Sprite _civilian;
        [SerializeField] private Sprite _boss;

        public void SetType(ProgressCharacterType type)
        {
            Image.sprite = type switch
                           {
                               ProgressCharacterType.Enemy    => _enemy,
                               ProgressCharacterType.Civilian => _civilian,
                               ProgressCharacterType.Boss     => _boss,
                               _                              => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                           };
        }

        public void CrossOut()
        {
            _crossOutImage.gameObject.SetActive(true);
        }
    }
}