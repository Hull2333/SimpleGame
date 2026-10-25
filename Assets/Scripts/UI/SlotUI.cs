using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static UnityEditor.Progress;
namespace MFarm.Inventory
{
    //IPointerClickHandler 点击事件的接口;IBeginDragHandler,IDragHandler,IEndDragHandler 分别是物品开始拖拽，拖拽过程，拖拽结束的接口方法
    public class SlotUI : MonoBehaviour,IPointerClickHandler,IBeginDragHandler,IDragHandler,IEndDragHandler //调用在预制体Slot_Bag上
    {
        [Header("组件获取")]
        [SerializeField] private Image slotImage;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private Button button;
        //商店Slot需要的名字和单价
        [SerializeField] private TextMeshProUGUI itemName;
        [SerializeField] private TextMeshProUGUI itemPrice;
        [SerializeField] private GameObject slider;
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
        public int currentAmount;
        private CursorManager cursorManager;
        //字体原来的颜色
        public Color originColor;
        public InventoryLocation Location
        {
            get
            {
                return slotType switch
                {
                    SlotType.Bag => InventoryLocation.Player,
                    SlotType.Box => InventoryLocation.Box,
                    SlotType.SellBox => InventoryLocation.SellBox,
                    SlotType.SellCar => InventoryLocation.SellCar,
                    SlotType.Shop => InventoryLocation.Shop,
                    _ => InventoryLocation.Player
                };              
            }
        }
       

