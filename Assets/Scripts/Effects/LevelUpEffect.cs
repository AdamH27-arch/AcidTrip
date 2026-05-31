using System.Collections;
using UnityEngine;
using TMPro;

public class LevelUpEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private XPSystem xpSystem;
    [SerializeField] private Transform playerTransform;

    [Header("Ring Burst")]
    [SerializeField] private int particleCount = 16;
    [SerializeField] private float burstRadius = 3f;
    [SerializeField] private float burstDuration = 0.5f;

    [Header("Text")]
    [SerializeField] private float textRiseSpeed = 2f;
    [SerializeField] private float textLifetime = 1.2f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip levelUpSound;
    [SerializeField][Range(0f, 1f)] private float levelUpSoundVolume = 1f;

    private static Sprite _circle;

    private void Start()
    {
        if (xpSystem == null)
            xpSystem = FindFirstObjectByType<XPSystem>();

        if (playerTransform == null)
            playerTransform = GameObject.FindWithTag("Player")?.transform;

        xpSystem.OnLevelUp.AddListener(OnLevelUp);
    }

    private void OnLevelUp(int level)
    {
        if (playerTransform == null) return;
        Vector3 pos = playerTransform.position;

        if (levelUpSound != null)
            AudioSource.PlayClipAtPoint(levelUpSound, pos, levelUpSoundVolume);

        StartCoroutine(RingBurst(pos));
        StartCoroutine(FloatingText("LEVEL " + level, pos));
    }

    private IEnumerator RingBurst(Vector3 origin)
    {
        if (_circle == null) _circle = GenerateCircle(16);

        GameObject[] parts = new GameObject[particleCount];
        SpriteRenderer[] srs = new SpriteRenderer[particleCount];
        Vector2[] dirs = new Vector2[particleCount];

        for (int i = 0; i < particleCount; i++)
        {
            float angle = (i / (float)particleCount) * Mathf.PI * 2f;
            dirs[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            float hue = (float)i / particleCount;
            Color col = Color.HSVToRGB(hue, 0.85f, 1f);

            GameObject go = new GameObject("LvlParticle");
            go.transform.position = origin;
            go.transform.localScale = Vector3.one * Random.Range(0.12f, 0.22f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _circle;
            sr.color = col;
            sr.sortingOrder = 20;

            parts[i] = go;
            srs[i] = sr;
        }

        float elapsed = 0f;

        while (elapsed < burstDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / burstDuration;
            float eased = 1f - (1f - t) * (1f - t);

            for (int i = 0; i < particleCount; i++)
            {
                if (parts[i] == null) continue;

                parts[i].transform.position = origin + (Vector3)(dirs[i] * burstRadius * eased);

                Color c = srs[i].color;
                c.a = 1f - t;
                srs[i].color = c;
            }

            yield return null;
        }

        foreach (GameObject go in parts)
            if (go != null) Destroy(go);
    }

    private IEnumerator FloatingText(string message, Vector3 origin)
    {
        GameObject go = new GameObject("LvlText");
        go.transform.position = origin + Vector3.up * 0.5f;

        TextMeshPro tmp = go.AddComponent<TextMeshPro>();
        tmp.text = message;
        tmp.fontSize = 5f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.sortingOrder = 20;

        go.GetComponent<RectTransform>().sizeDelta = new Vector2(6f, 2f);

        float elapsed = 0f;
        float hue = 0f;

        while (elapsed < textLifetime)
        {
            elapsed += Time.deltaTime;
            hue = (hue + Time.deltaTime * 0.8f) % 1f;
            float t = elapsed / textLifetime;

            go.transform.position += Vector3.up * textRiseSpeed * Time.deltaTime;

            Color c = Color.HSVToRGB(hue, 0.9f, 1f);
            c.a = t < 0.2f ? t / 0.2f : 1f - ((t - 0.2f) / 0.8f);
            tmp.color = c;

            yield return null;
        }

        Destroy(go);
    }

    private Sprite GenerateCircle(int res)
    {
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[res * res];
        float center = res * 0.5f;

        for (int y = 0; y < res; y++)
            for (int x = 0; x < res; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float t = Mathf.Clamp01(1f - d / center);
                pixels[y * res + x] = new Color(1f, 1f, 1f, t * t);
            }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), Vector2.one * 0.5f);
    }

    private void OnDestroy()
    {
        if (xpSystem != null)
            xpSystem.OnLevelUp.RemoveListener(OnLevelUp);
    }
}