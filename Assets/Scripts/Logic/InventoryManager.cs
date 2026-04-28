using MFarm.Save;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using static UnityEditor.Progress;
using static UnityEditor.Timeline.Actions.MenuPriority;
//创建一个命名空间，可以在其他脚本中using该命名空间来使用该脚本里的方法
namespace MFarm.Inventory
{
    public class InventoryManager : Singleton<InventoryManager>,ISaveable   //调用在InventoryManager对象上
    {
        [Header("物品数据")]
        public ItemDataList_SO itemDataList_SO;
        [Header("建造蓝图")]
        public BluPrintData_SO bluPrintData;
        [Header("菜谱")]
        public CookedData_SO cookedData;
        [Header("鱼类")]
        public FishData_SO fishData;
        [Header("背包数据")]
        public InventoryBag_SO playerBagTemp;
        public InventoryBag_SO playerBag;
        //当前打开的箱子
        private InventoryBag_SO currentBoxBag;
        public InventoryBag_SO sellBoxBag;
        public InventoryBag_SO sellBoxBagTemp;
       [Header("交易")]
        public int playerMoney;
        //存储地图中所有的箱子里的物品数据
        private Dictionary<string, List<InventoryItem>> boxDataDict = new Dictionary<string, List<InventoryItem>>();
        public int boxDataAmount => boxDataDict.Count;

        public string GUID => GetComponent<DataGUID>().guid;
        private AnimatorOverride animatorOverride;
        //所有鱼类的列表
        public static List<ItemDetails> fishList = new List<ItemDetails>();
        //不同时间段的淡水鱼和海水鱼的列表
        public List<ItemDetails> laterFishList = new List<ItemDetails>();
        public List<ItemDetails> seaFishList = new List<ItemDetails>();
        //当前选择的物品
        public ItemDetails currentSelectedItem;
        //出售状态
        public bool isSellState;
        //物品对半分
        public bool isHalfAmount;
        /// <summary>
        /// 在每次游戏开始时就执行一次玩家背包数据更新
        /// </summary>
        private void Start()
        {
            
            ISaveable saveable = this;
            saveable.RegisterSaveable();
            animatorOverride = GameObject.FindAnyObjectByType<AnimatorOverride>();
            //EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);

        }

        private void OnEnable()
        {
            EventHandler.DropItemEvent += OnDropItemEvent;
            EventHandler.HarvestAtPlayerPosition += OnHarvestAtPlayerPosition;
            //建造物品后删除背包素材
            EventHandler.BuildFurnitureEvent += OnBuildFurnitureEvent;
            //菜肴制作后删除背包中的所需食材
            EventHandler.CookedMakeEvent += OnCookedMakeEvent;
            //箱子打开存放事件
            EventHandler.BaseBagOpenEvent += OnBaseBagOpenEvent;
            //新游戏开始事件
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
            //使用物品恢复玩家状态
            EventHandler.UseItemRecoverEvent += OnUseItemRecoverEvent;
            EventHandler.FishListInSpriteDayFreshEvent += OnFishListInSpriteDayFreshEvent;
            EventHandler.RemoveTreeSeedCount += OnRemoveTreeSeedCount;
        }

        private void OnDisable()
        {
            EventHandler.DropItemEvent -= OnDropItemEvent;
            EventHandler.HarvestAtPlayerPosition -= OnHarvestAtPlayerPosition;
            //建造物品后删除背包素材
            EventHandler.BuildFurnitureEvent -= OnBuildFurnitureEvent;
            EventHandler.CookedMakeEvent -= OnCookedMakeEvent;
            EventHandler.BaseBagOpenEvent -= OnBaseBagOpenEvent;
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
            EventHandler.UseItemRecoverEvent += OnUseItemRecoverEvent;
            EventHandler.FishListInSpriteDayFreshEvent -= OnFishListInSpriteDayFreshEvent;
            EventHandler.RemoveTreeSeedCount -= OnRemoveTreeSeedCount;
        }

       

        private void OnUseItemRecoverEvent(ItemDetails details, PlayerController playerController)
        {
            animatorOverride.PlayEatAnim();
            RemoveItem(details.itemID, 1);
            playerController.PlayerHurted(-details.recoverHealth, -details.recoverStmina,null);
        }

        private void OnBaseBagOpenEvent(SlotType slotType, InventoryBag_SO bag_SO)
        {
            isSellState = true;
            currentBoxBag = bag_SO;
        }

