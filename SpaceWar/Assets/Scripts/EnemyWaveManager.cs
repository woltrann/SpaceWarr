using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Buraya yazýyoruz
[System.Serializable]
public class EnemyWave 
{
    public GameObject enemyPrefab;
    public float startTime = 10f; // Ne zaman çýkmaya baþlasýn?
    public float initialSpawnInterval = 5f; // Baþta ne kadar sürede bir spawn
    public float minSpawnInterval = 1f; // En fazla ne kadar hýzlansýn
    public float spawnSpeedUpRate = 0.1f; // Ne kadar sürede bir hýzlansýn
}
[System.Serializable]
public class EnemyWaveSet
{
    public int level; // Bu set hangi levelde aktif olacak?
    public List<EnemyWave> enemies;
}

public class EnemyWaveManager : MonoBehaviour
{
    public static EnemyWaveManager Instance;
    public List<EnemyWaveSet> levelWaves;
    private int currentLevelforEnemy = 1;
    public Transform playerTransform; // Uzay gemisinin transformu
    public float spawnRadius = 50f; // Oyuncu etrafýnda spawn yarýçapý
    private int lastLevel = -1;

    private List<Coroutine> activeCoroutines = new List<Coroutine>();

    private void Awake()
    {
        Instance= this;
    }
    private void Start()
    {
        StartLevel(GameManager.currentLevel);
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }
    private void Update()
    {
        currentLevelforEnemy = GameManager.currentLevel;
        int currentLevel = GameManager.currentLevel;

        if (currentLevel != lastLevel)
        {
            lastLevel = currentLevel;
            StopAllWaves();
            StartLevel(currentLevel);
        }
    }

    public void StartLevel(int level)
    {
        // currentLevel'den küçük veya eþit olan en büyük level setini bul
        EnemyWaveSet waveSet = null;
        int maxValidLevel = int.MinValue;

        foreach (var set in levelWaves)
        {
            if (set.level <= level && set.level > maxValidLevel)
            {
                maxValidLevel = set.level;
                waveSet = set;
            }
        }

        if (waveSet == null)
        {
            Debug.LogWarning($"Level {level} için uygun dalga seti bulunamadý!");
            return;
        }

        foreach (EnemyWave wave in waveSet.enemies)
        {
            Coroutine c = StartCoroutine(SpawnWave(wave));
            activeCoroutines.Add(c);
        }
    }
    private IEnumerator SpawnWave(EnemyWave data)
    {
        yield return new WaitForSeconds(data.startTime);

        float interval = data.initialSpawnInterval;

        while (true)
        {
            SpawnEnemy(data.enemyPrefab);
            yield return new WaitForSeconds(interval);
            if (interval > data.minSpawnInterval)    
                interval -= data.spawnSpeedUpRate;           
        }
    }

    private void StopAllWaves()
    {
        foreach (var coroutine in activeCoroutines)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
        activeCoroutines.Clear();
    }

    private void SpawnEnemy(GameObject prefab)
    {
        if (playerTransform != null)
        {
            if (PlayerSmoothFollow.Instance.enemyMoveOff) return;

            float angle = Random.Range(0f, 360f);   // 0 ile 360 derece arasýnda rastgele bir açý seç
            float radians = angle * Mathf.Deg2Rad;
            Vector3 spawnPosition = playerTransform.position + new Vector3(Mathf.Cos(radians), 0f, Mathf.Sin(radians)) * spawnRadius;   // Bu açýya göre X ve Z koordinatlarýný hesapla  
            spawnPosition.y = playerTransform.position.y;    // Yüksekliðini sabit tutalým (yerde doðacak mesela)
            Instantiate(prefab, spawnPosition, Quaternion.identity);   // Düþmaný oluþtur
        }
        if (GameManager.Instance.spawnStart)
        {

        }
        else
        {
            playerTransform = null;
        }
    }
    public void transformm()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

    }
}

