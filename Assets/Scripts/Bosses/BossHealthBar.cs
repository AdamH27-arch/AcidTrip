using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Enemy boss;
    [SerializeField] private TMP_Text healthText;

    private void Start()
    {
        if (boss == null)
            boss = GetComponentInParent<Enemy>();

        boss.OnHealthChanged.AddListener(OnHealthChanged);
        SetFill(boss.Data.maxHealth, boss.Data.maxHealth);
    }

    private void OnHealthChanged(int current, int max)
    {
        SetFill(current, max);
    }

    public void SetFill(int current, int max)
    {
        if (slider != null)
            slider.value = (float)current / max;
        if (healthText != null)
            healthText.text = current + " / " + max;
    }

    private void OnDestroy()
    {
        if (boss != null)
            boss.OnHealthChanged.RemoveListener(OnHealthChanged);
    }
}