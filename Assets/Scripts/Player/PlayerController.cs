using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 22f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.8f;


    private Rigidbody2D _rb;
    private Camera _cam;

    private Vector2 _moveInput;
    private Vector2 _aimDir;
    private Vector2 _mouseWorld;

    private bool _isDashing;
    private float _dashTimer;
    private float _dashCooldownTimer;
    private Vector2 _dashDir;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _cam = Camera.main;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Update()
    {
        ReadMoveInput();
        ReadMouseAim();
        HandleDashInput();
        HandlePowerMoveInput();
        TickTimers();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

    private void ReadMoveInput()
    {
        var kb = Keyboard.current;
        float h = 0f;
        float v = 0f;

        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) h -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v -= 1f;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v += 1f;

        _moveInput = new Vector2(h, v).normalized;
    }

    private void ReadMouseAim()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouse = _cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, 0f));
        _mouseWorld = new Vector2(mouse.x, mouse.y);

        Vector2 toMouse = _mouseWorld - (Vector2)transform.position;
        _aimDir = toMouse.sqrMagnitude > 0.001f ? toMouse.normalized : Vector2.up;
    }

    private void HandleDashInput()
    {
        if (!Keyboard.current.leftShiftKey.wasPressedThisFrame) return;
        if (_isDashing) return;
        if (_dashCooldownTimer > 0f) return;

        _dashDir = _moveInput.sqrMagnitude > 0.01f ? _moveInput : _aimDir;
        _isDashing = true;
        _dashTimer = dashDuration;
        _dashCooldownTimer = dashCooldown;

        OnDashStart();
    }

    private void HandlePowerMoveInput()
    {
        var kb = Keyboard.current;
        if (kb.digit1Key.wasPressedThisFrame) OnPowerMove(1);
        if (kb.digit2Key.wasPressedThisFrame) OnPowerMove(2);
        if (kb.digit3Key.wasPressedThisFrame) OnPowerMove(3);
        if (kb.digit4Key.wasPressedThisFrame) OnPowerMove(4);
    }

    private void ApplyMovement()
    {
        if (_isDashing)
        {
            _rb.linearVelocity = _dashDir * dashSpeed;
        }
        else
        {
            _rb.linearVelocity = _moveInput * moveSpeed;
        }
    }

 

    private void TickTimers()
    {
        if (_dashCooldownTimer > 0f)
            _dashCooldownTimer -= Time.deltaTime;

        if (_isDashing)
        {
            _dashTimer -= Time.deltaTime;
            if (_dashTimer <= 0f)
            {
                _isDashing = false;
                OnDashEnd();
            }
        }
    }

    private void OnDashStart()
    {
        Debug.Log("Dash started");
    }

    private void OnDashEnd()
    {
        Debug.Log("Dash ended");
    }

    private void OnPowerMove(int slot)
    {
        Debug.Log("Power move slot " + slot + " -- not yet implemented");
    }

    public Vector2 AimDirection => _aimDir;
    public Vector2 MouseWorldPos => _mouseWorld;
    public bool IsDashing => _isDashing;

    private float GetMoveSpeed() => UpgradeSystem.Instance != null ? UpgradeSystem.Instance.MoveSpeed : moveSpeed;
    private float GetDashDuration() => UpgradeSystem.Instance != null ? UpgradeSystem.Instance.DashDuration : dashDuration;
    private float GetDashCooldown() => UpgradeSystem.Instance != null ? UpgradeSystem.Instance.DashCooldown : dashCooldown;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, (Vector3)_aimDir * 1.5f);
    }
#endif
}