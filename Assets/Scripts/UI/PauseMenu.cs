using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;
    [SerializeField] private SettingsMenu settingsMenu;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenu";

    public bool _isPaused;

    private void Start()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            Toggle();
    }

    public void Resume()
    {
        UISound.PlayClick();
        SetPaused(false);
    }

    public void QuitToMenu()
    {
        UISound.PlayClick();
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void QuitGame()
    {
        UISound.PlayClick();
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void Toggle()
    {
        if (!_isPaused && UpgradeUI.IsOpen)
            return;

        if (_isPaused)
            CloseAll();
        else
            SetPaused(true);
    }

    private void CloseAll()
    {
        // Force close settings if it's open
        if (settingsMenu != null)
            settingsMenu.ForceClose();

        SetPaused(false);
    }

    private void SetPaused(bool paused)
    {
        _isPaused = paused;
        panel.SetActive(paused);
        Time.timeScale = paused ? 0f : 1f;

        // Fade music out when paused, back in when resumed
        if (paused)
            MusicManager.Instance?.Pause();
        else
            MusicManager.Instance?.Resume();
    }

    public void OpenSettings()
    {
        UISound.PlayClick();
        settingsMenu?.Open(panel);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}