using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public static Enemy Instance;

    public bool isBoss = false;
    public bool inBlackHole=false;

    public GameObject expPrefab;
    public int expAmount = 3;
    public GameObject goldPrefab;
    public int goldAmount = 3;
    public int score = 0;

    private Transform playerTransform;

    public float moveSpeed = 5f;
    public GameObject bulletPrefab;
    public float shootInterval = 2f;
    private float shootTimer;
    public float bulletSpeed = 10f;

    public GameObject hitParticle;
    public float attackRange = 20f;

    public float maxHealth = 100f;
    private float currentHealth;
    public int attackDamage = 5;

    // ✔ Stat çarpanı
    public float statMultiplier = 1f;

    private void Start()
    {
        Instance = this;
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        int selectedMapIndex = MapManager.Instance.currentIndex;
        int usedMapIndex = MapManager.Instance.maps[selectedMapIndex].isUnlocked
            ? selectedMapIndex
            : MapManager.Instance.GetLastUnlockedMapIndex();

        int mapLevel = usedMapIndex + 1; // çünkü 3^1, 3^2 gibi başlıyor
        statMultiplier = Mathf.Pow(3f, mapLevel);

        maxHealth *= statMultiplier;
        currentHealth = maxHealth;
        attackDamage = Mathf.RoundToInt(attackDamage * statMultiplier);

        // Şu ikisi hep 1 oluyorsa gerek yok
        moveSpeed *= 1f;
        bulletSpeed *= 1f;

        statMultiplier = 0;
    }
    private void Update()
    {
        if (playerTransform == null) return;
        if (PlayerSmoothFollow.Instance.isInvisible) return;

        if (maxHealth > 0 && !PlayerSmoothFollow.Instance.enemyMoveOff)
        {

            //if(inBlackHole) return;
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(direction);

            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= attackRange)
            {
                shootTimer += Time.deltaTime;
                if (shootTimer >= shootInterval )
                {
                    if (PlayerSmoothFollow.Instance.enemyBulletOff) return; // hedefleme veya vurma iptal
                    if (PlayerSmoothFollow.Instance.isInvisible) return;
                    Shoot();
                    shootTimer = 0f;
                }
            }
        }
     
    }
    private void Shoot()
    {
        if (playerTransform == null) return;

        Vector3 direction = (playerTransform.position - transform.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.LookRotation(direction));
        bullet.GetComponent<Rigidbody>().linearVelocity = direction * bulletSpeed;

        EnemyBullet projScript = bullet.GetComponent<EnemyBullet>();
        if (projScript != null)
        {
            projScript.attackDamage = attackDamage;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    private void Die()
    {
        if (isBoss)
        {
            GameManager.Instance.Congrats();
        }

        if (hitParticle != null)
        {
            GameObject particleEffect = Instantiate(hitParticle, transform.position, Quaternion.identity);
            ParticleSystem ps = particleEffect.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            Destroy(particleEffect, 2f);
        }

        for (int i = 0; i < expAmount; i++)
        {
            Vector3 offset = Random.insideUnitSphere * 2.5f;
            offset.y = 0;
            Instantiate(expPrefab, transform.position + offset, Quaternion.identity);
        }

        for (int i = 0; i < goldAmount; i++)
        {
            Vector3 offset = Random.insideUnitSphere * 2.5f;
            offset.y = 0;
            Instantiate(goldPrefab, transform.position + offset, goldPrefab.transform.rotation);
        }

        Destroy(gameObject);
        GameManager.Instance.AddScore(score);
        GameManager.Instance.KilledScore();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null)
            {
                TakeDamage(bullet.GetDamage());
            }
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Player"))
        {
            TakeDamage(100f);
        }
        if (other.CompareTag("ShieldArea"))
        {
            TakeDamage(100f);
        }
        if (other.CompareTag("Meteor"))
        {
            TakeDamage(PlayerSmoothFollow.Instance.damage * 3);
        }
        if(other.CompareTag("BlackHole"))
        {
            inBlackHole = true;
            Debug.Log("BlackHole");
        }
        else
        {
            inBlackHole = false;
        }
        if(other.CompareTag("inBlackHole"))
        {
            Destroy(gameObject);
        }
    }
}
