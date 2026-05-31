using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text roundsSurvivedText;

    [Header("Death Animation")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private string deathTrigger = "isDead";
    [SerializeField] private float deathDuration = 1.5f;

    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "Tutorial";
    [SerializeField] private string menuSceneName = "MainMenu";

    private PlayerHealth _playerHealth;
    private WaveSpawner _waveSpawner;

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        _playerHealth = FindAnyObjectByType<PlayerHealth>();
        _waveSpawner = FindAnyObjectByType<WaveSpawner>();

        if (_playerHealth == null)
        {
            Debug.LogError("GameOverManager: PlayerHealth not found.");
            return;
        }

        _playerHealth.OnDeath.AddListener(OnPlayerDied);
    }

    private void OnPlayerDied()
    {
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        DisablePlayerMovement();

        if (playerAnimator != null)
            playerAnimator.SetTrigger(deathTrigger);

        yield return new WaitForSecondsRealtime(deathDuration);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (roundsSurvivedText != null && _waveSpawner != null)
        {
            int rounds = Mathf.Max(0, _waveSpawner.CurrentRound - 1);
            roundsSurvivedText.text = "You survived " + rounds + " rounds";
        }

        Time.timeScale = 0f;
    }

    private void DisablePlayerMovement()
    {
        PlayerController controller = FindAnyObjectByType<PlayerController>();
        if (controller != null) controller.enabled = false;

        PlayerMovement movement = FindAnyObjectByType<PlayerMovement>();
        if (movement != null) movement.enabled = false;
    }

    public void Restart()
    {
        Time.timeScale = 1f;

        // Reset ability so the selection screen appears again at level 5
        if (AbilityManager.Instance != null)
            AbilityManager.Instance.ResetAbility();

        SceneManager.LoadScene(gameSceneName);
    }

    // Wire to your Main Menu button OnClick
    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;

        if (_playerHealth != null)
            _playerHealth.OnDeath.RemoveListener(OnPlayerDied);
    }
}