using UnityEngine;

public class LightningStormTrigger : MonoBehaviour
{
    [SerializeField] private int damage;
    
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Player")
        {
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                playerHealth.TakeDamage(damage); 
        }
    }

    //Activates collider during lightning bolt animation. Note this method is called
    //by the EnableCollider() method within the LightningAnimationEvent script.
    public void EnableCollider()
    {
        Collider2D collider = GetComponent<Collider2D>();
        collider.enabled = true;
    }

    //Deactivates Collider and destroys lightning bolt object. Called by the DisableCollider() method
    //within the LightningAnimationEvent script.
    public void DisableCollider()
    {
        Collider2D collider = GetComponent<Collider2D>();
        collider.enabled = false;
        Destroy(gameObject);
    }
}
