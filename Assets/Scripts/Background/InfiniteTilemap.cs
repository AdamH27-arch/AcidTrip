using UnityEngine;

public class InfiniteTilemap : MonoBehaviour
{
    [Tooltip("Drag your Main Camera here")]
    public Transform target;

    [Tooltip("Must perfectly match your background width in Unity Units!")]
    public float tileSize = 32f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 newPos = transform.position;

        newPos.x = Mathf.Round(target.position.x / tileSize) * tileSize;
        newPos.y = Mathf.Round(target.position.y / tileSize) * tileSize;

        transform.position = newPos;
    }
}