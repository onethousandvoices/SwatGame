using SWAT.LevelScripts;
using UnityEngine;

namespace SWAT.Behaviour
{
    public abstract class RunState : IState
    {
        private readonly IRunStateReady _character;
        private readonly IState SwitchState;
        protected PathPoint TargetPathPoint;

        protected RunState(IRunStateReady character, IState switchOnPathUpdate)
        {
            _character = character;
            SwitchState = switchOnPathUpdate;
        }

        public virtual void Enter()
        {
            TargetPathPoint = _character.Path.GetPoint();
        }

        private void UpdatePathIndex()
        {
            if (TargetPathPoint.IsStopPoint)
            {
                _character.StateEngine.SwitchState(SwitchState);
                return;
            }

            TargetPathPoint = _character.Path.GetPoint();
        }

        public void Run()
        {
            Vector3 enemyPos = _character.Transform.position;
            Vector3 direction = TargetPathPoint.transform.position - enemyPos;
            direction.y = 0;
            Quaternion rotation = Quaternion.LookRotation(direction);
            _character.Transform.rotation = Quaternion.Slerp(_character.Transform.rotation, rotation, Time.deltaTime * 20f);

            //todo constrain velocty

            _character.Rb.AddForce(_character.Transform.forward * (_character.Speed * 100 * Time.deltaTime), ForceMode.Force);

            if ((TargetPathPoint.transform.position - enemyPos).sqrMagnitude > 2f)
                return;

            UpdatePathIndex();
        }

        public abstract void Exit();
    }
}