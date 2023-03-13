using System;
using UnityEngine;

namespace SWAT
{
    [Serializable]
    public struct HitPoint
    {
        [SerializeField]               public Transform Target;
        [SerializeField, Range(1, 96)] public int       Value;

        public HitPoint(Transform target)
        {
            Target = target;
            Value  = 1;
        }
    }
}