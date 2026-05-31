using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Renders a small directional arrow at the screen edge for every off-screen enemy.
// Enemy.cs calls Register and Unregister automatically.
public class OffScreenIndicator : MonoBehaviour
{
    public static OffScreenIndicator Instance;

    [Header("References")]
    [SerializeField] private Canvas _targetCanvas;      // assign your existing canvas, or leave empty to auto-create one
    [SerializeField] private Sprite _customArrowSprite; // assign your arrow sprite, or leave empty to use the generated triangle

    [Header("Visual")]
    [SerializeField] private float arrowSize = 28f;
    [SerializeField] private float edgePadding = 55f;
    [SerializeField] private float arrowAlpha = 0.85f;

    private Camera _cam;
    private Canvas _canvas;

    private readonly Dictionary<Enemy, RectTransform> _arrows = new Dictionary<Enemy, RectTransform>();
    private readonly Dictionary<Enemy, Image> _images = new Dictionary<Enemy, Image>();

    private Sprite _arrowSprite;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _cam = Camera.main;

        // Use assigned canvas if provided, otherwise build a dedicated one
        _canvas = _targetCanvas != null ? _targetCanvas : BuildCanvas();

        // Use assigned sprite if provided, otherwise generate a triangle
        _arrowSprite = _customArrowSprite != null ? _customArrowSprite : GenerateTriangle(32);
    }

    private void LateUpdate()
    {
        if (_cam == null) return;

        List<Enemy> dead = new List<Enemy>();

        foreach (var kvp in _arrows)
        {
            Enemy enemy = kvp.Key;

            // Queue up destroyed enemies for cleanup
            if (enemy == null) { dead.Add(enemy); continue; }

            RectTransform rt = kvp.Value;
            Image img = _images[enemy];

            Vector3 vp = _cam.WorldToViewportPoint(enemy.transform.position);

            bool offScreen = vp.z < 0 || vp.x < 0 || vp.x > 1 || vp.y < 0 || vp.y > 1;
            rt.gameObject.SetActive(offScreen);

            if (!offScreen) continue;

            // When enemy is behind camera, flip the viewport direction
            if (vp.z < 0) { vp.x = 1 - vp.x; vp.y = 1 - vp.y; }

            // Direction from screen center to enemy in -1 to 1 space
            Vector2 dir = new Vector2(vp.x - 0.5f, vp.y - 0.5f) * 2f;

            // Rotate arrow to face the enemy
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rt.localRotation = Quaternion.Euler(0f, 0f, angle);

            // Normalise so the largest component equals 1
            // This snaps the arrow to the nearest screen edge
            float maxComp = Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
            Vector2 edgeDir = dir / maxComp;

            float halfW = Screen.width * 0.5f - edgePadding;
            float halfH = Screen.height * 0.5f - edgePadding;

            rt.position = new Vector3(
                Screen.width * 0.5f + edgeDir.x * halfW,
                Screen.height * 0.5f + edgeDir.y * halfH,
                0f
            );
        }

        // Remove arrows for enemies that have been destroyed
        foreach (Enemy e in dead)
        {
            if (_arrows.ContainsKey(e) && _arrows[e] != null)
                Destroy(_arrows[e].gameObject);
            _arrows.Remove(e);
            _images.Remove(e);
        }
    }

    // Called from Enemy.Start()
    public void Register(Enemy enemy)
    {
        if (_arrows.ContainsKey(enemy)) return;

        // Use the enemy's data colour so each type has its own arrow colour
        Color col = (enemy.Data != null) ? enemy.Data.color : Color.red;
        col.a = arrowAlpha;

        GameObject go = new GameObject("Indicator");
        go.transform.SetParent(_canvas.transform, false);
        go.SetActive(false);

        Image img = go.AddComponent<Image>();
        img.sprite = _arrowSprite;
        img.color = col;
        img.raycastTarget = false;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(arrowSize, arrowSize);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        _arrows[enemy] = rt;
        _images[enemy] = img;
    }

    // Called from Enemy.Die() before Destroy
    public void Unregister(Enemy enemy)
    {
        if (!_arrows.ContainsKey(enemy)) return;
        if (_arrows[enemy] != null) Destroy(_arrows[enemy].gameObject);
        _arrows.Remove(enemy);
        _images.Remove(enemy);
    }

    private Canvas BuildCanvas()
    {
        GameObject go = new GameObject("IndicatorCanvas");
        Canvas c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 25;
        go.AddComponent<GraphicRaycaster>();
        return c;
    }

    // Generates a right-pointing triangle sprite procedurally
    // The arrow rotation in LateUpdate is relative to this base direction
    private Sprite GenerateTriangle(int res)
    {
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[res * res];

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                // Triangle: bottom-left (0,0)  top-left (0,res)  tip (res, res/2)
                bool above = y >= x * 0.5f;           // above bottom edge to tip
                bool below = y <= res - x * 0.5f;     // below top edge to tip
                pixels[y * res + x] = (above && below) ? Color.white : Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
    }
}