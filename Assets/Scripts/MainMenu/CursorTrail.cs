using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class CursorTrail : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 0.04f;
    [SerializeField] private float dotLifetime = 0.6f;
    [SerializeField] private float dotSize = 14f;

    private Canvas _canvas;
    private RectTransform _canvasRect;
    private Sprite _dotSprite;
    private float _spawnTimer;
    private float _hue;

    private void Start()
    {
        _canvas = FindFirstObjectByType<Canvas>();
        _canvasRect = _canvas.GetComponent<RectTransform>();
        _dotSprite = GenerateCircleSprite(32);
    }

    private void Update()
    {
        _hue = (_hue + 0.4f * Time.deltaTime) % 1f;

        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer > 0f) return;

        SpawnDot();
        _spawnTimer = spawnInterval;
    }

    private void SpawnDot()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect, mouseScreen, null, out Vector2 localPos
        );

        GameObject go = new GameObject("Dot");
        go.transform.SetParent(_canvas.transform, false);

        Image img = go.AddComponent<Image>();
        img.sprite = _dotSprite;
        img.raycastTarget = false;
        img.color = Color.HSVToRGB(_hue, 0.85f, 1f);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(dotSize, dotSize);
        rt.anchoredPosition = localPos;

        StartCoroutine(FadeAndShrink(go, img));
    }

    private IEnumerator FadeAndShrink(GameObject go, Image img)
    {
        Color start = img.color;
        float elapsed = 0f;

        while (elapsed < dotLifetime)
        {
            if (go == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / dotLifetime;

            img.color = new Color(start.r, start.g, start.b, Mathf.Lerp(1f, 0f, t));
            float scale = Mathf.Lerp(1f, 0.1f, t);
            go.transform.localScale = new Vector3(scale, scale, 1f);

            yield return null;
        }

        if (go != null) Destroy(go);
    }

    private Sprite GenerateCircleSprite(int res)
    {
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[res * res];
        float center = res * 0.5f;

        for (int y = 0; y < res; y++)
            for (int x = 0; x < res; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float t = Mathf.Clamp01(1f - dist / center);
                pixels[y * res + x] = new Color(1f, 1f, 1f, t * t);
            }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
    }
}