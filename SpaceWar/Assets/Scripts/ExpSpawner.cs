using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpSpawner : MonoBehaviour
{
    public GameObject expPrefab;
    public float spawnRadius = 5f;
    public float spawnInterval = 2f;
    public int maxExpCount = 5;

    private Transform player;

    void Start()
    {

        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(SpawnExpRoutine());
    }
    IEnumerator SpawnExpRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            GameObject[] existingExps = GameObject.FindGameObjectsWithTag("Exp");
            if (existingExps.Length >= maxExpCount)
                continue;

            Vector2 randomXZ = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPosition = new Vector3(
                player.position.x + randomXZ.x,
                player.position.y, // Y deðeri artýk oyuncunun yüksekliðiyle ayný
                player.position.z + randomXZ.y
            );

            Instantiate(expPrefab, spawnPosition, Quaternion.identity);
        }
    }

}
