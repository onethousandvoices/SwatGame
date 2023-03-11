using NTC.Global.Cache;
using System.Linq;
using UnityEngine;

namespace SWAT
{
    public class HitPointsHolder : MonoCache
    {
        [field: SerializeField] public HitPoint[] HitPoints { get; private set; }

        private void OnValidate()
        {
            int sum = HitPoints.Sum(x => x.Value);
            if (sum > 100) Debug.LogError($"{this} Hit Points overall value is more than 100");
        }

        private void OnDrawGizmos()
        {
            if (HitPoints.Length < 1) return;
            
            Gizmos.color = Color.cyan;
            
            foreach (HitPoint hitPoint in HitPoints)
            {
                Gizmos.DrawWireCube(hitPoint.Target.position, new Vector3(0.3f, 0.3f, 0.3f));
            }
        }
    }
}