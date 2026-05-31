using UnityEngine;

public class InfiniteBackgroundGrid : MonoBehaviour
{
    public Transform target;
    public float chunkSize = 32f;
    public int columns = 3;
    public int rows = 3;

    // Use this to compensate for the player not starting at world origin.
    // e.g. if player starts at x = -20, set offsetX = 20 to re-centre the grid.
    public float offsetX = 20f;
    public float offsetY = 0f;

    private void LateUpdate()
    {
        float gridWidth  = chunkSize * columns;
        float gridHeight = chunkSize * rows;
        float threshold  = chunkSize * (columns - 1); // 64

        foreach (Transform child in transform)
        {
            Vector3 pos = child.position;

            float distX = (target.position.x + offsetX) - pos.x;
            float distY = (target.position.y + offsetY) - pos.y;

            if (distX >  threshold) pos.x += gridWidth;
            if (distX < -threshold) pos.x -= gridWidth;
            if (distY >  threshold) pos.y += gridHeight;
            if (distY < -threshold) pos.y -= gridHeight;

            child.position = pos;
        }
    }
}