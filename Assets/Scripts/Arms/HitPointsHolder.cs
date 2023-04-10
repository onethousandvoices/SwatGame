using NTC.Global.Cache;
using SWAT.Utility;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace SWAT
{
    public class HitPointsHolder : MonoCache
    {
        [field: SerializeField] public List<Transform> HitPoints { get; private set; }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (HitPoints == null || HitPoints.Count < 1) return;

            Vector3 gizmosScale = new Vector3(0.1f, 0.1f, 0.1f);

            foreach (Transform hitPoint in HitPoints)
            {
                Gizmos.color = Color.cyan;
                
                if (hitPoint == null) return;
                
                Collider[] r = Physics.OverlapBox(hitPoint.position, gizmosScale / 2);

                if (r.Any(x => x.GetComponentInParent<Player>()))
                    Gizmos.color = Color.red;

                Gizmos.DrawWireCube(hitPoint.position, gizmosScale);
                
                int index = int.Parse(new string(hitPoint.name.Where(char.IsDigit).ToArray()));
                Handles.Label(hitPoint.position, index.ToString(), new GUIStyle()
                {
                    normal =
                    {
                        textColor = Color.yellow
                    },
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 17
                });
            }
        }
#endif
    }
}