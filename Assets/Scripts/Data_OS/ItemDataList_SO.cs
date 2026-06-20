using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName ="ItemDataList_OS",menuName="Inventory/ItemDataList")]
public class ItemDataList_SO : ScriptableObject
{

    public List<ItemDetails> itemDetailsList;
    [ContextMenu("编辑模式运行，itemID自动从小到大排序")]
    public void SortItemDetailsList()
    {
        itemDetailsList.Sort((a, b) => a.itemID.CompareTo(b.itemID));
    }

}
