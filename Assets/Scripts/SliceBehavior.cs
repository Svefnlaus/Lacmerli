using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SliceBehavior : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Player.canMove = false;
        TutorialPlayer.canMove = false;
        Player.accessToMove = true;
        TutorialPlayer.accessToMove = true;
        if (Player.swordRange != null) Player.swordRange.gameObject.SetActive(true);
        if (TutorialPlayer.swordRange != null) TutorialPlayer.swordRange.gameObject.SetActive(true);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Player.canMove = true;
        TutorialPlayer.canMove = true;
        Player.accessToMove = false;
        TutorialPlayer.accessToMove = false;
        if (Player.swordRange != null) Player.swordRange.gameObject.SetActive(false);
        if (TutorialPlayer.swordRange != null) TutorialPlayer.swordRange.gameObject.SetActive(false);
    }

    private IEnumerator showSword()
    {
        if (Player.swordRange != null) Player.swordRange.gameObject.SetActive(true);
        if (TutorialPlayer.swordRange != null) TutorialPlayer.swordRange.gameObject.SetActive(true);
        yield return null;
    }
}
