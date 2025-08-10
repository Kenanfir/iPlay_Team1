using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class WaveEntry
{
    public EnemyClass enemyPrefab;
    public int count = 1;
}

[System.Serializable]
public class Wave
{
    public List<WaveEntry> entries = new List<WaveEntry>();
    public float spawnInterval = 0.4f; // time between spawns in a wave
}

public class WaveController : MonoBehaviour
{
    [Header("Setup")]
    public List<Wave> waves = new List<Wave>();
    public Transform[] spawnPoints;
    public float timeBetweenWaves = 2f;
    public bool autoStart = true;

    [Header("References")]
    public GameManager gameManager; // optional; if null we load scene directly

    int currentWave = -1;
    int aliveInWave = 0;
    bool spawning = false;

    void Start()
    {
        if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
        if (autoStart) StartNextWave();
    }

    public void StartNextWave()
    {
        currentWave++;
        if (currentWave >= waves.Count)
        {
            TryWin();
            return;
        }
        StartCoroutine(SpawnWave(waves[currentWave]));
    }

    IEnumerator SpawnWave(Wave wave)
    {
        spawning = true;

        foreach (var entry in wave.entries)
        {
            for (int i = 0; i < entry.count; i++)
            {
                var sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
                var enemy = Instantiate(entry.enemyPrefab, sp.position, Quaternion.identity);

                aliveInWave++;
                enemy.OnEnemyDied += OnEnemyDied;

                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }

        spawning = false;

        // wait until all are dead
        while (aliveInWave > 0) yield return null;

        yield return new WaitForSeconds(timeBetweenWaves);
        StartNextWave();
    }

    void OnEnemyDied(EnemyClass e)
    {
        aliveInWave = Mathf.Max(0, aliveInWave - 1);
    }

    void Update()
    {
        if (!spawning && currentWave >= waves.Count && aliveInWave == 0)
            TryWin();
    }

    void TryWin()
    {
        // All waves spawned & cleared
        if (gameManager != null)
            gameManager.WinGame();
        else
            SceneManager.LoadScene("Steven - Winning");
    }
}
