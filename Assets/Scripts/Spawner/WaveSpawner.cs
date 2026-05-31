using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum EnemyType
{
    Ducky,
    Dragon,
    Witch,
    FlyingGolem,
    ExplodingHobbit,
    WeepingAngel,
    FlyingLizard,
    TeleportingWizard,
    IronGolem,
    LordOfHobbits,
    FinalBoss
}

public class WaveSpawner : MonoBehaviour
{
    // Assign each enemy prefab once here -- referenced by all waves
    [System.Serializable]
    public class EnemyEntry
    {
        public EnemyType type;
        public GameObject prefab;
    }

    // One entry per enemy type in a wave -- weight controls how often it spawns
    [System.Serializable]
    public class SpawnEntry
    {
        public EnemyType enemy;
        [Range(0f, 10f)] public float weight = 1f;
    }

    [System.Serializable]
    public class WaveData
    {
        public string waveName = "Wave";
        public float duration = 60f;
        public float spawnInterval = 2f;
        public int maxAlive = 10;
        public bool waitForClear = true;
        public List<SpawnEntry> enemies = new List<SpawnEntry>();
    }

    [System.Serializable]
    public class RoundData
    {
        public string roundName = "Round";
        public bool isBossRound = false;
        public List<WaveData> waves = new List<WaveData>();

        [Header("Boss Minions")]
        public List<SpawnEntry> minionEnemies = new List<SpawnEntry>();
        public float minionInterval = 5f;
        public int minionMaxAlive = 4;
    }

    [Header("Enemy Registry -- assign each prefab once")]
    [SerializeField] private List<EnemyEntry> enemyRegistry = new List<EnemyEntry>();

    [Header("Rounds")]
    [SerializeField] private List<RoundData> rounds;
    [SerializeField] private float timeBetweenRounds = 5f;
    [SerializeField] private float timeBetweenWaves = 3f;

    [Header("Spawn")]
    [SerializeField] private float spawnRadius = 12f;

    

    [Header("Events")]
    public UnityEvent<int> OnRoundStarted;
    public UnityEvent<int> OnRoundCompleted;
    public UnityEvent OnBossRound;
    public UnityEvent OnAllRoundsComplete;


    private int _bossEnemiesAlive = 0;
    private Coroutine _minionCoroutine;
    public static WaveSpawner Instance;
    private Dictionary<EnemyType, GameObject> _prefabLookup = new Dictionary<EnemyType, GameObject>();
    private Transform _player;
    private int _currentRoundIndex;
    private int _currentWaveIndex;
    private int _enemiesAlive;
    private float _timeRemaining;
    private bool _roundActive;

    public int CurrentRound => _currentRoundIndex + 1;
    public int TotalRounds => rounds.Count;
    public int CurrentWave => _currentWaveIndex + 1;
    public int WavesInRound => _currentRoundIndex < rounds.Count
                                   ? rounds[_currentRoundIndex].waves.Count : 0;
    public int EnemiesAlive => _enemiesAlive;
    public float TimeRemaining => _timeRemaining;
    public bool RoundActive => _roundActive;
    public bool IsUnlimitedWave => _currentRoundIndex < rounds.Count &&
                                    rounds[_currentRoundIndex].isBossRound;

    public string CurrentRoundName => _currentRoundIndex < rounds.Count
    ? rounds[_currentRoundIndex].roundName : "";

    private void Awake()
    {
        Instance = this;

        foreach (var entry in enemyRegistry)
        {
            if (entry.prefab != null && !_prefabLookup.ContainsKey(entry.type))
                _prefabLookup[entry.type] = entry.prefab;
        }
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogError("WaveSpawner: Player not found.");
            return;
        }

