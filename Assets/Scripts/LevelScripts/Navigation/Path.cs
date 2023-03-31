using NaughtyAttributes;
using NTC.Global.Cache;
using System.Linq;
using UnityEngine;

namespace SWAT.LevelScripts.Navigation
{
    public class Path : MonoCache
    {
        public PathPoint Start
        {
            get
            {
                _pathPoints ??= GetComponentsInChildren<PathPoint>();
                return _pathPoints[0];
            }
        }

        [SerializeField, HideInInspector] private PathPoint[] _pathPoints;
        
        private int _index;
        private int _mod = 1;
        private bool _isRecursive;
        private bool _isRecursiveAble;
        private bool _wasRecursive;

        public PathPoint GetPoint()
        {
            _index += _mod;

            if (_index < 0)
            {
                _index += 2;
                _mod = 1;
            }
            else if (_index == _pathPoints.Length)
            {
                if (_isRecursive)
                {
                    _index = 1;
                    _mod = 1;
                }
                else
                {
                    _index -= 2;
                    _mod = -1;
                }
            }

            return _pathPoints[_index];
        }

        [ShowIf("_isRecursiveAble"), Button("Set Path Recursive")]
        private void SetRecursive()
        {
            _isRecursive = true;
            _wasRecursive = true;
            _pathPoints.Last().transform.position = _pathPoints.First().transform.position;
        }

        [ShowIf("_isRecursiveAble"), Button("Set Path Non-Recursive")]
        private void UnsetRecursive()
        {
            _isRecursive = false;

            if (_wasRecursive == false) return;
            _wasRecursive = false;

            _pathPoints.Last().transform.position = _pathPoints.First().transform.position + new Vector3(0f, 0f, 2f);
        }

        private void OnDrawGizmos()
        {
            _pathPoints = GetComponentsInChildren<PathPoint>();

            if (_pathPoints.Length < 1)
            {
                _isRecursiveAble = false;
                return;
            }

            if (_pathPoints.Length >= 4)
            {
                _isRecursiveAble = true;
            }
            else
            {
                _isRecursiveAble = false;
                _isRecursive = false;
                UnsetRecursive();
            }

            Color cyan = Color.cyan;
            cyan.a = 0.5f;

            if (Start != null)
            {
                Gizmos.color = cyan;
                Gizmos.DrawSphere(Start.transform.position, 0.5f);
            }

            for (int i = 1; i < _pathPoints.Length; i++)
            {
                PathPoint point = _pathPoints[i];
                if (point == null) continue;
                if (point.name == "Start")
                {
                    point.transform.SetSiblingIndex(0);
                }
                else if (point.name == "End")
                {
                    point.transform.SetSiblingIndex(transform.childCount);
                }

                point.Draw();
                Gizmos.color = cyan;
                Gizmos.DrawLine(_pathPoints[i - 1].transform.position, point.transform.position);
            }
        }
    }
}