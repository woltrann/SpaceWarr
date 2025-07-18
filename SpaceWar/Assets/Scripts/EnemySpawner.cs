using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab; // Spawnlanacak düþman prefabý
    public Transform playerTransform; // Uzay gemisinin transformu
    public float spawnRadius = 10f; // Oyuncu etrafýnda spawn yarýçapý
    public float spawnInterval = 2f; // Kaç saniyede bir spawn olacak

    private float timer;

    void Update()
    {
        if (GameManager.Instance.spawnStart==true)
        {
            timer += Time.deltaTime;

            if (timer >= spawnInterval)
            {
                SpawnEnemy();
                timer = 0f;
            }
        }
        
    }

    void SpawnEnemy()
    {
        if (playerTransform == null) return;   
        float angle = Random.Range(0f, 360f);   // 0 ile 360 derece arasýnda rastgele bir açý seç
        float radians = angle * Mathf.Deg2Rad; 
        Vector3 spawnPosition = playerTransform.position + new Vector3(Mathf.Cos(radians), 0f, Mathf.Sin(radians)) * spawnRadius;   // Bu açýya göre X ve Z koordinatlarýný hesapla  
        spawnPosition.y = playerTransform.position.y;    // Yüksekliðini sabit tutalým (yerde doðacak mesela)
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);   // Düþmaný oluþtur
    }
}
