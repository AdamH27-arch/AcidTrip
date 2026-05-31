using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenManager : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Tutorial";

    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject howToPlayPanel;

    [Header("Title")]
    [SerializeField] private GameObject titleText;

    private void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
    }

    public void OnPlayClicked()
    {
        UISound.PlayClick();
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnSettingsClicked()
    {
        UISound.PlayClick();
        if (settingsPanel == null) return;

        bool opening = !settingsPanel.activeSelf;
        settingsPanel.SetActive(opening);

        if (opening && howToPlayPanel != null)
            howToPlayPanel.SetActive(false);

        UpdateTitleVisibility();
    }

    public void OnHowToPlayClicked()
    {
        UISound.PlayClick();
        if (howToPlayPanel == null) return;

        bool opening = !howToPlayPanel.activeSelf;
        howToPlayPanel.SetActive(opening);

        if (opening && settingsPanel != null)
            settingsPanel.SetActive(false);

        UpdateTitleVisibility();
    }

    public void OnClosePanelClicked()
    {
        howToPlayPanel.SetActive(false);
        settingsPanel.SetActive(false);
        UpdateTitleVisibility();
    }

    public void OnExitClicked()
    {
        UISound.PlayClick();
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void UpdateTitleVisibility()
    {
        if (titleText == null) return;

        bool anyPanelOpen = (settingsPanel != null && settingsPanel.activeSelf)
                         || (howToPlayPanel != null && howToPlayPanel.activeSelf);

        titleText.SetActive(!anyPanelOpen);
    }
}