using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private TMP_Text healthText;
    private void Start()
    {
        if (playerHealth == null)
            playerHealth = GetComponentInParent<PlayerHealth>();

        playerHealth.OnHealthChanged.AddListener(OnHealthChanged);
        SetFill(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }

    private void LateUpdate()
    {
        // Cancel any rotation inherited from the player so the bar stays upright
        transform.rotation = Quaternion.identity;
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
        if (playerHealth != null)
            playerHealth.OnHealthChanged.RemoveListener(OnHealthChanged);
    }
}