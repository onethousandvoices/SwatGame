namespace SWAT.LevelScripts
{
    public class EndPosition : TargetPosition
    {
        private const string _name = "End";
        
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