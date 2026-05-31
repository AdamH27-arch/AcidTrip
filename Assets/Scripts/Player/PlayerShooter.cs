using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Stats")]
    [SerializeField] private float fireRate = 0.1f;

    private float GetFireInterval() => UpgradeSystem.Instance != null ? UpgradeSystem.Instance.FireInterval : fireRate;
    private PlayerController _controller;
    private float _fireCooldown;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (_fireCooldown > 0f)
            _fireCooldown -= Time.deltaTime;

        if (Mouse.current.leftButton.isPressed)
            TryShoot();
    }

    private void TryShoot()
    {
        if (_fireCooldown > 0f) return;
        if (bulletPrefab == null) return;

        Vector2 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector2 direction = _controller.AimDirection;

        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        bullet.GetComponent<Bullet>().SetDirection(direction);
        Bullet b = bullet.GetComponent<Bullet>();

        if (UpgradeSystem.Instance != null)
            b.SetDamage(UpgradeSystem.Instance.Damage);


        _fireCooldown = GetFireInterval();
    }
}