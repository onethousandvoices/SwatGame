using NTC.Global.Cache;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SWAT
{
    public class HitPointsHolder : MonoCache
    {
        [field: SerializeField] public List<HitPoint> HitPoints { get; private set; }

        private void OnValidate()
        {
            if (HitPoints == null || HitPoints.Count < 1)
            {
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
            }

            int sum = HitPoints.Sum(x => x.Value);
            if (sum > 100) Debug.LogError($"{this} Hit Points overall value is more than 100");
        }

        private void OnDrawGizmos()
        {
            if (HitPoints == null || HitPoints.Count < 1) return;

            foreach (HitPoint hitPoint in HitPoints)
            {
                Gizmos.color = Color.cyan;

                if (hitPoint.Target == null) continue;

                Collider[] r = Physics.OverlapBox(hitPoint.Target.position, new Vector3(0.15f, 0.15f, 0.15f));

                if (r.Any(x => x.GetComponentInParent<Player>()))
                    Gizmos.color = Color.red;
                
                Gizmos.DrawWireCube(hitPoint.Target.position, new Vector3(0.3f, 0.3f, 0.3f));
            }
        }
    }
}