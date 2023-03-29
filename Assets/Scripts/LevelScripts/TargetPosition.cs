using UnityEngine;

namespace SWAT.LevelScripts
{
    public class TargetPosition : MonoBehaviour
    {
        public Vector3 Position
        {
            get
            {
                return transform.position;
            }
        }
    }
}