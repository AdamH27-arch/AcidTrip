using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TripBackground : MonoBehaviour
{
    [Header("Blobs")]
    [SerializeField] private int blobCount = 8;
    [SerializeField] private float minBlobSize = 350f;
    [SerializeField] private float maxBlobSize = 650f;
    [SerializeField] private float maxDriftSpeed = 45f;
    [SerializeField] private float blobAlpha = 0.30f;

    [Header("Hue Shift")]
    [SerializeField] private float minHueSpeed = 0.025f;
    [SerializeField] private float maxHueSpeed = 0.06f;

    [Header("Title")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private float titleHueSpeed = 0.04f;

    private class Blob
    {
        public RectTransform rect;
        public Image image;
        public float hue;
        public float hueSpeed;
        public Vector2 velocity;
    }

    private Blob[] _blobs;
    private float _titleHue = 0.5f;

    private void Start()
    {
        Sprite circle = GenerateCircleSprite(128);
        _blobs = new Blob[blobCount];

        for (int i = 0; i < blobCount; i++)
        {
            GameObject go = new GameObject("Blob_" + i);
            go.transform.SetParent(transform, false);

            Image img = go.AddComponent<Image>();
            img.sprite = circle;
            img.raycastTarget = false;

            float size = Random.Range(minBlobSize, maxBlobSize);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = new Vector2(
                Random.Range(-960f, 960f),
                Random.Range(-540f, 540f)
            );

            float hue = Random.value;
            Color c = Color.HSVToRGB(hue, 0.75f, 0.85f);
            img.color = new Color(c.r, c.g, c.b, blobAlpha);

            float angle = Random.Range(0f, Mathf.PI * 2f);
            float speed = Random.Range(10f, maxDriftSpeed);

            _blobs[i] = new Blob
            {
                rect = rt,
                image = img,
                hue = hue,
                hueSpeed = Random.Range(minHueSpeed, maxHueSpeed),
                velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed
            };
        }
    }

    private void Update()
    {
        foreach (Blob blob in _blobs)
        {
            blob.hue = (blob.hue + blob.hueSpeed * Time.deltaTime) % 1f;

            Color c = Color.HSVToRGB(blob.hue, 0.75f, 0.85f);
            blob.image.color = new Color(c.r, c.g, c.b, blobAlpha);

            blob.rect.anchoredPosition += blob.velocity * Time.deltaTime;

            Vector2 pos = blob.rect.anchoredPosition;
            float r = blob.rect.sizeDelta.x * 0.5f;

            if (pos.x - r > 960f) pos.x = -960f - r;
            if (pos.x + r < -960f) pos.x = 960f + r;
            if (pos.y - r > 540f) pos.y = -540f - r;
            if (pos.y + r < -540f) pos.y = 540f + r;

            blob.rect.anchoredPosition = pos;
        }

        if (titleText == null) return;

        _titleHue = (_titleHue + titleHueSpeed * Time.deltaTime) % 1f;
        titleText.color = Color.HSVToRGB(_titleHue, 0.85f, 1f);
    }

    private Sprite GenerateCircleSprite(int res)
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
                t = t * t;
                pixels[y * res + x] = new Color(1f, 1f, 1f, t);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
    }

    public void SetTitleText(TMP_Text text)
    {
        titleText = text;
    }
}