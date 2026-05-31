using System.Collections;
using UnityEngine;
using static EnemyData;

[RequireComponent(typeof(AudioSource))]
public class FinalBoss : Enemy
{
    [Header("Boss Settings")]
    [SerializeField] private float stormRadius = 5f;
    [SerializeField] private float spearSpeed = 8f;
    [SerializeField] private float spearBarrageDelay = 0.3f;
    [SerializeField] private float chaseSpearInterval = 0.4f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float timeTillNextSpear = 0.5f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip lightningStormSound;
    [SerializeField][Range(0f, 1f)] private float lightningStormVolume = 1f;
    [SerializeField] private AudioClip spearChaseSound;
    [SerializeField][Range(0f, 1f)] private float spearChaseSoundVolume = 1f;

    private AudioSource _audioSource;
    private bool _isAttacking = false;
    private float _attackCooldownTimer = 0f;

    private enum BossAttack { LightningStorm, SpearChase }

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
            _rb.linearVelocity = direction * chaseSpeed;
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
        _rb.linearVelocity = Vector2.zero;

        switch (attack)
        {
            case BossAttack.LightningStorm:
                yield return StartCoroutine(AttackLightningStorm());
                break;
            case BossAttack.SpearChase:
                yield return StartCoroutine(AttackSpearChase());
                break;
        }

        _isAttacking = false;
        _attackCooldownTimer = Data.attackCooldown;
    }

    private IEnumerator AttackLightningStorm()
    {
        if (Data.lightningBoltPrefab == null || Player == null) yield break;

        // One sound for the whole storm
        if (lightningStormSound != null)
            _audioSource.PlayOneShot(lightningStormSound,
                lightningStormVolume * SettingsMenu.SFXVolume);

        ani.SetTrigger("isAttacking");

        int count = Data.lightningBoltCount;
        for (int i = 0; i < count; i++)
        {
            Vector2 pos = (Vector2)(Player.transform.position) + Random.insideUnitCircle * stormRadius;
            Instantiate(Data.lightningBoltPrefab, pos, Quaternion.identity);
        }
    }

    private IEnumerator AttackSpearChase()
    {
        ani.SetTrigger("isAttacking");

        if (Data.lightningSpearPrefab == null || Player == null) yield break;

        // One sound when the chase begins, not per spear
        if (spearChaseSound != null)
            _audioSource.PlayOneShot(spearChaseSound,
                spearChaseSoundVolume * SettingsMenu.SFXVolume);

        int count = Data.spearCount;
        for (int i = 0; i < count; i++)
        {
            if (Player == null) yield break;
            Instantiate(Data.lightningSpearPrefab, Player.transform.position, Quaternion.identity);
            yield return new WaitForSeconds(timeTillNextSpear);
        }
    }
}