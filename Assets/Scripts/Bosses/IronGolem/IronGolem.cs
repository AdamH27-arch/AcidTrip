using UnityEngine;
using System.Collections;
using static EnemyData;

[RequireComponent(typeof(AudioSource))]
public class IronGolem : Enemy
{
    [Header("Boss Settings")]

    [Header("Sound Effects")]
    [SerializeField] private AudioClip spearAttackSound;
    [SerializeField][Range(0f, 1f)] private float spearAttackVolume = 1f;
    [SerializeField][Range(0f, 5f)] private float spearAttackSoundDelay = 0f;
    [SerializeField] private AudioClip chargeSound;
    [SerializeField][Range(0f, 1f)] private float chargeSoundVolume = 1f;

    private AudioSource _audioSource;
    private bool _isAttacking = false;
    private float _attackCooldownTimer = 0f;

    private enum BossAttack { Charge, StraightLine, RingOfSpears }

    protected override void Awake()
    {
        base.Awake();
        _audioSource = GetComponent<AudioSource>();
    }

    protected override void MoveTowardPlayer()
    {
        if (Player == null || _isAttacking) return;

        if (_attackCooldownTimer > 0f)
        {
            _attackCooldownTimer -= Time.fixedDeltaTime;
            Vector2 direction = (Vector2)(Player.transform.position - transform.position).normalized;
            ani.SetBool("isMoving", true);
            _rb.linearVelocity = direction * Data.GolemSpeed;
            return;
        }

        ani.SetBool("isMoving", false);
        _rb.linearVelocity = Vector2.zero;
        BossAttack attack = (BossAttack)Random.Range(0, 3);
        StartCoroutine(ExecuteAttack(attack));
    }

    private IEnumerator ExecuteAttack(BossAttack attack)
    {
        _isAttacking = true;

        switch (attack)
        {
            case BossAttack.StraightLine:
                yield return StartCoroutine(AttackStraightLine());
                break;
            case BossAttack.Charge:
                yield return StartCoroutine(AttackCharge());
                break;
            case BossAttack.RingOfSpears:
                yield return StartCoroutine(AttackRingOfSpears());
                break;
        }

        _isAttacking = false;
        _attackCooldownTimer = Data.spearCooldown;
    }

    private IEnumerator AttackStraightLine()
    {
        if (Data.IronGolemSpearPrefab == null) yield break;

        // One sound for the whole attack
        if (spearAttackSound != null)
            StartCoroutine(PlayDelayed(spearAttackSound, spearAttackVolume, spearAttackSoundDelay));

        ani.SetTrigger("isAttacking");

        Vector2 directionToPlayer = (Vector2)(Player.position - transform.position).normalized;
        float spacing = 3f;
        int numberOfSpears = 10;

        for (int i = 0; i < numberOfSpears; i++)
        {
            Vector2 spawnPos = (Vector2)transform.position + directionToPlayer * (spacing * (i + 1));
            Instantiate(Data.IronGolemSpearPrefab, spawnPos, Quaternion.identity);
        }
    }

    private IEnumerator AttackCharge()
    {
        if (Data.IronGolemSpearPrefab == null) yield break;

        if (chargeSound != null)
            _audioSource.PlayOneShot(chargeSound,
                chargeSoundVolume * SettingsMenu.SFXVolume);

        ani.SetBool("isMoving", true);
        Vector2 Direction = ((Vector2)(Player.position - transform.position)).normalized;
        _rb.linearVelocity = Direction * Data.golemChargeSpeed;
        yield return new WaitForSeconds(Data.golemChargeDuration);
        ani.SetBool("isMoving", false);
        _rb.linearVelocity = Vector2.zero;
        yield return StartCoroutine(AttackRingOfSpears());
    }

    private IEnumerator AttackRingOfSpears()
    {
        if (Data.IronGolemSpearPrefab == null) yield break;

        // One sound for the whole ring attack
        if (spearAttackSound != null)
            StartCoroutine(PlayDelayed(spearAttackSound, spearAttackVolume, spearAttackSoundDelay));

        ani.SetTrigger("isAttacking");

        int numberOfRings = 3;
        float ringSpacing = 4f;
        float spearSpacing = 4f;

        for (int ring = 1; ring <= numberOfRings; ring++)
        {
            float radius = ring * ringSpacing;
            float circumference = 2f * Mathf.PI * radius;
            int spearCount = Mathf.FloorToInt(circumference / spearSpacing);
            float angleStep = 360f / spearCount;

            for (int i = 0; i < spearCount; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                Vector2 spawnPos = (Vector2)transform.position + offset;
                Instantiate(Data.IronGolemSpearPrefab, spawnPos, Quaternion.identity);
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator PlayDelayed(AudioClip clip, float volume, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        _audioSource.PlayOneShot(clip, volume * SettingsMenu.SFXVolume);
    }
}