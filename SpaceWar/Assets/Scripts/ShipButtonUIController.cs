using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections.Generic;
using System;

public class ShipButtonUIController : MonoBehaviour
{
    public LocalizedString buyText;
    public LocalizedString selectText;
    public LocalizedString selectedText;

    public static ShipButtonUIController Instance;
    public Text actionButtonText;
    public Button actionButton;

    private ShipDetails currentShip;

    public GameObject[] shipPrefabs; // 0 -> Gemi1, 1 -> Gemi2, ...
    private GameObject currentSpawnedShip;
    public Transform shipParent;

    public Button upgradeButton;
    public Text upgradeButtonText;

    private int saveShipLevel;
    private float saveattackPower;
    private float saveattackSpeed;
    private float savemoveSpeed;
    private float saveHealth;
    private float saveShield;
    private float saveattackRange;


    private void Start()
    {
        Instance = this;
        if (GameManager.Instance.selectedShip != null)
        {
            currentSpawnedShip = Instantiate(GameManager.Instance.selectedShip.prefab, shipParent);
            currentSpawnedShip.transform.localPosition = Vector3.zero;
            Camera.main.GetComponent<CameraFollow>().SetTarget(currentSpawnedShip.transform);
        }
    }


    public void UpdateButton(ShipDetails newShip)
    {
        currentShip = newShip;


        if (GameManager.Instance.ownedShips.Contains(currentShip))
        {
            if (GameManager.Instance.selectedShip == currentShip)
            {
                actionButtonText.text = selectedText.GetLocalizedString(); // "Seçildi"
                actionButton.interactable = false;
                Debug.Log("Seçildi");
            }
            else
            {
                actionButtonText.text = selectText.GetLocalizedString();   // "Seç"
                actionButton.interactable = true;
                actionButton.onClick.RemoveAllListeners();
                actionButton.onClick.AddListener(() => SelectShip(currentShip));
            }

            // Upgrade butonu aktif
            upgradeButton.gameObject.SetActive(true);
            upgradeButton.onClick.RemoveAllListeners();

            if (currentShip.shipLevel < currentShip.shipmaxLevel)
            {
                upgradeButtonText.text = $"({currentShip.GetUpgradeCost()} G)";
                upgradeButton.onClick.AddListener(() => UpgradeShip(currentShip));
                upgradeButton.interactable = GameManager.Instance.totalGold >= currentShip.GetUpgradeCost();
            }
            else
            {
                upgradeButtonText.text = "MAX";
                upgradeButton.interactable = false;
            }
        }
        else
        {
            string currencySymbol = currentShip.currencyType == ShipDetails.CurrencyType.Gold ? "G" : "C";
            actionButtonText.text = $"{buyText.GetLocalizedString()} ({currentShip.price}{currencySymbol})";
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => BuyShip(currentShip));
            upgradeButton.gameObject.SetActive(false);

            if (currentShip.currencyType == ShipDetails.CurrencyType.Gold)
            {
                actionButton.interactable = GameManager.Instance.totalGold >= currentShip.price;
            }
            else if (currentShip.currencyType == ShipDetails.CurrencyType.Coin)
            {
                actionButton.interactable = true; // Coinle alýnan gemiler daima aktif
            }
        }
    }
    private void UpgradeShip(ShipDetails ship)
    {
        int cost = ship.GetUpgradeCost();

        if (GameManager.Instance.totalGold >= cost)
        {
            GameManager.Instance.totalGold -= cost;
            GameManager.Instance.TotalGoldText.text = GameManager.Instance.totalGold.ToString();
            PlayerPrefs.SetFloat("TotalGold", GameManager.Instance.totalGold);

            upgradeButtonText.text =cost.ToString();
            if (ship.shipLevel < ship.shipmaxLevel) ship.shipLevel++;
            if (ship.attackPower < 100) ship.attackPower += 2f;
            if (ship.attackSpeed < 50) ship.attackSpeed += 2f; // Örnek artýþ
            if (ship.moveSpeed < 20) ship.moveSpeed += 0.5f;
            if (ship.health < 200) ship.health += 5f;
            if (ship.shield < 200) ship.shield += 5f; // Ornek
            if (ship.attackRange < 60) ship.attackRange += 1f;

            int shipIndex = GameManager.Instance.allShips.IndexOf(ship);
            if (shipIndex != -1)
            {
                PlayerPrefs.SetInt("Ship" + shipIndex + "_Level", ship.shipLevel);
                PlayerPrefs.SetFloat("Ship" + shipIndex + "_Attack", ship.attackPower);
                PlayerPrefs.SetFloat("Ship" + shipIndex + "_Speed", ship.attackSpeed);
                PlayerPrefs.SetFloat("Ship" + shipIndex + "_Move", ship.moveSpeed);
                PlayerPrefs.SetFloat("Ship" + shipIndex + "_Health", ship.health);
                PlayerPrefs.SetFloat("Ship" + shipIndex + "_Shield", ship.shield);
                PlayerPrefs.SetFloat("Ship" + shipIndex + "_Range", ship.attackRange);
            }

            HangarController.Instance.UpdateShipDisplay();
            Debug.Log($"{ship.shipName} upgrade edildi! Level: {ship.shipLevel}");

            // Sahnedeki gemiyi güncelle
            if (currentSpawnedShip != null)
            {
                PlayerSmoothFollow shipController = currentSpawnedShip.GetComponent<PlayerSmoothFollow>();
                if (shipController != null)
                {
                    shipController.ApplyStats(ship); // Bu fonksiyonu birazdan yazýyoruz
                }
            }

            // Buton güncelle
            UpdateButton(ship);
        }
        else
        {
            Debug.Log("Upgrade için yeterli altýn yok!");
        }
    }
    private void SelectShip(ShipDetails ship)
    {
        GameManager.Instance.selectedShip = ship;
        Debug.Log($"Seçilen gemi: {ship.shipName}");
        // ship'in index'ini bul
        int index = GameManager.Instance.allShips.IndexOf(ship);
        PlayerPrefs.SetInt("SelectedShipIndex", index);
        PlayerPrefs.Save();

        // Önce varsa eski gemiyi yok et
        if (currentSpawnedShip != null)
        {
            Destroy(currentSpawnedShip);
        }

        // Gemi prefab'ýný instantiate et
        if (ship.prefab != null)
        {
            currentSpawnedShip = Instantiate(ship.prefab, shipParent);
            currentSpawnedShip.transform.localPosition = Vector3.zero; // Ortala
            Camera.main.GetComponent<CameraFollow>().SetTarget(currentSpawnedShip.transform);

        }
        else
        {
            Debug.LogWarning("Ship prefab atanmamýþ!");
        }
        UpdateButton(ship);
    } 
    private void BuyShip(ShipDetails ship)
    {
        if (ship.currencyType == ShipDetails.CurrencyType.Gold)
        {
            if (GameManager.Instance.totalGold >= ship.price)
            {
                GameManager.Instance.totalGold -= ship.price;
                GameManager.Instance.TotalGoldText.text = GameManager.Instance.totalGold.ToString();
                PlayerPrefs.SetFloat("TotalGold", GameManager.Instance.totalGold);
                CompleteShipPurchase(ship);
            }
            else
            {
                Debug.Log("Yetersiz altýn!");
            }
        }
        else if (ship.currencyType == ShipDetails.CurrencyType.Coin)
        {
            if (IAP.Instance.coin >= ship.price)
            {
                IAP.Instance.RemoveCoin(ship.price);
                CompleteShipPurchase(ship);
            }
            else
            {
                IAP.Instance.panelOpen();
                Debug.Log("Yetersiz coin!");
            }
        }
    }
    private void CompleteShipPurchase(ShipDetails ship)
    {
        int index = GameManager.Instance.allShips.IndexOf(ship);
        if (index >= 0)
        {
            PlayerPrefs.SetInt("Ship" + index, 1); // Bu gemi satýn alýndý
            PlayerPrefs.Save();
        }

        GameManager.Instance.ownedShips.Add(ship);
        Debug.Log($"Satýn alýndý: {ship.shipName}");
        UpdateButton(ship); // Butonu "Seç" olarak güncelle
    }
    public void ResetBuyingShip()
    {
        PlayerPrefs.DeleteKey("Ship0");
        PlayerPrefs.DeleteKey("Ship1");
        //PlayerPrefs.DeleteKey("Ship2");
        PlayerPrefs.DeleteKey("Ship3");
        PlayerPrefs.DeleteKey("Ship4");
        //PlayerPrefs.DeleteKey("Ship5");
        PlayerPrefs.SetInt("SelectedShipIndex", 0);
        PlayerPrefs.Save();
    }

    #region Dil deðiþiminde buton güncellemesi
    void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }
    private void OnLocaleChanged(Locale locale)
    {
        if (currentShip != null)
            UpdateButton(currentShip);
    }
    void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }
    void OnLocaleChanged(UnityEngine.Localization.Locale newLocale, UnityEngine.Localization.Locale previousLocale)
    {
        if (currentShip != null)
            UpdateButton(currentShip);
    }
    #endregion

}
