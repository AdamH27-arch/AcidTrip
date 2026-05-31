using System.Collections;
using UnityEngine;

// Sets a delay on spear strike, then triggers the animation.
// Sound is played once by FinalBoss when the attack starts, not per-spear.
public class SpearTimer : MonoBehaviour
{
    [SerializeField] private float spearDelay;

    private Animator spearAnimator;

    void Start()
    {
        spearAnimator = GetComponentInChildren<Animator>();

        if (spearAnimator == null)
            Debug.Log("SpearTimer: animator not found");

        StartCoroutine(DelayStrike());
    }

    private IEnumerator DelayStrike()
    {
        yield return new WaitForSeconds(spearDelay);
        Debug.Log("firing spear");
        spearAnimator.SetTrigger("isFiring");
    }
}