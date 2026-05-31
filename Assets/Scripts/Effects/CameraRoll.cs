using System.Collections;
using UnityEngine;

// Occasionally tilts the camera a few degrees then slowly rolls back to level.
// Attach directly to the Camera GameObject.
public class CameraRoll : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float minInterval = 6f;   // min seconds between tilts
    [SerializeField] private float maxInterval = 18f;  // max seconds between tilts
    [SerializeField] private float holdDuration = 1.5f; // how long it stays tilted

    [Header("Tilt")]
    [SerializeField] private float minAngle = 1.5f;    // minimum tilt in degrees
    [SerializeField] private float maxAngle = 4.5f;    // maximum tilt in degrees

    [Header("Speed")]
    [SerializeField] private float tiltSpeed = 2f;   // how fast it tilts over
    [SerializeField] private float returnSpeed = 0.6f; // how slowly it returns to level

    [Header("Low Health")]
    [SerializeField] private float lowHPThreshold = 0.3f;
    [SerializeField] private float lowHPAngleBonus = 4f;  // extra degrees at low health
    [SerializeField] private float lowHPSpeedBonus = 2f;  // faster interval at low health

    private float _targetAngle;
    private float _currentAngle;
    private float _velocity;
    private PlayerHealth _playerHealth;

    private void Start()
    {
        _playerHealth = FindAnyObjectByType<PlayerHealth>();
        StartCoroutine(TiltLoop());
    }

    private void LateUpdate()
    {
        // Use SmoothDamp for organic ease -- different speed depending on
        // whether tilting over or returning to level
        float speed = Mathf.Abs(_targetAngle) > 0.01f ? tiltSpeed : returnSpeed;

        _currentAngle = Mathf.SmoothDamp(
            _currentAngle,
            _targetAngle,
            ref _velocity,
            1f / speed
        );

        // Apply roll on the Z axis -- X and Y stay as they are
        Vector3 euler = transform.eulerAngles;
        euler.z = _currentAngle;
        transform.eulerAngles = euler;
    }

    private IEnumerator TiltLoop()
    {
        while (true)
        {
            float hp = GetHpPct();
            float danger = hp < lowHPThreshold
                ? Mathf.InverseLerp(lowHPThreshold, 0f, hp)
                : 0f;

            // At low health events happen more frequently
            float interval = Mathf.Lerp(
                Random.Range(minInterval, maxInterval),
                Random.Range(1.5f, 4f),
                danger
            );

            yield return new WaitForSeconds(interval);

            // Random direction, random angle -- stronger at low health
            float direction = Random.value > 0.5f ? 1f : -1f;
            float angle = Random.Range(minAngle, maxAngle)
                            + danger * lowHPAngleBonus;

            _targetAngle = direction * angle;

            // Hold the tilt briefly
            yield return new WaitForSeconds(holdDuration + danger * 0.5f);

            // Return to level
            _targetAngle = 0f;
        }
    }

    private float GetHpPct()
    {
        if (_playerHealth == null) return 1f;
        return (float)_playerHealth.CurrentHealth / _playerHealth.MaxHealth;
    }
}