        _player = playerObj.transform;
        StartCoroutine(RunRounds());
    }

    private void Update()
    {
        if (_roundActive && _timeRemaining > 0f)
            _timeRemaining -= Time.deltaTime;
    }

    private IEnumerator RunRounds()
    {
        for (_currentRoundIndex = 0; _currentRoundIndex < rounds.Count; _currentRoundIndex++)
        {
            RoundData round = rounds[_currentRoundIndex];

            if (round.isBossRound)
                OnBossRound?.Invoke();

            OnRoundStarted?.Invoke(CurrentRound);

            yield return StartCoroutine(RunRound(round));

            // Only wait for enemies on boss rounds -- regular rounds carry enemies over
            if (round.isBossRound)
                yield return StartCoroutine(WaitForEnemiesClear());

            OnRoundCompleted?.Invoke(CurrentRound);

            if (_currentRoundIndex < rounds.Count - 1)
                yield return new WaitForSeconds(timeBetweenRounds);
        }

        OnAllRoundsComplete?.Invoke();
    }

    private IEnumerator RunRound(RoundData round)
    {
        _roundActive = true;

        if (round.isBossRound && round.minionEnemies.Count > 0)
            _minionCoroutine = StartCoroutine(SpawnMinions(round));

        for (_currentWaveIndex = 0; _currentWaveIndex < round.waves.Count; _currentWaveIndex++)
        {
            WaveData wave = round.waves[_currentWaveIndex];
            bool isLastWave = _currentWaveIndex == round.waves.Count - 1;

            yield return StartCoroutine(RunWave(wave));

            // Boss died -- stop minions and kill everything else
            if (round.isBossRound)
            {
                if (_minionCoroutine != null)
                {
                    StopCoroutine(_minionCoroutine);
                    _minionCoroutine = null;
                }

                ForceKillAllEnemies();
            }

            if (!isLastWave)
            {
                if (wave.waitForClear)
                    yield return StartCoroutine(WaitForEnemiesClear());

                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }

        _roundActive = false;
        _timeRemaining = 0f;
    }

    private IEnumerator RunWave(WaveData wave)
    {
        if (wave.enemies == null || wave.enemies.Count == 0)
            yield break;

        bool isBoss = rounds[_currentRoundIndex].isBossRound;
        float spawnTimer = 0f;
        bool hasSpawned = false;

        _timeRemaining = wave.duration;
        _bossEnemiesAlive = 0;

        while (true)
        {
            if (!isBoss && _timeRemaining <= 0f) break;
            if (isBoss && hasSpawned && _bossEnemiesAlive <= 0) break;

            spawnTimer -= Time.deltaTime;

            if (spawnTimer <= 0f && _enemiesAlive < wave.maxAlive)
            {
                SpawnEnemy(wave);
                spawnTimer = wave.spawnInterval;
                hasSpawned = true;
            }

            yield return null;
        }
    }

    private void SpawnEnemy(WaveData wave)
    {
        if (_player == null) return;

        float totalWeight = 0f;
        foreach (var entry in wave.enemies)
            totalWeight += entry.weight;

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        EnemyType selected = wave.enemies[0].enemy;

        foreach (var entry in wave.enemies)
        {
            cumulative += entry.weight;
            if (roll <= cumulative) { selected = entry.enemy; break; }
        }

        if (!_prefabLookup.TryGetValue(selected, out GameObject prefab) || prefab == null)
        {
            Debug.LogWarning("WaveSpawner: No prefab found for " + selected);
            return;
        }

        float radius = (BossArenaManager.Instance != null && BossArenaManager.Instance.ArenaActive)
            ? BossArenaManager.Instance.BossSpawnRadius : spawnRadius;
        float angle = Random.Range(0f, Mathf.PI * 2f);
        Vector2 spawnPos = (Vector2)_player.position + new Vector2(
            Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

        GameObject go = Instantiate(prefab, spawnPos, Quaternion.identity);
        _enemiesAlive++;

        Enemy enemy = go.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.OnDied += OnEnemyDied;
            enemy.ApplyRoundScaling(CurrentRound);

            if (rounds[_currentRoundIndex].isBossRound)
            {
                _bossEnemiesAlive++;
                enemy.OnDied += () => _bossEnemiesAlive = Mathf.Max(0, _bossEnemiesAlive - 1);
            }
        }
    }

    private IEnumerator SpawnMinions(RoundData round)
    {
        while (true)
        {
            yield return new WaitForSeconds(round.minionInterval);

            int minionCount = _enemiesAlive - _bossEnemiesAlive;
            if (minionCount < round.minionMaxAlive)
                SpawnMinionEnemy(round);
        }
    }

    private void SpawnMinionEnemy(RoundData round)
    {
        if (_player == null || round.minionEnemies.Count == 0) return;

        float totalWeight = 0f;
        foreach (var entry in round.minionEnemies)
            totalWeight += entry.weight;

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        EnemyType selected = round.minionEnemies[0].enemy;

        foreach (var entry in round.minionEnemies)
        {
            cumulative += entry.weight;
            if (roll <= cumulative) { selected = entry.enemy; break; }
        }

        if (!_prefabLookup.TryGetValue(selected, out GameObject prefab) || prefab == null) return;

        float radius = (BossArenaManager.Instance != null && BossArenaManager.Instance.ArenaActive)
            ? BossArenaManager.Instance.BossSpawnRadius : spawnRadius;
        float angle = Random.Range(0f, Mathf.PI * 2f);
        Vector2 spawnPos = (Vector2)_player.position + new Vector2(
            Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

        GameObject go = Instantiate(prefab, spawnPos, Quaternion.identity);
        _enemiesAlive++;

        Enemy enemy = go.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.OnDied += OnEnemyDied;
            enemy.ApplyRoundScaling(CurrentRound);
        }
    }

    private void ForceKillAllEnemies()
    {
        Enemy[] remaining = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy e in remaining)
            Destroy(e.gameObject);

        _enemiesAlive = 0;
        _bossEnemiesAlive = 0;
    }

    private IEnumerator WaitForEnemiesClear()
    {
        while (_enemiesAlive > 0)
            yield return new WaitForSeconds(0.5f);
    }

    private void OnEnemyDied()
    {
        _enemiesAlive = Mathf.Max(0, _enemiesAlive - 1);
    }

    public static void ResetOrbiterCount() { }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_player == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_player.position, spawnRadius);
    }
#endif
}