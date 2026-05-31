using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class UpgradeUINew : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("Available Points")]
    [SerializeField] private TMP_Text pointsText;

    [Header("Level Labels (0 / 10) -- one per stat in order")]
    [SerializeField] private TMP_Text[] levelTexts = new TMP_Text[6];

    [Header("Next Value Labels -- one per stat in order")]
    [SerializeField] private TMP_Text[] nextValueTexts = new TMP_Text[6];

    [Header("Stat Info Labels (current value) -- one per stat in order")]
    [SerializeField] private TMP_Text[] statInfoTexts = new TMP_Text[6];

    [Header("Plus Buttons -- one per stat in order")]
    [SerializeField] private Button[] plusButtons = new Button[6];

    [Header("Minus Buttons -- one per stat in order")]
    [SerializeField] private Button[] minusButtons = new Button[6];

    [Header("Reset Button")]
    [SerializeField] private Button resetButton;

    private PauseMenu _pauseMenu;

    // What each level up gives -- shown in the stat info label
    private static readonly string[] PerLevelDesc =
    {
        "+1 Damage",
        "+20 HP",
        "+0.2 Attack Range",
        "+0.05 Dash Distance",
        "+0.2 Move Speed",
        "+5% Luck"
    };

    // Format for current value display
    private string GetCurrentValueText(int i)
    {
        UpgradeSystem up = UpgradeSystem.Instance;
        switch (i)
        {
            case 0: return "Damage: " + up.Damage;
            case 1: return "Max HP: " + (100 + up.BonusMaxHealth);
            case 2: return "Range: " + up.ProjectileRange.ToString("0.00");
            case 3: return "Dash Dist: " + up.DashDistanceBonus.ToString("0.00");
            case 4: return "Move Speed: " + up.MoveSpeed.ToString("0.00");
            case 5: return "Luck: " + (up.LuckMultiplier * 100f - 100f).ToString("0") + "%";
            default: return "";
        }
    }

    private string GetNextValueText(int i)
    {
        UpgradeSystem up = UpgradeSystem.Instance;
        if (up.GetLevel((UpgradeStat)i) >= 10) return "MAX";

        switch (i)
        {
            case 0: return "Next: " + (up.Damage + 1);
            case 1: return "Next: " + (100 + up.BonusMaxHealth + 20) + " HP";
            case 2: return "Next: " + (up.ProjectileRange + 0.2f).ToString("0.00");
            case 3: return "Next: " + (up.DashDistanceBonus + 0.05f).ToString("0.00");
            case 4: return "Next: " + (up.MoveSpeed + 0.2f).ToString("0.00");
            case 5: return "Next: +" + ((up.LuckMultiplier * 100f - 100f) + 5f).ToString("0") + "%";
            default: return "";
        }
    }

    private void Start()
    {

        _pauseMenu = FindAnyObjectByType<PauseMenu>();

        panel.SetActive(false);

        for (int i = 0; i < 6; i++)
        {
            int index = i;
            plusButtons[i].onClick.AddListener(() => OnPlus(index));
            minusButtons[i].onClick.AddListener(() => OnMinus(index));
        }

        if (resetButton != null)
            resetButton.onClick.AddListener(OnReset);

        if (UpgradeSystem.Instance != null)
        {
            UpgradeSystem.Instance.OnPointsChanged += _ => Refresh();
            UpgradeSystem.Instance.OnStatsChanged += Refresh;
        }
    }

    private void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            if (IsOpen) Close();
            else Open();
        }
    }

    public void Open()
    {
        if (_pauseMenu != null && _pauseMenu._isPaused) return;
        IsOpen = true;
        panel.SetActive(true);
        Time.timeScale = 0f;
        Refresh();
    }

    public void Close()
    {
        IsOpen = false;
        panel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void OnPlus(int i)
    {
        if (UpgradeSystem.Instance != null && UpgradeSystem.Instance.CanUpgrade((UpgradeStat)i))
        {
            UpgradeSystem.Instance.Upgrade((UpgradeStat)i);
            UISound.PlayUpgrade();
        }
    }

    private void OnMinus(int i)
    {
        if (UpgradeSystem.Instance != null && UpgradeSystem.Instance.CanDowngrade((UpgradeStat)i))
        {
            UpgradeSystem.Instance.Downgrade((UpgradeStat)i);
            UISound.PlayUpgrade();
        }
    }

    private void OnReset()
    {
        UpgradeSystem.Instance?.ResetAll();
        UISound.PlayUpgrade();
    }

    private void Refresh()
    {
        UpgradeSystem up = UpgradeSystem.Instance;
        if (up == null) return;

        if (pointsText != null)
            pointsText.text = "Points Available: " + up.AvailablePoints;

        for (int i = 0; i < 6; i++)
        {
            int level = up.GetLevel((UpgradeStat)i);

            if (levelTexts[i] != null)
                levelTexts[i].text = level + " / 10";

            if (nextValueTexts[i] != null)
                nextValueTexts[i].text = GetNextValueText(i);

            if (statInfoTexts[i] != null)
                statInfoTexts[i].text = PerLevelDesc[i] + "\n" + GetCurrentValueText(i);

            if (plusButtons[i] != null)
                plusButtons[i].interactable = up.CanUpgrade((UpgradeStat)i);

            if (minusButtons[i] != null)
                minusButtons[i].interactable = up.CanDowngrade((UpgradeStat)i);
        }
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}