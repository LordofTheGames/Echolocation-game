using UnityEngine;

public class MonsterSoundTrigger : StateMachineBehaviour
{
    // OnStateEnter is called the exact frame the animation state begins
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Find the Tutorial script sitting on the same object as the Animator, and play the sound
        animator.GetComponent<Tutorial>().PlayMonsterSound();
    }
}