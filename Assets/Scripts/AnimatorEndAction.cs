using UnityEngine;

public class AnimatorEndAction : StateMachineBehaviour
{
    public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        animator.GetComponent<PlayerController>().EndAction();
    }  
}
