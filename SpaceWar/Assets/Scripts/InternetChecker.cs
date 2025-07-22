using UnityEngine;
using UnityEngine.UI;

public class InternetChecker : MonoBehaviour
{
    public GameObject noInternetPanel;

    void Start()
    {
        InvokeRepeating(nameof(CheckInternet), 0f, 3f); // Her 3 saniyede bir kontrol
    }

    void CheckInternet()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            if (!noInternetPanel.activeSelf)
            {
                noInternetPanel.SetActive(true);
                Time.timeScale = 0; // Oyunu durdur
            }
        }
        else
        {
            if (noInternetPanel.activeSelf)
            {
                noInternetPanel.SetActive(false);
                Time.timeScale = 1; // Oyunu devam ettir
            }
        }
    }
}
