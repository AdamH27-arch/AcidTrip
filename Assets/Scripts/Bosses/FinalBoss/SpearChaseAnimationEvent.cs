using UnityEngine;


//This script exists to activate the collider on the parent object "LightningSpear" from the animation
//event child object. 
public class SpearChaseAnimationEvent : MonoBehaviour
{
    SpearChaseTrigger trigger;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        trigger = GetComponentInParent<SpearChaseTrigger>();  
        if(trigger == null)
        {
            Debug.Log("trigger is null");
            return;
        }
    }

    private void EnableParentCollider()
    {
        Debug.Log("EnableParentCollider called");
        trigger.EnableCollider();
    }

    private void DisableParentCollider()
    {
        Debug.Log("DisableParentCollider called");
        trigger.DisableCollider();
    }

}
