using System.Collections.Generic;
using UnityEngine.Localization.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private ShipDetails currentShip;

    public List<ShipDetails> allShips; // Inspector'dan atarsın, sırayla 0-5 gemiler
    public List<ShipDetails> ownedShips = new List<ShipDetails>();
    public ShipDetails selectedShip;
    //public List<GameObject> shipObjects;


    public AudioMixer mixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    public Button Turkish;
    public Button English;
    public Button Korean;
    public Button German;
    public Button Russian;
    public Button Spanish;
    public Button Chinese;

    private int score = 0;
    public Text scoreText;
    public Text GameoverScoreText;
    public Text GameoverKilledText;
    public Text GameovercoinText;
    public Text CongratsScoreText;
    public Text CongratsKilledText;
    public Text CongratscoinText;
    public Text newmapText;
    public Text TotalGoldText;
    public static int killed = 0;

    [Header("UI")]
    public Toggle vibrationToggle;
    public int vibrateValue = 100;
    public bool isLockOnEnabled = false; // Toggle ile kontrol edilecek
    public Toggle lockOnToggle;
    public Slider expSlider;
    public Text levelText;
    public GameObject LevelUpPanel;
    public GameObject Joystick;
    public GameObject GUIPanel;
    public GameObject WaveStartPanel;
    public GameObject MainPanel;
    public GameObject HangarPanel;
    public GameObject SkillsPanel;
    public GameObject PausePanel;
    public GameObject SettingPanel;
    public GameObject InfoPanel;
    public GameObject LanguagePanel;
    public GameObject GameOverPanel;
    public GameObject CongratsPanel;
    public GameObject BGPanel;
    public GameObject ResetPanel;
    public GameObject GoldPanel;

    public static int currentLevel = 1;
    public float currentExp = 0;
    public float currentGold = 0;
    public float totalGold = 0;
    public float expToNextLevel = 100; // İlk seviye için gerekli exp

    [Header("ExpDrop")]
    public GameObject expPrefab;
    public float spawnRadius = 5f; // Player etrafında kaç birimlik alanda spawnlansın
    public float spawnInterval = 2f;
    public int maxExpCount = 5;
    private Transform player;


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        musicSlider.onValueChanged.AddListener((value) => mixer.SetFloat("BGMusicVolume", Mathf.Log10(value) * 20));
        sfxSlider.onValueChanged.AddListener((value) => mixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20));
    }
    void Start()
    {
        LoadOwnedShips();
        LoadVolumeSettings();
        LoadLanguage();
        
        score = 0;
        killed = 0;
        totalGold = PlayerPrefs.GetFloat("TotalGold", 0f);
        TotalGoldText.text = totalGold.ToString();

        vibrateValue = PlayerPrefs.GetInt("VibrationValue", 100);
        vibrationToggle.isOn = PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;
        vibrationToggle.onValueChanged.AddListener(OnVibrationToggleChanged);        // Değişiklik olunca fonksiyon çağrılır

        lockOnToggle.onValueChanged.AddListener(OnLockToggleChanged);
        isLockOnEnabled = PlayerPrefs.GetInt("LockOnEnabled", 0) == 1;
        lockOnToggle.isOn = isLockOnEnabled;

        Turkish.onClick.AddListener(() => SetLanguage("tr")); 
        English.onClick.AddListener(() => SetLanguage("en"));
        Korean.onClick.AddListener(() => SetLanguage("ko-KR"));
        German.onClick.AddListener(() => SetLanguage("de"));
        Russian.onClick.AddListener(() => SetLanguage("ru"));
        Spanish.onClick.AddListener(() => SetLanguage("es"));
        Chinese.onClick.AddListener(() => SetLanguage("zh"));
        PlayerSmoothFollow.fight = false;

        TotalGoldText.text = totalGold.ToString();
        UpdateScoreUI();

    }
    void Update()
    {
        musicSlider.onValueChanged.AddListener((value) => mixer.SetFloat("BGMusicVolume", Mathf.Log10(value) * 20));
        sfxSlider.onValueChanged.AddListener((value) => mixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20));
        if(PausePanel.activeSelf|| SettingPanel.activeSelf||LevelUpPanel.activeSelf||CongratsPanel.activeSelf) Time.timeScale = 0;
        else Time.timeScale = 1;
        SaveVolumeSettings();

    }

    public void StartGame()
    {
        PlayerSmoothFollow.fight = true;
        currentLevel = 1;
        currentGold = 0;
        MainPanel.SetActive(false);
        HangarPanel.SetActive(false);
        WaveStartPanel.SetActive(true);
        GUIPanel.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
        SkillsPanel.GetComponent<RectTransform>().localScale = new Vector3(0f, 0f, 0f);
        GoldPanel.SetActive(false);
       
    }

    //Exp Sistemi
    public void AddExp(int amount)
    {
        currentExp += amount;
        if (currentExp >= expToNextLevel)  LevelUp();
        UpdateUI();
    }
    private void LevelUp()
    {         
        Time.timeScale = 0;
        currentExp -= expToNextLevel;
        currentLevel++;
        expToNextLevel = Mathf.RoundToInt(expToNextLevel * 1.2f); // her seviye %20 zorlaşır
        LevelUpPanel.SetActive(true);
        Joystick.SetActive(false); 
    }
    public void LevelBoostChoosing()
    {
        Time.timeScale = 1;
        LevelUpPanel.SetActive(false);
        Joystick.SetActive(true);
    }
    void UpdateUI()
    {
        if (expSlider != null)
        {
            expSlider.maxValue = expToNextLevel;
            expSlider.value = currentExp;
        }

        if (levelText != null)
        {
            levelText.text = $"Level: {currentLevel}";
        }
    }
    
    //Gold Sistemi
    public void AddGold(int amount)
    {
        currentGold += amount;
    }
    public void ResetTotalGold()
    {
        totalGold = 0;
        PlayerPrefs.SetFloat("TotalGold", totalGold);
        PlayerPrefs.Save();
        TotalGoldText.text = totalGold.ToString();
    }

    //Puan Sistemi
    public void AddScore(int value)
    {
        score += value;
        UpdateScoreUI();
    }
    private void UpdateScoreUI()
    {
        scoreText.text = score.ToString();
    }
    public void KilledScore()
    {
        killed++;
    }
    

    //Panel Sistemi
    public void GameOver()
    {
        LevelUpPanel.SetActive(false);
        Time.timeScale = 0;
        GameOverPanel.SetActive(true);
        GameoverKilledText.text=killed.ToString();
        GameoverScoreText.text =score.ToString();
        GameovercoinText.text = currentGold.ToString();
        totalGold += currentGold;
        TotalGoldText.text = totalGold.ToString();
        GUIPanel.SetActive(false);
        PlayerPrefs.SetFloat("TotalGold", totalGold);
        PlayerPrefs.Save();
    }
    public void Congrats()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }
        GameObject[] gold = GameObject.FindGameObjectsWithTag("Gold");
        foreach (GameObject golds in gold)
        {
            Destroy(golds);
        }
        GameObject[] exp = GameObject.FindGameObjectsWithTag("Exp");
        foreach (GameObject exps in exp)
        {
            Destroy(exps);
        }
        GameObject[] expp = GameObject.FindGameObjectsWithTag("Drop");
        foreach (GameObject expps in expp)
        {
            Destroy(expps);
        }
        MapManager.Instance.UnlockNextMap();
        Time.timeScale = 0;
        CongratsPanel.SetActive(true);
        CongratsKilledText.text = killed.ToString();
        CongratsScoreText.text = score.ToString();
        CongratscoinText.text = currentGold.ToString();
        newmapText.text = "New Map Unlocked";
        totalGold += currentGold;
        TotalGoldText.text = totalGold.ToString();
        GUIPanel.SetActive(false);
        PlayerPrefs.SetFloat("TotalGold", totalGold);
        PlayerPrefs.Save();
    }
    public void Continue()
    {      
        MapManager.Instance.LoadNextMapAfterBoss();  // Harita geçişini tetikle
        Time.timeScale = 1;
        CongratsPanel.SetActive(false);
        GUIPanel.SetActive(true);
        currentLevel = 1;
        currentExp = 0;
        expToNextLevel = 100;
        UpdateUI();
        MapManager.Instance.UpdateUI();
    }
    public void RestartGame()
    {
        Time.timeScale = 1;

        // Videonun tekrar oynamasını istemiyorsan bunu buraya EKLE
        PlayerPrefs.SetInt("VideoPlayed", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene(1);
    }

    public void ResetProgres()
    {
        MapManager.Instance.ResetMapProgress();
        ShipButtonUIController.Instance.ResetBuyingShip();
        ResetTotalGold();
        for (int i = 0; i < allShips.Count; i++)
        {
            PlayerPrefs.DeleteKey("Ship" + i + "_Level");
            PlayerPrefs.DeleteKey("Ship" + i + "_Attack");
            PlayerPrefs.DeleteKey("Ship" + i + "_Speed");
            PlayerPrefs.DeleteKey("Ship" + i + "_Move");
            PlayerPrefs.DeleteKey("Ship" + i + "_Health");
            PlayerPrefs.DeleteKey("Ship" + i + "_Shield");
            PlayerPrefs.DeleteKey("Ship" + i + "_Range");
        }
        PlayerPrefs.Save();
        PlayerPrefs.DeleteAll();
        Debug.Log("Tüm upgrade kayıtları sıfırlandı.");
        RestartGame();
    }
    
    public void PausePanelOC()  => PausePanel.SetActive(!PausePanel.activeSelf); 
    public void SettingPanelOC()
    {
        SettingPanel.SetActive(!SettingPanel.activeSelf); 
        BGPanel.SetActive(SettingPanel.activeSelf);
        LanguagePanel.SetActive(false);
    }

    public void InfoPanelOC() => InfoPanel.SetActive(!InfoPanel.activeSelf);
    public void LanguagePanelOC() => LanguagePanel.SetActive(!LanguagePanel.activeSelf);
    public void GUIPanelOC() => GUIPanel.SetActive(!GUIPanel.activeSelf);
    public void MainPanelOC() => MainPanel.SetActive(!MainPanel.activeSelf);
    public void HangarPanelOC() => HangarPanel.SetActive(!HangarPanel.activeSelf);
    public void ResetPanelOC() => ResetPanel.SetActive(!ResetPanel.activeSelf);
    public void SkillsPanelOC() => SkillsPanel.SetActive(!SkillsPanel.activeSelf);

    public void SetLanguage(string localeCode)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        PlayerPrefs.SetString("Language", localeCode);
        PlayerPrefs.Save();       
    }
    void LoadLanguage()
    {
        string savedLocale = PlayerPrefs.GetString("Language", "en");
        SetLanguage(savedLocale);
    }
    void LoadVolumeSettings()
    {
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        musicSlider.value = musicVolume;
        sfxSlider.value = sfxVolume;

        mixer.SetFloat("BGMusicVolume", Mathf.Log10(musicVolume) * 20);
        mixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolume) * 20);
    }
    void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
        PlayerPrefs.Save();
    }
    void OnVibrationToggleChanged(bool isOn)
    {
        vibrateValue = isOn ? 100 : 0;
        Debug.Log("vibration value: "+ vibrateValue);
        // Ayarı kaydet
        PlayerPrefs.SetInt("VibrationEnabled", isOn ? 1 : 0);
        PlayerPrefs.SetInt("VibrationValue", vibrateValue);
        PlayerPrefs.Save();

        
    }
    public void OnLockToggleChanged(bool isOn)
    {
        isLockOnEnabled = isOn;
        PlayerPrefs.SetInt("LockOnEnabled", isOn ? 1 : 0);

    }
    public void LoadOwnedShips()
    {
        ownedShips.Clear();
        // 0. gemi her zaman eklensin
        if (!GameManager.Instance.ownedShips.Contains(GameManager.Instance.allShips[0]))
        {
            GameManager.Instance.ownedShips.Add(GameManager.Instance.allShips[0]);
        }

        for (int i = 0; i < allShips.Count; i++)
        {
            if (PlayerPrefs.GetInt("Ship" + i, 0) == 1)
            {
                ownedShips.Add(allShips[i]);
            }
        }
        if (PlayerPrefs.HasKey("SelectedShipIndex"))
        {
            int selectedIndex = PlayerPrefs.GetInt("SelectedShipIndex");

            if (selectedIndex >= 0 && selectedIndex < GameManager.Instance.allShips.Count)
            {
                GameManager.Instance.selectedShip = GameManager.Instance.allShips[selectedIndex];
            }
        }

        for (int i = 0; i < ownedShips.Count; i++)
        {
            ShipDetails ship = ownedShips[i];
            int index = GameManager.Instance.allShips.IndexOf(ship);

            ship.shipLevel = PlayerPrefs.GetInt("Ship" + index + "_Level", ship.shipLevel);
            ship.attackPower = PlayerPrefs.GetFloat("Ship" + index + "_Attack", ship.attackPower);
            ship.attackSpeed = PlayerPrefs.GetFloat("Ship" + index + "_Speed", ship.attackSpeed);
            ship.moveSpeed = PlayerPrefs.GetFloat("Ship" + index + "_Move", ship.moveSpeed);
            ship.health = PlayerPrefs.GetFloat("Ship" + index + "_Health", ship.health);
            ship.shield = PlayerPrefs.GetFloat("Ship" + index + "_Shield", ship.shield);
            ship.attackRange = PlayerPrefs.GetFloat("Ship" + index + "_Range", ship.attackRange);
        }

    }


}
