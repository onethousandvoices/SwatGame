using NTC.Global.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SWAT
{
    public class HitPointsHolder : MonoCache
    {
        [field: SerializeField] public List<HitPoint> HitPoints { get; private set; }

        [SerializeField] private Transform _hitPoint0;
        [SerializeField] private int       _hitPoint0Value;
        [SerializeField] private Transform _hitPoint1;
        [SerializeField] private int       _hitPoint1Value;
        [SerializeField] private Transform _hitPoint2;
        [SerializeField] private int       _hitPoint2Value;
        [SerializeField] private Transform _hitPoint3;
        [SerializeField] private int       _hitPoint3Value;
        [SerializeField] private Transform _hitPoint4;
        [SerializeField] private int       _hitPoint4Value;

        public int Sum
        {
            get
            {
                return HitPoints.Sum(x => x.Value);
            }
        }

        private void OnValidate()
        {
            if (HitPoints == null || HitPoints.Count < 1 || transform.childCount < 5)
            {
                foreach (Transform child in transform) 
                {
                    Destroy(child);
                }
                
                HitPoints = new List<HitPoint>();

                for (int i = 0; i < 5; i++)
                {
                    GameObject emptyGO = new GameObject($"HitPoint_{i}")
                    {
                        transform =
                        {
                            parent        = transform,
                            localPosition = Vector3.zero
                        }
                    };

                    HitPoints.Add(new HitPoint(emptyGO.transform));
                }
                
                _hitPoint0      = HitPoints[0].Target;
                _hitPoint0Value = HitPoints[0].Value;
                _hitPoint1      = HitPoints[1].Target;
                _hitPoint1Value = HitPoints[1].Value;
                _hitPoint2      = HitPoints[2].Target;
                _hitPoint2Value = HitPoints[2].Value;
                _hitPoint3      = HitPoints[3].Target;
                _hitPoint3Value = HitPoints[3].Value;
                _hitPoint4      = HitPoints[4].Target;
                _hitPoint4Value = HitPoints[4].Value;
            }
            else
            {
                HitPoints[0] = new HitPoint(_hitPoint0, _hitPoint0Value);
                HitPoints[1] = new HitPoint(_hitPoint1, _hitPoint1Value);
                HitPoints[2] = new HitPoint(_hitPoint2, _hitPoint2Value);
                HitPoints[3] = new HitPoint(_hitPoint3, _hitPoint3Value);
                HitPoints[4] = new HitPoint(_hitPoint4, _hitPoint4Value);
            }
        }

        private void OnDrawGizmos()
        {
            if (HitPoints == null || HitPoints.Count < 1) return;

            Vector3 gizmosScale = new Vector3(0.1f, 0.1f, 0.1f);

            foreach (HitPoint hitPoint in HitPoints)
            {
                Gizmos.color = Color.cyan;

                if (hitPoint.Target == null) continue;

                Collider[] r = Physics.OverlapBox(hitPoint.Target.position, gizmosScale / 2);

                if (r.Any(x => x.GetComponentInParent<Player>()))
                    Gizmos.color = Color.red;

                Gizmos.DrawWireCube(hitPoint.Target.position, gizmosScale);
            }
        }
    }
}