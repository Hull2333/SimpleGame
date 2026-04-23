using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "CookedDataList_SO", menuName = "Inventory/CookedDataList_SO")]
public class CookedData_SO : ScriptableObject
{
    public List<CookedDetails> cookedDataList;
    /// <summary>
    /// 根据ID查找对应的菜肴信息
    /// </summary>
    /// <param name="itemID"></param>
    /// <returns></returns>
    public CookedDetails GetCookedDetails(int itemID)
    {
        return cookedDataList.Find(c => c.ID == itemID);
    }
}

[System.Serializable]
public class CookedDetails
{
    public int ID;
    //需要食材
    public InventoryItem[] cookResource = new InventoryItem[3];
    public GameObject cookPrefab;
}
