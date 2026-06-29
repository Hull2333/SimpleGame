using UnityEngine;
using UnityEngine.EventSystems;
namespace MFarm.Inventory
{
    [RequireComponent(typeof(SlotUI))]
    //IPointerEnterHandler,IPointerExitHandler 鼠标滑进和滑出的接口方法
    public class ShowItemTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler   //调用在预制体Slot_Bag上
    {
        private SlotUI slotUI;
        private float mouseToScreenDistant;
        /// <summary>
        /// 鼠标滑进玩家背包格时
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (slotUI.itemDetails != null)
            {
                InventoryUI.Instance.itemTooltip.gameObject.SetActive(true);
                InventoryUI.Instance.itemTooltip.SetupTooltip(slotUI.itemDetails, slotUI.slotType);
                //判断如果道具类型为Furniture,显示额外材料需求UI
                //if (slotUI.itemDetails.itemType == ItemType.Furniture)
                //{
                //    InventoryUI.Instance.itemTooltip.resourcePanel.SetActive(true) ;
                //    InventoryUI.Instance.itemTooltip.SetupResourcePanel(slotUI.itemDetails.itemID);
                //}
                //类型不是Furniture就关闭材料需求
                //else
                //{
                //    InventoryUI.Instance.itemTooltip.resourcePanel.SetActive(false);
                //}
            }
            else
            {
                InventoryUI.Instance.itemTooltip.gameObject.SetActive(false);
            }
        }
        /// <summary>
        /// 鼠标划出背包格时
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerExit(PointerEventData eventData)
        {
            InventoryUI.Instance.itemTooltip.gameObject.SetActive(false);
        }
        /// <summary>
        /// 鼠标持续在背包格时
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerMove(PointerEventData eventData)
        {
            float distanceToTop = Screen.height - Input.mousePosition.y;
            RectTransform tooltipRect = InventoryUI.Instance.itemTooltip.GetComponent<RectTransform>();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //物品介绍显示在鼠标左下方
            if (distanceToTop <= 300f)
            {
                tooltipRect.pivot = new Vector2(1.1f, 1.1f);
            }
            //物品介绍显示在鼠标右上方
            else
            {
                tooltipRect.pivot = new Vector2(-0.1f, -0.1f);
            }
            tooltipRect.position = worldPos;
        }

        private void Awake()
        {
            slotUI = GetComponent<SlotUI>();
        }
    }

}
