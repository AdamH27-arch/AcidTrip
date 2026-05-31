using UnityEngine;
using TMPro;

public class WaveUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WaveSpawner _spawner;

    [Header("Text Fields")]
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text enemiesText;

    private void Update()
    {
        if (_spawner == null) return;

        // Round counter
        roundText.text = _spawner.CurrentRound + " / " + _spawner.TotalRounds;

        // Wave counter within current round
        if (_spawner.WavesInRound > 1)
            waveText.text = _spawner.CurrentWave + " / " + _spawner.WavesInRound;
        else
            waveText.text = ""; // hide wave counter for single-wave rounds

        // Timer
        if (_spawner.RoundActive)
        {
            if (_spawner.IsUnlimitedWave)
                timerText.text = "KILL THE BOSS";
            else
            {
                int secs = Mathf.CeilToInt(_spawner.TimeRemaining);
                timerText.text = "0:" + secs.ToString("00");
            }
        }
        else
        {
            timerText.text = "INCOMING";
        }

        // Enemy count
        enemiesText.text = _spawner.EnemiesAlive + " enemies alive";
    }
}