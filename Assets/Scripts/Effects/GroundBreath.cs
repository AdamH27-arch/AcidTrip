using UnityEngine;
using UnityEngine.Tilemaps;

// Attach to the InfiniteBackgroundGrid parent.
// All chunk Tilemaps share one hue so repositioned chunks
// always match the rest of the map seamlessly.
public class GroundBreath : MonoBehaviour
{
    [Header("Hue Cycling")]
    [SerializeField] private float hueSpeed = 0.02f;
    [SerializeField] private float saturation = 0.75f;

    [Header("Breathing")]
    [SerializeField] private float breatheSpeed = 0.25f;
    [SerializeField] private float minValue = 0.45f;
    [SerializeField] private float maxValue = 0.7f;

    private Tilemap[] _tilemaps;
    private float _hue;

    private void Start()
    {
        _tilemaps = GetComponentsInChildren<Tilemap>();
        _hue = Random.value;
    }

    private void Update()
    {
        // Advance a single shared hue
        _hue = (_hue + hueSpeed * Time.deltaTime) % 1f;

        float breathe = (Mathf.Sin(Time.time * breatheSpeed) + 1f) * 0.5f;
        float value = Mathf.Lerp(minValue, maxValue, breathe);

        // Apply the exact same colour to every chunk
        Color col = Color.HSVToRGB(_hue, saturation, value);

        for (int i = 0; i < _tilemaps.Length; i++)
            _tilemaps[i].color = col;
    }
}