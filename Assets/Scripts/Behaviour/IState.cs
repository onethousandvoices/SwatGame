namespace SWAT.Behaviour
{
    public interface IState
    {
        public void Enter();
        public void Run();
        public void Exit();
    }
}