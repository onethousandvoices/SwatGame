using NTC.Global.Cache;
using UnityEngine;

namespace SWAT.LevelScripts
{
    public class EnemyPositions : MonoCache
    {
        [field: SerializeField] public Transform[] TargetPositions { get; private set; }
        [field: SerializeField] public Path[] Paths { get; private set; }

        public Path GetPath(Transform target)
        {
            foreach (Path path in Paths)
            {
                if (path.End != target && path.Start != target) return null;

                if (path.End == target)
                {
                    path.CheckInverse();
                    return path;
                }
                if (path.Start == target)
                {
                    path.Inverse();
                    return path;
                }
            }
            return null;
        }
    }
}