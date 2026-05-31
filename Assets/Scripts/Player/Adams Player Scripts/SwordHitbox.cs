using UnityEngine;
//Test to merge.
public class SwordHitBox : MonoBehaviour
{
    public float damage = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Use upgraded damage if available, fall back to the inspector value
                int dmg = UpgradeSystem.Instance != null
                    ? UpgradeSystem.Instance.Damage
                    : (int)damage;

                enemy.TakeDamage(dmg);
            }
        }
    }

    public void EnableHitbox()//animation event method for spawning hitbox
    {
        GetComponent<Collider2D>().enabled = true;
    }

    public void DisableHitbox()//animation event method for despawning hitbox
    {
        GetComponent<Collider2D>().enabled = false;
    }
}
