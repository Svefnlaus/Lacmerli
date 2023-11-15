using UnityEngine;

public class SliceBehavior : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Player.canMove = false;
        Player.swordRange.gameObject.SetActive(true);
        Player.accessToMove = true;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Player.canMove = true;
        Player.swordRange.gameObject.SetActive(false);
        Player.accessToMove = false;
    }
}
