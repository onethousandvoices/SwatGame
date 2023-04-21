using UnityEngine;

public class TriggerReset : StateMachineBehaviour
{
    [SerializeField] private string _trigger;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        => animator.ResetTrigger(_trigger);
}
