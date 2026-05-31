using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    private TMP_Text _text;
    private Color _color;
    private float _elapsed;
    private float _lifetime = 0.8f;
    private float _fadeStart = 0.35f;
    private float _floatSpeed = 1.8f;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }

    public void Setup(int damage, Vector3 position)
    {
        transform.position = position + new Vector3(
            Random.Range(-0.25f, 0.25f), 0.3f, 0f
        );

        // Give the RectTransform an explicit size so the canvas
        // renderer can build a valid bounding box
        GetComponent<RectTransform>().sizeDelta = new Vector2(3f, 1.5f);

        if (damage >= 50) _color = new Color(1f, 0.2f, 0.2f);
        else if (damage >= 25) _color = new Color(1f, 0.75f, 0.1f);
        else _color = Color.white;

        _text.text = damage.ToString();
        _text.color = _color;
        _text.fontSize = damage >= 50 ? 5f : 3.5f;

        Destroy(gameObject, _lifetime);
    }

    // Called when a specific colour is required regardless of damage amount
    public void Setup(int damage, Color forcedColor)
    {
        GetComponent<RectTransform>().sizeDelta = new Vector2(3f, 1.5f);
        _text.text = damage.ToString();
        _text.color = forcedColor;
        _text.fontSize = 3.5f;
        Destroy(gameObject, _lifetime);
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;
        transform.position += Vector3.up * _floatSpeed * Time.deltaTime;

        if (_elapsed > _fadeStart)
        {
            float t = (_elapsed - _fadeStart) / (_lifetime - _fadeStart);
            _text.color = new Color(_color.r, _color.g, _color.b, 1f - t);
        }
    }
}