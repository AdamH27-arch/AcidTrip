using UnityEngine;

// Short range bullet used by Scatter Shot ability.
// Does not pierce -- destroys on first enemy hit.
public class ScatterBullet : MonoBehaviour
{
    [SerializeField] private float speed = 18f;
    [SerializeField] private float lifetime = 0.6f; // short range feel

    private Vector2 _direction;
    private int _damage;

    public void Launch(Vector2 dir, int damage)
    {
        _direction = dir.normalized;
        _damage = damage;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += (Vector3)(_direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        other.GetComponent<Enemy>()?.TakeDamage(_damage);
        Destroy(gameObject);
    }
}