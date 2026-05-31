using UnityEngine;
using System.Collections;

// After a brief delay, triggers the spear attack animation.
// Sound is played once by IronGolem when the attack starts, not per-spear.
public class GolemSpearTimer : MonoBehaviour
{
    [SerializeField] private float spearDelay;

    private Animator spearAnimator;

    void Start()
    {
        spearAnimator = GetComponentInChildren<Animator>();

        if (spearAnimator == null)
            Debug.Log("GolemSpearTimer: animator not found");

        StartCoroutine(DelayStrike());
    }

    private IEnumerator DelayStrike()
    {
        yield return new WaitForSeconds(spearDelay);
        Debug.Log("firing spear");
        spearAnimator.SetTrigger("isFiring");
    }
}