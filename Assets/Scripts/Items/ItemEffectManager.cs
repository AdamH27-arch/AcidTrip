using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Handles all item effects that need coroutines or persistent state.
// Attach to any persistent GameObject.
public class ItemEffectManager : MonoBehaviour
{
    public static ItemEffectManager Instance;

    private PlayerHealth _playerHealth;
    private Transform _playerTransform;
    private GameObject _paranoiaOverlay;
    private GameObject _shieldBubble;
    private SpriteRenderer _shieldRenderer;

    private static Sprite _paranoiaSprite;
    private static Sprite _bubbleSprite;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _playerHealth = FindFirstObjectByType<PlayerHealth>();
        _playerTransform = _playerHealth?.transform;

        BuildShieldBubble();
    }

    // -- Entry point called from DroppedItem ----------------------------------

    public void Apply(ItemData data, Collider2D player)
    {

        switch (data.type)
        {

            case ItemType.HealthPotion:
                player.GetComponent<PlayerHealth>()?.Heal((int)data.primaryValue);
                break;

            case ItemType.Shield:
                player.GetComponent<PlayerHealth>()?.ActivateShield(data.primaryValue);
                ActivateShieldBubble(data.primaryValue);
                break;

            case ItemType.TimeHiccup:
                StartCoroutine(TimeHiccupRoutine(data.primaryValue));
                break;

            case ItemType.GravityFlip:
                StartCoroutine(GravityFlipRoutine(data.primaryValue));
                break;

            case ItemType.XPVacuum:
                StartCoroutine(XPVacuumRoutine());
                break;

            case ItemType.Paranoia:
                StartCoroutine(ParanoiaRoutine(data.primaryValue));
                break;
        }
    }

    // -- Time Hiccup ----------------------------------------------------------

    // Slows everything to 20% speed while keeping the player at normal speed
    private IEnumerator TimeHiccupRoutine(float duration)
    {
        // Slow only the enemies -- player is completely unaffected
        Enemy.SetGlobalSpeedMultiplier(0.2f);

        yield return new WaitForSeconds(duration);

        Enemy.SetGlobalSpeedMultiplier(1f);
    }

    // -- Gravity Flip ---------------------------------------------------------

    // Reverses all enemy movement direction for the duration
    private IEnumerator GravityFlipRoutine(float duration)
    {
        Enemy.SetGravityFlipped(true);
        yield return new WaitForSeconds(duration);
        Enemy.SetGravityFlipped(false);
    }

    // -- XP Vacuum ------------------------------------------------------------

    // Pulls every XP orb in the scene toward the player
    private IEnumerator XPVacuumRoutine()
    {
        if (_playerTransform == null)
        {
            Debug.LogWarning("XP Vacuum: player transform is null");
            yield break;
        }

        // Copy the list first since collecting orbs modifies it mid-loop
        List<XPOrb> orbs = new List<XPOrb>(XPOrb.AllOrbs);

        Debug.Log("XP Vacuum: found " + orbs.Count + " orbs");

        foreach (XPOrb orb in orbs)
        {
            if (orb != null)
                orb.Vacuum(_playerTransform);
        }

        yield return null;
    }

    // -- Paranoia -------------------------------------------------------------

    // Covers the screen with darkness except for a small transparent circle
    // around the player using a world-space sprite overlay
    private IEnumerator ParanoiaRoutine(float duration)
    {
        if (_paranoiaSprite == null)
            _paranoiaSprite = GenerateParanoiaSprite(256);

        // Screen Space Overlay canvas renders on top of all world sprites
        GameObject canvasGo = new GameObject("ParanoiaCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        // Full screen image using the radial gradient sprite
        GameObject imageGo = new GameObject("ParanoiaOverlay");
        imageGo.transform.SetParent(canvasGo.transform, false);

        UnityEngine.UI.Image img = imageGo.AddComponent<UnityEngine.UI.Image>();
        img.sprite = _paranoiaSprite;
        img.color = Color.white;
        img.raycastTarget = false;

        // Stretch to fill the entire screen
        RectTransform rt = imageGo.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        yield return new WaitForSeconds(duration);

        Destroy(canvasGo);
    }

    // Radial gradient: transparent at center, opaque black at edges
    private Sprite GenerateParanoiaSprite(int res)
    {
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[res * res];
        float center = res * 0.5f;
        float inner = res * 0.18f; // size of the visible window
        float outer = center;

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float t = Mathf.Clamp01((dist - inner) / (outer - inner));

                // Ease in so the edge of the visible area is soft
                t = t * t;
                pixels[y * res + x] = new Color(0f, 0f, 0f, t);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        return Sprite.Create(tex, new Rect(0, 0, res, res), Vector2.one * 0.5f);
    }

    // -- Shield Bubble --------------------------------------------------------

    // Creates the bubble as a child of the player, hidden by default
    private void BuildShieldBubble()
    {
        if (_playerTransform == null) return;

        if (_bubbleSprite == null)
            _bubbleSprite = GenerateBubbleSprite(128);

        _shieldBubble = new GameObject("ShieldBubble");
        _shieldBubble.transform.SetParent(_playerTransform, false);
        _shieldBubble.transform.localPosition = Vector3.zero;
        _shieldBubble.transform.localScale = Vector3.one * 1.8f;

        _shieldRenderer = _shieldBubble.AddComponent<SpriteRenderer>();
        _shieldRenderer.sprite = _bubbleSprite;
        _shieldRenderer.color = new Color(0.3f, 0.7f, 1f, 0.45f);
        _shieldRenderer.sortingOrder = 10;

        _shieldBubble.SetActive(false);
    }

    public void ActivateShieldBubble(float duration)
    {
        if (_shieldBubble == null) return;
        StartCoroutine(ShieldBubbleRoutine(duration));
    }

    // Shows the bubble and pulses its scale while the shield is active
    private IEnumerator ShieldBubbleRoutine(float duration)
    {
        _shieldBubble.SetActive(true);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            // Slow pulse between 1.7 and 2.0 scale
            float pulse = 1.85f + Mathf.Sin(elapsed * 4f) * 0.15f;
            _shieldBubble.transform.localScale = Vector3.one * pulse;
            elapsed += Time.deltaTime;
            yield return null;
        }

        _shieldBubble.SetActive(false);
    }

    // Soft blue circle with transparent center hole and glowing edge
    private Sprite GenerateBubbleSprite(int res)
    {
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[res * res];
        float center = res * 0.5f;
        float inner = center * 0.7f;
        float outer = center * 0.95f;

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float t = Mathf.Clamp01((dist - inner) / (outer - inner));

                // Ring shape: transparent inside and outside, opaque at the ring itself
                float ring = Mathf.Sin(t * Mathf.PI);
                pixels[y * res + x] = new Color(1f, 1f, 1f, ring);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        return Sprite.Create(tex, new Rect(0, 0, res, res), Vector2.one * 0.5f);
    }
}