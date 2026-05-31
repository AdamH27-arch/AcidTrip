using UnityEngine;
using UnityEngine.UI;

// True chromatic aberration -- captures the scene and displays it three times
// with red, green, and blue tints offset from each other.
// Requires a second camera in the scene assigned in the Inspector.
public class ChromaticAberration : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;

    [Header("Offset")]
    [SerializeField] private float baseOffset = 5f;
    [SerializeField] private float maxOffset = 30f;
    [SerializeField] private float pulseSpeed = 1.5f;

    [Header("Opacity")]
    [SerializeField] private float baseAlpha = 0.35f;
    [SerializeField] private float maxAlpha = 0.75f;

    [Header("Health")]
    [SerializeField] private float lowHPThreshold = 0.35f;

    private RenderTexture _rt;
    private Camera _captureCam;
    private RawImage _redChannel;
    private RawImage _blueChannel;
    private PlayerHealth _playerHealth;

    private void Start()
    {
        _playerHealth = FindAnyObjectByType<PlayerHealth>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        SetupCaptureCam();
        BuildCanvas();

        Debug.Log("ChromaticAberration started. RT size: " + _rt.width + "x" + _rt.height);
    }

    private void SetupCaptureCam()
    {
        // Render texture that matches screen resolution
        _rt = new RenderTexture(Screen.width, Screen.height, 16);

        // Create a second camera that captures the scene to the RT
        GameObject camGo = new GameObject("ChromaticCaptureCam");
        _captureCam = camGo.AddComponent<Camera>();
        _captureCam.CopyFrom(mainCamera);
        _captureCam.targetTexture = _rt;
        _captureCam.depth = mainCamera.depth - 1;
    }

    private void LateUpdate()
    {
        // Keep capture camera in sync with main camera
        if (_captureCam != null && mainCamera != null)
        {
            _captureCam.transform.position = mainCamera.transform.position;
            _captureCam.transform.rotation = mainCamera.transform.rotation;
            _captureCam.orthographicSize = mainCamera.orthographicSize;
        }

        float hp = GetHpPct();
        float danger = hp < lowHPThreshold
            ? Mathf.InverseLerp(lowHPThreshold, 0f, hp)
            : 0f;

        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float offset = Mathf.Lerp(baseOffset, maxOffset, danger) * (0.7f + pulse * 0.3f);
        float alpha = Mathf.Lerp(baseAlpha, maxAlpha, danger);

        // Red shifts left/up, blue shifts right/down
        _redChannel.rectTransform.anchoredPosition = new Vector2(-offset, offset * 0.5f);
        _blueChannel.rectTransform.anchoredPosition = new Vector2(offset, -offset * 0.5f);

        _redChannel.color = new Color(1f, 0f, 0f, alpha);
        _blueChannel.color = new Color(0f, 0f, 1f, alpha);
    }

    private void BuildCanvas()
    {
        GameObject canvasGo = new GameObject("ChromaticCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        // Blue behind red
        _blueChannel = CreateChannel("Blue", canvasGo.transform);
        _redChannel = CreateChannel("Red", canvasGo.transform);
    }

    private RawImage CreateChannel(string name, Transform parent)
    {
        GameObject go = new GameObject(name + "Channel");
        go.transform.SetParent(parent, false);

        RawImage img = go.AddComponent<RawImage>();
        img.texture = _rt;
        img.raycastTarget = false;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        return img;
    }

    private float GetHpPct()
    {
        if (_playerHealth == null) return 1f;
        return (float)_playerHealth.CurrentHealth / _playerHealth.MaxHealth;
    }

    private void OnDestroy()
    {
        if (_rt != null) _rt.Release();
        if (_captureCam != null) Destroy(_captureCam.gameObject);
    }
}