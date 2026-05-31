using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum AbilityType { Sniper, Boomerang, Bomb, Scatter, Blink }

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance;

    [Header("Prefabs")]
    [SerializeField] private GameObject sniperPrefab;
    [SerializeField] private GameObject boomerangPrefab;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private GameObject scatterBulletPrefab;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sniperSound;
    [SerializeField] private AudioClip boomerangSound;
    [SerializeField] private AudioClip scatterSound;
    [SerializeField] private AudioClip blinkSound;
    [SerializeField][Range(0f, 1f)] private float abilityVolume = 1f;

    [Header("Cooldowns")]
    [SerializeField] private float sniperCooldown = 3f;
    [SerializeField] private float boomerangCooldown = 2f;
    [SerializeField] private float bombCooldown = 3.5f;
    [SerializeField] private float scatterCooldown = 1.5f;
    [SerializeField] private float blinkCooldown = 5f;

    [Header("Scatter")]
    [SerializeField] private float scatterSpread = 50f;

    [Header("Blink")]
    [SerializeField] private float blinkRangeT1 = 8f;
    [SerializeField] private float blinkRangeT2 = 12f;
    [SerializeField] private float blinkRangeT3 = 15f;
    [SerializeField] private float blinkArrivalDamageRadius = 3f;

    // Current ability state
    private AbilityType? _currentAbility = null;
    private int _abilityTier = 0;
    private float _cooldown = 0f;
    private int _blinkCharges = 0;

    public AbilityType? CurrentAbility => _currentAbility;
    public int AbilityTier => _abilityTier;
    public float CooldownRemaining => _cooldown;
    public bool HasAbility => _currentAbility.HasValue;

    private Camera _cam;

    private void Awake() => Instance = this;
    private void Start()
    {
        _cam = Camera.main;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (_cooldown > 0f) _cooldown -= Time.deltaTime;

        if (!HasAbility) return;

        if (Mouse.current.rightButton.wasPressedThisFrame)
            TryFire();
    }

    // -- Called by AbilitySelectionUI -----------------------------------------

    // Pick a new ability at tier 1
    public void SelectAbility(AbilityType type)
    {
        _currentAbility = type;
        _abilityTier = 1;
        _cooldown = 0f;

        // Reset blink charges on select
        if (type == AbilityType.Blink)
            _blinkCharges = _abilityTier >= 3 ? 2 : 0;
    }

    // Increase current ability tier by one
    public void UpgradeAbility()
    {
        if (!_currentAbility.HasValue) return;
        if (_abilityTier >= 3) return;

        _abilityTier++;

        if (_currentAbility == AbilityType.Blink && _abilityTier == 3)
            _blinkCharges = 2;
    }

    // -- Firing ---------------------------------------------------------------

    private void TryFire()
    {
        // Tier 3 blink uses charges instead of cooldown
        if (_currentAbility == AbilityType.Blink && _abilityTier == 3)
        {
            if (_blinkCharges > 0)
            {
                Vector3 mouse = GetMouseWorld();
                Blink(mouse);
                _blinkCharges--;
                if (_blinkCharges <= 0)
                    _cooldown = blinkCooldown;
            }
            return;
        }

        if (_cooldown > 0f) return;

        Vector3 target = GetMouseWorld();

        switch (_currentAbility)
        {
            case AbilityType.Sniper: FireSniper(target); break;
            case AbilityType.Boomerang: FireBoomerang(target); break;
            case AbilityType.Bomb: ThrowBomb(target); break;
            case AbilityType.Scatter: FireScatter(target); break;
            case AbilityType.Blink: Blink(target); break;
        }

        _cooldown = GetCooldown();
    }

    private void FireSniper(Vector3 target)
    {
        if (sniperPrefab == null) return;

        PlaySound(sniperSound);

        Vector2 dir = ((Vector2)target - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        GameObject go = Instantiate(sniperPrefab, transform.position, Quaternion.Euler(0, 0, angle));
        go.GetComponent<SniperBullet>()?.Launch(dir, _abilityTier);
    }

    private void FireBoomerang(Vector3 target)
    {
        if (boomerangPrefab == null) return;

        PlaySound(boomerangSound);
        Vector2 baseDir = ((Vector2)target - (Vector2)transform.position).normalized;
        int count = _abilityTier == 1 ? 1 : _abilityTier == 2 ? 2 : 3;

        for (int i = 0; i < count; i++)
        {
            // Spread boomerangs slightly apart for tiers 2 and 3
            float spread = (i - (count - 1) * 0.5f) * 15f;
            float angle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg + spread;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            GameObject go = Instantiate(boomerangPrefab, transform.position, Quaternion.identity);
            go.GetComponent<Boomerang>()?.Launch(dir, transform, _abilityTier);
        }
    }

    private void ThrowBomb(Vector3 target)
    {
        if (bombPrefab == null) return;
        GameObject go = Instantiate(bombPrefab, transform.position, Quaternion.identity);
        go.GetComponent<Bomb>()?.Throw(target, _abilityTier);
    }

    private void FireScatter(Vector3 target)
    {
        if (scatterBulletPrefab == null) return;

        PlaySound(scatterSound);
        int count = _abilityTier == 1 ? 10 : _abilityTier == 2 ? 15 : 20;
        float dmgMult = _abilityTier == 1 ? 0.6f : _abilityTier == 2 ? 0.7f : 0.8f;
        int damage = UpgradeSystem.Instance != null
                      ? Mathf.RoundToInt(UpgradeSystem.Instance.Damage * dmgMult)
                      : 15;

        float baseAngle = Mathf.Atan2(
            target.y - transform.position.y,
            target.x - transform.position.x
        ) * Mathf.Rad2Deg;

        for (int i = 0; i < count; i++)
        {
            float angle = baseAngle + Random.Range(-scatterSpread * 0.5f, scatterSpread * 0.5f);
            Vector2 dir = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            GameObject go = Instantiate(
                scatterBulletPrefab,
                transform.position,
                Quaternion.Euler(0f, 0f, angle)
            );

            go.GetComponent<ScatterBullet>()?.Launch(dir, damage);
        }
    }

    private void Blink(Vector3 target)
    {
        float maxRange = _abilityTier == 1 ? blinkRangeT1
                            : _abilityTier == 2 ? blinkRangeT2
                            : blinkRangeT3;

        PlaySound(blinkSound);

        Vector2 dir = ((Vector2)target - (Vector2)transform.position).normalized;
        float dist = Mathf.Min(Vector2.Distance(target, transform.position), maxRange);
        Vector2 destination = (Vector2)transform.position + dir * dist;

        DeathBurst.Spawn(transform.position, new Color(0.5f, 0f, 1f));

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) { rb.position = destination; rb.linearVelocity = Vector2.zero; }
        else transform.position = destination;

        DeathBurst.Spawn((Vector3)(Vector2)destination, new Color(0.5f, 0f, 1f));

        // Tier 2: damage enemies at arrival point
        if (_abilityTier >= 2)
        {
            int dmg = UpgradeSystem.Instance != null ? UpgradeSystem.Instance.Damage * 2 : 50;
            foreach (Collider2D hit in Physics2D.OverlapCircleAll(destination, blinkArrivalDamageRadius))
                hit.GetComponent<Enemy>()?.TakeDamage(dmg);
        }

        // Tier 3: restore charges after cooldown via coroutine in Start
        if (_abilityTier == 3 && _blinkCharges <= 0)
            StartCoroutine(RestoreBlinkCharges());
    }

    private IEnumerator RestoreBlinkCharges()
    {
        yield return new WaitForSeconds(blinkCooldown);
        if (_abilityTier == 3)
            _blinkCharges = 2;
    }

    private float GetCooldown() => _currentAbility.HasValue ? GetMaxCooldown(_currentAbility.Value): 1f;

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip, abilityVolume);
    }

    private Vector3 GetMouseWorld()
    {
        Vector3 m = _cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        m.z = 0f;
        return m;
    }

    public float GetMaxCooldown(AbilityType t)
    {
        switch (t)
        {
            case AbilityType.Sniper: return sniperCooldown;
            case AbilityType.Boomerang: return boomerangCooldown;
            case AbilityType.Bomb: return bombCooldown;
            case AbilityType.Scatter: return scatterCooldown;
            case AbilityType.Blink: return blinkCooldown;
            default: return 1f;
        }
    }

    public void ResetAbility()
    {
        _currentAbility = null;
        _abilityTier = 0;
        _cooldown = 0f;
        _blinkCharges = 0;
    }
}