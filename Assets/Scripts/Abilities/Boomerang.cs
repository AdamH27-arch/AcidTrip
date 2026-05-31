using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Travels outward toward the cursor then returns to the player.
// Hits enemies on both the outward and return trip.
// Count and damage scale with ability tier.
public class Boomerang : MonoBehaviour
{
    [SerializeField] private float outSpeed = 14f;
    [SerializeField] private float returnSpeed = 18f;
    [SerializeField] private float maxDistance = 9f;

    private Transform _owner;
    private int _damage;
    private HashSet<Enemy> _hitThisTrip = new HashSet<Enemy>();

    public void Launch(Vector2 direction, Transform owner, int tier)
    {
        _owner = owner;

        int dmgMult = tier <= 2 ? 2 : 3;
        int baseDamage = UpgradeSystem.Instance != null
            ? UpgradeSystem.Instance.Damage
            : 25;

        _damage = baseDamage * dmgMult;

        StartCoroutine(Travel(direction.normalized));
    }

    private IEnumerator Travel(Vector2 dir)
    {
        // Outward trip
        float traveled = 0f;
        while (traveled < maxDistance)
        {
            float step = outSpeed * Time.deltaTime;
            transform.position += (Vector3)(dir * step);
            transform.Rotate(0f, 0f, 900f * Time.deltaTime);
            traveled += step;
            yield return null;
        }

        // Clear hit list so enemies can be hit again on the return
        _hitThisTrip.Clear();

        // Return trip
        while (_owner != null)
        {
            Vector2 toOwner = ((Vector2)_owner.position - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(toOwner * returnSpeed * Time.deltaTime);
            transform.Rotate(0f, 0f, -900f * Time.deltaTime);

            if (Vector2.Distance(transform.position, _owner.position) < 0.6f)
                break;

            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null || _hitThisTrip.Contains(enemy)) return;

        _hitThisTrip.Add(enemy);
        enemy.TakeDamage(_damage);
    }
}