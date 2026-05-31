using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;

    [Header("I-Frames")]
    [SerializeField] private float invincibleDuration = 0.5f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip hurtSound;
    [SerializeField][Range(0f, 1f)] private float hurtSoundVolume = 1f;
    [SerializeField] private AudioClip deathSound;
    [SerializeField][Range(0f, 1f)] private float deathSoundVolume = 1f;

    public UnityEvent<int, int> OnHealthChanged;
    public UnityEvent OnDeath;

    private SpriteRenderer[] _renderers;
    private Color[] _originalColors;

    private int _currentHealth;
    private float _invincibleTimer;
    private bool _shieldActive;

    public int CurrentHealth => _currentHealth;
    public int MaxHealth => maxHealth;

    public bool IsInvincible => _invincibleTimer > 0f || _shieldActive;

    private void Awake()
    {
        _currentHealth = maxHealth;
        _renderers = GetComponentsInChildren<SpriteRenderer>();
        _originalColors = new Color[_renderers.Length];

        for (int i = 0; i < _renderers.Length; i++)
            _originalColors[i] = _renderers[i].color;
    }

    private void Update()
    {
        if (_invincibleTimer > 0f)
            _invincibleTimer -= Time.deltaTime;
    }

    public void TakeDamage(int amount)
    {
        if (IsInvincible) return;
        if (_currentHealth <= 0) return;

        _currentHealth = Mathf.Max(0, _currentHealth - amount);
        _invincibleTimer = invincibleDuration;

        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        Vector3 pos = Vector3.zero;
        AudioSource.PlayClipAtPoint(hurtSound, pos);
        // Flash green and show red damage number
        StopCoroutine("FlashRed");
        StartCoroutine("FlashRed");
        DamageNumberSpawner.Instance?.SpawnWithColor(amount, transform.position, Color.red);

        if (_currentHealth <= 0)
        {
            AudioSource.PlayClipAtPoint(deathSound, pos);
            OnDeath?.Invoke();
        }
            
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null) return;
        if (!enemy.Data.dealsContactDamage) return;
        TakeDamage(enemy.Data.contactDamage);
    }

    public void Heal(int amount)
    {
        _currentHealth = Mathf.Min(_currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
    }

    public void ActivateShield(float duration)
    {
        StartCoroutine(ShieldRoutine(duration));
    }

    private IEnumerator ShieldRoutine(float duration)
    {
        _shieldActive = true;
        yield return new WaitForSeconds(duration);
        _shieldActive = false;
    }

    private IEnumerator FlashRed()
    {
        foreach (SpriteRenderer sr in _renderers)
            sr.color = Color.red;

        yield return new WaitForSeconds(0.15f);

        for (int i = 0; i < _renderers.Length; i++)
            _renderers[i].color = _originalColors[i];
    }

    public void SetMaxHealth(int newMax)
    {
        maxHealth = newMax;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, maxHealth);

        PlayerHealthBar bar = FindAnyObjectByType<PlayerHealthBar>();
        if (bar != null) bar.SetFill(_currentHealth, maxHealth);
    }
}