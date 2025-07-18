using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using Random = UnityEngine.Random;

public class ChestTimer : MonoBehaviour
{
    public static ChestTimer Instance;
    private SkillDragHandler currentSkillDragHandler;
    public SkillParentController skillParentController;

    [Header("Ayarlar")]
    public int chestID;
    public float hoursToUnlock = 4;

    [Header("UI")]
    public Sprite[] skillSprites;
    public string[] skillNames = { "FlashLeap", "Regenerative Core", "Deflector Shield", "Stealth Cloak ", "AutoTurretDeploy", "EMPWave", "MeteorStrike", "MirrorIllusion", "Timelock", "OverdriveMode", "BlackHoleGranade", "NeutronBomb" };
    public Text timerText;
    public Button openChestButton;
    public GameObject ChestResultPanel;
    public GameObject ChestImage;
    public Image skillImageUI;
    public Text Reward;

    private string chestKey;
    private DateTime unlockTime;
    private bool chestReady = false;

    private int coinOpenCount = 0;
    private string coinOpenKey;
    private int freeOpenCount = 0;
    private string freeOpenKey;
    private int price = 0;

    void Awake() => Instance = this;
    void Start()
    {
        chestKey = "ChestUnlockTime" + chestID;
        coinOpenKey = "Chest_CoinOpenCount_" + chestID;
        freeOpenKey = "Chest_FreeOpenCount_" + chestID;

        coinOpenCount = PlayerPrefs.GetInt(coinOpenKey, 0);
        freeOpenCount = PlayerPrefs.GetInt(freeOpenKey, 0);

        if (PlayerPrefs.HasKey(chestKey))
            unlockTime = DateTime.Parse(PlayerPrefs.GetString(chestKey));
        else
        {
            unlockTime = DateTime.Now.AddHours(hoursToUnlock);
            PlayerPrefs.SetString(chestKey, unlockTime.ToString());
            PlayerPrefs.Save();
        }
    }

    void Update()
    {
        if (chestID == 3 && coinOpenCount < 3)
        {
            price = (coinOpenCount + 1) * 10;
            timerText.text = $"{price} COIN İLE AÇ";
            openChestButton.interactable = true;
            chestReady = true;
            return;
        }

        if (chestID == 1 && freeOpenCount < 2)
        {
            timerText.text = "AÇ";
            openChestButton.interactable = true;
            chestReady = true;
            return;
        }

        TimeSpan remaining = unlockTime - DateTime.Now;

        if (remaining.TotalSeconds <= 0)
        {
            chestReady = true;
            timerText.text = "Kasa Açılabilir!";
            openChestButton.interactable = true;

            if (chestID == 3)
            {
                coinOpenCount = 0;
                PlayerPrefs.SetInt(coinOpenKey, coinOpenCount);
            }

            if (chestID == 1)
            {
                freeOpenCount = 0;
                PlayerPrefs.SetInt(freeOpenKey, freeOpenCount);
            }
            PlayerPrefs.Save();
        }
        else
        {
            chestReady = false;
            timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", remaining.Hours, remaining.Minutes, remaining.Seconds);
            openChestButton.interactable = false;
        }
    }
    public void OpenChest1()
    {
        if (!chestReady) return;

        Debug.Log($"KASA {chestID} AÇILDI!");
        int randomValue = Random.Range(0, 100);
        string rewardMessage = "";

        if (chestID == 1 && freeOpenCount < 2)
        {
            GiveReward(randomValue, ref rewardMessage);
            ShowRewardPanel(rewardMessage);

            freeOpenCount++;
            PlayerPrefs.SetInt(freeOpenKey, freeOpenCount);
            PlayerPrefs.Save();

            if (freeOpenCount >= 2)
            {
                unlockTime = DateTime.Now.AddHours(hoursToUnlock);
                PlayerPrefs.SetString(chestKey, unlockTime.ToString());
                PlayerPrefs.Save();
                chestReady = false;
                openChestButton.interactable = false;
            }
        }
    }
    public void OnOpenChest()
    {
        if (!chestReady) return;

        Debug.Log($"KASA {chestID} AÇILDI!");
        int randomValue = Random.Range(0, 100);
        string rewardMessage = "";

        
        if (chestID == 2)
        {
            GiveReward(randomValue, ref rewardMessage);
            ShowRewardPanel(rewardMessage);

            unlockTime = DateTime.Now.AddHours(hoursToUnlock);
            PlayerPrefs.SetString(chestKey, unlockTime.ToString());
            PlayerPrefs.Save();

            chestReady = false;
            openChestButton.interactable = false;
        }
        else if (chestID == 3 && coinOpenCount < 3)
        {
            if (IAP.Instance.coin >= price)
            {
                IAP.Instance.RemoveCoin(price);
                GiveReward(randomValue, ref rewardMessage);
                ShowRewardPanel(rewardMessage);

                coinOpenCount++;
                PlayerPrefs.SetInt(coinOpenKey, coinOpenCount);
                PlayerPrefs.Save();

                if (coinOpenCount >= 3)
                {
                    unlockTime = DateTime.Now.AddHours(hoursToUnlock);
                    PlayerPrefs.SetString(chestKey, unlockTime.ToString());
                    PlayerPrefs.Save();

                    chestReady = false;
                    openChestButton.interactable = false;
                }
            }
            else
            {
                IAP.Instance.panelOpen();
            }
        }
    }

