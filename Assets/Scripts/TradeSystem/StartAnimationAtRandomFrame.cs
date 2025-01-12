using UnityEngine;

public class StartAnimationAtRandomFrame : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Get all Animator components in the children (including the object itself)
        Animator[] animators = GetComponentsInChildren<Animator>();
        float startTimer = Random.Range(0.0f, 1.0f);

        // Loop through each Animator
        foreach (var animator in animators)
        {
            if (animator != null)
            {
                var state = animator.GetCurrentAnimatorStateInfo(0);
                animator.Play(state.fullPathHash, 0, startTimer);
            }
        }
    }
}
