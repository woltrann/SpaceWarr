using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Components;


public class SkillPurchaseHandler : MonoBehaviour
{
    public static SkillPurchaseHandler Instance;
    [Header("Skill Details Panel Bilgileri")]
    public SkillDetails skillDetails; // Inspector’dan atanacak
    public Button CloseButton;
    public Button BuyButton;
    public Text skillNameText;
    public LocalizeStringEvent skillDescriptionLocalizedText;
    public Text skillDescriptionText;
    public Image skillIcon;
    public Text skillPriceText;
    public GameObject skillDetailsPanel; // Panelin kendisi (aktif/pasif yapmak için)

    private SkillDetails currentSkillDetails;
    private SkillDragHandler currentSkillDragHandler;
    private int currentSkillPrice;
    private int currentSkillID;

    public int skillIndex; // Örn: 1, 2, 3...
    public bool isUnlocked = false;

    public int skillPrice = 100; // Bu skillin fiyatý (Inspector’dan ayarlanýr)
    public SkillDragHandler skillDragHandler; // Ayný objedeki SkillDragHandler referansý

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        if (skillDragHandler != null && skillDragHandler.isUnlocked)        // Skill zaten açýlmýþsa, butonu kapat
        {
            skillDragHandler.UnlockSkill();
            GetComponent<Button>().interactable = false;
        }
        GetComponent<Button>().onClick.AddListener(() => ShowSkillInfoPanel(skillDetails, skillPrice));
        CloseButton.onClick.AddListener(CloseSkillPanel);
        BuyButton.onClick.AddListener(TryBuySkill);
    }
    public void ActivateSkill(int id)
    {
        skillIndex = id;
        if (PlayerPrefs.GetInt("SkillUnlocked_" + skillIndex, 0) == 1)        // Skill zaten açýlmýþsa, butonu kapat
        {
            currentSkillDragHandler.UnlockSkill();
            GetComponent<Button>().interactable = false;
            
        }

    }

    private void TryBuySkill()
    {
        float totalGold = GameManager.Instance.totalGold;

        if (currentSkillDragHandler == null)
        {
            return;
        }

        if (currentSkillDragHandler.isUnlocked)
        {
            return;
        }

        if (totalGold >= currentSkillPrice)
        {
            GameManager.Instance.totalGold -= currentSkillPrice;
            GameManager.Instance.TotalGoldText.text = GameManager.Instance.totalGold.ToString();
            PlayerPrefs.SetFloat("TotalGold", GameManager.Instance.totalGold);

            PlayerPrefs.SetInt("SkillUnlocked_" + skillIndex, 1);
            PlayerPrefs.Save();

            currentSkillDragHandler.UnlockSkill();

            CloseSkillPanel();
        }
        else
        {
        }
    }

    public void ShowSkillInfoPanel(SkillDetails skillDetails, int skillPrice)
    {    
        if (skillDetails == null)
        {
            return;
        }

        currentSkillDetails = skillDetails;
        currentSkillPrice = skillPrice;
        currentSkillDragHandler = skillDragHandler;
        currentSkillID = skillIndex;


        skillNameText.text = skillDetails.cardName;
        skillDescriptionLocalizedText.StringReference = skillDetails.cardDescription;
        skillDescriptionLocalizedText.RefreshString();
        skillDescriptionText.text = skillDetails.cardDescription.GetLocalizedString(); // Eðer çeviri önceden load edildiyse

        skillIcon.sprite = skillDetails.cardImage;
        skillPriceText.text = skillPrice + "G";

        // Eski listener'larý kaldýr
        BuyButton.onClick.RemoveAllListeners();

        // Yeni listener ekle
        BuyButton.onClick.AddListener(TryBuySkill);

        skillDetailsPanel.SetActive(true);
    }

    public void CloseSkillPanel()
    {
        skillDetailsPanel.SetActive(false);
    }

}
