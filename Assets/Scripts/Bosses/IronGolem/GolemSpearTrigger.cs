using UnityEngine;
using UnityEngine.Rendering;


//Methods for enabling and disabling collider are called during animation
//event on the GolemSpear prefab.
public class GolemSpearTrigger : MonoBehaviour
{
    [SerializeField] private int spearDamage;
    private Collider2D spearCollider;

    public void EnableCollider()
    {
        spearCollider = GetComponent<Collider2D>();
        spearCollider.enabled = true;
    }

    public void DisableCollider()
    {
        spearCollider.enabled = false;
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
