using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "BluPrintDataList_SO",menuName = "Inventory/BluPrintDataList_SO")]
public class BluPrintData_SO : ScriptableObject
{
    public List<BluPrintDetails> bluPrintDetails;   
    
    public BluPrintDetails GetBluPrintDetails(int itemID)
    {
        return bluPrintDetails.Find(b => b.ID == itemID);
    }

}
[System.Serializable]
public class BluPrintDetails
{
    //图纸ID
    public int ID;
    public InventoryItem[] resourceItem = new InventoryItem[4];
    //建造物品的Prefab
    public GameObject buildPrefab;
}
