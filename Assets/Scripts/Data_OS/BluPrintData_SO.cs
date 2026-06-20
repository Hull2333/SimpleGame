using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "BluPrintDataList_SO",menuName = "Inventory/BluPrintDataList_SO")]
public class BluPrintData_SO : ScriptableObject
{
    public List<BluPrintDetails> bluPrintDetails;
    public List<BuildingDetails> buildingDetails;
    
    public BluPrintDetails GetBluPrintDetails(int itemID)
    {
        return bluPrintDetails.Find(b => b.ID == itemID);
    }
    public BuildingDetails GetBuildingDetails(int buildID)
    {
        return buildingDetails.Find(b => b.ID == buildID);
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
[System.Serializable]
public class BuildingDetails 
{
    public string buildName;
    //建筑ID
    public int ID;
    public Sprite buildSprite;
    public InventoryItem[] resourceItem = new InventoryItem[4];
    //建造物品的Prefab
    public GameObject buildPrefab;
}

