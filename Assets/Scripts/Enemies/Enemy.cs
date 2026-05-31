using System.Collections;
using TMPro;
using UnityEngine;
using static EnemyData;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(AudioSource))]
public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyData data;
    [SerializeField] private SpriteRenderer overrideRenderer;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private SpriteRenderer _sr;
    [SerializeField] private float _roundSpeedMult = 1f;

    private SpriteRenderer[] _renderers;
    private Color[] _originalColors;
    protected Rigidbody2D _rb;
    protected Animator ani;
    private Transform _player;
    private PlayerHealth _playerHealth;
    private float _currentHealth;
    private float _damageCooldownTimer;
    private bool _movementOverridden;
    private float _fireCooldown;
    private float _zigzagOffset;
    private Vector2 _sprintTarget;
    private Color _originalColor;

    protected bool invertSprite = false;
    private static bool _gravityFlipped;
    private bool _isDead = false;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip deathSound;
    [SerializeField][Range(0f, 1f)] private float deathSoundVolume = 1f;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField][Range(0f, 1f)] private float hurtSoundVolume = 0.6f;

    private AudioSource _audioSource;

    public System.Action OnDied;

    public EnemyData Data => data;
    public Transform Player => _player;
    protected Rigidbody2D Rb => _rb;

    private enum MoveState { Holding, Advancing, Retreating }
    private MoveState _moveState = MoveState.Holding;
    private float _stateTimer;
    private float _pulsatorTimer;
    private bool _pulsatorSprinting = true;
    private float _orbitAngle;

    //Teleporting Wizard datafields.
    private float _teleportTimer;
    private bool _waitingToFire;
    private bool _waitingToTeleport;

    //Witch datafields
    private float witchBlastTimer;

    
    public static float GlobalSpeedMultiplier { get; private set; } = 1f;


    //Listener to update the boss health bar.
    public UnityEvent<int, int> OnHealthChanged;

    public static void SetGlobalSpeedMultiplier(float mult)
    {
        GlobalSpeedMultiplier = mult;
    }

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _audioSource = GetComponent<AudioSource>();

        if (overrideRenderer != null)
            _sr = overrideRenderer;
        else if (_sr == null)
            _sr = GetComponentInChildren<SpriteRenderer>();

        ani = GetComponentInChildren<Animator>();

        _rb.gravityScale = 0f;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        ApplyData();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");

        OffScreenIndicator.Instance?.Register(this);

        if (playerObj == null)
        {
            Debug.LogWarning("Enemy could not find Player.");
            return;
        }

        if (_player != null && data.movementPattern == MovementPattern.Orbiter)
        {
            Vector2 toEnemy = _rb.position - (Vector2)_player.position;
            _orbitAngle = Mathf.Atan2(toEnemy.y, toEnemy.x);
        }

        if (data.movementPattern == MovementPattern.Pulsator)
        {
            _pulsatorSprinting = Random.value > 0.5f;
            _pulsatorTimer = _pulsatorSprinting
                ? Random.Range(0f, data.sprintDuration)
                : Random.Range(0f, data.freezeDuration);
        }

        if (data.movementPattern == MovementPattern.Pulsator)
        {
            _pulsatorSprinting = Random.value > 0.5f;
            _pulsatorTimer = _pulsatorSprinting
                ? Random.Range(0f, data.sprintDuration)
                : Random.Range(0f, data.freezeDuration);
            _sprintTarget = _player != null
                ? (Vector2)_player.position + Random.insideUnitCircle * 1.5f
                : (Vector2)transform.position;
        }

        if (data.movementPattern == MovementPattern.ExplodingHobbit || data.movementPattern == MovementPattern.Witch)
            invertSprite = true;

        if (data.movementPattern == MovementPattern.Witch)
            witchBlastTimer = data.timeTillBlast;

        _zigzagOffset = Random.Range(0f, Mathf.PI * 2f);

        _player = playerObj.transform;
        _playerHealth = playerObj.GetComponent<PlayerHealth>();
        _pulsatorTimer = data.sprintDuration;
    }

    private void ApplyData()
    {
        if (data == null)
        {
            Debug.LogError(gameObject.name + " has no EnemyData assigned.");
            return;
        }

        _currentHealth = data.maxHealth;

        _renderers = GetComponentsInChildren<SpriteRenderer>();
        _originalColors = new Color[_renderers.Length];

        for (int i = 0; i < _renderers.Length; i++)
        {
            _renderers[i].color = data.color;
            _originalColors[i] = data.color;
        }

        transform.localScale = new Vector3(data.scale.x, data.scale.y, 1f);
    }

    private void Update()
    {
        if (_damageCooldownTimer > 0f)
            _damageCooldownTimer -= Time.deltaTime;

        if (_fireCooldown > 0f)
            _fireCooldown -= Time.deltaTime;

        FacePlayer();
        TryShoot();
    }

    private void FixedUpdate()
    {
        MoveTowardPlayer();

        float combined = GlobalSpeedMultiplier * _roundSpeedMult;
        if (combined != 1f)
            _rb.linearVelocity *= combined;
    }

    private void FacePlayer()
    {
        if (_player == null) return;

        if (invertSprite == false)
            _sr.flipX = _player.position.x > transform.position.x;
        else
            _sr.flipX = _player.position.x < transform.position.x;
    }

    private IEnumerator FlashRed()
    {
        foreach (SpriteRenderer sr in _renderers)
            sr.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < _renderers.Length; i++)
            _renderers[i].color = _originalColors[i];
    }

    public void ApplyRoundScaling(int round)
    {
        if (round <= 5) return;

        int scalingSteps = round - 5;

        float hpMult = Mathf.Pow(1.06f, scalingSteps);
        float speedMult = Mathf.Pow(1.02f, scalingSteps);

        _currentHealth *= hpMult;
        _roundSpeedMult = speedMult;
    }

    private void TryShoot()
    {
        if (!data.canDoRangedAttack) return;
        if (data.bulletPrefab == null) return;
        if (_player == null) return;
        if (_fireCooldown > 0f) return;

        ani.SetBool("isMoving", false);
        ani.SetTrigger("attack");

        float dist = Vector2.Distance(transform.position, _player.position);
        if (dist > data.attackRange) return;

        Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;

        GameObject go = Instantiate(data.bulletPrefab, transform.position, Quaternion.identity);

        Collider2D bulletCol = go.GetComponent<Collider2D>();
        if (bulletCol != null)
            bulletCol.enabled = false;

        EnemyBullet bullet = go.GetComponent<EnemyBullet>();
        if (bullet != null)
        {
            bullet.SetDamage(data.contactDamage);
            bullet.SetDirection(dir);
        }

        StartCoroutine(EnableBulletCollider(bulletCol));
        _fireCooldown = 1f / data.fireRate;

        ani.SetBool("isMoving", true);
    }

    private void FireBulletRing()
    {
        Debug.Log("FireBulletRing method started!");

        if (data.TeleporterBullet == null)
        {
            Debug.LogError("FAIL: TeleporterBullet is NULL in the EnemyData ScriptableObject!");
            return;
        }

        if (data.projectileCount <= 0)
        {
            Debug.LogWarning("FAIL: projectileCount is 0! No bullets will be spawned.");
            return;
        }

        float angleStep = 360f / data.projectileCount;

        for (int i = 0; i < data.projectileCount; i++)
        {
            Debug.Log($"Spawning bullet number: {i}");

            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector2 directionOfBullet = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            GameObject bullet = Instantiate(data.TeleporterBullet, transform.position, Quaternion.identity);

            if (bullet == null)
            {
                Debug.LogError("Instantiate returned null! This shouldn't happen.");
                continue;
            }

            Animator bulletAnimator = bullet.GetComponent<Animator>();
            if (bulletAnimator != null)
                bulletAnimator.SetTrigger("isFired");

            EnemyBullet enemyBulletScript = bullet.GetComponent<EnemyBullet>();
            if (enemyBulletScript != null)
            {
                enemyBulletScript.SetDirection(directionOfBullet);
                enemyBulletScript.SetDamage(data.contactDamage);
            }
        }
    }

    public static void SetGravityFlipped(bool flipped)
    {
        _gravityFlipped = flipped;
    }

    private IEnumerator EnableBulletCollider(Collider2D col)
    {
        if (col == null) yield break;
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        if (col != null) col.enabled = true;
    }

    protected virtual void MoveTowardPlayer()
    {
        if (_player == null) return;
        switch (data.movementPattern)
        {
            case MovementPattern.Chase:           HandleChase();           break;
            case MovementPattern.Ranged:          HandleRanged();          break;
            case MovementPattern.Zigzag:          HandleZigzag(1f);        break;
            case MovementPattern.Orbiter:         HandleOrbiter();         break;
            case MovementPattern.Pulsator:        HandlePulsator();        break;
            case MovementPattern.Coward:          HandleCoward();          break;
            case MovementPattern.Weeping:         HandleWeeping();         break;
            case MovementPattern.Teleporter:      HandleTeleporter();      break;
            case MovementPattern.ExplodingHobbit: HandleExplodingHobbit(); break;
            case MovementPattern.Witch:           HandleWitch();           break;   
        }
    }

    private void HandleExplodingHobbit()
    {
        Animator HobbitAnimator = GetComponent<Animator>();
        

        Vector2 direction = ((Vector2)_player.position - _rb.position).normalized;

        if (_gravityFlipped) direction = -direction;

        _rb.linearVelocity = direction * data.HobbitSpeed;
    }

    //Witch Chases the player and fires a slowing projectile that halves the player speed.
    private void HandleWitch()
    {
        if (_player == null) return;

        
        Debug.Log("timeTillBlast: " + witchBlastTimer);

        if (witchBlastTimer > 0)
        {
            witchBlastTimer -= Time.deltaTime;
            Vector2 direction = (Vector2)(Player.position - transform.position).normalized;
            _rb.linearVelocity = direction * data.moveSpeed;
        }
        else
        {
            Debug.Log("Firing witch blast, prefab: " + data.witchBlastPrefab);
            Vector2 directionOfBullet = (Vector2)(Player.position - transform.position).normalized;
            GameObject blast = Instantiate(data.witchBlastPrefab, transform.position, Quaternion.identity);
            WitchBlast witchBlast = blast.GetComponent<WitchBlast>();
            if (witchBlast != null)
            {
                witchBlast.SetDirection(directionOfBullet);
                witchBlast.SetDamage(data.witchBlastDamage);
            }
            witchBlastTimer = data.timeTillBlast;
            
        }
    }
    private void HandleTeleporter()
    {
        if (_player == null) return;

        _teleportTimer -= Time.fixedDeltaTime;

        if (!_waitingToFire && !_waitingToTeleport)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * data.teleportDistance;

            _rb.position = (Vector2)_player.position + offset;
            _rb.linearVelocity = Vector2.zero;

            _waitingToFire = true;
            _teleportTimer = data.waitBeforeFire;

            ani.SetBool("isMoving", false);
        }
        else if (_waitingToFire && _teleportTimer <= 0f)
        {
            ani.SetTrigger("attack");
            FireBulletRing();

            _waitingToFire = false;
            _waitingToTeleport = true;
            _teleportTimer = data.waitAfterFire;
        }
        else if (_waitingToTeleport && _teleportTimer <= 0f)
        {
            _waitingToTeleport = false;
        }
    }

    private void HandleWeeping()
    {
        if (_player == null) return;

        Vector2 directionToEnemy = ((Vector2)transform.position - (Vector2)_player.position).normalized;
        PlayerMovement playerMovement = _player.GetComponent<PlayerMovement>();
        Vector2 playerAim = playerMovement != null ? playerMovement.AimDirection : Vector2.up;

        float angle = Vector2.Angle(playerAim, directionToEnemy);
        bool isBeingWatched = angle < data.detectionAngle;

        Vector2 chaseDir = ((Vector2)_player.position - _rb.position).normalized;

        if (_gravityFlipped) chaseDir = -chaseDir;

        _rb.linearVelocity = isBeingWatched
            ? chaseDir * (data.chaseSpeed / 2)
            : chaseDir * data.chaseSpeed;
    }

    private void HandleOrbiter()
    {
        _orbitAngle += data.orbitSpeed * Time.fixedDeltaTime;

        Vector2 targetPos = (Vector2)_player.position + new Vector2(
            Mathf.Cos(_orbitAngle),
            Mathf.Sin(_orbitAngle)
        ) * data.orbitRadius;

        _rb.MovePosition(targetPos);
    }

    private void HandlePulsator()
    {
        _pulsatorTimer -= Time.deltaTime;

        if (_pulsatorSprinting)
        {
            Vector2 dir = (_sprintTarget - _rb.position).normalized;
            _rb.linearVelocity = dir * data.moveSpeed * 2f;

            float distToTarget = Vector2.Distance(_rb.position, _sprintTarget);

            if (_pulsatorTimer <= 0f || distToTarget < 0.3f)
            {
                _pulsatorSprinting = false;
                _pulsatorTimer = data.freezeDuration;
                _rb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            _rb.linearVelocity = Vector2.zero;

            if (_pulsatorTimer <= 0f)
            {
                _pulsatorSprinting = true;
                _pulsatorTimer = data.sprintDuration;

                Vector2 baseDir = _gravityFlipped
                    ? (_rb.position - (Vector2)_player.position).normalized
                    : ((Vector2)_player.position - _rb.position).normalized;

                _sprintTarget = _rb.position + baseDir * data.moveSpeed + Random.insideUnitCircle * 1.5f;
            }
        }
    }

    private void HandleCoward()
    {
        float dist = Vector2.Distance(transform.position, _player.position);

        Vector2 fleeDir = (_rb.position - (Vector2)_player.position).normalized;
        Vector2 chaseDir = -fleeDir;

        if (_gravityFlipped) { Vector2 temp = fleeDir; fleeDir = chaseDir; chaseDir = temp; }

        if (dist < data.fleeRange)
        {
            Vector2 perp = new Vector2(-fleeDir.y, fleeDir.x);
            fleeDir = (fleeDir + perp * 0.3f).normalized;
            _rb.linearVelocity = fleeDir * data.fleeSpeed;
        }
        else
        {
            _rb.linearVelocity = chaseDir * data.moveSpeed * 0.5f;
        }
    }

    private void HandleChase()
    {
        Vector2 dir = ((Vector2)_player.position - _rb.position).normalized;

        if (_gravityFlipped)
            dir = -dir;

        _rb.linearVelocity = dir * data.moveSpeed;
    }

    private void HandleZigzag(float directionMultiplier)
    {
        Vector2 toPlayer = ((Vector2)_player.position - _rb.position).normalized;

        if (_gravityFlipped) toPlayer = -toPlayer;

        Vector2 perp = new Vector2(-toPlayer.y, toPlayer.x);
        float wave = Mathf.Sin((Time.time + _zigzagOffset) * data.zigzagFrequency) * data.zigzagAmplitude;
        Vector2 move = (toPlayer * directionMultiplier + perp * wave).normalized;

        _rb.linearVelocity = move * data.moveSpeed;
    }

    private void HandleRanged()
    {
        float dist = Vector2.Distance(transform.position, _player.position);
        _stateTimer -= Time.deltaTime;

        Vector2 toward = ((Vector2)_player.position - _rb.position).normalized;
        Vector2 away = -toward;
        if (_gravityFlipped) { Vector2 temp = toward; toward = away; away = temp; }

        switch (_moveState)
        {
            case MoveState.Holding:
                if (dist < data.preferredRange - 0.5f)
                    _rb.linearVelocity = away * data.moveSpeed * 0.5f;
                else if (dist > data.preferredRange + 0.5f)
                    _rb.linearVelocity = toward * data.moveSpeed * 0.5f;
                else
                    _rb.linearVelocity = Vector2.zero;

                if (_stateTimer <= 0f)
                {
                    _moveState = MoveState.Advancing;
                    _stateTimer = data.advanceDuration;
                }
                break;

            case MoveState.Advancing:
                HandleZigzag(1f);
                if (_stateTimer <= 0f || dist < data.preferredRange * 0.5f)
                {
                    _moveState = MoveState.Retreating;
                    _stateTimer = data.retreatDuration;
                }
                break;

            case MoveState.Retreating:
                HandleZigzag(-1f);
                if (_stateTimer <= 0f || dist >= data.preferredRange)
                {
                    _moveState = MoveState.Holding;
                    _stateTimer = data.holdDuration;
                }
                break;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (_damageCooldownTimer > 0f) return;
        if (_playerHealth == null) return;

        if (data.ExplodesOnDeath)
        {
            Debug.Log($"{data.enemyName} detonated by colliding with player");
            Die();
            return;
        }

        ani.SetTrigger("attack");
        _playerHealth.TakeDamage(data.contactDamage);
        _damageCooldownTimer = data.damageCooldown;
    }

    public virtual void TakeDamage(int amount)
    {
        if (_isDead) return;

        _currentHealth -= amount;
        //Notify listener in BossHealthBar script.
        OnHealthChanged?.Invoke((int)_currentHealth, data.maxHealth);
        DamageNumberSpawner.Instance?.Spawn(amount, transform.position);

        StopCoroutine("FlashRed");
        StartCoroutine("FlashRed");

        if (_currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Only play hurt sound if the enemy is surviving this hit.
            // If dying, the death sound covers it.
            if (hurtSound != null && _audioSource != null)
                _audioSource.PlayOneShot(hurtSound, hurtSoundVolume);
        }
    }

    private void SpawnDrops()
    {
        if (data.possibleDrops == null || data.possibleDrops.Count == 0) return;

        float luckMult = UpgradeSystem.Instance != null
            ? UpgradeSystem.Instance.LuckMultiplier : 1f;

        foreach (ItemDrop drop in data.possibleDrops)
        {
            if (drop.item == null) continue;

            float roll = Random.Range(0f, 100f);
            if (roll > drop.dropChance * luckMult) continue;

            if (drop.item.droppedPrefab != null)
                Instantiate(drop.item.droppedPrefab, transform.position, Quaternion.identity);
            else
                DroppedItem.Spawn(drop.item, transform.position);

            TutorialController.Instance?.TriggerStep("item_drop");
        }
    }

    public void Die()
    {
        if (_isDead) return;
        _isDead = true;

        // PlayClipAtPoint spawns a temporary AudioSource at the world position
        // so the sound finishes even after this GameObject is destroyed
        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position, deathSoundVolume);

        OnDied?.Invoke();
        DeathBurst.Spawn(transform.position, data.color);

        if (data.ExplodesOnDeath && data.ExplosionPrefab != null)
        {
            Debug.Log($"{data.enemyName} detonated on death");
            Instantiate(data.ExplosionPrefab, transform.position, Quaternion.identity);
        }

        if (data.xpOrbPrefab != null)
        {
            GameObject orb = Instantiate(data.xpOrbPrefab, transform.position, Quaternion.identity);
            XPOrb xpOrb = orb.GetComponent<XPOrb>();
            if (xpOrb != null)
            {
                int round = WaveSpawner.Instance != null ? WaveSpawner.Instance.CurrentRound : 1;
                int scaledXP = Mathf.RoundToInt(data.xpValue * Mathf.Pow(1.1f, round - 1));
                xpOrb.SetValue(scaledXP);
            }
        }

        OffScreenIndicator.Instance?.Unregister(this);
        SpawnDrops();
        Destroy(gameObject);
    }
}