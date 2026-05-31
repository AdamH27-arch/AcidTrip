using UnityEngine;
using UnityEngine.Events;

public class XPSystem : MonoBehaviour
{
    [Header("Leveling")]
    [SerializeField] private int baseXP = 100;
    [SerializeField] private float scalingFactor = 1.25f;

    public UnityEvent<int, int> OnXPChanged;  // current, required
    public UnityEvent<int> OnLevelUp;         // new level

    private int _currentXP = 0;
    private int _currentLevel = 1;
    private int _xpRequired;

    public int CurrentXP => _currentXP;
    public int CurrentLevel => _currentLevel;
    public int XPRequired => _xpRequired;

    private void Awake()
    {
        _xpRequired = CalculateXPRequired(_currentLevel);
    }

    public void AddXP(int amount)
    {
        float multiplier = UpgradeSystem.Instance != null
            ? UpgradeSystem.Instance.LuckMultiplier : 1f;

        // Boost XP gain after round 13
        if (WaveSpawner.Instance != null && WaveSpawner.Instance.CurrentRound > 13)
            multiplier *= 2f;

        _currentXP += Mathf.RoundToInt(amount * multiplier);

        while (_currentXP >= _xpRequired)
        {
            _currentXP -= _xpRequired;
            LevelUp();
        }

        OnXPChanged?.Invoke(_currentXP, _xpRequired);
    }



    private void LevelUp()
    {
        _currentLevel++;
        _xpRequired = CalculateXPRequired(_currentLevel);
        OnLevelUp?.Invoke(_currentLevel);

        UpgradeSystem.Instance?.AddPoint();

        TutorialController.Instance?.TriggerStep("levelup");
    }

    private int CalculateXPRequired(int level)
    {
        return Mathf.FloorToInt(baseXP * Mathf.Pow(scalingFactor, level - 1));
    }


}