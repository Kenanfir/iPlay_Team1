using UnityEngine;
using System;
using Random = UnityEngine.Random;

/// <summary>
/// Central game state: score, player hearts (in half-hearts),
/// battery, and kill/drop resolution. Keep this tiny & explicit for jam use.
/// Hook your UI to the public events.
/// </summary>
public class GameMaster : MonoBehaviour
{
    // --- Singleton (simple & jam-safe) ---
    public static GameMaster I { get; private set; }

    [Header("Player Hearts (Half-Heart System)")]
    [Tooltip("Total hearts displayed to the player.")]
    [SerializeField] private int maxHearts = 3;       // 3 hearts total
    [Tooltip("Half hearts = hearts * 2. Do not edit at runtime.")]
    [SerializeField] private int currentHalfHearts;   // runtime, clamped 0..max*2

    [Header("Battery")]
    [Tooltip("0..100. Drain elsewhere; add here when collecting batteries.")]
    [SerializeField, Range(0, 100)] private float batteryPercent = 100f;

    [Header("Score")]
    [SerializeField] private int score = 0;

    [Header("Wave System")]
    [Tooltip("Assign the WaveController in the scene.")]
    [SerializeField] private WaveController waveController;

    // --- Events for UI or other systems ---
    public event Action<int, int> OnHeartsChanged;      // (currentHalfHearts, maxHalfHearts)
    public event Action<float> OnBatteryChanged;        // battery 0..100
    public event Action<int> OnScoreChanged;            // new score
    public event Action<int, int> OnPlayerDamaged;      // (amountHalfHearts, remainingHalfHearts)

    // Optional: event others can listen to when an enemy dies.
    public event Action<EnemyTier> OnEnemyKilled;

    // --- Constants from design sheet (tweak freely) ---
    private const int PointsL1 = 10;
    private const int PointsL2 = 20;
    private const int PointsL3 = 30;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;

        // Start at full health unless set in inspector.
        int maxHalf = MaxHalfHearts;
        if (currentHalfHearts <= 0) currentHalfHearts = maxHalf;

        // Let UI initialize
        OnHeartsChanged?.Invoke(currentHalfHearts, MaxHalfHearts);
        OnBatteryChanged?.Invoke(batteryPercent);
        OnScoreChanged?.Invoke(score);
    }

    // --- Public read-only accessors for convenience ---
    public int Score => score;
    public int MaxHalfHearts => maxHearts * 2;
    public int CurrentHalfHearts => currentHalfHearts;
    public float BatteryPercent => batteryPercent;

    // --- Player Health ---
    /// <summary>Deal damage in half-hearts (1 = half-heart).</summary>
    public void DamagePlayer(int halfHearts = 1)
    {
        if (halfHearts <= 0) return;
        currentHalfHearts = Mathf.Max(0, currentHalfHearts - halfHearts);
        OnPlayerDamaged?.Invoke(halfHearts, currentHalfHearts);
        OnHeartsChanged?.Invoke(currentHalfHearts, MaxHalfHearts);

        if (currentHalfHearts <= 0)
        {
            // TODO: trigger death sequence / game over screen.
            Debug.Log("[GameMaster] Player died.");
        }
    }

    /// <summary>Add half-hearts (e.g., Level 3 drop = Half Heart 20%).</summary>
    public void AddHalfHearts(int halfHearts = 1)
    {
        if (halfHearts <= 0) return;
        currentHalfHearts = Mathf.Clamp(currentHalfHearts + halfHearts, 0, MaxHalfHearts);
        OnHeartsChanged?.Invoke(currentHalfHearts, MaxHalfHearts);
    }

    // --- Battery ---
    /// <summary>Add battery as percent (0..100). Negative values allowed for drain if needed.</summary>
    public void AddBattery(float deltaPercent)
    {
        batteryPercent = Mathf.Clamp(batteryPercent + deltaPercent, 0f, 100f);
        OnBatteryChanged?.Invoke(batteryPercent);
    }

    // --- Score ---
    public void AddScore(int points)
    {
        if (points <= 0) return;
        score += points;
        OnScoreChanged?.Invoke(score);
    }

    // --- Enemy Kill + Drop Resolution (from design card) ---
    /// <summary>
    /// Call this when an enemy dies. GameMaster handles points & drops.
    /// Typically invoked by an Enemy component.
    /// </summary>
    public void RegisterEnemyKill(EnemyTier tier)
    {
        // Points (100% for all tiers)
        switch (tier)
        {
            case EnemyTier.Level1: AddScore(PointsL1); break;
            case EnemyTier.Level2: AddScore(PointsL2); break;
            case EnemyTier.Level3: AddScore(PointsL3); break;
        }

        // Drops per design:
        // L1: 100% Score only.
        // L2: + 30% Battery.
        // L3: + 50% Battery, + 20% Half-Heart.
        if (tier == EnemyTier.Level2)
        {
            if (Random.value <= 0.30f) AddBattery(+15f); // choose a sensible battery amount
        }
        else if (tier == EnemyTier.Level3)
        {
            if (Random.value <= 0.50f) AddBattery(+20f);
            if (Random.value <= 0.20f) AddHalfHearts(1); // 1 half-heart
        }

        // Inform listeners (e.g., WaveController progress)
        OnEnemyKilled?.Invoke(tier);
        if (waveController != null) waveController.NotifyEnemyKilled(tier);
    }

    // --- Simple helper for debug / cheats during jam ---
    [ContextMenu("DEBUG: Heal to Full")]
    private void Debug_HealFull()
    {
        currentHalfHearts = MaxHalfHearts;
        OnHeartsChanged?.Invoke(currentHalfHearts, MaxHalfHearts);
    }
}

/// <summary>Enemy tiers used across systems. Keep tiny and explicit.</summary>
public enum EnemyTier { Level1 = 1, Level2 = 2, Level3 = 3 }