        /// <summary>
        /// 游戏开始时，背包为空
        /// </summary>
        private void Start()
        {
            cursorManager = GameObject.FindWithTag("CursorManager").GetComponent<CursorManager>();
            //amountText.color = originColor;
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
        /// <param name="showAllAmount">是否需要显示总数量，不需要allAmount填"0"</param>
        /// <param name="allAmount">物品的总数量</param>
        public void UpdateSlot(ItemDetails item, int amount , bool showAllAmount , int allAmount)
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
            slotImage.sprite = item.itemIcon;
            //设置item在玩家背包中的图片显示比例
            if(slotType != SlotType.Ingredient && slotType != SlotType.Shop)
            {
                if (item.scale == SpriteScale.Normal)
                {
                    slotImage.rectTransform.sizeDelta = new Vector2(24f, 24f);
                }
                if (item.scale == SpriteScale.Middle)
                {
                    slotImage.SetNativeSize();
                    float newWidth = slotImage.rectTransform.rect.width / 3f;
                    float newHight = slotImage.rectTransform.rect.height / 3f;
                    slotImage.rectTransform.sizeDelta = new Vector2(newWidth, newHight);
                }
            }
           
            itemAmount = amount;
            //显示总数量和不显示总数量
            if (showAllAmount)
            {
                amountText.text = amount.ToString() + "/" + allAmount.ToString();
            }
            else
            {
                amountText.text = amount.ToString();
            }
            button.interactable = true;
            slotImage.enabled = true;
           if(slotType == SlotType.Shop)
            {
                itemName.text = itemDetails.itemName;
                itemPrice.text = itemDetails.itemPrice.ToString();
            }
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
        /// 设置数量Text变红
        /// </summary>
        public void SetRedAmountText()
        {
            originColor = amountText.color;
            amountText.color = Color.gray;
        }
        /// <summary>
        /// 恢复原来的字体颜色
        /// </summary>
        public void RecoverOriginTextColor()
        {
            originColor = amountText.color;
            amountText.color = originColor;
        }
        /// <summary>
        /// 点击Slot时
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            EventHandler.CallSlotUISelectIndexEvent(slotIndex);
            //如果物品不存在或者玩家背包无法打开时不让点选
            if (itemDetails == null || !InventoryUI.Instance.canOpenBag || slotType == SlotType.Ingredient || slotType == SlotType.Reward) return;
           
            // 玩家背包内的物品只有左键点击才会被选中，右键只执行快捷移动
            if (slotType != SlotType.Bag || eventData.button == PointerEventData.InputButton.Left)
            {
                isSelected = !isSelected;
            
                inventoryUI.UpdateSlotHighlight(slotIndex);
                //只有点击的玩家背包内的物品，才实现物品图片出现在头顶
                if(slotType == SlotType.Bag)
                {
                    //在InventoryManager上拿到当前选取的物品信息
                    InventoryManager.Instance.currentSelectedItem = itemDetails;
                    if (!isSelected)
                    {
                        InventoryManager.Instance.currentSelectedItem = null;
                    }
                    EventHandler.CallItemSelectEvent(itemDetails,isSelected);
                }
            }
            //左键点击的时商店格子
            if (Location == InventoryLocation.Shop)
            {
                if (inventoryUI.GetEmptySellBoxIndex(itemDetails.itemID, InventoryManager.Instance.playerBag.itemList, Location) >= 0)
                {
                    int playerbagIndex = inventoryUI.GetEmptySellBoxIndex(itemDetails.itemID, InventoryManager.Instance.playerBag.itemList, Location);
                    //打开交易选择物品UI
                    EventHandler.CallShowTradeUI(itemDetails, false, itemAmount, slotIndex, playerbagIndex, Location, InventoryLocation.Player, false);
                }
                return;
            }
            //右键点击Slot
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                // 右键穿戴：只有仅打开玩家背包时，装备/装饰品可以直接穿上，已有装备则替换
                if (TryEquipByRightClick())
                {
                    return;
                }
                // 右键脱下：右键点装备格里的装备，放回玩家背包的空格
                if (TryUnequipByRightClick())
                {
                    return;
                }
                // 卖车面板打开时，右键把整组物品送进卖车格子
                if (Location == InventoryLocation.Player && InventoryUI.Instance.IsSellCarBoxOpened)
                {
                    // 只允许指定类型的物品送进卖车格子
                    if (!IsSellCarMovable(itemDetails))
                    {
                        return;
                    }
                    int sellCarIndex = inventoryUI.GetEmptySellCarIndex(itemDetails.itemID);
                    if (sellCarIndex >= 0)
                    {
                        InventoryManager.Instance.SwapItem(Location, slotIndex, InventoryLocation.SellCar, sellCarIndex, false, itemAmount);
                    }
                    return;
                }
                // 卖车面板打开时，右键把卖车格子里的整组物品收回背包
                if (Location == InventoryLocation.SellCar && InventoryUI.Instance.IsSellCarBoxOpened)
                {
                    int bagIndex = inventoryUI.GetEmptySellBoxIndex(itemDetails.itemID, InventoryManager.Instance.playerBag.itemList, InventoryLocation.SellCar);
                    if (bagIndex >= 0)
                    {
                        InventoryManager.Instance.SwapItem(Location, slotIndex, InventoryLocation.Player, bagIndex, false, itemAmount);
                    }
                    return;
                }
                //在商店打开时
                if (InventoryManager.Instance.isSellState)
                {
                    //右键点击的是背包格
                    if (Location == InventoryLocation.Player)
                    {
                        if (inventoryUI.GetEmptySellBoxIndex(itemDetails.itemID, InventoryManager.Instance.sellBoxBag.itemList, Location) >= 0)
                        {
                            InventoryManager.Instance.SwapItem(Location, slotIndex, InventoryLocation.SellBox, inventoryUI.GetEmptySellBoxIndex(itemDetails.itemID, InventoryManager.Instance.sellBoxBag.itemList, Location), false, itemAmount);
                        }
                        return;
                    }
                    //右键点击的是出售箱格子
                    if (Location == InventoryLocation.SellBox)
                    {

                        if (inventoryUI.GetEmptySellBoxIndex(itemDetails.itemID, InventoryManager.Instance.playerBag.itemList, Location) >= 0)
                        {
                            InventoryManager.Instance.SwapItem(Location, slotIndex, InventoryLocation.Player, inventoryUI.GetEmptySellBoxIndex(itemDetails.itemID, InventoryManager.Instance.playerBag.itemList, Location), false, itemAmount);
                        }
                        return;
                    }
                }
            }
        }
        /// <summary>
        /// 物品开始拖拽
        /// </summary>
        /// <param name="eventData"></param>
        public void OnBeginDrag(PointerEventData eventData)
        {

            // 右键任何情况下都不允许拖动物品，只有左键可以拖拽
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            EventHandler.CallControlPlayerBagOpen(false);
            //空格子不可以拖拽和任务栏的奖励物品也不可以拖拽
            if (itemDetails != null &&itemDetails.itemID != 0 && slotType != SlotType.Reward && slotType != SlotType.Shop && slotType != SlotType.Ingredient)
            {
                if (Input.GetKey(KeyCode.LeftControl) && slotType == SlotType.Bag && itemAmount > 1) 
                {
                    InventoryManager.Instance.isHalfAmount = true;
                    if (itemAmount % 2 != 0)
                    {
                        itemAmount = itemAmount / 2;
                        currentAmount = itemAmount + 1;
                    }
                    else
                    {
                        itemAmount = itemAmount / 2;
                        currentAmount = itemAmount;
                    }
                    //同时刷新一下玩家背包
                    amountText.text = currentAmount.ToString();
                }
                //关闭鼠标Image
                cursorManager.cursorImage.enabled = false;
                inventoryUI.dragItem.enabled = true;
                inventoryUI.dragItemAmount.enabled = true;
                inventoryUI.dragItem.sprite = slotImage.sprite;
                inventoryUI.dragItemAmount.text = itemAmount.ToString();
                //自带的方法，使拖出来的dragItem图片尺寸为你提前设置好的尺寸
                if (itemDetails.scale == SpriteScale.Normal)
                {
                    inventoryUI.dragItem.rectTransform.sizeDelta = new Vector2(32f, 32f);
                }
                if (itemDetails.scale == SpriteScale.Middle)
                {
                    inventoryUI.dragItem.SetNativeSize();
                    float newWidth = inventoryUI.dragItem.rectTransform.rect.width / 2f;
                    float newHight = inventoryUI.dragItem.rectTransform.rect.height / 2f;
                    inventoryUI.dragItem.rectTransform.sizeDelta = new Vector2(newWidth, newHight);
                }
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
            // 右键任何情况下都不允许拖动物品，只有左键可以拖拽
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            //拖拽过程中拖拽物体位置随鼠标移动
            inventoryUI.dragItem.transform.position = Input.mousePosition;
        }
        /// <summary>
        /// 拖拽结束
        /// </summary>
        /// <param name="eventData"></param>
        public void OnEndDrag(PointerEventData eventData)
        {
          
            // 右键任何情况下都不允许拖动物品，只有左键可以拖拽
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            cursorManager.cursorImage.enabled = true;
            inventoryUI.dragItem.enabled = false;
            inventoryUI.dragItemAmount.enabled = false;
            EventHandler.CallControlPlayerBagOpen(true);
            if (itemDetails != null)
            {
               
                //确保拖拽到的地方不是空的
                if (eventData.pointerCurrentRaycast.gameObject != null)
                {
                    //且拖拽的地方必须有SlotUI脚本组件，即Bag_Slot
                    if (eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>() == null)
                    {
                        itemAmount += currentAmount;
                        InventoryManager.Instance.isHalfAmount = false;
                        EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, InventoryManager.Instance.playerBag.itemList, 0);
                        return;
                    }
                    //拿到拖拽目的点的组件和slot序号
                    var targetSlot = eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>();
                    int targetIndex = targetSlot.slotIndex;
                    //从背包格拖出
                    if (slotType == SlotType.Bag)
                    {
                       
                        //拖到背包格
                        if (targetSlot.slotType == slotType)
                        {
                            InventoryManager.Instance.Swapitem(slotIndex, targetIndex, itemAmount,currentAmount);
                            return;
                        }
                        //从玩家背包拖拽到装备(头)上
                        if (targetSlot.slotType == SlotType.Equipment_Head)
                        {
                            if (itemDetails.itemType == ItemType.Equipment_Head)
                            {
                                inventoryUI.SetEquipImage(itemDetails, targetSlot);
                                InventoryManager.Instance.Swapitem(slotIndex, targetIndex,1,0);
                                return;
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
                                InventoryManager.Instance.Swapitem(slotIndex, targetIndex,1,0);
                                return;
                            }
                            else
                            {
                                return;
                            }
                        }
                        // 从背包把装饰品拖到装饰品格子
                        if (targetSlot.slotType == SlotType.Equipment_Decoration)
                        {
                            // 只有装饰品类型的物品才能放入
                            if (itemDetails.itemType == ItemType.Decoration)
                            {
                                InventoryManager.Instance.Swapitem(slotIndex, targetIndex, 1, 0);
                                return;
                            }
                            else
                            {
                                return;
                            }
                        }
                        //拖到出售箱
                        if(targetSlot.slotType == SlotType.SellBox)
                        {
                            if (InventoryManager.Instance.isHalfAmount)
                            {
                                //同时刷新一下玩家背包
                                EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, InventoryManager.Instance.playerBag.itemList,0);
                                InventoryManager.Instance.isHalfAmount = false;
                                return;
                            }
                            else
                            {
                                //数量只有一个就直接加到出售箱
                                if (itemAmount == 1)
                                {
                                    InventoryManager.Instance.SwapItem(Location, slotIndex, targetSlot.Location, targetSlot.slotIndex, false, 1);
                                }
                                else
                                {
                                    InventoryManager.Instance.isHalfAmount = false;
                                    //打开交易选择物品UI
                                    EventHandler.CallShowTradeUI(itemDetails, true, itemAmount, slotIndex, targetIndex, Location, targetSlot.Location, true);
                                }
                            }
                        }
                        // 拖到卖车格子：整组物品移过去，售卖逻辑暂不处理
                        if (targetSlot.slotType == SlotType.SellCar)
                        {
                            if (InventoryManager.Instance.isHalfAmount)
                            {
                                // 半分拖拽不支持拖进卖车格子，直接刷新背包复原
                                InventoryManager.Instance.isHalfAmount = false;
                                EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, InventoryManager.Instance.playerBag.itemList, 0);
                                return;
                            }
                            // 只允许指定类型的物品送进卖车格子
                            if (!IsSellCarMovable(itemDetails))
                            {
                                return;
                            }
                            InventoryManager.Instance.SwapItem(Location, slotIndex, targetSlot.Location, targetIndex, false, itemAmount);
                            return;
                        }
                        //拖到商店
                        if(targetSlot.slotType == SlotType.Shop)
                        {
                            itemAmount += currentAmount;
                            InventoryManager.Instance.isHalfAmount = false;
                            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, InventoryManager.Instance.playerBag.itemList, 0);
                            return;
                        }
                        //拖到鱼饵
                        if(targetSlot.slotType == SlotType.Bait)
                        {
                            if (itemDetails.isBait)
                            {
                                InventoryManager.Instance.Swapitem(slotIndex, targetIndex, itemAmount, currentAmount);
                                return;
                            }
                            else
                            {
                                return;
                            }
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
                    // 从卖车格子拖出：拖回背包，或在卖车格子之间交换
                    if (slotType == SlotType.SellCar)
                    {
                        if (targetSlot.slotType == SlotType.Bag || targetSlot.slotType == SlotType.SellCar)
                        {
                            // 拖回卖车格子的同一格时直接忽略，避免数量翻倍
                            if (targetSlot.slotType == SlotType.SellCar && slotIndex == targetIndex)
                            {
                                return;
                            }
                            InventoryManager.Instance.SwapItem(Location, slotIndex, targetSlot.Location, targetIndex, false, itemAmount);
                        }
                        return;
                    }
                    //从头装备栏拖出
                    if (slotType == SlotType.Equipment_Head)
                    {
                        
                        //不可以从头部位置拖拽到身体位置，反之亦然
                        if (targetSlot.slotType == SlotType.Equipment_Body)
                        {
                            return;
                        }
                        //拖到背包中
                        if(targetSlot.slotType == SlotType.Bag)
                        {
                            // 背包格必须为空，或者装有头部装备，其他类型一律不交换
                            if (targetSlot.itemDetails != null && targetSlot.itemDetails.itemType != ItemType.Equipment_Head)
                            {
                                return;
                            }
                            // 背包格里有头部装备时，交换后头格会换成它，需要刷新角色外观
                            if (targetSlot.itemDetails != null)
                            {
                                inventoryUI.SetEquipImage(targetSlot.itemDetails, this);
                            }
                            else
                            {
                                inventoryUI.ResetEquipImage(slotType);
                            }
                            InventoryManager.Instance.Swapitem(slotIndex, targetIndex,1,0);
                        }
                        inventoryUI.UpdatePlayerDefenseNum();
                        return;
                    }
                    //从身体装备拖出
                    if (slotType == SlotType.Equipment_Body)
                    {

                        //不可以从身体位置拖拽到头部位置，反之亦然
                        if (targetSlot.slotType == SlotType.Equipment_Head)
                        {
                            return;
                        }
                        //拖到背包中
                        if (targetSlot.slotType == SlotType.Bag)
                        {
                            // 背包格必须为空，或者装有身体装备，其他类型一律不交换
                            if (targetSlot.itemDetails != null && targetSlot.itemDetails.itemType != ItemType.Equipment_Body)
                            {
                                return;
                            }
                            // 背包格里有身体装备时，交换后身体格会换成它，需要刷新角色外观
                            if (targetSlot.itemDetails != null)
                            {
                                inventoryUI.SetEquipImage(targetSlot.itemDetails, this);
                            }
                            else
                            {
                                inventoryUI.ResetEquipImage(slotType);
                            }
                            InventoryManager.Instance.Swapitem(slotIndex, targetIndex, 1, 0);
                        }
                        inventoryUI.UpdatePlayerDefenseNum();
                        return;
                    }
                    //从装饰品格子拖出
                    if (slotType == SlotType.Equipment_Decoration)
                    {
                        // 只允许放回背包，其他格子一律不接收
                        if (targetSlot.slotType != SlotType.Bag)
                        {
                            return;
                        }
                        // 背包格已有物品时，只有装饰品才能与它交换
                        if (targetSlot.itemDetails != null && targetSlot.itemDetails.itemType != ItemType.Decoration)
                        {
                            return;
                        }
                        InventoryManager.Instance.Swapitem(slotIndex, targetIndex, 1, 0);
                        return;
                    }
                    //从鱼饵格中拖出
                    if (slotType == SlotType.Bait)
                    {
                        //只可以拖到玩家背包
                        if(targetSlot.slotType == SlotType.Bag)
                        {
                            if (targetSlot.itemDetails != null)
                            {
                                if (!targetSlot.itemDetails.isBait)
                                {
                                    return;
                                }
                                else
                                {
                                    InventoryManager.Instance.Swapitem(slotIndex, targetIndex, itemAmount, currentAmount);
                                }
                                return;
                            }
                            //对方格子为空
                            else
                            {
                                InventoryManager.Instance.Swapitem(slotIndex, targetIndex, itemAmount, currentAmount);
                                return;
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    //跨背包交换物品数据
                    if (slotType != SlotType.Shop && targetSlot.slotType != SlotType.Shop && slotType != targetSlot.slotType)
                    {
                        
                        InventoryManager.Instance.SwapItem(Location, slotIndex, targetSlot.Location, targetSlot.slotIndex,false, 0);
                    }

                    //取消所有高亮显示，因为Slot序号不可能为-1
                    inventoryUI.UpdateSlotHighlight(-1);
                }
                //检测拖拽到地上 
                else if(!InventoryManager.Instance.anyBagOpened)
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
        /// <summary>
        /// 判断该物品是否允许在背包和卖车格子之间移动
        /// </summary>
        private bool IsSellCarMovable(ItemDetails item)
        {
            switch (item.itemType)
            {
                case ItemType.Seed:
                case ItemType.Commodity:
                case ItemType.Furniture:
                case ItemType.Cooked:
                case ItemType.Laterfish:
                case ItemType.Seafish:
                case ItemType.TreeSeed:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 右键穿戴：只有仅打开玩家背包时生效，装备/装饰品直接穿到对应装备格，已有装备则替换
        /// </summary>
        private bool TryEquipByRightClick()
        {
            // 只处理玩家背包格里的物品
            if (Location != InventoryLocation.Player || slotType != SlotType.Bag || itemDetails == null)
            {
                return false;
            }
            // 商店、出售、卖车等界面同时打开时不可以右键穿戴
            if (!inventoryUI.IsOnlyPlayerBagOpened)
            {
                return false;
            }
            SlotType targetType;
            switch (itemDetails.itemType)
            {
                case ItemType.Equipment_Head:
                    targetType = SlotType.Equipment_Head;
                    break;
                case ItemType.Equipment_Body:
                    targetType = SlotType.Equipment_Body;
                    break;
                case ItemType.Decoration:
                    targetType = SlotType.Equipment_Decoration;
                    break;
                default:
                    return false;
            }
            SlotUI equipSlot = FindEquipSlot(targetType);
            if (equipSlot == null)
            {
                return false;
            }
            // 身上已经是同一件装备时不处理
            if (equipSlot.itemDetails != null && equipSlot.itemDetails.itemID == itemDetails.itemID)
            {
                return false;
            }
            // 头/身装备先刷新预览外观（Swapitem 刷新界面时会把背包格的 itemDetails 置空）
            if (targetType == SlotType.Equipment_Head || targetType == SlotType.Equipment_Body)
            {
                inventoryUI.SetEquipImage(itemDetails, equipSlot);
            }
            // 交换两格数据：装备格为空直接穿上，已有装备则旧装备回到刚才点击的背包格
            InventoryManager.Instance.Swapitem(slotIndex, equipSlot.slotIndex, 1, 0);
            // 刷新防御力（读取交换后的装备格数据）
            if (targetType == SlotType.Equipment_Head || targetType == SlotType.Equipment_Body)
            {
                inventoryUI.UpdatePlayerDefenseNum();
            }
            return true;
        }

        /// <summary>
        /// 在玩家背包所有格子中查找指定类型的装备格
        /// </summary>
        private SlotUI FindEquipSlot(SlotType targetType)
        {
            if (inventoryUI.playerSlots == null)
            {
                return null;
            }
            foreach (SlotUI slot in inventoryUI.playerSlots)
            {
                if (slot != null && slot.slotType == targetType)
                {
                    return slot;
                }
            }
            return null;
        }

        /// <summary>
        /// 右键脱下装备：只有仅打开玩家背包时生效，装备放回背包空格，没有空格则不做任何事
        /// </summary>
        private bool TryUnequipByRightClick()
        {
            // 只处理玩家身上的三个装备格
            if (Location != InventoryLocation.Player || itemDetails == null)
            {
                return false;
            }
            if (slotType != SlotType.Equipment_Head && slotType != SlotType.Equipment_Body && slotType != SlotType.Equipment_Decoration)
            {
                return false;
            }
            // 商店、出售、卖车等界面同时打开时不可以脱下
            if (!inventoryUI.IsOnlyPlayerBagOpened)
            {
                return false;
            }
            // 背包没有空格就什么也不做
            int emptyIndex = FindEmptyBagIndex();
            if (emptyIndex < 0)
            {
                return false;
            }
            // 装备从装备格移动到背包空格
            InventoryManager.Instance.Swapitem(slotIndex, emptyIndex, 1, 0);
            // 头/身装备脱下后恢复默认外观并刷新防御力
            if (slotType == SlotType.Equipment_Head || slotType == SlotType.Equipment_Body)
            {
                inventoryUI.ResetEquipImage(slotType);
                inventoryUI.UpdatePlayerDefenseNum();
            }
            return true;
        }

        /// <summary>
        /// 查找玩家背包格中第一个空格的下标，没有空格返回 -1
        /// </summary>
        private int FindEmptyBagIndex()
        {
            if (inventoryUI.playerSlots == null)
            {
                return -1;
            }
            foreach (SlotUI slot in inventoryUI.playerSlots)
            {
                if (slot == null || slot.slotType != SlotType.Bag)
                {
                    continue;
                }
                if (slot.itemDetails != null || slot.slotIndex >= InventoryManager.Instance.playerBag.itemList.Count)
                {
                    continue;
                }
                if (InventoryManager.Instance.playerBag.itemList[slot.slotIndex].itemID == 0)
                {
                    return slot.slotIndex;
                }
            }
            return -1;
        }
    }
}

