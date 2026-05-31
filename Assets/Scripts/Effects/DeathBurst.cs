using System.Collections;
using UnityEngine;

// Spawns a ring of flame-coloured particles that burst outward and drift
// upward on enemy death. Called statically from Enemy.Die() so no prefab needed.
public class DeathBurst : MonoBehaviour
{
    private static Sprite _sprite;

    // Entry point — creates a self-contained burst at the given world position
    public static void Spawn(Vector3 position, Color color)
    {
        GameObject go = new GameObject("DeathBurst");
        go.transform.position = position;
        go.AddComponent<DeathBurst>().Play(color);
    }

    // Kicks off the burst coroutine
    private void Play(Color color)
    {
        // Generate the sprite once and cache it — no need to regenerate for every burst
        if (_sprite == null) _sprite = GenerateCircle(32);
        StartCoroutine(Burst(color));
    }

    private IEnumerator Burst(Color baseColor)
    {
        int count = 14;
        float duration = 0.65f;
        float speed = 3.8f;
        float upDrift = 1.8f; // how much particles float upward like flames rising

        // Flame gradient from hot white-yellow core to deep red outer
        Color[] flameColors =
        {
            new Color(1f,   1f,   0.85f), // white-yellow — hottest point
            new Color(1f,   0.85f, 0f),   // bright yellow
            new Color(1f,   0.55f, 0f),   // orange
            new Color(1f,   0.25f, 0f),   // orange-red
            new Color(0.8f, 0f,   0f),    // deep red — coolest outer edge
        };

        GameObject[] parts = new GameObject[count];
        SpriteRenderer[] srs = new SpriteRenderer[count];
        Vector2[] dirs = new Vector2[count];
        float[] speeds = new float[count];
        float[] sizes = new float[count];

        for (int i = 0; i < count; i++)
        {
            // Spread particles evenly around a full circle
            float angle = (i / (float)count) * Mathf.PI * 2f;
            dirs[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            // Randomise speed and size so the burst looks organic rather than uniform
            speeds[i] = Random.Range(0.7f, 1.35f);
            sizes[i] = Random.Range(0.28f, 0.58f);

            // Pick a random colour from the flame palette
            Color col = flameColors[Random.Range(0, flameColors.Length)];

            GameObject p = new GameObject("fp");
            p.transform.SetParent(transform);
            p.transform.position = transform.position;
            p.transform.localScale = Vector3.one * sizes[i];

            SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
            sr.sprite = _sprite;
            sr.color = col;
            sr.sortingOrder = 10;

            parts[i] = p;
            srs[i] = sr;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Ease-out curve so particles decelerate as they expand
            float eased = 1f - (1f - t) * (1f - t);

            for (int i = 0; i < count; i++)
            {
                if (parts[i] == null) continue;

                // Move outward in burst direction with individual speed variation
                Vector3 outward = (Vector3)(dirs[i] * speed * speeds[i] * eased);

                // Add upward drift that peaks mid-burst and fades — simulates rising heat
                Vector3 rise = Vector3.up * upDrift * eased * (1f - t);

                parts[i].transform.position = transform.position + outward + rise;

                float scale = sizes[i] * Mathf.Lerp(1f, 0.15f, t);
                parts[i].transform.localScale = Vector3.one * scale;

                // use a power curve so particles stay bright longer
                // then vanish quickly at the end rather than dragging out
                Color c = srs[i].color;
                c.a = Mathf.Pow(1f - t, 1.8f);
                srs[i].color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    // Generates a soft radial gradient sprite used for each flame particle.
    private Sprite GenerateCircle(int res)
    {
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[res * res];
        float center = res * 0.5f;

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float t = Mathf.Clamp01(1f - dist / center);

                t = t * t * t;

                pixels[y * res + x] = new Color(1f, 1f, 1f, t);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), Vector2.one * 0.5f);
    }
}