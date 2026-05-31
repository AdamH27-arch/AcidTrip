using UnityEngine;
using System;

public enum NewUpgradeStat
{
    Damage = 0,
    Health = 1,
    AttackRange = 2,
    DashDistance = 3,
    MoveSpeed = 4,
    Luck = 5
}

public class NewUpgradeSystem : MonoBehaviour
{
    public static NewUpgradeSystem Instance;

    private const int MaxLevel = 10;
    private const int StatCount = 6;

    private int[] _levels = new int[StatCount];
    private int _availablePoints = 0;

    public event Action<int> OnPointsChanged;
    public event Action OnStatsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public int AvailablePoints => _availablePoints;

    // Called by XPSystem on level up
    public void AddPoint()
    {
        _availablePoints++;
        OnPointsChanged?.Invoke(_availablePoints);
    }


    public bool CanUpgrade(NewUpgradeStat stat) => _availablePoints > 0 && _levels[(int)stat] < MaxLevel;
    public bool CanDowngrade(NewUpgradeStat stat) => _levels[(int)stat] > 0;

    public void Upgrade(NewUpgradeStat stat)
    {
        if (!CanUpgrade(stat)) return;
        _levels[(int)stat]++;
        _availablePoints--;
        OnPointsChanged?.Invoke(_availablePoints);
        OnStatsChanged?.Invoke();

        if (stat == NewUpgradeStat.Health)
            ApplyMaxHealth();
    }

    public void Downgrade(NewUpgradeStat stat)
    {
        if (!CanDowngrade(stat)) return;
        _levels[(int)stat]--;
        _availablePoints++;
        OnPointsChanged?.Invoke(_availablePoints);
        OnStatsChanged?.Invoke();

        if (stat == NewUpgradeStat.Health)
            ApplyMaxHealth();
    }

    public void ResetAll()
    {
        int refund = 0;
        for (int i = 0; i < StatCount; i++)
        {
            refund += _levels[i];
            _levels[i] = 0;
        }
        _availablePoints += refund;
        OnPointsChanged?.Invoke(_availablePoints);
        OnStatsChanged?.Invoke();
        ApplyMaxHealth();
    }



    public int GetLevel(NewUpgradeStat stat) => _levels[(int)stat];
    public float GetCurrentValue(NewUpgradeStat stat) => _levels[(int)stat] * PerLevel[(int)stat];

    private static readonly float[] PerLevel =
    {
        5f,    // Damage:       +5 flat damage per level
        20f,   // Health:       +20 max HP per level
        0.2f,  // AttackRange:  +0.2 lifetime per level
        0.5f,  // DashDistance: +0.5 units per level
        0.5f,  // MoveSpeed:    +0.5 units per level
        5f,    // Luck:         +5% per level
    };



    public int Damage => 10 + _levels[0] * 5;
    public int BonusMaxHealth => _levels[1] * 20;
    public float AttackRangeBonus => _levels[2] * 0.2f;
    public float DashDistanceBonus => _levels[3] * 0.5f;
    public float MoveSpeedBonus => _levels[4] * 0.5f;
    public float LuckMultiplier => 1f + _levels[5] * 0.05f;


    private void ApplyMaxHealth()
    {
        PlayerHealth ph = FindAnyObjectByType<PlayerHealth>();
        if (ph != null)
            ph.SetMaxHealth(100 + BonusMaxHealth);
    }
}