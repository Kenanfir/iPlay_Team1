using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns 3 predefined waves. Tracks progress (kills / total).
/// Keep spawn rules simple so designers can tune counts in the inspector.
/// </summary>
public class WaveController : MonoBehaviour
{
    [System.Serializable]
    public class WaveDefinition
    {
        [Header("Counts per enemy tier")]
        public int level1Count = 5;
        public int level2Count = 3;
        public int level3Count = 2;

        public int Total => Mathf.Max(0, level1Count) + Mathf.Max(0, level2Count) + Mathf.Max(0, level3Count);
    }

    [Header("Waves (3 as per design)")]
    [Tooltip("Configure exactly three waves for the jam spec.")]
    public WaveDefinition[] waves = new WaveDefinition[3]
    {
        new WaveDefinition(){ level1Count = 6, level2Count = 0, level3Count = 0 },
        new WaveDefinition(){ level1Count = 6, level2Count = 3, level3Count = 0 },
        new WaveDefinition(){ level1Count = 4, level2Count = 3, level3Count = 2 },
    };

    [Header("Spawn Setup")]
    [Tooltip("Areas (or exact points) the wave can spawn from—e.g., your 3 boxes.")]
    public Transform[] spawnAreas;                 // drop 3 Transforms representing the boxes
    [Tooltip("Prefabs mapped by tier index (1=L1, 2=L2, 3=L3).")]
    public GameObject enemyLevel1Prefab;
    public GameObject enemyLevel2Prefab;
    public GameObject enemyLevel3Prefab;
    [Tooltip("Seconds between spawns.")]
    public float spawnInterval = 0.6f;

    [Header("Wave Progress UI Hook")]
    [Tooltip("0..1 progress value every time a kill happens.")]
    public UnityEngine.Events.UnityEvent<float> OnProgressChanged;
    [Tooltip("Wave index changed (1-based index, total waves).")]
    public UnityEngine.Events.UnityEvent<int, int> OnWaveChanged;

    private int _currentWaveIndex = -1;         // -1 = not started
    private int _killsThisWave = 0;
    private int _alive = 0;

    private void Start()
    {
        StartNextWave(); // auto-start; or call manually from GameMaster/menu
    }

    // --- Public: called by GameMaster when an enemy dies ---
    public void NotifyEnemyKilled(EnemyTier _)
    {
        if (!IsWaveActive) return;
        _killsThisWave = Mathf.Clamp(_killsThisWave + 1, 0, CurrentWave.Total);
        _alive = Mathf.Max(0, _alive - 1);

        float progress = CurrentWave.Total == 0 ? 1f : (float)_killsThisWave / CurrentWave.Total;
        OnProgressChanged?.Invoke(progress);

        // Wave ends when all enemies for this wave have been killed
        if (_killsThisWave >= CurrentWave.Total && _alive == 0)
        {
            StartNextWave();
        }
    }

    // --- Internals ---
    private bool IsWaveActive => _currentWaveIndex >= 0 && _currentWaveIndex < waves.Length;
    private WaveDefinition CurrentWave => waves[Mathf.Clamp(_currentWaveIndex, 0, waves.Length - 1)];

    private void StartNextWave()
    {
        _currentWaveIndex++;
        if (_currentWaveIndex >= waves.Length)
        {
            Debug.Log("[WaveController] All waves cleared!");
            OnWaveChanged?.Invoke(waves.Length, waves.Length);
            // TODO: Notify victory / open high-score screen.
            return;
        }

        _killsThisWave = 0;
        _alive = 0;

        OnProgressChanged?.Invoke(0f);
        OnWaveChanged?.Invoke(_currentWaveIndex + 1, waves.Length);

        StopAllCoroutines();
        StartCoroutine(SpawnWaveRoutine(CurrentWave));
    }

    private IEnumerator SpawnWaveRoutine(WaveDefinition def)
    {
        // Spawn each tier in simple round-robin to keep pressure balanced.
        int l1 = def.level1Count, l2 = def.level2Count, l3 = def.level3Count;

        while (l1 > 0 || l2 > 0 || l3 > 0)
        {
            // Level 1
            if (l1-- > 0) Spawn(enemyLevel1Prefab);
            yield return new WaitForSeconds(spawnInterval);

            // Level 2
            if (l2-- > 0) Spawn(enemyLevel2Prefab);
            yield return new WaitForSeconds(spawnInterval);

            // Level 3
            if (l3-- > 0) Spawn(enemyLevel3Prefab);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void Spawn(GameObject prefab)
    {
        if (prefab == null) return;
        if (spawnAreas == null || spawnAreas.Length == 0) return;

        Transform area = spawnAreas[Random.Range(0, spawnAreas.Length)];
        Vector3 pos = GetRandomPoint(area);
        Instantiate(prefab, pos, Quaternion.identity);
        _alive++;
    }

    /// <summary>
    /// If you assign a Rect/Box transform as an area, this picks a random point inside.
    /// For a single point, just use its position.
    /// </summary>
    private Vector3 GetRandomPoint(Transform area)
    {
        // If the area has a BoxCollider as a helper, use its bounds.
        var box = area.GetComponent<BoxCollider2D>();
        if (box != null)
        {
            Bounds b = box.bounds;
            return new Vector3(Random.Range(b.min.x, b.max.x), Random.Range(b.min.y, b.max.y), 0f);
        }

        // Otherwise, just use the transform position (drop multiple points to fake an area)
        return area.position;
    }
}
