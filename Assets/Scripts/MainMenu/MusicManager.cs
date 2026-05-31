using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Music Clips")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip[] gameplayTracks; // shuffled playlist

    // BOSS MUSIC -- uncomment and assign when ready:
    // [SerializeField] private AudioClip bossMusic;

    [Header("Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField][Range(0f, 1f)] private float musicVolume = 0.5f;
    [SerializeField] private float crossfadeDuration = 1.5f;

    private AudioSource _audioSource;
    private Coroutine _fadeCoroutine;
    private Coroutine _playlistCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _audioSource = GetComponent<AudioSource>();
        _audioSource.loop = false;
        _audioSource.volume = musicVolume;
        _audioSource.playOnAwake = false;
    }

    private void Start()
    {
        PlayForScene(SceneManager.GetActiveScene().name);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayForScene(scene.name);
    }

    private void PlayForScene(string sceneName)
    {
        if (sceneName == mainMenuSceneName)
            PlaySingle(mainMenuMusic);
        else
            StartPlaylist();
    }

    // -- Playlist -------------------------------------------------------------

    private void StartPlaylist()
    {
        if (_playlistCoroutine != null)
            StopCoroutine(_playlistCoroutine);

        _playlistCoroutine = StartCoroutine(PlaylistRoutine());
    }

    private IEnumerator PlaylistRoutine()
    {
        if (gameplayTracks == null || gameplayTracks.Length == 0) yield break;

        // Fisher-Yates shuffle
        AudioClip[] shuffled = (AudioClip[])gameplayTracks.Clone();
        for (int i = shuffled.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            AudioClip tmp = shuffled[i];
            shuffled[i] = shuffled[j];
            shuffled[j] = tmp;
        }

        int index = 0;
        while (true)
        {
            AudioClip track = shuffled[index];
            if (track != null)
            {
                yield return StartCoroutine(CrossfadeTo(track));

                while (_audioSource.isPlaying)
                    yield return null;
            }

            index++;
            if (index >= shuffled.Length)
            {
                // Re-shuffle before repeating
                for (int i = shuffled.Length - 1; i > 0; i--)
                {
                    int j = Random.Range(0, i + 1);
                    AudioClip tmp = shuffled[i];
                    shuffled[i] = shuffled[j];
                    shuffled[j] = tmp;
                }
                index = 0;
            }
        }
    }

    // -- Boss music hooks (inactive) ------------------------------------------
    // To enable boss music later:
    // 1. Uncomment the bossMusic field above
    // 2. Uncomment StartBossMusic() and StopBossMusic() below
    // 3. Call them from BossArenaManager.ShowArena() and HideArena()

    /*
    public void StartBossMusic()
    {
        // Pause the playlist at the current position so it resumes later
        if (_playlistCoroutine != null)
        {
            StopCoroutine(_playlistCoroutine);
            _playlistCoroutine = null;
        }
        _audioSource.Pause(); // Pause (not Stop) so position is preserved
        PlaySingle(bossMusic);
    }

    public void StopBossMusic()
    {
        // Resume the paused gameplay track right where it left off
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(ResumePlaylist());
    }

    private IEnumerator ResumePlaylist()
    {
        // Fade out boss music
        float startVol = _audioSource.volume;
        float elapsed = 0f;
        float half = crossfadeDuration * 0.5f;

        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            _audioSource.volume = Mathf.Lerp(startVol, 0f, elapsed / half);
            yield return null;
        }

        // Resume the paused gameplay track and fade back in
        _audioSource.clip = null; // clear boss clip
        _audioSource.UnPause();
        elapsed = 0f;

        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            _audioSource.volume = Mathf.Lerp(0f, musicVolume, elapsed / half);
            yield return null;
        }

        _audioSource.volume = musicVolume;
        _fadeCoroutine = null;

        // Hand back to the playlist coroutine to pick up the next track when ready
        _playlistCoroutine = StartCoroutine(PlaylistRoutine());
    }
    */

    // -- Pause / Resume -------------------------------------------------------

    public void Pause()
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeTo(0f));
    }

    public void Resume()
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeTo(musicVolume));
    }

    // -- Helpers --------------------------------------------------------------

    private void PlaySingle(AudioClip clip)
    {
        if (clip == null) return;
        if (_audioSource.clip == clip && _audioSource.isPlaying) return;

        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(CrossfadeTo(clip));
    }

    private IEnumerator CrossfadeTo(AudioClip newClip)
    {
        float startVol = _audioSource.volume;
        float elapsed = 0f;
        float half = crossfadeDuration * 0.5f;

        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            _audioSource.volume = Mathf.Lerp(startVol, 0f, elapsed / half);
            yield return null;
        }

        _audioSource.Stop();
        _audioSource.clip = newClip;
        _audioSource.Play();
        elapsed = 0f;

        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            _audioSource.volume = Mathf.Lerp(0f, musicVolume, elapsed / half);
            yield return null;
        }

        _audioSource.volume = musicVolume;
        _fadeCoroutine = null;
    }

    private IEnumerator FadeTo(float targetVolume)
    {
        float startVolume = _audioSource.volume;
        float elapsed = 0f;
        float duration = crossfadeDuration * 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        _audioSource.volume = targetVolume;
        _fadeCoroutine = null;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        if (_audioSource.volume > 0f || volume == 0f)
            _audioSource.volume = volume;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}