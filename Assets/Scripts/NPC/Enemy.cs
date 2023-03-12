using SWAT.Behaviour;

namespace SWAT
{
    public class Enemy : NPC
    {
        
        
        protected override void OnEnabled()
        {
            base.OnEnabled();
            
        }

        protected override void Run()
        {
            base.Run();
            
        }

#region States
        private class FiringState : IState
        {
            public void Enter()
            {
            }

            public void Run()
            {
            }

            public void Exit()
            {
            }
        }
#endregion
    }
}