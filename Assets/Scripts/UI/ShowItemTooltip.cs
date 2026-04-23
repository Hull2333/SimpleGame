using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.EventSystems;
namespace MFarm.Inventory
{
    [RequireComponent(typeof(SlotUI))]
    //IPointerEnterHandler,IPointerExitHandler 鼠标滑进和滑出的接口方法
    public class ShowItemTooltip : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler    //调用在预制体Slot_Bag上
    {
        private SlotUI slotUI;
        private InventoryUI inventoryUI => GetComponentInParent<InventoryUI>();
        /// <summary>
        /// 鼠标滑进玩家背包格时
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if(slotUI.itemDetails != null)
            {
                inventoryUI.itemTooltip.gameObject.SetActive(true);
                inventoryUI.itemTooltip.SetupTooltip(slotUI.itemDetails, slotUI.slotType);
                //设置物品描述的锚点以及显示在物品的上方
                inventoryUI.itemTooltip.GetComponent<RectTransform>().pivot = new Vector2(0.5f,0);
                inventoryUI.itemTooltip.transform.position = transform.position + Vector3.up * 40;
                //判断如果道具类型为Furniture,显示额外材料需求UI
                if (slotUI.itemDetails.itemType == ItemType.Furniture)
                {
                    inventoryUI.itemTooltip.resourcePanel.SetActive(true) ;
                    inventoryUI.itemTooltip.SetupResourcePanel(slotUI.itemDetails.itemID);
                }
                //类型不是Furniture就关闭材料需求
                else
                {
                    inventoryUI.itemTooltip.resourcePanel.SetActive(false);
                }
            }
            else
            {
                inventoryUI.itemTooltip.gameObject.SetActive(false);
            }
        }
        /// <summary>
        /// 鼠标划出背包格时
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerExit(PointerEventData eventData)
        {
            inventoryUI.itemTooltip.gameObject.SetActive(false);
        }

        private void Awake()
        {
            slotUI = GetComponent<SlotUI>();
        }
    }

}
