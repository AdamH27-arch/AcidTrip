using Unity.Hierarchy;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Dash")]
    public float dashSpeed = 12f;
    public float dashDuration = 0.3f;
    public float dashCooldown = 0.8f;

    [Header("References")]
    [SerializeField] public GameObject projectilePrefab;
    public Transform swordTip;
    public SwordHitBox swordHitbox;

    private Rigidbody2D rb;
    private Animator animator;
    private AudioSource audioSource;

    private Vector2 moveInput;
    private Vector2 lastMoveDirection;
    private Vector2 dashDirection;
    private Vector2 aimDirection;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private bool wasAttackPressed;

    private float _lastFireTime = -1f;
    private const float FireGap = 0.05f;

    //For slowdown effects from witch blast attack
    private float _slowTimer = 0f;
    private float _slowMultiplier = 1f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField][Range(0f, 1f)] private float attackSoundVolume = 1f;
    [SerializeField] private AudioClip dashSound;
    [SerializeField][Range(0f, 1f)] private float dashSoundVolume = 1f;

    public Vector2 AimDirection => aimDirection.normalized;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
        

        lastMoveDirection = Vector2.down;
        animator.SetFloat("Speed", 0);
        animator.SetFloat("MoveX", lastMoveDirection.x);
        animator.SetFloat("MoveY", lastMoveDirection.y);
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        if (moveInput != Vector2.zero)
            lastMoveDirection = moveInput.normalized;
    }

    // Called by Animation Events on each attack direction.
    // Sound is here so it fires once per swing, in sync with the animation.
    public void FireProjectile()
    {
        if (Time.time - _lastFireTime < FireGap) return;
        _lastFireTime = Time.time;

        PlayAttackSound();

        if (projectilePrefab != null && swordTip != null)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mousePosition.z = 0;
            aimDirection = (mousePosition - transform.position).normalized;
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;

            GameObject proj = Instantiate(projectilePrefab, swordTip.position, Quaternion.Euler(0, 0, angle));
            Projectile p = proj.GetComponent<Projectile>();
            p.Launch(aimDirection);

            if (UpgradeSystem.Instance != null)
            {
                p.SetDamage(UpgradeSystem.Instance.Damage);
                p.SetRange(UpgradeSystem.Instance.ProjectileRange);
            }
        }
    }

    public void EnableSwordHitbox()
    {
        swordHitbox.EnableHitbox();
    }

    public void DisableSwordHitbox()
    {
        swordHitbox.DisableHitbox();
    }

    void Update()
    {
        if (Keyboard.current.leftShiftKey.wasPressedThisFrame && !isDashing && dashCooldownTimer <= 0f)
        {
            isDashing = true;
            dashTimer = UpgradeSystem.Instance != null ? UpgradeSystem.Instance.DashDuration : dashDuration;
            dashCooldownTimer = UpgradeSystem.Instance != null ? UpgradeSystem.Instance.DashCooldown : dashCooldown;
            dashDirection = moveInput.sqrMagnitude > 0.01f ? moveInput.normalized : lastMoveDirection;
            animator.SetBool("isDashing", true);

            if (dashSound != null && audioSource != null)
                audioSource.PlayOneShot(dashSound, dashSoundVolume);
        }

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        if (_slowTimer > 0f)
            _slowTimer -= Time.deltaTime;

        Attack();
    }

    void FixedUpdate()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePosition.z = 0;
        aimDirection = mousePosition - transform.position;

        if (isDashing)
        {
            rb.linearVelocity = dashDirection * GetDashSpeed();
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
                animator.SetBool("isDashing", false);
            }
        }
    }

    private void Attack()
    {
        rb.linearVelocity = moveInput * GetMoveSpeed();
        animator.SetFloat("Speed", moveInput.magnitude);

        UpdateAttackSpeed();

        bool isAttackPressed = Mouse.current.leftButton.IsPressed();

        if (isAttackPressed)
        {
            animator.SetFloat("MoveX", aimDirection.x);
            animator.SetFloat("MoveY", aimDirection.y);
            animator.SetBool("isAttacking", true);
        }
        else
        {
            animator.SetBool("isAttacking", false);
            animator.SetFloat("MoveX", lastMoveDirection.x);
            animator.SetFloat("MoveY", lastMoveDirection.y);
        }

        wasAttackPressed = isAttackPressed;
    }

    private void UpdateAttackSpeed()
    {
        if (animator == null) return;

        float baseAnimDuration = 0.517f;
        float interval = UpgradeSystem.Instance != null
            ? UpgradeSystem.Instance.FireInterval
            : baseAnimDuration;

        float speedMult = baseAnimDuration / interval;
        animator.SetFloat("AttackSpeed", speedMult);
    }

    private void PlayAttackSound()
    {
        if (audioSource != null && attackSound != null)
            audioSource.PlayOneShot(attackSound, attackSoundVolume);
    }

    public void IncreaseAttackRange(float amount) => projectilePrefab.GetComponent<Projectile>().SetRange(amount);

    public void ApplySlow(float multiplier, float duration)
    {
        //Check to see if player already is slowed
        if (_slowTimer > 0f) return;
        _slowMultiplier = multiplier;
        _slowTimer = duration;
    }

    //Takes slowing effects of enemy attacks into account.
    private float GetMoveSpeed()
    {
        float speed = UpgradeSystem.Instance != null ? UpgradeSystem.Instance.MoveSpeed : moveSpeed;
        return _slowTimer > 0f ? speed * _slowMultiplier : speed;
    }

    private float GetDashSpeed()
    {
        if (UpgradeSystem.Instance == null) return dashSpeed;
        float speedRatio = UpgradeSystem.Instance.MoveSpeed / moveSpeed;
        return dashSpeed * speedRatio;
    }

   
}