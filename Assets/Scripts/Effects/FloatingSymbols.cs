using System.Collections;
using UnityEngine;

// Spawns symbol sprites at random positions within camera view.
// They drift upward, wobble slightly, and fade out.
// Assign your symbol sprites (eyes, spirals, crosses, etc.) in the Inspector.
public class FloatingSymbols : MonoBehaviour
{
    [Header("Symbols")]
    [SerializeField] public Sprite[] symbols;

    [Header("Spawn")]
    [SerializeField] private float minInterval = 1.5f;
    [SerializeField] private float maxInterval = 4f;
    [SerializeField] private int maxAtOnce = 6;

    [Header("Movement")]
    [SerializeField] private float minSpeed = 0.3f;
    [SerializeField] private float maxSpeed = 0.7f;
    [SerializeField] private float minLifetime = 3f;
    [SerializeField] private float maxLifetime = 6f;

    [Header("Visuals")]
    [SerializeField] private float minScale = 0.15f;
    [SerializeField] private float maxScale = 0.5f;
    [SerializeField] private float peakAlpha = 0.55f;
    [SerializeField] private int sortingOrder = 50;

    private Camera _cam;
    private int _active;

    private void Start()
    {
        _cam = Camera.main;

        if (symbols == null || symbols.Length == 0)
        {
            Debug.LogWarning("FloatingSymbols: no sprites assigned.");
            return;
        }

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));

            if (_active < maxAtOnce)
                StartCoroutine(FloatSymbol());
        }
    }

    private IEnumerator FloatSymbol()
    {
        _active++;

        // Random position within camera bounds
        float h = _cam.orthographicSize;
        float w = h * _cam.aspect;
        Vector3 pos = _cam.transform.position
                    + new Vector3(Random.Range(-w, w), Random.Range(-h, h), 0f);
        pos.z = 0f;

        GameObject go = new GameObject("Symbol");
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * Random.Range(minScale, maxScale);

        // Random rotation for variety
        go.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = symbols[Random.Range(0, symbols.Length)];
        sr.color = new Color(1f, 1f, 1f, 0f);
        sr.sortingOrder = sortingOrder;

        float speed = Random.Range(minSpeed, maxSpeed);
        float lifetime = Random.Range(minLifetime, maxLifetime);
        float elapsed = 0f;
        float wobble = Random.Range(-0.4f, 0.4f);
        float phase = Random.Range(0f, Mathf.PI * 2f);

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;

            // Drift upward with a gentle horizontal wobble
            go.transform.position += new Vector3(
                Mathf.Sin(Time.time * 1.5f + phase) * wobble * Time.deltaTime,
                speed * Time.deltaTime,
                0f
            );

            // Slow rotation while drifting
            go.transform.Rotate(0f, 0f, 8f * Time.deltaTime);

            // Fade in fast then fade out slowly
            float alpha = t < 0.15f
                ? Mathf.Lerp(0f, peakAlpha, t / 0.15f)
                : Mathf.Lerp(peakAlpha, 0f, (t - 0.15f) / 0.85f);

            Color c = sr.color;
            c.a = alpha;
            sr.color = c;

            yield return null;
        }

        Destroy(go);
        _active--;
    }
}