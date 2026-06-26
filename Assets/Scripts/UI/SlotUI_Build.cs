using MFarm.Inventory;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotUI_Build : MonoBehaviour //调用在Slot_Building上
{
    public int buildID;
    private BuildingDetails buildingDetails;
    public BluPrintData_SO bluPrintData;
    public Image spriteImage;
    public TextMeshProUGUI buildName;
    public Transform resourseParent;
    public SlotUI resourseSlot;
    private Button button;
    //可以建造
    private bool canBuilding;


    public void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(SwitchBuildMode);
    }
    /// <summary>
    /// 更新建筑选项UI
    /// </summary>
    public void UpdateBuildSlotMessage()
    {
        for (int i = 0; i < resourseParent.childCount; i++)
        {
            Destroy(resourseParent.GetChild(i).gameObject);
        }
        buildingDetails = bluPrintData.GetBuildingDetails(buildID);
        spriteImage.sprite = buildingDetails.buildSprite;
        buildName.text = buildingDetails.buildName;
        canBuilding = true;
        foreach (var resourse in buildingDetails.resourceItem)
        {
            ItemDetails resourseItem = InventoryManager.Instance.GetItemDetails(resourse.itemID);
            var resourseItemSlot = Instantiate(resourseSlot, resourseParent);
            int resourseAllAmount = InventoryManager.Instance.playerBag.GetItemAllAmount(resourse.itemID);
            resourseItemSlot.UpdateSlot(resourseItem, resourse.itemAmount, true, resourseAllAmount);
            //数量字体变灰
            if (resourse.itemAmount > resourseAllAmount)
            {
                resourseItemSlot.SetRedAmountText();
                canBuilding = false;
            }
            else
            {
                resourseItemSlot.RecoverOriginTextColor();
            }
        }
    }
    /// <summary>
    /// 切换到建造模式
    /// </summary>
    public void SwitchBuildMode()
    {
        if (canBuilding)
        {
            EventHandler.CallBuildindModeEvent(buildingDetails, null, true);
        }
        else
        {
            InventoryUI.Instance.ShowResouceLackText();
        }
    }
}
