using UnityEngine;

// Singleton audio helper for UI click sounds.
// Place a GameObject with this script in your scene (or in a persistent scene object).
// Every menu script calls UISound.PlayClick() -- no AudioSource wiring needed per-menu.
[RequireComponent(typeof(AudioSource))]
public class UISound : MonoBehaviour
{
    public static UISound Instance { get; private set; }

    [Header("Sound Effects")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField][Range(0f, 1f)] private float clickVolume = 1f;
    [SerializeField] private AudioClip upgradeSound;
    [SerializeField][Range(0f, 1f)] private float upgradeVolume = 1f;

    private AudioSource _audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // persists across scene loads (title -> game)
        _audioSource = GetComponent<AudioSource>();
    }

    public static void PlayClick()
    {
        if (Instance != null && Instance.clickSound != null)
            Instance._audioSource.PlayOneShot(Instance.clickSound,
                Instance.clickVolume * SettingsMenu.SFXVolume);
    }

    public static void PlayUpgrade()
    {
        if (Instance != null && Instance.upgradeSound != null)
            Instance._audioSource.PlayOneShot(Instance.upgradeSound,
                Instance.upgradeVolume * SettingsMenu.SFXVolume);
    }
}