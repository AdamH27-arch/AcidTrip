using UnityEngine;

using UnityEngine.InputSystem;
public class AdamCrosshair : MonoBehaviour
{
    public Transform player;
    public float orbitDistance = 2f;

    // Update is called once per frame
    void Update()
    {
        //upon player death, destroy the crosshair object to prevent an error.
        if(player == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePosition.z = 0;

        Vector3 direction = (mousePosition - player.position).normalized;
        transform.position = player.position + direction * orbitDistance;
    }
}