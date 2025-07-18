using UnityEngine;

[CreateAssetMenu(fileName = "ShipDetails", menuName = "Stats/ShipStats")]
[System.Serializable]
public class ShipDetails : ScriptableObject
{
    public enum CurrencyType { Gold, Coin }
    public CurrencyType currencyType;


    public string shipName;
    public int shipLevel;
    public int shipmaxLevel;
    public float price;
    public int upgradeCost;
    public float health;
    public float attackPower;
    public float attackSpeed;
    public float attackRange;
    public float shield;
    public float moveSpeed;
    public GameObject prefab;

    public GameObject customButtonPrefab; // 👈 Her geminin özel butonu
    public GameObject upgradeButtonPrefab;

    public int GetUpgradeCost()
    {
        return upgradeCost * (shipLevel + 2);       
    }



}
