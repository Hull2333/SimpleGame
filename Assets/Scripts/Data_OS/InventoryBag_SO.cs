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
    /// <summary>
    /// 获取该物品在玩家背包中的总数量
    /// </summary>
    /// <param name="ID"></param>
    /// <returns></returns>
    public int GetItemAllAmount(int ID)
    {
        int allAmount = 0;
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].itemID == ID)
            {
                allAmount += itemList[i].itemAmount;
            }
        }
        return allAmount;
    }
}
