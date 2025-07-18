using UnityEngine;

public class ExpPickup : MonoBehaviour
{
    public int expValue = 10; // Her pickup ne kadar exp verir?
    public static float magnetRange = 5f;         // Oyuncuya yaklaþma mesafesi
    public float moveSpeed = 5f;           // Yaklaþma hýzý
    public float destroyTime=0;
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
        Destroy(gameObject, destroyTime);   
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameManager.Instance.AddExp(expValue);
            Destroy(gameObject);  
        }
    }
}
