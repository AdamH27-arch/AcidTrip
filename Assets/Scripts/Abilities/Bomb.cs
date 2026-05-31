using System.Collections;
using UnityEngine;

// Lobs toward the cursor, pauses briefly, then explodes in an area.
// Radius, damage, and cluster behaviour scale with ability tier.
public class Bomb : MonoBehaviour
{
    [SerializeField] private float travelSpeed = 10f;
    [SerializeField] private float fuseTime = 0.8f;
    [SerializeField] private AudioClip explosionSound;

    // Optional circle sprite to show explosion radius before detonation
    [SerializeField] private GameObject radiusIndicatorPrefab;

    private int _damage;
    private int _tier;
    private float _explosionRadius;

    public void Throw(Vector3 target, int tier)
    {
        _tier = tier;
        _explosionRadius = tier == 1 ? 5f : tier == 2 ? 7f : 9f;

        int dmgMult = tier == 1 ? 3 : tier == 2 ? 4 : 5;
        int baseDamage = UpgradeSystem.Instance != null
            ? UpgradeSystem.Instance.Damage
            : 25;

        _damage = baseDamage * dmgMult;

        StartCoroutine(TravelAndExplode(target));
    }

    private IEnumerator TravelAndExplode(Vector3 target)
    {
        Vector3 start = transform.position;

        // Travel toward target with a pulsing scale to simulate arc height
        while (Vector3.Distance(transform.position, target) > 0.15f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, target, travelSpeed * Time.deltaTime
            );

            float t = 1f - Vector3.Distance(transform.position, target)
                            / Mathf.Max(0.01f, Vector3.Distance(start, target));
            float arc = Mathf.Sin(t * Mathf.PI) * 0.4f;
            transform.localScale = Vector3.one * (1f + arc);

            yield return null;
        }

        transform.position = target;
        transform.localScale = Vector3.one;

        // Show radius indicator while fuse burns
        GameObject indicator = null;
        if (radiusIndicatorPrefab != null)
        {
            indicator = Instantiate(radiusIndicatorPrefab, target, Quaternion.identity);
            indicator.transform.localScale = Vector3.one * _explosionRadius * 2f;
        }

        yield return new WaitForSeconds(fuseTime);

        Explode(target);

        if (indicator != null) Destroy(indicator);
        Destroy(gameObject);
    }

    private void Explode(Vector3 center)
    {

        if (explosionSound != null)
            AudioSource.PlayClipAtPoint(explosionSound, center);

        // Main explosion -- damages all enemies in radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, _explosionRadius);
        foreach (Collider2D hit in hits)
            hit.GetComponent<Enemy>()?.TakeDamage(_damage);

        DeathBurst.Spawn(center, new Color(1f, 0.45f, 0f));

        // Tier 3 -- three additional cluster explosions around the impact
        if (_tier >= 3)
        {
            for (int i = 0; i < 3; i++)
            {
                float angle = i * 120f * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * 3f;
                Vector3 pos = center + offset;

                Collider2D[] cluster = Physics2D.OverlapCircleAll(pos, 2.5f);
                foreach (Collider2D hit in cluster)
                    hit.GetComponent<Enemy>()?.TakeDamage(_damage / 2);

                DeathBurst.Spawn(pos, new Color(1f, 0.3f, 0f));
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _explosionRadius);
    }
}