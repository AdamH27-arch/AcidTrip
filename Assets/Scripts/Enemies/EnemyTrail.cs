using System.Collections;
using UnityEngine;

// creating a colour trail effect that matches the enemy's EnemyData colour.
public class EnemyTrail : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 0.05f; // seconds between each ghost spawn
    [SerializeField] private float ghostLifetime = 0.3f;  // how long each ghost takes to fade
    [SerializeField] private float startAlpha = 0.45f; // opacity the ghost starts at

    private SpriteRenderer[] _renderers; // all sprite renderers on this enemy and its children
    private Enemy _enemy;     // reference to grab the enemy's colour from EnemyData

    private void Start()
    {
        _renderers = GetComponentsInChildren<SpriteRenderer>();
        _enemy = GetComponent<Enemy>();
        StartCoroutine(SpawnLoop());
    }

    // Continuously spawns ghosts at a fixed interval for as long as the enemy is alive
    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnGhost();
        }
    }

    // Creates a faded copy of every visible sprite at their current world position.
    private void SpawnGhost()
    {
        foreach (SpriteRenderer sr in _renderers)
        {
            if (sr == null || sr.sprite == null) continue;

            // Create a plain GameObject at the exact world position and rotation of this sprite
            GameObject go = new GameObject("Ghost");
            go.transform.position = sr.transform.position;
            go.transform.rotation = sr.transform.rotation;
            go.transform.localScale = sr.transform.lossyScale; // lossyScale gives world scale

            // Copy the sprite and its display settings
            SpriteRenderer ghost = go.AddComponent<SpriteRenderer>();
            ghost.sprite = sr.sprite;
            ghost.flipX = sr.flipX;
            ghost.sortingLayerName = sr.sortingLayerName;
            ghost.sortingOrder = sr.sortingOrder - 1; // render just behind the real sprite

            // Use the enemy's defined colour if available, otherwise fall back to the sprite's own colour
            Color c = _enemy != null ? _enemy.Data.color : sr.color;
            c.a = startAlpha;
            ghost.color = c;

            StartCoroutine(FadeOut(go, ghost));
        }
    }

    // Gradually reduces the ghost's alpha to zero over its lifetime then destroys it
    private IEnumerator FadeOut(GameObject go, SpriteRenderer sr)
    {
        float elapsed = 0f;
        Color start = sr.color;

        while (elapsed < ghostLifetime)
        {
            if (go == null) yield break; // safety check in case the enemy died mid-fade

            elapsed += Time.deltaTime;
            Color c = start;
            c.a = Mathf.Lerp(startAlpha, 0f, elapsed / ghostLifetime);
            sr.color = c;
            yield return null;
        }

        if (go != null) Destroy(go);
    }
}