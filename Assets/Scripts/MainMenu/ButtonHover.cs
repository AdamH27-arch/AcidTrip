using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Image _image;
    private Outline _outline;

    private static readonly Color NormalBg = new Color(0f, 0f, 0f, 0.55f);
    private static readonly Color HoverBg = new Color(1f, 1f, 1f, 0.10f);
    private static readonly Color ClickBg = new Color(1f, 1f, 1f, 0.18f);
    private static readonly Color NormalBorder = new Color(1f, 1f, 1f, 0.18f);
    private static readonly Color HoverBorder = new Color(1f, 1f, 1f, 0.45f);

    private void Awake()
    {
        _image = GetComponent<Image>();
        _outline = GetComponent<Outline>();
        SetNormal();
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (_image != null) _image.color = HoverBg;
        if (_outline != null) _outline.effectColor = HoverBorder;
    }

    public void OnPointerExit(PointerEventData e) => SetNormal();

    public void OnPointerDown(PointerEventData e)
    {
        if (_image != null) _image.color = ClickBg;
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (_image != null) _image.color = HoverBg;
    }

    private void SetNormal()
    {
        if (_image != null) _image.color = NormalBg;
        if (_outline != null) _outline.effectColor = NormalBorder;
    }
}