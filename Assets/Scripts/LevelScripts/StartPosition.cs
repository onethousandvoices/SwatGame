using System;

namespace SWAT.LevelScripts
{
    public class StartPosition : TargetPosition
    {
        private const string _name = "Start";
        
        private void OnValidate()
        {
            gameObject.name = _name;
        }

        private void OnDrawGizmos()
        {
            OnValidate();
        }
    }
}