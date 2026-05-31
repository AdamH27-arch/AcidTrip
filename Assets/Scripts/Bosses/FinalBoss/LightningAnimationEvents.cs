using UnityEngine;

public class LightningAnimationEvents : MonoBehaviour
{
    private LightningStormTrigger trigger;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        trigger = GetComponentInParent<LightningStormTrigger>();
    }

   private void EnableCollider()
    {
        trigger.EnableCollider();
    }

    private void DisableCollider()
    {
        trigger.DisableCollider();
    }
}