        /// <summary>
        /// 家具建造后删除家具图纸和家具素材
        /// </summary>
        /// <param name="ID">图纸ID</param>
        /// <param name="mousePos"></param>
        private void OnBuildFurnitureEvent(int ID, Vector3 mousePos)
        {
            RemoveItem(ID, 1);
            BluPrintDetails bluePrint = bluPrintData.GetBluPrintDetails(ID);
            foreach(var item in bluePrint.resourceItem)
            {
                RemoveItem(item.itemID, item.itemAmount);
            }
        }

        private void OnCookedMakeEvent(int ID)
        {
            CookedDetails cookedDetails = cookedData.GetCookedDetails(ID);
            //删除指定数量的食材
            foreach(var ingredient in cookedDetails.cookResource)
            {
                RemoveItem(ingredient.itemID, ingredient.itemAmount);
            }
        }

        private void OnHarvestAtPlayerPosition(int ID)
        {
            var index = GetItemIndexInBag(ID,playerBag.itemList);
            AddItemAtIndex(ID, index, 1);
            //刷新玩家背包
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player,playerBag.itemList);
        }

        /// <summary>
        /// 丢弃物品
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="pos"></param>
        private void OnDropItemEvent(int ID, Vector3 pos, ItemType itemType)
        {
            RemoveItem(ID,1);
        }
        /// <summary>
        /// 重新开始游戏时需要初始化的数据
        /// </summary>
        /// <param name="obj"></param>
        private void OnStartNewGameEvent(int obj)
        {
            //初始化玩家背包数据
            playerBag = Instantiate(playerBagTemp);
            //初始化玩家金额
            playerMoney = Settings.playerStartMoney;
            //清空箱子shuju
            boxDataDict.Clear();
            //清空鱼类列表,然后重新添加鱼类列表
            fishList.Clear();
            for(int i = 0; i < itemDataList_SO.itemDetailsList.Count; i++)
            {
                var itemDetails = itemDataList_SO.itemDetailsList[i];
                if(itemDetails.itemType == ItemType.Laterfish || itemDetails.itemType == ItemType.Seafish)
                {
                    fishList.Add(itemDetails);
                }
            }
            Debug.Log(fishList.Count);
            //刷新当前时段的渔获列表
            EventHandler.CallFishListInSpriteDayFreshEvent(fishList);
            //更新玩家背包UI
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player,playerBag.itemList);
            //初始化并刷新出售箱
            sellBoxBag = Instantiate(sellBoxBagTemp);
            //for (int i = 0; i < sellBoxBag.itemList.Count; i++)
            //{
            //    sellBoxBag.itemList[i] = new InventoryItem();
            //}
            EventHandler.CallUpdateInventoryUI(InventoryLocation.SellBox, sellBoxBag.itemList);

        }
        private void OnFishListInSpriteDayFreshEvent(List<ItemDetails> list)
        {
            //初始化各个鱼列表
            laterFishList.Clear();
            seaFishList.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].itemType == ItemType.Laterfish)
                {
                    if (list[i].fishlifeSeason == FishlifeSeason.Sprite || list[i].fishlifeSeason == FishlifeSeason.SpriteAndAutumn || list[i].fishlifeSeason == FishlifeSeason.SpriteAndSummer)
                    {
                        if(list[i].fishlifeTime == FishlifeTime.AllDay || list[i].fishlifeTime == FishlifeTime.Day)
                        {
                            laterFishList.Add(list[i]);
                        }
                        
                    }
                    //Debug.Log(list[i].itemType + " " + list[i].itemName);
                }
                if (list[i].itemType == ItemType.Seafish)
                {
                    if (list[i].fishlifeSeason == FishlifeSeason.Sprite || list[i].fishlifeSeason == FishlifeSeason.SpriteAndAutumn || list[i].fishlifeSeason == FishlifeSeason.SpriteAndSummer)
                    {
                        if (list[i].fishlifeTime == FishlifeTime.AllDay || list[i].fishlifeTime == FishlifeTime.Day)
                        {
                            seaFishList.Add(list[i]);
                        }

                    }
                    //Debug.Log(list[i].itemType + " " + list[i].itemName);
                }
               
            }
            Debug.Log("LaterFish:" + laterFishList.Count);
            Debug.Log("SeaFish:" + seaFishList.Count);
        }
        private void OnRemoveTreeSeedCount(int seedID)
        {
            RemoveItem(seedID, 1);
        }
        /// <summary>
        /// 通过ID返回物品信息
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ItemDetails GetItemDetails(int ID)
        {
            //通过传进来的ID来和itemDetailList中的i.itemID(item.itemID)进行比对搜索,搜索的结果再放回给上面的方法
            return itemDataList_SO.itemDetailsList.Find(i => i.itemID == ID);
        }
        /// <summary>
        /// 将拾取物品的数据添加到背包
        /// </summary>
        /// <param name="item"></param>
        /// <param name="toDestory">是否要销毁物品</param>
        public void AddItem(Item item,bool toDestory)
        {
            if (toDestory)
            {
                Destroy(item.gameObject);
            }
            //背包格序号
            var index = GetItemIndexInBag(item.itemID, playerBag.itemList);
            AddItemAtIndex(item.itemID, index, 1);
            
            //更新UI,将需要更新的背包位置和此背包的item列表输入进去
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }
        /// <summary>
        /// 获取任务奖励物品
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="amount"></param>
        public void AddRewardItem(int itemID,int amount)
        {
            var index = GetItemIndexInBag(itemID, playerBag.itemList);
            AddItemAtIndex(itemID, index, amount);
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }
        /// <summary>
        /// 检查背包是否有空位
        /// </summary>
        /// <returns></returns>
        private bool CheckBagCapacity()
        {
            for (int i = 0; i < playerBag.itemList.Count; i++)
            {
                if (playerBag.itemList[i].itemID == 0)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 通过物品ID找到背包已有物品位置
        /// </summary>
        /// <param name="ID">物品ID</param>
        /// <returns>-1则表示没有该物品，有的话返回对应的序号</returns>
        public int GetItemIndexInBag(int ID , List<InventoryItem> bagList)
        {
            for (int i = 0; i < bagList.Count; i++)
            {
                if (bagList[i].itemID == ID)
                {
                    return i;
                }
            }
            return -1;
        }
        /// <summary>
        /// 找到背包空位位置或者已有物品位置
        /// </summary>
        /// <param name="ID">拾取物品的itemID</param>
        /// <param name="index">已有物品在背包的位置</param>
        /// <param name="amount">物品的数量</param>
        private void AddItemAtIndex(int ID,int index,int amount)
        {
            //背包没有该物品,且有空位
            if (index == -1&& CheckBagCapacity())
            {
                //初始化这个物品
                var item = new InventoryItem { itemID = ID, itemAmount = amount };
                //找到背包空位的位置并把新物品赋值到空位上
                for (int i = 0; i < playerBag.itemList.Count; i++)
                {
                    if (playerBag.itemList[i].itemID == 0)
                    {
                        playerBag.itemList[i] = item;
                        break;
                    }
                }
            }
            //背包中已有该物品
            else
            {
                //增加当前物品的数量,并初始化物品的ID和数量和找到该物品的位置重新赋值新的物品信息
                int currentAmount = playerBag.itemList[index].itemAmount + amount;
                var item = new InventoryItem { itemID = ID, itemAmount = currentAmount };
                playerBag.itemList[index] = item; 

            }
        }
        /// <summary>
        /// 交换slot的序号
        /// </summary>
        /// <param name="fromIndex">开始拖拽的slot的序号</param>
        /// <param name="targetIndex">结束拖拽的slot的序号</param>
        public void Swapitem(int fromIndex,int targetIndex,int currentAmount,int halfAmount)
        {
            InventoryItem currentItem = playerBag.itemList[fromIndex];
            InventoryItem targetItem = playerBag.itemList[targetIndex];
            //目标背包格不为空
            if(targetItem.itemID != 0)
            {
                //原来物品和目标物品不同
                if (targetItem.itemID != currentItem.itemID)
                {
                    if (isHalfAmount)
                    {
                        isHalfAmount = false;
                        return;
                    }
                    else
                    {
                        //交换序号
                        playerBag.itemList[fromIndex] = targetItem;
                        playerBag.itemList[targetIndex] = currentItem;
                    }
                   
                }
                //原来物品和目标物品相同
                else
                {
                    //没移动物品
                    if(fromIndex == targetIndex)
                    {
                        //同时刷新一下玩家背包
                        EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
                        return;
                    }
                    //添加物品数量
                    else
                    {
                        if (isHalfAmount)
                        {
                            var amount = targetItem.itemAmount + currentAmount;
                            var item = new InventoryItem { itemID = currentItem.itemID, itemAmount = amount };
                            playerBag.itemList[targetIndex] = item;
                            playerBag.itemList[fromIndex] = new InventoryItem { itemID = currentItem.itemID, itemAmount = halfAmount };
                            isHalfAmount = false;
                        }
                        else
                        {
                            var amount = targetItem.itemAmount + currentItem.itemAmount;
                            var item = new InventoryItem { itemID = currentItem.itemID, itemAmount = amount };
                            playerBag.itemList[targetIndex] = item;
                            playerBag.itemList[fromIndex] = new InventoryItem();
                        } 
                    }
                }
             
            }
            //如果拖拽结束后slot是空的
            else
            {
                if (isHalfAmount)
                {
                    playerBag.itemList[targetIndex] = new InventoryItem { itemID = currentItem.itemID, itemAmount = currentAmount };
                    playerBag.itemList[fromIndex] = new InventoryItem { itemID = currentItem.itemID, itemAmount = halfAmount };
                    isHalfAmount = false;
                }
                else
                {
                    //把拖拽开始的slot设置为新的slot
                    playerBag.itemList[targetIndex] = currentItem;
                    playerBag.itemList[fromIndex] = new InventoryItem();
                }
              
            }
            //同时刷新一下玩家背包
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }
        /// <summary>
        /// 在出售箱内拖拽物品
        /// </summary>
        /// <param name="fromIndex"></param>
        /// <param name="targetIndex"></param>
        public void SwapSellBoxItem(int fromIndex, int targetIndex)
        {
            InventoryItem currentItem = sellBoxBag.itemList[fromIndex];
            InventoryItem targetItem = sellBoxBag.itemList[targetIndex];
            //拖拽结束后的slot不是空的
            if (targetItem.itemID != 0)
            {
                //拖拽的物品和目标物品不同
                if (currentItem.itemID != targetItem.itemID)
                {
                    //交换序号
                    sellBoxBag.itemList[fromIndex] = targetItem;
                    sellBoxBag.itemList[targetIndex] = currentItem;
                }
                //两个物品相等时
                else
                {
                    //没移动物品
                    if (fromIndex == targetIndex)
                    {
                        return;
                    }
                    //增加物品的方法很怪，不过没办法
                    else
                    {
                        var amount = targetItem.itemAmount + currentItem.itemAmount;
                        var item = new InventoryItem { itemID = currentItem.itemID, itemAmount = amount };
                        sellBoxBag.itemList[targetIndex] = item;
                        sellBoxBag.itemList[fromIndex] = new InventoryItem();
                    }
                   
                }
            }
            //如果拖拽结束后slot是空的
            else
            {
                //把拖拽开始的slot设置为新的slot
                sellBoxBag.itemList[targetIndex] = currentItem;
                sellBoxBag.itemList[fromIndex] = new InventoryItem();
            }
            //同时刷新一下出售箱
            EventHandler.CallUpdateInventoryUI(InventoryLocation.SellBox, sellBoxBag.itemList);
        }
        /// <summary>
        /// 跨背包交换数据
        /// </summary>
        /// <param name="locationFrom"></param>
        /// <param name="fromIndex"></param>
        /// <param name="locationTarget"></param>
        /// <param name="targetIndex"></param>
        /// <param name="isSelect">是否询问数量</param>
        /// <param name="selectAmount"></param>
        public void SwapItem(InventoryLocation locationFrom, int fromIndex, InventoryLocation locationTarget, int targetIndex, bool isSelect, int selectAmount)
        {
            var currentList = GetItemList(locationFrom);
            var targetList = GetItemList(locationTarget);
            InventoryItem currentItem = currentList[fromIndex];
            //不询问数量
            if (!isSelect)
            {
                if (targetIndex < targetList.Count)
                {
                    InventoryItem targetItem = targetList[targetIndex];
                    //当箱子和玩家背包交换的物品不同时
                    if (targetItem.itemID != 0 && currentItem.itemID != targetItem.itemID)
                    {
                        Debug.Log("diff");
                        currentList[fromIndex] = targetItem;
                        targetList[targetIndex] = currentItem;
                    }
                    //两个物品相等时
                    else if (currentItem.itemID == targetItem.itemID)
                    {
                        Debug.Log("same");
                        targetItem.itemAmount += currentItem.itemAmount;
                        targetList[targetIndex] = targetItem;
                        currentList[fromIndex] = new InventoryItem();
                    }
                    //目标是空格子
                    else
                    {
                        Debug.Log(locationTarget);
                        Debug.Log("empty");
                        targetList[targetIndex] = currentItem;
                        currentList[fromIndex] = new InventoryItem();

                    }
                }
            }
            //询问数量
            else
            {
                Debug.Log(fromIndex + "  " + targetIndex);
                if (targetIndex < targetList.Count)
                {
                    InventoryItem targetItem = targetList[targetIndex];
                    //当箱子和玩家背包交换的物品不同时
                    if (targetItem.itemID != 0 && currentItem.itemID != targetItem.itemID)
                    {
                        return;
                    }
                    //两个物品相等时
                    else if (currentItem.itemID == targetItem.itemID)
                    {
                        targetItem.itemAmount += selectAmount;
                        targetList[targetIndex] = targetItem;
                        currentItem.itemAmount -= selectAmount;
                        currentList[fromIndex] = currentItem;
                    }
                    //目标是空格子
                    else
                    {
                        
                        //var amount = targetItem.itemAmount + currentItem.itemAmount;
                        //var item = new InventoryItem { itemID = currentItem.itemID, itemAmount = amount };
                        //sellBoxBag.itemList[targetIndex] = item;
                        //sellBoxBag.itemList[fromIndex] = new InventoryItem();
                        var tempTargetItem = new InventoryItem { itemID = currentItem.itemID, itemAmount = selectAmount };
                        var tempCurrentItem = new InventoryItem { itemID = currentItem.itemID, itemAmount = currentItem.itemAmount - selectAmount };
                        targetList[targetIndex] = tempTargetItem;
                        currentList[fromIndex] = tempCurrentItem;
                    }
                }
            }
            //更新双方的格子UI
            EventHandler.CallUpdateInventoryUI(locationFrom, currentList);
            EventHandler.CallUpdateInventoryUI(locationTarget, targetList);
        }
        /// <summary>
        /// 根据位置返回背包数据列表
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private List<InventoryItem> GetItemList(InventoryLocation location)
        {
            return location switch
            {
                InventoryLocation.Player => playerBag.itemList,
                InventoryLocation.Box => currentBoxBag.itemList,
                InventoryLocation.SellBox => sellBoxBag.itemList,
                _ => null
            };
        }
/// <summary>
/// 移除指定数量的背包物品
/// </summary>
/// <param name="ID"></param>
/// <param name="removeAmount"></param>
        public void RemoveItem(int ID , int removeAmount)
        {
            //获取丢弃物品ID在背包格中的序号
            var index = GetItemIndexInBag(ID, playerBag.itemList);
            if (playerBag.itemList[index].itemAmount > removeAmount)
            {
                //更新当前物品数量
                var amount = playerBag.itemList[index].itemAmount - removeAmount;
                var item = new InventoryItem { itemID=ID, itemAmount = amount };
                playerBag.itemList[index] = item;
            }
            else if (playerBag.itemList[index].itemAmount == removeAmount)
            {
                var item = new InventoryItem();
                playerBag.itemList[index] = item;
            }
            //同时对UI也要刷新数量
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player,playerBag.itemList);
        }
        /// <summary>
        /// 交易物品
        /// </summary>
        /// <param name="itemDetails">交易物品的信息</param>
        /// <param name="amount">交易数量</param>
        /// <param name="isSellTrade">是否卖东西</param>
        public void TradeItem(ItemDetails itemDetails,int amount,bool isSellTrade,int playerBagIndex,int sellBoxIndex,InventoryLocation startLocation,InventoryLocation endLocation,bool isToSellBox)
        {
            int cost = itemDetails.itemPrice * amount;
            //获得物品背包位置
            //int index = GetItemIndexInBag(itemDetails.itemID);
            //卖
            if (isSellTrade)
            {
                //到出售箱
                if (isToSellBox)
                {
                    SwapItem(startLocation, playerBagIndex, endLocation, sellBoxIndex, true, amount);
                }
                //到商店
                else
                {
                    RemoveItem(itemDetails.itemID, amount);
                    playerMoney += (int)(cost * itemDetails.sellPercentage);
                }
            }
            //买，要有足够的钱
            else if(playerMoney - cost >= 0)
            {
                //检查背包是否还有空位
                if (CheckBagCapacity())
                {
                    AddItemAtIndex(itemDetails.itemID, playerBagIndex, amount);
                }
                playerMoney -= cost;
            }
            //刷新UI
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player,playerBag.itemList);
        }
        /// <summary>
        /// 检查图纸建造所需的库存
        /// </summary>
        /// <param name="ID">图纸ID</param>
        /// <returns></returns>
        public bool CheckStock(int ID)
        {
            var bluePrintDetails = bluPrintData.GetBluPrintDetails(ID);
            foreach(var resourceItem in bluePrintDetails.resourceItem)
            {
                //玩家背包库存
                var itemStock = playerBag.GetInventoryItem(resourceItem.itemID);
                //建造素材和玩家背包库存做比较
                if (itemStock.itemAmount >= resourceItem.itemAmount)
                {
                    continue;
                }
                else return false;
            }
            return true;
        }
        /// <summary>
        /// 检查玩家是否有足够的食材
        /// </summary>
        /// <param name="ID">菜肴ID</param>
        /// <returns></returns>
        public bool CheckIngredients(int ID)
        {
            var cookedDetails = cookedData.GetCookedDetails(ID);
            //ingredientsItem所需食材，ingredientStock玩家持有食材
            foreach (var ingredientsItem in cookedDetails.cookResource)
            {
                var ingredientStock = playerBag.GetInventoryItem(ingredientsItem.itemID);

                if (ingredientStock.itemAmount >= ingredientsItem.itemAmount)
                {
                    continue;
                }
                else return false;
            }
            return true;
        }
        /// <summary>
        /// 查找箱子数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<InventoryItem> GetBoxDataList(string key)
        {
            if (boxDataDict.ContainsKey(key))
            {
                return boxDataDict[key];
            }
            return null;
        }
        /// <summary>
        /// 加入箱子数据字典
        /// </summary>
        /// <param name="box"></param>
        public void AddBoxDataDict(Box box)
        {
            var key = box.name + box.index;
            if(!boxDataDict.ContainsKey(key))
            {
                boxDataDict.Add(key, box.boxBagData.itemList);
            }
        }
        /// <summary>
        /// 在接受任务时检查背包是否已有任务物品
        /// </summary>
        /// <param name="ID"></param>
        public void CheckQuestItemInBag(int ID)
        {
            var inventoryUI = GameObject.FindWithTag("InventoryUI").gameObject.GetComponent<InventoryUI>();
            for (int i = 0; i < inventoryUI.playerSlots.Length; i++)
            {
                if (inventoryUI.playerSlots[i].itemDetails != null)
                {
                    if (inventoryUI.playerSlots[i].itemDetails.itemID == ID)
                    {
                        EventHandler.CallUpdateQuestProgressEvent(inventoryUI.playerSlots[i].itemDetails.itemID, inventoryUI.playerSlots[i].itemAmount);
                    }
                }
            }
        }
        /// <summary>
        /// 修改出售箱金额
        /// </summary>
        /// <returns></returns>
        public int ModifySellBoxValue()
        {
            int currentAllValue = 0;
            foreach (InventoryItem item in sellBoxBag.itemList)
            {
                if (item.itemID != 0)
                {
                    currentAllValue += (int)(item.itemAmount * GetItemDetails(item.itemID).itemPrice * GetItemDetails(item.itemID).sellPercentage);
                }
                
            }
            return currentAllValue;
        }
        /// <summary>
        /// 增加玩家的金钱并清空出售箱的物品
        /// </summary>
        /// <param name="value"></param>
        public void IncreasePlayerMoney(int value)
        {
            playerMoney += value;
            //清空出售箱数据
            sellBoxBag = Instantiate(sellBoxBagTemp);
            EventHandler.CallUpdateInventoryUI(InventoryLocation.SellBox, sellBoxBag.itemList);
            
        }
        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            //玩家金钱
            saveData.playerMoney = this.playerMoney;
            //存档背包当前物品
            saveData.inventoryDict = new Dictionary<string,List<InventoryItem>>();
            saveData.inventoryDict.Add(playerBag.name, playerBag.itemList);
            //遍历游戏中所有的背包和箱子，保存背包数据
            foreach(var item in boxDataDict)
            {
                saveData.inventoryDict.Add(item.Key, item.Value);
            }
            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            //拿回金钱
            this.playerMoney = saveData.playerMoney;
            //读取存档时先把玩家背包生成出来，以免报错
            playerBag = Instantiate(playerBagTemp);
            playerBag.itemList = saveData.inventoryDict[playerBag.name];
            //在存档中拿取箱子数据，而不是游戏中
            foreach(var item in saveData.inventoryDict)
            {
                if (boxDataDict.ContainsKey(item.Key))
                {
                    boxDataDict[item.Key] = item.Value;
                }
            }
            //刷新UI
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
            //玩家金币UI更新
            
        }
       
    }
}

