using NaughtyAttributes;
using NTC.Global.Cache;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace SWAT
{
    public enum CheckpointState : byte
    {
        Locked,
        Completed,
        Current
    }

    public class Checkpoint : MonoCache
    {
        [field: SerializeField] public RectTransform Rect { get; private set; }
        [SerializeField] private Image _image;
        [HorizontalLine(color: EColor.Red)]
        [SerializeField] private Sprite _locked;
        [SerializeField] private Sprite _completed;
        [SerializeField] private Sprite _current;

        public void SetState(CheckpointState state)
        {
            _image.sprite = state switch
                            {
                                CheckpointState.Locked    => _locked,
                                CheckpointState.Completed => _completed,
                                CheckpointState.Current   => _current,
                                _                         => throw new ArgumentOutOfRangeException(nameof(state), state, null)
                            };
        }
    }
}