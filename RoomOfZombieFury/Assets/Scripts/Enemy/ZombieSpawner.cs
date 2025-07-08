using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [Header("Prefab zombie")]
    public GameObject zombiePrefab;

    [Header("Points de spawn")]
    private Vector3[] spawnPoints = new Vector3[]
    {
        new Vector3( 90f, -2f,  40f),
        new Vector3( 90f, -2f, -40f),
        new Vector3(-90f, -2f,  40f),
        new Vector3(-90f, -2f, -40f),
    };

    [Header("Paramètres de vague")]
    public float waveInterval = 20f;
    public int initialSpawnCount = 2;
    public int spawnIncrease = 2;

    private int currentSpawnCount;

    void Start()
    {
        currentSpawnCount = initialSpawnCount;
        StartCoroutine(SpawnWaves());
    }

    private IEnumerator SpawnWaves()
    {
        while (true)
        {
            for (int i = 0; i < currentSpawnCount; i++)
            {
                int idx = Random.Range(0, spawnPoints.Length);
                Instantiate(zombiePrefab, spawnPoints[idx], Quaternion.identity);
            }

            currentSpawnCount += spawnIncrease;

            yield return new WaitForSeconds(waveInterval);
        }
    }
}