    private void GiveReward(int randomValue, ref string rewardMessage)
    {
        if (chestID == 1)
        {
            if (randomValue < 68)
                AddGold(Random.Range(1000, 3001), ref rewardMessage);
            else if (randomValue < 83)
                GiveSkill(Random.Range(1, 4), ref rewardMessage);
            else if (randomValue < 93)
                GiveSkill(Random.Range(4, 7), ref rewardMessage);
            else if (randomValue < 98)
                GiveSkill(Random.Range(7, 10), ref rewardMessage);
            else
                GiveSkill(Random.Range(10, 13), ref rewardMessage);
        }
        else if (chestID == 2)
        {
            if (randomValue < 56)
                AddGold(Random.Range(3000, 5001), ref rewardMessage);
            else if (randomValue < 74)
                GiveSkill(Random.Range(1, 4), ref rewardMessage);
            else if (randomValue < 87)
                GiveSkill(Random.Range(4, 7), ref rewardMessage);
            else if (randomValue < 95)
                GiveSkill(Random.Range(7, 10), ref rewardMessage);
            else
                GiveSkill(Random.Range(10, 13), ref rewardMessage);
        }
        else if (chestID == 3)
        {
            if (randomValue < 44)
                AddGold(Random.Range(5000, 10001), ref rewardMessage);
            else if (randomValue < 65)
                GiveSkill(Random.Range(1, 4), ref rewardMessage);
            else if (randomValue < 81)
                GiveSkill(Random.Range(4, 7), ref rewardMessage);
            else if (randomValue < 92)
                GiveSkill(Random.Range(7, 10), ref rewardMessage);
            else
                GiveSkill(Random.Range(10, 13), ref rewardMessage);
        }
    }

    private void AddGold(int amount, ref string rewardMessage)
    {
        GameManager.Instance.totalGold += amount;
        GameManager.Instance.TotalGoldText.text = GameManager.Instance.totalGold.ToString();
        PlayerPrefs.SetFloat("TotalGold", GameManager.Instance.totalGold);
        rewardMessage = $"+ {amount} G";
    }

    private void GiveSkill(int skillIndex, ref string rewardMessage)
    {
        rewardMessage = skillNames[skillIndex - 1];
        TrySkills(skillIndex, rewardMessage);
    }

    private void ShowRewardPanel(string rewardMessage)
    {
        Reward.text = rewardMessage;
        ChestResultPanel.SetActive(true);
        ChestImage.SetActive(true);
        Invoke(nameof(HideResultPanel), 2f);
    }

    void HideResultPanel()
    {
        ChestResultPanel.SetActive(false);
        ChestImage.SetActive(false);
        skillImageUI.gameObject.SetActive(false);
    }

    public void TrySkills(int skillIndex, string rewardMessage)
    {
        if (PlayerPrefs.GetInt("SkillUnlocked_" + skillIndex, 0) == 1)
        {
            AddGold(Random.Range(2000, 8001), ref rewardMessage);
            skillImageUI.gameObject.SetActive(false);
        }
        else
        {
            PlayerPrefs.SetInt("SkillUnlocked_" + skillIndex, 1);
            PlayerPrefs.Save();
            rewardMessage = skillNames[skillIndex - 1];
            skillParentController.UnlockSkillByID(skillIndex);

            if (skillIndex - 1 < skillSprites.Length && skillIndex > 0)
            {
                skillImageUI.sprite = skillSprites[skillIndex - 1];
                skillImageUI.gameObject.SetActive(true);
            }
        }
    }
}
