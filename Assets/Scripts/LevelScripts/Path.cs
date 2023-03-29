using NTC.Global.Cache;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SWAT.LevelScripts
{
    public class Path : MonoCache
    {
        public List<PathPoint> PathPoints { get; private set; }
        public StartPosition Start { get; private set; }
        public EndPosition End { get; private set; }

        private bool _isInverse;

        public void CheckInverse()
        {
            if (_isInverse == false) return;
            Inverse();
        }

        public void Inverse()
        {
            Vector3 temp = Start.Position;
            Start.transform.position = End.Position;
            End.transform.position = temp;
            PathPoints.Reverse();
            _isInverse = true;
        }

        private void OnValidate()
        {
            PathPoints ??= new List<PathPoint>();
            PathPoints = GetComponentsInChildren<PathPoint>().ToList();

            Start = transform.parent.GetComponentInChildren<StartPosition>();
            End = transform.parent.GetComponentInChildren<EndPosition>();
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying == false)
                OnValidate();
#endif
            Color cyan = Color.cyan;
            cyan.a = 0.5f;
            Color red = Color.red;
            red.a = 0.5f;

            Gizmos.color = cyan;

            if (Start != null)
            {
                Gizmos.DrawSphere(Start.Position, 0.5f);
                if (PathPoints == null || PathPoints.Count < 1) return;
                PathPoint firstPoint = PathPoints[0];
                if (firstPoint == null) return;

                Gizmos.DrawLine(Start.Position, firstPoint.transform.position);
            }

            if (PathPoints != null)
            {
                foreach (PathPoint point in PathPoints)
                {
                    if (point == null) continue;
                    point.Draw();
                }

                for (int i = 1; i < PathPoints.Count; i++)
                {
                    PathPoint point = PathPoints[i];
                    if (point == null) continue;

                    Gizmos.DrawLine(PathPoints[i - 1].transform.position, point.transform.position);
                }

                if (End != null)
                {
                    PathPoint lastPoint = PathPoints[^1];
                    if (lastPoint == null) return;

                    Gizmos.DrawLine(lastPoint.transform.position, End.transform.position);
                }
            }

            Gizmos.color = red;

            if (End != null)
            {
                Gizmos.DrawSphere(End.Position, 0.5f);
            }
        }
    }
}