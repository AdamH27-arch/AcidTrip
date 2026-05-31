using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class WitchBlast : EnemyBullet
{
    [SerializeField] private float slowAmount = 0.5f;  // multiplier — 0.5 = half speed
    [SerializeField] private float slowDuration = 2f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy")) return;
        if (other.CompareTag("Bullet")) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(base.damage);

            // Apply slow to player movement
            PlayerMovement pm = other.GetComponent<PlayerMovement>();
            if (pm != null)
                pm.ApplySlow(slowAmount, slowDuration);

            Destroy(gameObject);
        }
    }
}