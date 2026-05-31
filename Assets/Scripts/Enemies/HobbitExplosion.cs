using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HobbitExplosion : MonoBehaviour
{
    [SerializeField] private int explosionDamage = 25;
    [SerializeField] private float explosionLifetime = 2f; // Adjust to match animation duration

    private void Start()
    {
        // Automatically destroy the explosion instance once the animation finishes
        Destroy(gameObject, explosionLifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Don't blow up other enemies unless your team wants friendly fire mechanics
        if (other.CompareTag("Enemy")) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(explosionDamage);
                Debug.Log("Player caught in hobbit explosion!");
            }
        }
    }
}