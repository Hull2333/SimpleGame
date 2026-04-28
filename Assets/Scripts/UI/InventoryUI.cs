using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

namespace MFarm.Inventory
{
    public class InventoryUI : MonoBehaviour    //调用在Inventory对象上
    {
        [Header("物品描述")]
        public ItemTooltip itemTooltip;
        [Header("拖拽图片")]
        public Image dragItem;
        public TextMeshProUGUI dragItemAmount;
        [Header("玩家背包UI")]
        [SerializeField] private GameObject bagUI;
        [SerializeField] private GameObject playerEquipUI;
        public GameObject[] bagSlotHolders;
        public UnityEngine.UI.Button bagSlotHoldNextPage, bagSlotHolderPreviousPage;
        //当前背包页页数
        private int currentBagSlotHolderPage = 0;
        public Text currentBagSlotHolderPageText;
        private bool bagOpened;
        [Header("通用背包")]
        [SerializeField] private GameObject baseBag;
        public GameObject shopSlotPrefab;
        public GameObject boxSlotPrefab;
        [Header("交易UI")]
        public UnityEngine.UI.Button quitShopButton;
        public TradeUI tradeUI;
        public TextMeshProUGUI playerMoneyText;
        public SlotUI[] playerSlots;
        public SlotUI[] sellBoxSlots;
        public GameObject sellBoxUI;
        public TextMeshProUGUI allValueText;
        [SerializeField] private List<SlotUI> baseBagSlots;
        //点击摇杆出售提示
        public GameObject SellTip;
        [Header("金币")]
        //金币预制体
        public GameObject coinPrefab;
        //金币生成物体的Parent
        public Transform coinParent;
        private List<GameObject> coinList = new List<GameObject>();
        private int coinNum;
        [Header("物品拾取提示")]
        //物品提示UI生成位置
        [SerializeField] private Transform itemGetBg;
        //物品提示UI
        public GameObject itemGetTipPrefab; 
        //玩家等级UI
        public GameObject skillPanel;
        [HideInInspector] public bool skillBarOpened;
        [Header("玩家面板")]
        public GameObject[] playerPanels;
        //装备框
        public SlotUI equipHeadSlot, equipBodySlot;
        //攻击防御数值
        public Text attackNumber, defenseNumber;
        [Header("装备图片列表")] 
        //装备显示图片库
        public List<EquipImage> equipImageSprites = new List<EquipImage>();
        //玩家身上装备图片
        public List<Image> equipImageList = new List<Image>();
        public Dictionary<string, Image> equipImageDict = new Dictionary<string, Image>();
        private void OnEnable()
        {
            EventHandler.UpdateInventoryUI += OnUpdateInventoryUI;
            EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
            EventHandler.BaseBagOpenEvent += OnBaseBagOpenEvent;
            EventHandler.BaseBagCloseEvent += OnBaseBagCloseEvent;
            EventHandler.ShowTradeUI += OnShowTradeUI;
            EventHandler.GetItemTipEvent += OnGetItemTipEvent;
            EventHandler.DisplayCollectItemSprite += OnDisplayCollectItemSprite;
            EventHandler.ItemSelectEvent += OnItemSelectEvent;
        }

