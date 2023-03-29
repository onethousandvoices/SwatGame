using NTC.Global.Cache;
using UnityEngine;

namespace SWAT.LevelScripts
{
    public class PathPoint : MonoCache
    {
        private Color _drawColor = Color.gray;
        
        private void OnDrawGizmos()
        {
            Path path = GetComponentInParent<Path>();
            
            if (path == null) return;

            if (path.PathPoints.Contains(this) == false)
            {
                _drawColor = Color.gray;
            }
            
            _drawColor.a = 0.5f;
            Gizmos.color = _drawColor;
            Gizmos.DrawSphere(transform.position, 0.3f);
        }

        public void Draw()
        {
            _drawColor = Color.green;
            _drawColor.a = 0.6f;
        }
    }
}