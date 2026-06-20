using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MFarm.Inventory;
using TMPro;
public class ItemTooltip : MonoBehaviour    //调用在ItemToolTip对象上
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Text valueText;
    [SerializeField] private GameObject bottomPart;
    //[Header("建造")]
    //public GameObject resourcePanel;
    //[SerializeField] private Image[] resourceItem;
    /// <summary>
    /// 设置物品描述的显示内容
    /// </summary>
    /// <param name="itemDetails"></param>
    /// <param name="slotType">物品类型</param>
    public void SetupTooltip(ItemDetails itemDetails,SlotType slotType)
    {
        nameText.text = itemDetails.itemName;
        typeText.text = GetItemType(itemDetails.itemType);
        descriptionText.text = itemDetails.itemDescription;

        if(itemDetails.itemType == ItemType.Seed || itemDetails.itemType == ItemType.Commodity|| itemDetails.itemType == ItemType.Furniture)
        {
            bottomPart.SetActive(true);

            var price = itemDetails.itemPrice;
            if (slotType == SlotType.Bag)
            {
                price = (int)(itemDetails.itemPrice * itemDetails.sellPercentage);
            }
            valueText.text = price.ToString();
        }
        else
        {
            bottomPart.SetActive(false);
        }
        //如果各物品之间的描述框高度不一，会立即改变刷新高度，不会有延迟
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }
    /// <summary>
    /// 给物品类型赋值中文名
    /// </summary>
    /// <param name="itemType"></param>
    /// <returns></returns>
    private string GetItemType(ItemType itemType)
    {  
        //循环ItemType中的枚举，并返回中文名，_表示default，其他
        return itemType switch
        {
            ItemType.Seed => "种子",
            ItemType.Commodity => "商品",
            ItemType.Furniture => "家具",
            ItemType.BreakTool => "工具",
            ItemType.AxeTool => "工具",
            ItemType.HoeTool => "工具",
            ItemType.ReapTool => "工具",
            ItemType.WaterTool => "工具",
            _ => "无"
        };
    }
    /// <summary>
    /// 设置启动图纸资源详情
    /// </summary>
    /// <param name="bluPrintDetails"></param>
    //public void SetupResourcePanel(int ID)
    //{
    //    var bluPrintDetails = InventoryManager.Instance.bluPrintData.GetBluPrintDetails(ID);
    //    for (int i = 0; i < resourceItem.Length; i++)
    //    {
    //        if(i < bluPrintDetails.resourceItem.Length)
    //        {
    //            var item = bluPrintDetails.resourceItem[i];
    //            //需要多少种资源显示多少中资源
    //            resourceItem[i].gameObject.SetActive(true);
    //            resourceItem[i].sprite = InventoryManager.Instance.GetItemDetails(item.itemID).itemIcon;
    //            resourceItem[i].transform.GetChild(0).GetComponent<Text>().text = item.itemAmount.ToString(); 
    //        }
    //        else
    //        {
    //            resourceItem[i].gameObject.SetActive(false); 
    //        }
    //    }
    //}
}
