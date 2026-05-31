using UnityEngine;

//Attached to the Lightningspear object. The enable and disable methods trigger during the animation
//of the spear. Final boss script instantiates spear, then animation immediately triggers. There will

public class SpearChaseTrigger : MonoBehaviour
{
    [SerializeField] private int spearDamage;
    private Collider2D spearCollider;
    
    
    //Triggers one second through spear animation to give player a chance to move.
    public void EnableCollider()
    {
        Debug.Log("EnableCollider was called");
        spearCollider = GetComponent<Collider2D>();
        spearCollider.enabled = true;
    }

    //Disable spear collider and destroy spear object.
    public void DisableCollider()
    {
        Debug.Log("DisableCollider was called");
        spearCollider = GetComponent<Collider2D>();
        spearCollider.enabled = false;
        Debug.Log("Destroying: " + gameObject.name);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Player")
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if(playerHealth != null)
            {
                playerHealth.TakeDamage(spearDamage);
            }
        }
    }


}