        private void OnDisable()
        {
            EventHandler.UpdateInventoryUI -= OnUpdateInventoryUI;
            EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
            EventHandler.BaseBagOpenEvent -= OnBaseBagOpenEvent;
            EventHandler.BaseBagCloseEvent -= OnBaseBagCloseEvent;
            EventHandler.ShowTradeUI -= OnShowTradeUI;
            EventHandler.GetItemTipEvent -= OnGetItemTipEvent;
            EventHandler.DisplayCollectItemSprite -= OnDisplayCollectItemSprite;
            EventHandler.ItemSelectEvent -= OnItemSelectEvent;
        }
        /// <summary>
        /// 加载新场景后物品高亮取消
        /// </summary>
        private void OnBeforeSceneUnloadEvent()
        {
            UpdateSlotHighlight(-1);
        }
        /// <summary>
        /// 设置打开的箱子或商店
        /// </summary>
        /// <param name="slotType"></param>
        /// <param name="bag_SO"></param>
        private void OnBaseBagOpenEvent(SlotType slotType, InventoryBag_SO bagData)
        {
            //TODO:通用箱子Prefab
            GameObject prefab = slotType switch
            {
                SlotType.Shop => shopSlotPrefab,
                SlotType.Box => boxSlotPrefab,
                _ => null,
            };
           
            //生成背包UI
            baseBag.SetActive(true);
            baseBagSlots = new List<SlotUI>();
            for (int i = 0; i < bagData.itemList.Count; i++)
            {
                //根据指定数量生成背包格
                var slot = Instantiate(prefab, baseBag.transform.GetChild(0)).GetComponent<SlotUI>();
                //将背包格序号与背包匹配
                slot.slotIndex = i;
                //再将匹配好的slot添加到baseBag列表中
                baseBagSlots.Add(slot);
            }
            //强制重建背包格局，使每次增加物品格时背包显示正常
            LayoutRebuilder.ForceRebuildLayoutImmediate(baseBag.GetComponent<RectTransform>());
            sellBoxUI.gameObject.SetActive(true);
            SellTip.SetActive(false);
            //打开商店时也顺便打开玩家背包,并重新设置背包位置
            if (slotType == SlotType.Shop)
            {
                bagUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(449.2f, 199.2f);
                bagUI.SetActive(true);
                bagOpened = true;
            }
            //更新UI显示
            OnUpdateInventoryUI(InventoryLocation.Box, bagData.itemList);

        }
        /// <summary>
        /// 商店或箱子关闭事件
        /// </summary>
        /// <param name="slotType"></param>
        /// <param name="bagData"></param>
        private void OnBaseBagCloseEvent(SlotType slotType, InventoryBag_SO bagData)
        {
            InventoryManager.Instance.isSellState = false;
            baseBag.SetActive(false);
            itemTooltip.gameObject.SetActive(false);
            UpdateSlotHighlight(-1);
            
            foreach (var slot in baseBagSlots)
            {
                Destroy(slot.gameObject);
            }
            baseBagSlots.Clear();
            sellBoxUI.gameObject.SetActive(false);
            //关闭商店后背包一起关闭,还原背包位置
            if (slotType == SlotType.Shop)
            {
                bagUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(385.2f, 197);
                bagUI.SetActive(false);
                bagOpened = false;
            }
        }
        /// <summary>
        /// 显示交易界面
        /// </summary>
        /// <param name="item"></param>
        /// <param name="isSell"></param>
        private void OnShowTradeUI(ItemDetails item, bool isSell,int maxAmount,int bagIndex,int sellIndex,InventoryLocation startLocation , InventoryLocation endLocation , bool isToSellBox)
        {
            //物品和目的物品不是用一种就不执行交易UI显示
            if (sellBoxSlots[sellIndex].itemDetails != null)
            {
                if (sellBoxSlots[sellIndex].itemDetails.itemID != playerSlots[bagIndex].itemDetails.itemID)
                {
                    return;
                }
            }
            tradeUI.gameObject.SetActive(true);
            tradeUI.SetupTradeUI(item, isSell, maxAmount, bagIndex, sellIndex,startLocation,endLocation, isToSellBox);
        }
        /// <summary>
        /// 将刚拾取到的物品添加到ItemTipList列表中
        /// </summary>
        /// <param name="getItemDetails"></param>
        private void OnGetItemTipEvent(ItemDetails getItemDetails)
        {
            if (getItemDetails != null)
            {
                //itemGetBg下的子物体
                ItemGetTipUI[] itemGetTipUIs;
                itemGetTipUIs = itemGetBg.GetComponentsInChildren<ItemGetTipUI>();
                //没有拾取提示时
                if(itemGetTipUIs.Length <= 0)
                {
                    GameObject getItem = Instantiate(itemGetTipPrefab, itemGetBg);
                    getItem.GetComponent<ItemGetTipUI>().ModifyItemParameter(getItemDetails);
                }
                //有拾取提示时
                else
                {
                    //短时间内捡到相同的物品时
                    if (FindItemGetTipUIs(getItemDetails, itemGetTipUIs))
                    {
                        foreach (var tip in itemGetTipUIs)
                        {
                            if (tip.itemID == getItemDetails.itemID)
                            {
                                tip.ModifyItemAmount();
                            }
                        }
                    }
                    //捡到不同物品时
                    else
                    {
                        GameObject getItem = Instantiate(itemGetTipPrefab, itemGetBg);
                        getItem.GetComponent<ItemGetTipUI>().ModifyItemParameter(getItemDetails);
                    }
                }
                //currentGetItemID = getItemDetails.itemID;
                ////根据是否重复来判断是在添加到列表中还是添加重复物品的数量
                //if (ItemListConditons(getItemDetails) == -1)
                //{
                //    itemGetTipList.Add(getItemDetails);
                //    Instantiate(itemGetTipPrefab, itemGetBg.transform);
                //    itemGetTipPrefab.transform.GetChild(0).GetComponent<Image>().sprite = getItemDetails.itemIcon;
                //    itemGetTipPrefab.transform.GetChild(1).GetComponent<Text>().text = "X" + " " + "1";
                //}
                ////若拾取的物品正显示在游戏界面上，只修改物品的拾取数量
                //else
                //{
                //    //获取正显示的物品所在的拾取物品列表的index
                //    int index = ItemListConditons(getItemDetails);
                //    int amountText = itemGetTipList[index].tipAmount++;
                //    foreach(var tip in itemGetBg.GetComponentsInChildren<ItemGetTipUI>())
                //    {
                //        if(tip.itemID == getItemDetails.itemID)
                //        {
                //            tip.ModifyItemAmount(amountText + 1);

                //        }
                //    }
                //}
            }
        }
        /// <summary>
        /// 查找itemGetTipUIs下的相同提示
        /// </summary>
        /// <param name="newTip"></param>
        /// <returns></returns>
        private bool FindItemGetTipUIs(ItemDetails newTip, ItemGetTipUI[] array)
        {
            foreach (var tip in array)
            {
                if (tip.itemID == newTip.itemID)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 收获农作物之后取消所有物品的选取
        /// </summary>
        /// <param name="ID"></param>
        private void OnDisplayCollectItemSprite(int ID)
        {
            UpdateSlotHighlight(-1);
            
        }
        private void OnItemSelectEvent(ItemDetails details, bool arg2)
        {
            UpdatePlayerAttackNum(details);
        }
        private void Start()
        {
            //给每个背包格子赋值序号
            for (int i = 0; i < playerSlots.Length; i++)
            {
                playerSlots[i].slotIndex = i;
            }
            //游戏开始时设置Player Bag 是否显示
            bagOpened = bagUI.activeInHierarchy;
            //初始化玩家金钱
            playerMoneyText.text = InventoryManager.Instance.playerMoney.ToString();
            itemGetBg = GameObject.FindGameObjectWithTag("ItemGetBg").transform;
            foreach (Image image in equipImageList)
            {
                equipImageDict.Add(image.name, image);
            }
            quitShopButton.onClick.AddListener(ClickQuitShopButton);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                OpenBagUI();
            }

        }
        /// <summary>
        /// 根据现有背包中的物品数量来增加拾取的物品
        /// </summary>
        /// <param name="location">更新哪个位置的背包</param>
        /// <param name="list"></param>
        private void OnUpdateInventoryUI(InventoryLocation location, List<InventoryItem> list)
        {
            switch (location)
            {
                case InventoryLocation.Player:
                    for (int i = 0; i < playerSlots.Length; i++)
                    {
                        if (list[i].itemAmount > 0)
                        {
                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                            playerSlots[i].UpdateSlot(item, list[i].itemAmount);
                        }
                        else
                        {
                            playerSlots[i].UpdateEmptySlot();
                        }
                    }
                    break;
                //Shop和Box属于同一类型
                case InventoryLocation.Box:
                    for (int i = 0; i < baseBagSlots.Count; i++)
                    {
                        if (list[i].itemAmount > 0)
                        {
                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                            baseBagSlots[i].UpdateSlot(item, list[i].itemAmount);
                        }
                        else
                        {
                            baseBagSlots[i].UpdateEmptySlot();
                        }
                    }
                    break;
                case InventoryLocation.SellBox:
                    for (int i = 0; i < sellBoxSlots.Length; i++)
                    {
                        if (list[i].itemAmount > 0)
                        {
                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                            sellBoxSlots[i].UpdateSlot(item, list[i].itemAmount);
                        }
                        else
                        {
                            sellBoxSlots[i].UpdateEmptySlot();
                        }
                    }
                    //修改出售箱显示的总价值
                    allValueText.text = InventoryManager.Instance.ModifySellBoxValue().ToString();
                    //根据出售箱金额来显示出售提示
                    if(InventoryManager.Instance.ModifySellBoxValue() > 0)
                    {
                        SellTip.SetActive(true);
                    }
                    else
                    {
                        SellTip.SetActive(false);
                    }
                    break;
                    
            }
            //更新玩家金钱
            playerMoneyText.text = InventoryManager.Instance.playerMoney.ToString();
            
        }
        /// <summary>
        /// 控制背包UI的开关
        /// </summary>
        public void OpenBagUI()
        {
            //实现Bag Button可以反复点按
            bagOpened = !bagOpened;
            if (bagOpened)
            {
                //Time.timeScale = 0f;
                EventHandler.CallUpdateGameStateEvent(GameState.Pause);
                //每一次背包打开都是默认显示背包栏
                playerPanels[0].SetActive(true);
                SwitchBagSlotHolderPageButton();
            }
            else
            {
                foreach (var panel in playerPanels)
                {
                    panel.gameObject.SetActive(false);
                }
                //每次关闭背包UI时，重新检测一下Player动画并取消举起动画
                EventHandler.CallEquipSlotEvent(equipHeadSlot, equipBodySlot);
                EventHandler.CallItemSelectEvent(InventoryManager.Instance.currentSelectedItem, false);
                //Time.timeScale = 1f;
                EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
                //关闭背包UI同时也关闭道具介绍UI
                itemTooltip.gameObject.SetActive(false);
            }
            playerEquipUI.SetActive(bagOpened);
            EventHandler.CallItemUselessEvent(bagOpened);          
        }

        /// <summary>
        /// 控制背包格选中高亮的显示和消失
        /// </summary>
        /// <param name="index">OnPointerClick方法传进来的slotIndex</param>
        public void UpdateSlotHighlight(int index)
        {
            //快速遍历playerSlots中每一项，声明为slot
            foreach (var slot in playerSlots)
            {
                if (slot.isSelected && slot.slotIndex == index)
                {
                    slot.slotHighlight.gameObject.SetActive(true);
                }
                else
                {
                    //其他没被选中的slot要取消选中状态以及不显示高亮
                    slot.isSelected = false;
                    slot.slotHighlight.gameObject.SetActive(false);
                }
            }
        }
        /// <summary>
        /// 开启玩家相关的各个面板
        /// </summary>
        /// <param name="index"></param>
        public void SwitchPlayerPanel(int index)
        {
            EventHandler.CallUpdateGameStateEvent(GameState.Pause);
            EventHandler.CallItemUselessEvent(true);
            for (int i = 0; i < playerPanels.Length; i++)
            {
                if (i == index)
                {
                    //开启玩家等级面板时
                    if(index == 1)
                    {
                        skillPanel.GetComponent<SkillBar>().UpdateSkillBar();
                    }
                    //开启任务面板时
                    if(index == 2)
                    {
                        QuestUI.Instance.SetUpQuestList();
                        QuestUI.Instance.ResetQuestListPage();
                    }
                    playerPanels[i].gameObject.SetActive(true);
                    
                }
                else
                {
                    playerPanels[i].gameObject.SetActive(false);
                }
            }
        }
        /// <summary>
        /// 玩家面板关闭按钮
        /// </summary>
        public void PlayerPanelsQuit()
        {
            foreach (var panel in playerPanels)
            {
                panel.gameObject.SetActive(false);
            }
            playerEquipUI.SetActive(bagOpened);
            EventHandler.CallItemUselessEvent(false);
            //每次关闭背包UI时，重新检测一下Player动画并取消举起动画
            EventHandler.CallEquipSlotEvent(equipHeadSlot, equipBodySlot);
            EventHandler.CallItemSelectEvent(InventoryManager.Instance.currentSelectedItem, false);
            bagOpened = false;
            //Time.timeScale = 1f;
            EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
            EventHandler.CallItemUselessEvent(false);
            playerEquipUI.SetActive(false);
        }
        /// <summary>
        /// 点击玩家背包下一页
        /// </summary>
        public void BagSlotHolderNextPage()
        {
            bagSlotHolders[currentBagSlotHolderPage].SetActive(false);
            bagSlotHolders[currentBagSlotHolderPage + 1].SetActive(true);
            currentBagSlotHolderPage ++;
            currentBagSlotHolderPageText.text = (currentBagSlotHolderPage + 1).ToString();
            SwitchBagSlotHolderPageButton();
        }
        /// <summary>
        /// 点击玩家背包上一页
        /// </summary>
        public void BagSlotHolderPreviousPage()
        {
            bagSlotHolders[currentBagSlotHolderPage].SetActive(false);
            bagSlotHolders[currentBagSlotHolderPage - 1].SetActive(true);
            currentBagSlotHolderPage --;
            currentBagSlotHolderPageText.text = (currentBagSlotHolderPage + 1).ToString();
            SwitchBagSlotHolderPageButton();
        }
        /// <summary>
        /// 检测背包翻上下页按钮的开关
        /// </summary>
        public void SwitchBagSlotHolderPageButton()
        {
            if (currentBagSlotHolderPage + 1 > bagSlotHolders.Length - 1)
            {
                bagSlotHoldNextPage.interactable = false;
            }
            else
            {
                bagSlotHoldNextPage.interactable = true;
            }
            if (currentBagSlotHolderPage == 0)
            {
                bagSlotHolderPreviousPage.interactable = false;
            }
            else
            {
                bagSlotHolderPreviousPage.interactable = true;
            }
        }
        /// <summary>
        /// 显示玩家身上的装备图片
        /// </summary>
        /// <param name="itemDetails"></param>
        public void SetEquipImage(ItemDetails itemDetails,SlotUI targetSlot)
        {
            if (itemDetails != null)
            {
                //装备头部itemDetails装备时
                if (targetSlot.slotType == SlotType.Equipment_Head)
                {
                    //搜索装备显示图片库
                    foreach (EquipImage s in equipImageSprites)
                    {
                        if (s.itemID == itemDetails.itemID && s.equipPartName == PartName.Head)
                        {
                            equipImageDict["Head"].sprite = s.equipSprite;
                        }
                    }

                }
                //装备身体装备时
                else
                {
                    //搜索装备显示图片库
                    foreach (EquipImage s in equipImageSprites)
                    {
                        if (s.itemID == itemDetails.itemID)
                        {
                            if(s.equipPartName == PartName.Arm)
                            {
                                equipImageDict["Arm"].sprite = s.equipSprite;
                            }
                            if (s.equipPartName == PartName.Body)
                            {
                                equipImageDict["Body"].sprite = s.equipSprite;
                            }
                        } 
                    }
                }
            }
            
        }
        /// <summary>
        /// 恢复默认装扮
        /// </summary>
        /// <param name="currentSlot"></param>
        public void ResetEquipImage(SlotType currentSlot)
        {
            if(currentSlot == SlotType.Equipment_Head)
            {
                equipImageDict["Head"].sprite = equipImageSprites[0].equipSprite;
            }
            else
            {
                equipImageDict["Arm"].sprite = equipImageSprites[1].equipSprite;
                equipImageDict["Body"].sprite = equipImageSprites[2].equipSprite;
            }
        }
        /// <summary>
        /// 更新显示玩家攻击数值
        /// </summary>
        /// <param name="itemDetail"></param>
        public void UpdatePlayerAttackNum(ItemDetails itemDetail)
        {
            if(itemDetail.itemType == ItemType.Sword)
            {
                int maxNum = 0;
                int minNum = 0;
                maxNum = itemDetail.maxValue;
                minNum = itemDetail.minValue;
                attackNumber.text = minNum + " ~ " + maxNum;
            }
            else
            {
                attackNumber.text = 0.ToString();
            }
        }
        /// <summary>
        /// 更新显示玩家防御数值
        /// </summary>
        public void UpdatePlayerDefenseNum()
        {
            int num = 0;
            //Debug.Log(equipHeadSlot.itemDetails.itemID);
            //Debug.Log(equipBodySlot.itemDetails.itemID);
            if (equipHeadSlot.itemDetails != null)
            {
                num += equipHeadSlot.itemDetails.maxValue;
            }
            if (equipBodySlot.itemDetails != null)
            {
                num += equipBodySlot.itemDetails.maxValue;
            }
            //更新玩家防御力
            EventHandler.CallEquipArmorEven(num);
            defenseNumber.text = num.ToString();
        }
        /// <summary>
        /// 按下sellButton开始出售,调用在sellButton按钮上
        /// </summary>
        public void ClickSellButton()
        {
            if(InventoryManager.Instance.ModifySellBoxValue() > 0)
            {
                GenerateCoins();
                MakeTween();
            }
            InventoryManager.Instance.IncreasePlayerMoney(InventoryManager.Instance.ModifySellBoxValue());
            playerMoneyText.text = InventoryManager.Instance.playerMoney.ToString();
        }
        /// <summary>
        /// 点击商店关闭按钮
        /// </summary>
        public void ClickQuitShopButton()
        {
            EventHandler.CallBaseBagCloseEvent(SlotType.Shop, null);
            EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
        }
        /// <summary>
        ///  获取对方箱子的已有物品index或者空的格子index
        /// </summary>
        /// <param name="currentItemID">当前物品</param>
        /// <param name="lists">对方箱子的InventoryItem队列</param>
        /// <param name="location">自身的箱子位置</param>
        /// <returns></returns>
        public int GetEmptySellBoxIndex(int currentItemID,List<InventoryItem> lists,InventoryLocation location)
        {
            int index = -1;
            index = InventoryManager.Instance.GetItemIndexInBag(currentItemID, lists);
           
            if(location == InventoryLocation.Player)
            {
                //出售箱没有这个物品
                if (index == -1)
                {
                    foreach (var slot in sellBoxSlots)
                    {
                        if (slot.itemDetails == null)
                        {
                            index = slot.slotIndex;
                            return index;
                        }
                    }
                }
                //有这个物品
                else
                {
                    return index;
                }
            }
            if(location == InventoryLocation.SellBox)
            {
                //没有这个物品 
                if (index == -1)
                {
                    foreach (var slot in playerSlots)
                    {
                        Debug.Log(slot.slotIndex);
                        if (slot.itemDetails == null)
                        {
                            index = slot.slotIndex;
                            return index;
                        }
                    }
                }
                //有这个物品
                else
                {
                    return index;
                }
            }
            return index;
        }
        /// <summary>
        /// 实现金币划到玩家金钱UI上
        /// </summary>
        private void MakeTween()
        {
            float delay = 0f;
           
            for (int i = 0; i < coinNum; i++)
            {
                Vector3 originPos = new Vector3(UnityEngine.Random.Range(-50, 50), (UnityEngine.Random.Range(-40, 40)));
                coinList[i].GetComponent<CoinTween>().PlayTween(delay, originPos);
                delay += 0.1f;
            }
        }
        /// <summary>
        /// 生成金币
        /// </summary>
        private void GenerateCoins()
        {
            coinList.Clear();
            //金币生成个数
            int num = 1;
            if(InventoryManager.Instance.ModifySellBoxValue() > 50)
            {
                num = 2;
            }
            if (InventoryManager.Instance.ModifySellBoxValue() > 100)
            {
                num = 3;
            }
            if (InventoryManager.Instance.ModifySellBoxValue() > 300)
            {
                num = 10;
            }
            if(InventoryManager.Instance.ModifySellBoxValue() > 500)
            {
                num = 5;
            }
            for (int i = 0; i < num; i++)
            {
                GameObject coin = Instantiate(coinPrefab, Vector3.zero, Quaternion.identity, coinParent);
                //金币开始生成的位置
                //coin.transform.localPosition = new Vector3(UnityEngine.Random.Range(-50, 50), (UnityEngine.Random.Range(-40, 40)));\
                coin.transform.localPosition = Vector3.zero;
                coinList.Add(coin);
            }
            coinNum = num;
        }
    }
}


