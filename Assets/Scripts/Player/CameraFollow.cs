using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Setup")]
    [Tooltip("Drag the Player GameObject here")]
    [SerializeField] private Transform target;

    private void LateUpdate()
    {
        if (target != null)
        {
            //Follow the players X and Y
            transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
        }
    }
}