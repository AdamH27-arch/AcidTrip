using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityHUD : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject hudPanel;

    [Header("UI Elements")]
    [SerializeField] private Image abilityIcon;    // selected ability image
    [SerializeField] private Image cooldownFill;   // ring fill -- Image Type: Filled, Radial 360
    [SerializeField] private Image centerCircle;   // middle circle -- goes clear when ready
    [SerializeField] private TMP_Text cooldownNumber; // countdown number in the center
    [SerializeField] private TMP_Text tierLabel;      // optional tier display

    [Header("Ability Icons")]
    [SerializeField] private Sprite sniperIcon;
    [SerializeField] private Sprite boomerangIcon;
    [SerializeField] private Sprite bombIcon;
    [SerializeField] private Sprite scatterIcon;
    [SerializeField] private Sprite blinkIcon;

    private void Start()
    {
        if (hudPanel != null)
            hudPanel.SetActive(false);
    }

    private void Update()
    {
        if (AbilityManager.Instance == null) return;

        bool hasAbility = AbilityManager.Instance.HasAbility;

        if (hudPanel != null)
            hudPanel.SetActive(hasAbility);

        if (!hasAbility) return;

        AbilityType type = AbilityManager.Instance.CurrentAbility.Value;
        float remaining = AbilityManager.Instance.CooldownRemaining;
        float max = AbilityManager.Instance.GetMaxCooldown(type);
        bool onCooldown = remaining > 0f;

        // Ability icon
        if (abilityIcon != null)
            abilityIcon.sprite = GetIcon(type);

        // Ring fill -- full when just used, drains to empty as cooldown expires
        if (cooldownFill != null)
            cooldownFill.fillAmount = max > 0f ? remaining / max : 0f;

        // Countdown number -- visible and counting down during cooldown, hidden when ready
        if (cooldownNumber != null)
        {
            cooldownNumber.gameObject.SetActive(onCooldown);
            if (onCooldown)
                cooldownNumber.text = Mathf.CeilToInt(remaining).ToString();
        }

        // Center circle -- visible during cooldown, clear when ready
        if (centerCircle != null)
        {
            Color c = centerCircle.color;
            c.a = onCooldown ? 1f : 0f;
            centerCircle.color = c;
        }

        // Optional tier label
        if (tierLabel != null)
            tierLabel.text = "TIER " + AbilityManager.Instance.AbilityTier;
    }

    private Sprite GetIcon(AbilityType type)
    {
        switch (type)
        {
            case AbilityType.Sniper: return sniperIcon;
            case AbilityType.Boomerang: return boomerangIcon;
            case AbilityType.Bomb: return bombIcon;
            case AbilityType.Scatter: return scatterIcon;
            case AbilityType.Blink: return blinkIcon;
            default: return null;
        }
    }
}