using UnityEngine;
using UnityEngine.InputSystem;

public class Crosshair : MonoBehaviour
{
    [Header("Shape")]
    [SerializeField] private float size = 0.25f;
    [SerializeField] private float lineWidth = 0.04f;

    [Header("Style")]
    [SerializeField] private Color color = Color.white;
    [SerializeField] private int sortingOrder = 100;
    [SerializeField] private string sortingLayer = "Default";

    private LineRenderer _arm1;
    private LineRenderer _arm2;
    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;
        _arm1 = BuildLine("Arm1");
        _arm2 = BuildLine("Arm2");

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void Update()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouse = _cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, 0f));
        mouse.z = 0f;
        transform.position = mouse;

        float s = size;

        _arm1.SetPosition(0, mouse + new Vector3(-s, s, 0f));
        _arm1.SetPosition(1, mouse + new Vector3(s, -s, 0f));

        _arm2.SetPosition(0, mouse + new Vector3(s, s, 0f));
        _arm2.SetPosition(1, mouse + new Vector3(-s, -s, 0f));
    }

    private void OnDestroy()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private LineRenderer BuildLine(string goName)
    {
        var go = new GameObject(goName);
        go.transform.SetParent(transform, false);

        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.startColor = color;
        lr.endColor = color;
        lr.sortingLayerName = sortingLayer;
        lr.sortingOrder = sortingOrder;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.material.color = color;

        return lr;
    }

    public Vector3 WorldPosition => transform.position;
}