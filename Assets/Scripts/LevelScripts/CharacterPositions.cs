using NTC.Global.Cache;
using UnityEngine;

namespace SWAT.LevelScripts
{
    public class CharacterPositions : MonoCache
    {
        public TargetPosition[] TargetPositions { get; private set; }
        private Path _path;

        public Path GetPath(Transform target)
        {
            if (_path.End.Position != target.position && _path.Start.Position != target.position) return null;

            if (_path.End.Position == target.position)
            {
                _path.CheckInverse();
                return _path;
            }
            if (_path.Start.Position == target.position)
            {
                _path.Inverse();
                return _path;
            }
            return null;
        }

        private void OnValidate()
        {
            TargetPositions = GetComponentsInChildren<TargetPosition>();
            _path = GetComponentInChildren<Path>();
        }

        private void OnDrawGizmos()
        {
            OnValidate();
        }
    }
}