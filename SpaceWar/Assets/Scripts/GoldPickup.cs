using UnityEngine;

public class GoldPickup : MonoBehaviour
{
    public int goldValue = 10; // Her pickup ne kadar exp verir?
    public static float magnetRange = 5f;         // Oyuncuya yaklaþma mesafesi
    public float moveSpeed = 5f;           // Yaklaþma hýzý

    private Transform player;

    private void Start()
    {

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void Update()
    {

        if (player != null)
        {
            if (Vector3.Distance(transform.position, player.position) < magnetRange)
            {
                // Oyuncuya doðru hareket et
                transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameManager.Instance.AddGold(goldValue);
            Destroy(gameObject);
        }
    }
}
