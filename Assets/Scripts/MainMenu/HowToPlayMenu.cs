using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HowToPlayMenu : MonoBehaviour
{
    [Header("Button Panel")]
    [SerializeField] private GameObject buttonPanel;

    [Header("Title")]
    [SerializeField] private TMP_Text titleText;

    [Header("Topic Buttons")]
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button upgradesButton;
    [SerializeField] private Button abilitiesButton;
    [SerializeField] private Button itemsButton;

    [Header("Content Panels")]
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject upgradesPanel;
    [SerializeField] private GameObject abilitiesPanel;
    [SerializeField] private GameObject itemsPanel;

    [Header("Back Button")]
    [SerializeField] private Button backButton;

    private const string DefaultTitle = "How to Play";

    private void Start()
    {
        controlsButton.onClick.AddListener(() => ShowPanel(controlsPanel, "Controls"));
        upgradesButton.onClick.AddListener(() => ShowPanel(upgradesPanel, "Upgrades"));
        abilitiesButton.onClick.AddListener(() => ShowPanel(abilitiesPanel, "Abilities"));
        itemsButton.onClick.AddListener(() => ShowPanel(itemsPanel, "Items"));

        backButton.onClick.AddListener(ShowButtons);

        HideAllPanels();
        backButton.gameObject.SetActive(false);
    }

    private void ShowPanel(GameObject panel, string title)
    {
        HideAllPanels();
        buttonPanel.SetActive(false);
        panel.SetActive(true);
        backButton.gameObject.SetActive(true);

        if (titleText != null)
            titleText.text = title;
    }

    public void ShowButtons()
    {
        HideAllPanels();
        buttonPanel.SetActive(true);
        backButton.gameObject.SetActive(false);

        if (titleText != null)
            titleText.text = DefaultTitle;
    }

    private void HideAllPanels()
    {
        controlsPanel.SetActive(false);
        upgradesPanel.SetActive(false);
        abilitiesPanel.SetActive(false);
        itemsPanel.SetActive(false);
    }
}