using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "InventoryBag_SO", menuName = "Inventory/InventoryBag_SO")]
public class InventoryBag_SO : ScriptableObject
{

    public List<InventoryItem> itemList;
    /// <summary>
    /// 通过ID查找背包是否有该ID物品
    /// </summary>
    /// <param name="ID">需查找的物品ID</param>
    /// <returns></returns>
    public InventoryItem GetInventoryItem(int ID)
    {
        return itemList.Find(i => i.itemID == ID);
    }
}
