using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class XPBar : MonoBehaviour
{
    [SerializeField] private Image fill;
    [SerializeField] private TMP_Text levelLabel;
    [SerializeField] private XPSystem xpSystem;
    [SerializeField] private TMP_Text xpLabel;

    private void Start()
    {
        if (xpSystem == null)
            xpSystem = GameObject.FindWithTag("Player").GetComponent<XPSystem>();

        xpSystem.OnXPChanged.AddListener(OnXPChanged);
        xpSystem.OnLevelUp.AddListener(OnLevelUp);

        UpdateFill(xpSystem.CurrentXP, xpSystem.XPRequired);
        UpdateLabel(xpSystem.CurrentLevel);
    }

    private void LateUpdate()
    {

        transform.rotation = Quaternion.identity;
    }

    private void OnXPChanged(int current, int required)
    {
        UpdateFill(current, required);
    }

    private void OnLevelUp(int newLevel)
    {
        UpdateLabel(newLevel);
    }

    private void UpdateFill(int current, int required)
    {
        if (fill != null)
            fill.fillAmount = (float)current / required;

        if (xpLabel != null)
            xpLabel.text = current + " / " + required;
    }

    private void UpdateLabel(int level)
    {
        if (levelLabel == null) return;
        levelLabel.text = "LVL " + level;
    }

    private void OnDestroy()
    {
        if (xpSystem == null) return;
        xpSystem.OnXPChanged.RemoveListener(OnXPChanged);
        xpSystem.OnLevelUp.RemoveListener(OnLevelUp);
    }


}