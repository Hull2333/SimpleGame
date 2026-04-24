using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace MFarm.Inventory
{
    //IPointerClickHandler 点击事件的接口;IBeginDragHandler,IDragHandler,IEndDragHandler 分别是物品开始拖拽，拖拽过程，拖拽结束的接口方法
    public class SlotUI : MonoBehaviour,IPointerClickHandler,IBeginDragHandler,IDragHandler,IEndDragHandler //调用在预制体Slot_Bag上
    {
        [Header("组件获取")]
        [SerializeField] private Image slotImage;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private Button button;
        //把声明直接赋值的一种方法
        public InventoryUI inventoryUI => GetComponentInParent<InventoryUI>();
        [Header("格子类型")]
        public SlotType slotType;
        public bool isSelected;
        public ItemDetails itemDetails;
        public int itemAmount;
        //每个背包格子的序号
        public int slotIndex;
        public Image slotHighlight;
        
        public InventoryLocation Location
        {
            get
            {
                return slotType switch
                {
                    SlotType.Bag => InventoryLocation.Player,
                    SlotType.Box => InventoryLocation.Box,
                    SlotType.SellBox => InventoryLocation.SellBox,
                    _ => InventoryLocation.Player
                };              
            }
        }
        /// <summary>
        /// 游戏开始时，背包为空
        /// </summary>
        private void Start()
        {
            isSelected = false;
            if (itemDetails == null)
            {
                UpdateEmptySlot();
            }
        }
        /// <summary>
        /// 更新格子和信息
        /// </summary>
        /// <param name="item"></param>
        /// <param name="amount"></param>
        public void UpdateSlot(ItemDetails item, int amount)
        {
            //不显示为负数的奖励，因为这是要减去背包内的所需物品
            if (slotType == SlotType.Reward && amount < 0)
            {
                gameObject.transform.GetChild(0).gameObject.SetActive(false);
                gameObject.transform.GetChild(1).gameObject.SetActive(false);
                gameObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0);
                amountText.gameObject.SetActive(false);
                return;
            }
            itemDetails = item;
            //slotImage.SetNativeSize();
            slotImage.sprite = item.itemIcon;
            itemAmount = amount;
            amountText.text = amount.ToString();
            button.interactable = true;
            slotImage.enabled = true;
           
        }

        /// <summary>
        /// 时刻刷新背包中的空格子
        /// </summary>
        public void UpdateEmptySlot()
        {
            //当空格子被选中时取消选中和高亮
            if (isSelected)
            {
                isSelected = false;
                inventoryUI.UpdateSlotHighlight(-1);
                EventHandler.CallItemSelectEvent(itemDetails,isSelected);
            }
            //空格子的物品信息、Image、文本、选取都取消
            itemDetails = null;
            slotImage.enabled = false;
            amountText.text = string.Empty;
            button.interactable = false;
        }
        /// <summary>
        /// 点击Slot时
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {   
            //如果物品不存在，不让点选
            if (itemDetails == null) return;
            isSelected = !isSelected;

            inventoryUI.UpdateSlotHighlight(slotIndex);
            //只有点击的玩家背包内的物品，才实现物品图片出现在头顶
            if(slotType == SlotType.Bag)
            {
                //在InventoryManager上拿到当前选取的物品信息
                InventoryManager.Instance.currentSelectedItem = itemDetails;
                EventHandler.CallItemSelectEvent(itemDetails,isSelected);
            }
        }
        /// <summary>
        /// 物品开始拖拽
        /// </summary>
        /// <param name="eventData"></param>
        public void OnBeginDrag(PointerEventData eventData)
        {
            //空格子不可以拖拽和任务栏的奖励物品也不可以拖拽
            if(itemDetails != null &&itemDetails.itemID != 0 && slotType != SlotType.Reward)
            {
                inventoryUI.dragItem.enabled = true;
                inventoryUI.dragItem.sprite = slotImage.sprite;
                //自带的方法，使拖出来的dragItem图片尺寸为你提前设置好的尺寸
                //inventoryUI.dragItem.SetNativeSize();
                inventoryUI.dragItem.transform.localScale = new Vector3(3, 3, 0);
                isSelected = true;
                inventoryUI.UpdateSlotHighlight(slotIndex);
            }
          
        }
        /// <summary>
        /// 拖拽过程
        /// </summary>
        /// <param name="eventData"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void OnDrag(PointerEventData eventData)
        {   
            //拖拽过程中拖拽物体位置随鼠标移动
            inventoryUI.dragItem.transform.position = Input.mousePosition;
        }
        /// <summary>
        /// 拖拽结束
        /// </summary>
        /// <param name="eventData"></param>
        public void OnEndDrag(PointerEventData eventData)
        {
           inventoryUI.dragItem.enabled = false;
            //Debug.Log(eventData.pointerCurrentRaycast.gameObject);
            if (itemDetails != null)
            {
                //确保拖拽到的地方不是空的
                if (eventData.pointerCurrentRaycast.gameObject != null)
                {
                    //且拖拽的地方必须有SlotUI脚本组件，即Bag_Slot
                    if (eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>() == null)
                    {
                        return;
                    }
                    //拿到拖拽目的点的组件和slot序号
                    var targetSlot = eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>();
                    int targetIndex = targetSlot.slotIndex;
                   // Debug.Log(" slotType:" + slotType + "   targetSlot: " + targetSlot.slotType);
                    //当初始和目标拖拽的背包类型都是Bag，即玩家自身的背包内拖拽
                    // if (slotType == SlotType.Bag && targetSlot.slotType == SlotType.Bag)
                    // {
                    //     InventoryManager.Instance.Swapitem(slotIndex, targetIndex);
                    // }
                    //从背包格拖出
                    if (slotType == SlotType.Bag)
                    {
                        //拖到背包格
                        if (targetSlot.slotType == slotType)
                        {
                            InventoryManager.Instance.Swapitem(slotIndex, targetIndex);
                        }
                        //从玩家背包拖拽到装备(头)上
                        if (targetSlot.slotType == SlotType.Equipment_Head)
                        {
                            if (itemDetails.itemType == ItemType.Equipment_Head)
                            {
                                inventoryUI.SetEquipImage(itemDetails, targetSlot);
                                InventoryManager.Instance.Swapitem(slotIndex, targetIndex);

                            }
                            else
                            {
                                return;
                            }
                        }
                        //从玩家背包拖拽到装备(身体)上
                        if (targetSlot.slotType == SlotType.Equipment_Body)
                        {
                            if (itemDetails.itemType == ItemType.Equipment_Body)
                            {
                                inventoryUI.SetEquipImage(itemDetails, targetSlot);
                                InventoryManager.Instance.Swapitem(slotIndex, targetIndex);  
                            }
                            else
                            {
                                return;
                            }
                        }
                        //拖到出售箱
                        if(targetSlot.slotType == SlotType.SellBox)
                        {
                            //打开交易选择物品UI
                            EventHandler.CallShowTradeUI(itemDetails, true, itemAmount, slotIndex, targetIndex,Location,targetSlot.Location);
                            //InventoryManager.Instance.SwapItem(Location, slotIndex, targetSlot.Location, targetSlot.slotIndex);
                        }
                        inventoryUI.UpdatePlayerDefenseNum();
                        return;
                    }
                    //从出售箱拖出
                    if (slotType == SlotType.SellBox)
                    {
                        //拖到玩家背包中
                        if (targetSlot.slotType == SlotType.Bag)
                        {
                            InventoryManager.Instance.SwapItem(Location, slotIndex, targetSlot.Location, targetSlot.slotIndex,false,0);
                        }
                        //拖到出售箱中
                        if (targetSlot.slotType == slotType)
                        {
                            InventoryManager.Instance.SwapSellBoxItem(slotIndex, targetIndex);
                        }
                        return;
                        
                    }
                   
                    //从头装备栏拖出
                    else if (slotType == SlotType.Equipment_Head)
                    {
                        
                        //不可以从头部位置拖拽到身体位置，反之亦然
                        if (targetSlot.slotType == SlotType.Equipment_Body)
                        {
                            return;
                        }
                        //拖到背包中
                        if(targetSlot.slotType == SlotType.Bag)
                        {
                            inventoryUI.ResetEquipImage(slotType);
                            InventoryManager.Instance.Swapitem(slotIndex, targetIndex);
                        }
                        inventoryUI.UpdatePlayerDefenseNum();
                    }
                    //从身体装备拖出
                    else if (slotType == SlotType.Equipment_Body)
                    {
                        
                        //不可以从身体位置拖拽到头部位置，反之亦然
                        if (targetSlot.slotType == SlotType.Equipment_Head)
                        {
                            return;
                        }
                        //拖到背包中
                        if (targetSlot.slotType == SlotType.Bag)
                        {
                            inventoryUI.ResetEquipImage(slotType);
                            InventoryManager.Instance.Swapitem(slotIndex, targetIndex);
                        }
                        inventoryUI.UpdatePlayerDefenseNum();
                    }
                    
                    //买
                    else if (slotType == SlotType.Shop && targetSlot.slotType == SlotType.Bag)
                    {
                        
                        //EventHandler.CallShowTradeUI(itemDetails, false);
                    }
                    //卖
                    else if (slotType == SlotType.Bag && targetSlot.slotType == SlotType.Shop)
                    {
                    
                        //EventHandler.CallShowTradeUI(itemDetails, true);
                    }
                    //跨背包交换物品数据
                    else if (slotType != SlotType.Shop && targetSlot.slotType != SlotType.Shop && slotType != targetSlot.slotType)
                    {
 
                        InventoryManager.Instance.SwapItem(Location, slotIndex, targetSlot.Location, targetSlot.slotIndex,false, 0);
                    }

                    //取消所有高亮显示，因为Slot序号不可能为-1
                    inventoryUI.UpdateSlotHighlight(-1);
                }
                //检测拖拽到地上 
                else
                {
                    //且物品可以丢弃
                    if (itemDetails.canDropped)
                    {
                        //将鼠标坐标转换为世界地图坐标
                        var pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
                        EventHandler.CallInstantiateItemInScene(itemDetails.itemID, pos);
                    }
                }
            }     
        }
    }
}

