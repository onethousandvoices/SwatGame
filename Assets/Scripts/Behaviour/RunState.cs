using SWAT.LevelScripts;
using UnityEngine;

namespace SWAT.Behaviour
{
    public abstract class RunState : IState
    {
        private readonly IRunStateReady _character;
        protected PathPoint TargetPathPoint;

        protected RunState(IRunStateReady character)
            => _character = character;

        public virtual void Enter()
            => TargetPathPoint = _character.Path.GetPoint();

        private void UpdatePathIndex()
        {
            if (TargetPathPoint.IsStopPoint)
                if (OnPathPointStop())
                    return;
            TargetPathPoint = _character.Path.GetPoint();
        }

        protected virtual bool OnPathPointStop()
            => false;

        public void Run()
        {
            if (_character.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.5f || _character.Animator.IsInTransition(0))
                return;
            
            Vector3 character = _character.Transform.position;
            Vector3 direction = TargetPathPoint.transform.position - character;
            direction.y = 0;
            Quaternion rotation = Quaternion.LookRotation(direction);
            _character.Transform.rotation = Quaternion.Slerp(_character.Transform.rotation, rotation, Time.deltaTime * 20f);

            if (_character.Rb.velocity.magnitude < _character.Speed)
                _character.Rb.AddForce(_character.Transform.forward * (_character.Speed * 100 * Time.deltaTime), ForceMode.Force);

            if ((TargetPathPoint.transform.position - character).sqrMagnitude > 2f)
                return;

            UpdatePathIndex();
        }

        public abstract void Exit();
    }
}