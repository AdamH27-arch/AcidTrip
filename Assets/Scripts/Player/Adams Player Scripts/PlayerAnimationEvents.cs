using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;

    void Start()
    {
        // Get PlayerMovement from the parent
        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    public void FireProjectile()
    {
        playerMovement.FireProjectile();
    }

    public void EnableHitbox()
    {
        playerMovement.EnableSwordHitbox();
    }

    public void DisableHitbox()
    {
        playerMovement.DisableSwordHitbox();
    }

    //used to delete the player object after the death animation.
    private void DeletePlayerObject()
    {
        Destroy(transform.parent.gameObject);
    }
}