using System.Collections;
using UnityEngine;

public class UltimateSMBehavior : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponentInChildren<AbberatingThunder>().QuakeOnDrop();
        animator.gameObject.transform.parent.gameObject.SetActive(false);
    }
}
