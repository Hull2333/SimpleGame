using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "FishDataList_SO", menuName = "Inventory/FishDataList_SO")]
public class FishData_SO : ScriptableObject
{
    public List<FishDetails> fishDataList;
    public FishDetails GetFishDetails(int itemID)
    {
        return fishDataList.Find(c => c.ID == itemID);
    }
}

[System.Serializable]
public class FishDetails
{
    public int ID;
    public GameObject fishPrefab;
}
