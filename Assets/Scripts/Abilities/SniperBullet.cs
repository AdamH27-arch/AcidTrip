using System.Collections.Generic;
using UnityEngine;

// Fast piercing bullet that travels through every enemy it hits.
// Damage and speed scale with ability tier.
[RequireComponent(typeof(Rigidbody2D))]
public class SniperBullet : MonoBehaviour
{
    [SerializeField] private float lifetime = 1.5f;

    private Rigidbody2D _rb;
    private int _damage;
    private HashSet<Collider2D> _pierced = new HashSet<Collider2D>();

    private void Awake() => _rb = GetComponent<Rigidbody2D>();

    public void Launch(Vector2 dir, int tier)
    {
        int dmgMult = tier == 1 ? 4 : tier == 2 ? 6 : 8;
        float speed = tier == 1 ? 35f : tier == 2 ? 45f : 55f;
        float scale = tier == 3 ? 1.8f : 1f;

        int baseDamage = UpgradeSystem.Instance != null
            ? UpgradeSystem.Instance.Damage
            : 25;

        _damage = baseDamage * dmgMult;

        _rb.linearVelocity = dir.normalized * speed;
        transform.localScale = Vector3.one * scale;

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        // Ignore enemies already pierced on this shot
        if (_pierced.Contains(other)) return;
        _pierced.Add(other);

        other.GetComponent<Enemy>()?.TakeDamage(_damage);

        // Does NOT destroy -- continues through the enemy
    }
}