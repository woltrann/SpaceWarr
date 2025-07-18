using UnityEngine;
using UnityEngine.UI;

public class CountdownCircle : MonoBehaviour
{
    public static CountdownCircle Instance;
    public Image circleImage;
    public Text countdownText;
    public float countdownTime = 5f; // 5 saniyelik geri sayım

    private float timer;
    private bool isCounting = false;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        ResetCountdown();
    }

    void Update()
    {
        if (!isCounting) return;

        timer -= Time.deltaTime;

        // FillAmount 1 ➔ 0 arası gider
        circleImage.fillAmount = Mathf.Clamp01(timer / countdownTime);

        // Text her saniye güncellenir, floor alarak tam sayı gösterir
        countdownText.text = Mathf.Ceil(timer).ToString();

        if (timer <= 0f)
        {
            isCounting = false;
            OnCountdownComplete();
        }
    }

    public void StartCountdown()
    {
        Debug.Log("Countdown started!");
        timer = countdownTime;
        isCounting = true;
    }

    public void ResetCountdown()
    {
        timer = countdownTime;
        circleImage.fillAmount = 1f;
        countdownText.text = countdownTime.ToString(); // Reset sırasında yazı güncelle
        isCounting = false;
    }
    public void CancelCountdown()
    {
        Debug.Log("Countdown cancelled!");
        isCounting = false;
        circleImage.fillAmount = 1f;
        countdownText.text = countdownTime.ToString(); // İptal sırasında yazı güncelle
    }
    private void OnCountdownComplete()
    {
        Debug.Log("Countdown finished!");
        GameManager.Instance.GameOver();
    }
}
