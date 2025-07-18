using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class IAP : MonoBehaviour
{
    public static IAP Instance;
    public float coin;
    public Text coinText;
    public GameObject infoPanel;

    [Header("SkillSlot")]
    public GameObject SkillSlotButton;
    public GameObject SkillSlot;
    public bool isPurchedSkillSlot=false;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        coinText.text=coin.ToString();
        coin=PlayerPrefs.GetFloat("Coin", 0);
        coinText.text=PlayerPrefs.GetString("CoinText", coinText.text);


        isPurchedSkillSlot = PlayerPrefs.GetInt("IsPurchedSkillSlot", 0) == 1;
        if (isPurchedSkillSlot)
        {
            SkillSlotButton.SetActive(false);
            SkillSlot.SetActive(true);
        }
    }

    #region New Skill Slot
    public void UnlockSkillSlot()
    {
        if (coin >= 50)
        {
            RemoveCoin(50);
            PurchedSlot();
        }
        else
        {
            StartCoroutine(panelopen());
        }
    }
    public void PurchedSlot()
    {
        SkillSlotButton.SetActive(false);
        SkillSlot.SetActive(true);
        isPurchedSkillSlot = true;
        PlayerPrefs.SetInt("IsPurchedSkillSlot", isPurchedSkillSlot ? 1 : 0);
    }
    #endregion


    public void AddCoin(float amount)
    {
        coin += amount;
        coinText.text = coin.ToString();
        PlayerPrefs.SetFloat("Coin", coin);
        PlayerPrefs.SetString("CoinText", coinText.text);
    }
    public void RemoveCoin(float amount)
    {
        coin -= amount;
        coinText.text = coin.ToString();
        PlayerPrefs.SetFloat("Coin", coin);
        PlayerPrefs.SetString("CoinText", coinText.text);
    }
    private IEnumerator panelopen()
    {
        infoPanel.SetActive(true);
        yield return new WaitForSeconds(2f);
        infoPanel.SetActive(false);
    }
    public void panelOpen()
    {
        StartCoroutine(panelopen());
    }
}
