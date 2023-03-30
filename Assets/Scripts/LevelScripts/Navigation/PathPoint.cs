using NTC.Global.Cache;
using UnityEngine;

namespace SWAT.LevelScripts
{
    public class PathPoint : MonoCache
    {
        [field: SerializeField] public bool IsStopPoint { get; private set; }
        
        private Color _drawColor = Color.gray;

        public void Draw()
        {
            float size = 0f;
            if (IsStopPoint)
            {
                _drawColor = Color.red;
                _drawColor.a = 0.6f;
                size = 0.5f;
            }
            else
            {
                _drawColor = Color.green;
                _drawColor.a = 0.6f;
                size = 0.3f;
            }

            Gizmos.color = _drawColor;
            Gizmos.DrawSphere(transform.position, size);
        }
    }
}