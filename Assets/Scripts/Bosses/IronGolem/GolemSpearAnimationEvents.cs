using UnityEngine;

//AnimationEvent is a child of the golem spear object where the collider is attached.
//THis script activates the parent collider during an animation event on the 
//GolemSpearPrefab.
public class GolemSpearAnimationEvents : MonoBehaviour
{
    private GolemSpearTrigger trigger;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        trigger = GetComponentInParent<GolemSpearTrigger>();
    }

    private void EnableParentCollider()
    {
        if(trigger != null)
        {
            trigger.EnableCollider();
        }
    }

    private void DisableParentCollider()
    {
        if(trigger != null)
        {
            trigger.DisableCollider();
        }
    }

   
}