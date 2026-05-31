using UnityEngine;

public class XPOrbSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject orbPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private int baseOrbCount = 5;
    [SerializeField] private int orbsPerRound = 2;
    [SerializeField] private float spawnRadius = 5f;

    [Header("XP Scaling")]
    [SerializeField] private int baseXPValue = 50;
    [SerializeField] private float xpScalePerRound = 1.1f;

    private WaveSpawner _waveSpawner;
    private Transform _player;

    private void Start()
    {
        if (orbPrefab == null)
        {
            Debug.LogError("XPOrbSpawner: no orb prefab assigned.");
            return;
        }

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;

        _waveSpawner = FindAnyObjectByType<WaveSpawner>();
        if (_waveSpawner != null)
            _waveSpawner.OnRoundStarted.AddListener(OnRoundStarted);
    }

    private void OnRoundStarted(int round)
    {
        if (orbPrefab == null || _player == null) return;

        int count = baseOrbCount + (round * orbsPerRound);
        int xpValue = Mathf.RoundToInt(baseXPValue * Mathf.Pow(xpScalePerRound, round - 1));

        SpawnOrbs(count, xpValue);
    }

    private void SpawnOrbs(int count, int xpValue)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = _player.position + new Vector3(offset.x, offset.y, 0f);

            GameObject orb = Instantiate(orbPrefab, spawnPos, Quaternion.identity);
            XPOrb xpOrb = orb.GetComponent<XPOrb>();
            if (xpOrb != null)
                xpOrb.SetValue(xpValue);
        }

        Debug.Log("XPOrbSpawner: spawned " + count + " orbs worth " + xpValue + " XP each.");
    }

    private void OnDestroy()
    {
        if (_waveSpawner != null)
            _waveSpawner.OnRoundStarted.RemoveListener(OnRoundStarted);
    }
}