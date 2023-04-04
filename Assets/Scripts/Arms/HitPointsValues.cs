using NaughtyAttributes;
using NTC.Global.Cache;
using UnityEngine;

namespace Arms
{
    public class HitPointsValues : MonoCache
    {
        [SerializeField, HorizontalLine(color: EColor.Red), ProgressBar("Sum", 100, EColor.Green), ShowIf("E_IsTarget")] private int _sumTarget;
        [SerializeField, HorizontalLine(color: EColor.Red), ProgressBar("Sum", 100, EColor.Red), ShowIf("E_IsNotTarget")] private int _sum;
        [HorizontalLine(color: EColor.Red)]
        [SerializeField, Range(1, 96)] private int _hitPoint1Value;
        [SerializeField, Range(1, 96)] private int _hitPoint2Value;
        [SerializeField, Range(1, 96)] private int _hitPoint3Value;
        [SerializeField, Range(1, 96)] private int _hitPoint4Value;
        [SerializeField, Range(1, 96)] private int _hitPoint5Value;

        private bool E_IsTarget;
        private bool E_IsNotTarget;

        public int[] Values
        {
            get
            {
                return new []
                {
                    _hitPoint1Value,
                    _hitPoint2Value,
                    _hitPoint3Value,
                    _hitPoint4Value,
                    _hitPoint5Value
                };
            }
        }

        private void OnValidate()
        {
            _sumTarget = _hitPoint1Value + _hitPoint2Value + _hitPoint3Value + _hitPoint4Value + _hitPoint5Value;
            _sum = _sumTarget;

            E_IsTarget = _sumTarget == 100;
            E_IsNotTarget = !E_IsTarget;
        }

        private void OnDrawGizmos()
        {
            OnValidate();
        }
    }
}