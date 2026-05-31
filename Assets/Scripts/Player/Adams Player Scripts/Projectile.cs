using UnityEngine;
using System.Collections.Generic;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public float damage = 1f;
    public float _attackRange = 0.25f;

    private Vector2 direction;
    //private HashSet<Enemy> _alreadyHit = new HashSet<Enemy>();

    public void Launch(Vector2 launchDirection)
    {
        direction = launchDirection.normalized;
        GetComponent<Rigidbody2D>().linearVelocity = direction * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy") && !other.CompareTag("Boss")) return;

        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy == null) return;
        //if (_alreadyHit.Contains(enemy)) return;

        //_alreadyHit.Add(enemy);
        enemy.TakeDamage((int)damage);
    }

    public void SetDamage(int damage) => this.damage = damage;
    public void SetRange(float r) => _attackRange = r;

    void Start()
    {
        Destroy(gameObject, _attackRange);
    }
}