using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject pausePanel;

    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Audio Mixer (optional)")]
    [SerializeField] private AudioMixer audioMixer;

    private GameObject _callerPanel;
    private const string MixerMaster = "MasterVolume";
    private const string MixerMusic = "MusicVolume";
    private const string MixerSFX = "SFXVolume";

    private const string PrefMaster = "Vol_Master";
    private const string PrefMusic = "Vol_Music";
    private const string PrefSFX = "Vol_SFX";

    // Shared SFX volume read by UISound and any other SFX sources
    public static float SFXVolume { get; private set; } = 1f;

    private void Start()
    {
        if (panel != null)
            panel.SetActive(false);

        LoadSettings();
        SetupListeners();
    }

    public void Open(GameObject callerPanel)
    {
        UISound.PlayClick();
        _callerPanel = callerPanel;

        if (_callerPanel != null)
            _callerPanel.SetActive(false);

        panel.SetActive(true);
    }

    public void Close()
    {
        UISound.PlayClick();
        SaveSettings();
        panel.SetActive(false);

        GameObject toRestore = _callerPanel != null ? _callerPanel : pausePanel;

        if (toRestore != null)
            toRestore.SetActive(true);
        else
            Debug.LogWarning("SettingsMenu: no pause panel to restore");
    }

    private void SetupListeners()
    {
        if (masterSlider != null)
            masterSlider.onValueChanged.AddListener(SetMasterVolume);

        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(SetMusicVolume);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMasterVolume(float value)
    {
        if (audioMixer != null)
            audioMixer.SetFloat(MixerMaster, SliderToDb(value));
        else
            AudioListener.volume = value;
    }

    public void SetMusicVolume(float value)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat(MixerMusic, SliderToDb(value));
        }
        else
        {
            // Drive MusicManager directly when no AudioMixer is assigned
            MusicManager.Instance?.SetMusicVolume(value);
        }
    }

    public void SetSFXVolume(float value)
    {
        SFXVolume = value;

        if (audioMixer != null)
        {
            audioMixer.SetFloat(MixerSFX, SliderToDb(value));
        }
        // Without a mixer, SFXVolume is read as a multiplier by UISound.PlayClick/PlayUpgrade
        // and by any other SFX callers that respect it
    }

    private void SaveSettings()
    {
        if (masterSlider != null) PlayerPrefs.SetFloat(PrefMaster, masterSlider.value);
        if (musicSlider != null)  PlayerPrefs.SetFloat(PrefMusic,  musicSlider.value);
        if (sfxSlider != null)    PlayerPrefs.SetFloat(PrefSFX,    sfxSlider.value);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        if (masterSlider != null)
        {
            float val = PlayerPrefs.GetFloat(PrefMaster, 1f);
            masterSlider.value = val;
            SetMasterVolume(val);
        }

        if (musicSlider != null)
        {
            float val = PlayerPrefs.GetFloat(PrefMusic, 1f);
            musicSlider.value = val;
            SetMusicVolume(val);
        }

        if (sfxSlider != null)
        {
            float val = PlayerPrefs.GetFloat(PrefSFX, 1f);
            sfxSlider.value = val;
            SetSFXVolume(val);
        }
    }

    private float SliderToDb(float value)
    {
        return value > 0.001f ? Mathf.Log10(value) * 20f : -80f;
    }

    public void ForceClose()
    {
        SaveSettings();
        panel.SetActive(false);
    }
}