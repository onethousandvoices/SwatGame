using NTC.Global.Cache;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SWAT.LevelScripts
{
    public class Path : MonoCache
    {
        [field: SerializeField] public Transform Start { get; private set; }
        [field: SerializeField] public List<PathPoint> PathPoints { get; private set; }
        [field: SerializeField] public Transform End { get; private set; }

        private bool _isInverse;

        public void CheckInverse()
        {
            if (_isInverse == false) return;
            Inverse();
        }
        
        public void Inverse()
        {
            (Start, End) = (End, Start);
            PathPoints.Reverse();
            _isInverse = true;
        }
        
        private void OnDrawGizmos()
        {
            Color cyan = Color.cyan;
            cyan.a = 0.5f;
            Color red = Color.red;
            red.a = 0.5f;
            
            Gizmos.color = cyan;

            if (Start != null)
            {
                Gizmos.DrawSphere(Start.position, 0.5f);
                // Handles.Label(Start.position, "START");
            }
            
            if (PathPoints != null)
            {
                foreach (PathPoint point in PathPoints)
                {
                    if (point == null) continue;
                    point.Draw();
                }
            }
            
            Gizmos.color = red;

            if (End != null)
            {
                Gizmos.DrawSphere(End.position, 0.5f);
                // Handles.Label(End.position, "END");
            }
        }
    }
}