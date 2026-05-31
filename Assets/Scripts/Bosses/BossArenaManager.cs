using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BossArenaManager : MonoBehaviour
{
    public static BossArenaManager Instance;

    [Header("References")]
    [SerializeField] private GameObject arenaWalls;
    [SerializeField] private WaveSpawner waveSpawner;

    [Header("Settings")]
    [SerializeField] private float bossSpawnRadius = 5f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip bossRoundStartSound;
    [SerializeField][Range(0f, 1f)] private float bossRoundStartVolume = 1f;

    public bool ArenaActive { get; private set; }
    public float BossSpawnRadius => bossSpawnRadius;

    private Transform _player;
    private AudioSource _audioSource;

    private void Awake()
    {
        Instance = this;
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        _player = GameObject.FindWithTag("Player")?.transform;

        if (waveSpawner == null)
            waveSpawner = FindAnyObjectByType<WaveSpawner>();

        if (arenaWalls != null)
            arenaWalls.SetActive(true);

        ArenaActive = true;

        waveSpawner.OnBossRound.AddListener(ShowArena);
        waveSpawner.OnRoundCompleted.AddListener(OnRoundCompleted);
    }

    private void Update()
    {
        if (_player == null || arenaWalls == null) return;

        if (!arenaWalls.activeSelf)
        {
            arenaWalls.transform.position = new Vector3(
                Mathf.Round(_player.position.x),
                Mathf.Round(_player.position.y),
                0f
            );
        }
    }

    private void OnRoundCompleted(int round)
    {
        if (round == 1 || ArenaActive)
            HideArena();
    }

    private void ShowArena()
    {
        ArenaActive = true;
        if (arenaWalls != null)
            arenaWalls.SetActive(true);

        if (bossRoundStartSound != null && _audioSource != null)
            _audioSource.PlayOneShot(bossRoundStartSound,
                bossRoundStartVolume * SettingsMenu.SFXVolume);

        // To enable boss music later, uncomment:
        // MusicManager.Instance?.StartBossMusic();
    }

    private void HideArena()
    {
        ArenaActive = false;
        if (arenaWalls != null)
            arenaWalls.SetActive(false);

        // To revert music after boss fight, uncomment:
        // MusicManager.Instance?.StopBossMusic();
    }

    private void OnDestroy()
    {
        if (waveSpawner == null) return;
        waveSpawner.OnBossRound.RemoveListener(ShowArena);
        waveSpawner.OnRoundCompleted.RemoveListener(OnRoundCompleted);
    }
}