using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using static AnimalData_SO;
namespace MFarm.Inventory
{
    public class InventoryUI : Singleton<InventoryUI>    //调用在Inventory对象上
    {
        [Header("物品描述")]
        public ItemTooltip itemTooltip;
        [Header("拖拽图片")]
        public Image dragItem;
        public TextMeshProUGUI dragItemAmount;
        [Header("玩家背包UI")]
        [SerializeField] private GameObject bagUI;
        [SerializeField] private GameObject playerEquipUI;
        //当前是否可以打开玩家背包
        [HideInInspector] public bool canOpenBag = true;
        public GameObject activeBagBar;
        public GameObject bagButton;
        private bool bagOpened;
        [Header("通用背包")]
        [SerializeField] private GameObject baseBag;
        //玩家背包UI中的切换面板按钮
        public GameObject[] playerBagSupButtons;
        public GameObject shopSlotPrefab;
        public GameObject boxSlotPrefab;
        //鱼饵SlotUI动画
        public Animator baitBarAnim;
        //鱼饵信息
        public SlotUI baitItemSlot;
        [Header("交易数量选择UI")]
        public GameObject tradeUIPanel;
        public TradeUI tradeUI;
        //还没卖完显示提醒UI
        public GameObject haveItemUIPanel;
        public Button yesButton;
        public Button noButton;
        public TextMeshProUGUI playerMoneyText;
        public Animator playerMoneyUIAnim;
        public SlotUI[] playerSlots;
        public SlotUI[] sellBoxSlots;
        [Header("NPC商店UI")]
        public GameObject shopSlotHolder;
        public Button quitShopButton;
        public Button shopButton;
        [Header("NPC出售箱UI")]
        public GameObject sellBoxSlotHolder;
        public Button sellBoxButton;
        public GameObject sellJoy;
        public GameObject allValue;
        public TextMeshProUGUI allValueText;
        //提示还有东西没卖掉
        public GameObject sellHaveItemTipPrefab;
        //点击摇杆出售提示
        public GameObject sellTip;
        [SerializeField] private List<SlotUI> baseBagSlots;
        //出售箱中残存的物品
        private List<InventoryItem> haveSellBoxItemList = new List<InventoryItem>();
        [Header("金币")]
        private RectTransform coinRectTran;
        //金币预制体
        public GameObject coinPrefab;
        //金币生成物体的Parent
        public Transform coinParent;
        private List<GameObject> coinList = new List<GameObject>();
        private int coinNum;
        //金币的终点
        public Transform coinTargetPos;
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
        [Header("好感度")]
        public Image[] friendlinessImages;
        [Header("建造UI")]
        public SlotUI_Build buildSlotUI;
        public Transform buildSlotUIParent;
        public GameObject buildShopPanel;
        public GameObject buildModePanel;
        public Button buildModeExitButton;
        public Button animalModeExitButton;
        public Button buildingShopExitButton;
        //提示材料不足
        public GameObject resourceLock;
        [Header("动物商店")]
        public GameObject animalShopUI;
        public Transform animalSlotParent;
        public AnimalSlotUI animalShopSlotUI;
        public Button quitAnimalShopButton;
        //询问购买动物数量UI
        public GameObject animalAskUI;
        public Image askImage;
        public TextMeshProUGUI askAmountText;
        public Button increaseAmountButton;
        public Button decreaseAmountButton;
        public Button quitAskUIButton;
        //确定数量按钮
        public Button commitAmountButton;
        //当前选择的数量
        private int currentAskAmount;
        
        private void OnEnable()
        {
            EventHandler.UpdateInventoryUI += OnUpdateInventoryUI;
            EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
            EventHandler.BaseBagOpenEvent += OnBaseBagOpenEvent;
            EventHandler.BaseBagCloseEvent += OnBaseBagCloseEvent;
            EventHandler.ShowTradeUI += OnShowTradeUI;
            EventHandler.GetItemTipEvent += OnGetItemTipEvent;
            EventHandler.DisplayCollectItemSprite += OnDisplayCollectItemSprite;
            EventHandler.ItemSelectEvent += OnItemSelectEvent;
            EventHandler.DoTweenPlayerMoneyChageEvent += OnDoTweenPlayerMoneyChageEvent;
            EventHandler.UpdateNPCFriendlinessUIPanel += OnUpdateNPCFriendlinessUIPanel;
            EventHandler.ControlPlayerBagOpen += OnControlPlayerBagOpen;
            EventHandler.BuildindModeEvent += OnBuildindModeEvent;
            EventHandler.OpenBuildShopEvent += OnOpenBuildShopEvent;
            EventHandler.OpenAnimalShopEvent += OnOpenAnimalShopEvent;
        }

        private void OnDisable()
        {
            EventHandler.UpdateInventoryUI -= OnUpdateInventoryUI;
            EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
            EventHandler.BaseBagOpenEvent -= OnBaseBagOpenEvent;
            EventHandler.BaseBagCloseEvent -= OnBaseBagCloseEvent;
            EventHandler.ShowTradeUI -= OnShowTradeUI;
            EventHandler.GetItemTipEvent -= OnGetItemTipEvent;
            EventHandler.DisplayCollectItemSprite -= OnDisplayCollectItemSprite;
            EventHandler.ItemSelectEvent -= OnItemSelectEvent;
            EventHandler.DoTweenPlayerMoneyChageEvent -= OnDoTweenPlayerMoneyChageEvent;
            EventHandler.UpdateNPCFriendlinessUIPanel -= OnUpdateNPCFriendlinessUIPanel;
            EventHandler.ControlPlayerBagOpen -= OnControlPlayerBagOpen;
            EventHandler.BuildindModeEvent -= OnBuildindModeEvent;
            EventHandler.OpenBuildShopEvent -= OnOpenBuildShopEvent;
            EventHandler.OpenAnimalShopEvent -= OnOpenAnimalShopEvent;
        }

     



        /// <summary>
        /// 加载新场景后物品高亮取消
        /// </summary>
        private void OnBeforeSceneUnloadEvent()
        {
           
            UpdateSlotHighlight(-1);
        }
        private void OnAfterSceneLoadedEvent()
        {
            //初始化玩家金钱
            playerMoneyText.text = InventoryManager.Instance.playerMoney.ToString();
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
            //显示背包UI
            baseBag.SetActive(true);
            foreach (var button in playerBagSupButtons)
            {
                button.SetActive(false);
            }
            ClickShopSwitchButton();
            InventoryManager.Instance.anyBagOpened = true;
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
            //打开商店时也顺便打开玩家背包,并重新设置背包位置
            if (slotType == SlotType.Shop)
            {
                bagUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(449.2f, 199.2f);
                bagUI.SetActive(true);
                bagOpened = true;
            }
            EventHandler.CallRestoreNormalCursorImageEvent();
            //更新UI显示
            OnUpdateInventoryUI(InventoryLocation.Box, bagData.itemList,0);

        }
        /// <summary>
        /// 商店或箱子关闭事件
        /// </summary>
        /// <param name="slotType"></param>
        /// <param name="bagData"></param>
        private void OnBaseBagCloseEvent(SlotType slotType, InventoryBag_SO bagData)
        {
            InventoryManager.Instance.anyBagOpened = false;
            InventoryManager.Instance.isSellState = false;
            foreach (var button in playerBagSupButtons)
            {
                button.SetActive(true);
            }
            baseBag.SetActive(false);
            //取消当前选中的物品
            InventoryManager.Instance.currentSelectedItem = null;
            EventHandler.CallItemSelectEvent(InventoryManager.Instance.currentSelectedItem, false);
            itemTooltip.gameObject.SetActive(false);
            UpdateSlotHighlight(-1);
            foreach (var slot in baseBagSlots)
            {
                Destroy(slot.gameObject);
            }
            baseBagSlots.Clear();
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
        private void OnShowTradeUI(ItemDetails item, bool isSell,int maxAmount,int fromIndex,int toIndex,InventoryLocation startLocation , InventoryLocation endLocation , bool isToSellBox)
        {
            if(toIndex < sellBoxSlots.Length)
            {
                //物品和目的物品不是用一种就不执行交易UI显示
                if (sellBoxSlots[toIndex].itemDetails != null && isSell)
                {
                    if (sellBoxSlots[toIndex].itemDetails.itemID != playerSlots[fromIndex].itemDetails.itemID)
                    {
                        return;
                    }
                }
            }
            tradeUIPanel.SetActive(true);
            tradeUI.SetupTradeUI(item, isSell, maxAmount, fromIndex, toIndex,startLocation,endLocation, isToSellBox);
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
                //    itemGetTipPrefab.transform.GetChild(1).GetComponent<Text>().recipeName = "X" + " " + "1";
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
        /// 根据现有背包中的物品数量来增加拾取的物品
        /// </summary>
        /// <param name="location">更新哪个位置的背包</param>
        /// <param name="list"></param>
        private void OnUpdateInventoryUI(InventoryLocation location, List<InventoryItem> list, int diffence)
        {
            switch (location)
            {
                case InventoryLocation.Player:
                    for (int i = 0; i < playerSlots.Length; i++)
                    {
                        if (list[i].itemAmount > 0)
                        {
                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                            playerSlots[i].UpdateSlot(item, list[i].itemAmount,false,0);
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
                            baseBagSlots[i].UpdateSlot(item, list[i].itemAmount,false,0);
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
                            sellBoxSlots[i].UpdateSlot(item, list[i].itemAmount,false,0);
                        }
                        else
                        {
                            sellBoxSlots[i].UpdateEmptySlot();
                        }
                    }
                    //修改出售箱显示的总价值
                    allValueText.text = InventoryManager.Instance.ModifySellBoxValue().ToString();
                    //根据出售箱金额来显示出售提示
                    if (InventoryManager.Instance.ModifySellBoxValue() > 0)
                    {
                        sellTip.SetActive(true);
                    }
                    else
                    {
                        sellTip.SetActive(false);
                    }
                    break;
            }
            if (diffence != 0)
            {
                EventHandler.CallDoTweenPlayerMoneyChageEvent(InventoryManager.Instance.playerMoney, diffence);
            }
        }
        /// <summary>
        /// 修改玩家金钱并实现玩家金钱“流动”的变化
        /// </summary>
        /// <param name="fromValue"></param>
        /// <param name="difference"></param>
        public void OnDoTweenPlayerMoneyChageEvent(int fromValue, int difference)
        {
            //金钱变化前的值
            int originValue = fromValue;
            DOTween.To(
            () => fromValue,
            x =>
            {
                fromValue = x;
                playerMoneyText.text = Mathf.RoundToInt(x).ToString();
            },
            fromValue + difference,
            0.5f
            ).OnComplete(() =>
            {
                InventoryManager.Instance.playerMoney = originValue + difference;
            });
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
        private void OnItemSelectEvent(ItemDetails details, bool isSelected)
        {
            if(details != null)
            {
                UpdatePlayerAttackNum(details);
                if (!isSelected)
                {
                    baitBarAnim.SetBool("appear", false);
                }
                else
                {
                    if(details.itemType == ItemType.FishingRod)
                    {
                        baitBarAnim.SetBool("appear", true);
                    }
                    else
                    {
                        baitBarAnim.SetBool("appear", false);
                    }
                }
            }
            else
            {
                baitBarAnim.SetBool("appear", false);
            }
           
        }
        private void OnUpdateNPCFriendlinessUIPanel(string name, float value)
        {
            
            for (int x = 0; x < friendlinessImages.Length; x++)
            {
                if (friendlinessImages[x].gameObject.name == name)
                {
                    friendlinessImages[x].fillAmount = value / 30f;
                }
            }
        }
        private void OnControlPlayerBagOpen(bool canOpen)
        {
            canOpenBag = canOpen;
        }
        private void OnBuildindModeEvent(BuildingDetails build,AnimalDetails animal, bool startMode)
        {
            if(startMode == true)
            {
                buildModePanel.SetActive(true);
                buildShopPanel.SetActive(false);
                animalShopUI.SetActive(false);
                if (build != null)
                {
                    buildModeExitButton.gameObject.SetActive(true);
                    animalModeExitButton.gameObject.SetActive(false);
                    return;
                }
                if (animal != null)
                {
                    buildModeExitButton.gameObject.SetActive(false);
                    animalModeExitButton.gameObject.SetActive(true);
                    return;
                }
            }
            else
            {
                buildModePanel.SetActive(false);
            }
            playerMoneyUIAnim.gameObject.SetActive(!startMode);
            activeBagBar.SetActive(!startMode);
            bagButton.SetActive(!startMode);
        }
        private void OnOpenBuildShopEvent(BuildingBagData_SO buildShop)
        {
            buildShopPanel.SetActive(true);
            //清空buildSlotUIParent下的子物体
            for (int i = 0; i < buildSlotUIParent.childCount; i++)
            {
                Destroy(buildSlotUIParent.GetChild(i).gameObject);
            }
            //生成对应的可造建筑
            for (int i = 0; i < buildShop.buildingList.Count; i++)
            {
                var buildingSlot = Instantiate(buildSlotUI, buildSlotUIParent);
                buildingSlot.buildID = buildShop.buildingList[i];
                buildingSlot.UpdateBuildSlotMessage();
            }
        }
        private void OnOpenAnimalShopEvent(AnimalBagData_SO animalBag)
        {
            animalShopUI.SetActive(true);
            for (int i = 0; i < animalSlotParent.childCount; i++)
            {
                Destroy(animalSlotParent.GetChild(i).gameObject);
            }
            for (int i = 0; i < animalBag.animalList.Count; i++)
            {
                var animalSlot = Instantiate(animalShopSlotUI, animalSlotParent);
                animalSlot.UpdateAnimalSlotUI(animalBag.animalList[i]);
            }
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
            itemGetBg = GameObject.FindGameObjectWithTag("ItemGetBg").transform;
            foreach (Image image in equipImageList)
            {
                equipImageDict.Add(image.name, image);
            }
           shopButton.onClick.AddListener(ClickShopSwitchButton);
            sellBoxButton.onClick.AddListener(ClickSellSwitchButton);
            quitShopButton.onClick.AddListener(ClickQuitShopButton);
            yesButton.onClick.AddListener(ClickYesButton);
            noButton.onClick.AddListener(ClickNoButton);
            buildModeExitButton.onClick.AddListener(ExitBuildMode);
            animalModeExitButton.onClick.AddListener(ExitAnimalMode);
            buildingShopExitButton.onClick.AddListener(QuitBuildingShopUI);
            increaseAmountButton.onClick.AddListener(ClickAnimalIncreaseButton);
            decreaseAmountButton.onClick.AddListener(ClickAnimalDecreaseButton);
            quitAskUIButton.onClick.AddListener(QuitAnimalAskUI);
            quitAnimalShopButton.onClick.AddListener(QuitAnimalShopUI);
            commitAmountButton.onClick.AddListener(ClickAnimalCommitAmountButton);
            coinParent.GetComponentInParent<Canvas>().worldCamera = Camera.main;
            coinParent.GetComponentInParent<Canvas>().sortingLayerName = "ValueTile";
        }
      
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B) )
            {
                OpenBagUI();
            }
        }
       
        /// <summary>
        /// 控制背包UI的开关
        /// </summary>
        public void OpenBagUI()
        {
            if (canOpenBag)
            {
                //实现Bag Button可以反复点按
                bagOpened = !bagOpened;
                InventoryManager.Instance.anyBagOpened = bagOpened;

                if (bagOpened)
                {
                    //Time.timeScale = 0f;
                    EventHandler.CallUpdateGameStateEvent(GameState.Pause);
                    //每一次背包打开都是默认显示背包栏
                    playerPanels[0].SetActive(true);
                    //SwitchBagSlotHolderPageButton();
                }
                else
                {
                    //除了ActionBar的物品外，其他被选择后要进行取消举手动作，把当前的物品设置为null
                    for (int i = 0; i < playerSlots.Length - 9; i++)
                    {
                        if (playerSlots[i].isSelected)
                        {
                            UpdateSlotHighlight(-1);
                            InventoryManager.Instance.currentSelectedItem = null;
                            //每次关闭背包UI时，重新检测一下Player动画并取消举起动画
                            EventHandler.CallItemSelectEvent(InventoryManager.Instance.currentSelectedItem, false);
                        }
                    }
                    foreach (var panel in playerPanels)
                    {
                        panel.gameObject.SetActive(false);
                    }
                    EventHandler.CallEquipSlotEvent(equipHeadSlot, equipBodySlot);
                    //Time.timeScale = 1f;
                    EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
                    //关闭背包UI同时也关闭道具介绍UI
                    itemTooltip.gameObject.SetActive(false);
                }
                playerEquipUI.SetActive(bagOpened);
                EventHandler.CallItemUselessEvent(bagOpened);
                EventHandler.CallRestoreNormalCursorImageEvent();
            }
           
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
        /// 开启玩家相关的各个面板,调用在玩家背包UI的选择按钮上
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
        /// 玩家面板关闭按钮,调用在PlayerEquipUI下的QuitButton按钮上
        /// </summary>
        public void PlayerPanelsQuit()
        {
            foreach (var panel in playerPanels)
            {
                panel.gameObject.SetActive(false);
            }
            playerEquipUI.SetActive(bagOpened);
            InventoryManager.Instance.anyBagOpened = false;
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
        ///// <summary>
        ///// 点击玩家背包下一页
        ///// </summary>
        //public void BagSlotHolderNextPage()
        //{
        //    bagSlotHolders[currentBagSlotHolderPage].SetActive(false);
        //    bagSlotHolders[currentBagSlotHolderPage + 1].SetActive(true);
        //    currentBagSlotHolderPage ++;
        //    currentBagSlotHolderPageText.recipeName = (currentBagSlotHolderPage + 1).ToString();
        //    SwitchBagSlotHolderPageButton();
        //}
        ///// <summary>
        ///// 点击玩家背包上一页
        ///// </summary>
        //public void BagSlotHolderPreviousPage()
        //{
        //    bagSlotHolders[currentBagSlotHolderPage].SetActive(false);
        //    bagSlotHolders[currentBagSlotHolderPage - 1].SetActive(true);
        //    currentBagSlotHolderPage --;
        //    currentBagSlotHolderPageText.recipeName = (currentBagSlotHolderPage + 1).ToString();
        //    SwitchBagSlotHolderPageButton();
        //}
        ///// <summary>
        ///// 检测背包翻上下页按钮的开关
        ///// </summary>
        //public void SwitchBagSlotHolderPageButton()
        //{
        //    if (currentBagSlotHolderPage + 1 > bagSlotHolders.Length - 1)
        //    {
        //        bagSlotHoldNextPage.interactable = false;
        //    }
        //    else
        //    {
        //        bagSlotHoldNextPage.interactable = true;
        //    }
        //    if (currentBagSlotHolderPage == 0)
        //    {
        //        bagSlotHolderPreviousPage.interactable = false;
        //    }
        //    else
        //    {
        //        bagSlotHolderPreviousPage.interactable = true;
        //    }
        //}
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
            //优先刷新一下sellBox的UI
            if(InventoryManager.Instance.ModifySellBoxValue() > 0)
            {
                for (int i = 0; i < sellBoxSlots.Length; i++)
                {
                    sellBoxSlots[i].UpdateEmptySlot();
                }
                allValueText.text = "0";
                sellTip.SetActive(false);
                GenerateCoins();
            }
            //playerMoneyText.recipeName = InventoryManager.Instance.playerMoney.ToString();
           
        }
        /// <summary>
        /// 点击商店切换按钮 
        /// </summary>
        public void ClickShopSwitchButton()
        {
            foreach (var slot in sellBoxSlots)
            {
                //有物品还在出售箱就显示询问UI
                if (slot.itemDetails != null)
                {
                    Instantiate(sellHaveItemTipPrefab, Input.mousePosition, Quaternion.identity, baseBag.transform);
                    return;
                }
            }
            InventoryManager.Instance.isSellState = false;
            shopSlotHolder.SetActive(true);
            sellBoxSlotHolder.SetActive(false);
            sellJoy.SetActive(false);
            allValue.SetActive(false);
            sellTip.SetActive(false);
        }
        /// <summary>
        /// 点击出售箱切换按钮
        /// </summary>
        public void ClickSellSwitchButton()
        {
           
            InventoryManager.Instance.isSellState = true;
            shopSlotHolder.SetActive(false);
            sellBoxSlotHolder.SetActive(true);
            sellJoy.SetActive(true);
            allValue.SetActive(true);
            
        }
        /// <summary>
        /// 点击商店关闭按钮
        /// </summary>
        public void ClickQuitShopButton()
        {
            haveSellBoxItemList.Clear();
            //关闭商店UI时检查出售箱是否还有残存物品
            foreach (var slot in sellBoxSlots)
            {
                if (slot.itemDetails != null)
                {
                    InventoryItem haveItem = new InventoryItem { itemID = slot.itemDetails.itemID, itemAmount = slot.itemAmount };
                    haveSellBoxItemList.Add(haveItem);
                }
            }
          
            //有物品还在出售箱就显示询问UI
            if(haveSellBoxItemList.Count > 0)
            {
                haveItemUIPanel.SetActive(true);
            }
            else
            {
                EventHandler.CallBaseBagCloseEvent(SlotType.Shop, null);
                EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
            }
           
        }
        /// <summary>
        /// 点击收回物品询问是的按钮
        /// </summary>
        private void ClickYesButton()
        {
            EventHandler.CallClickHaveItemYesEvent(haveSellBoxItemList);
        }
        private void ClickNoButton()
        {
            haveItemUIPanel.SetActive(false);
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
                //出售箱没有这个物品就返回一个空的index
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
            if(location == InventoryLocation.SellBox || location == InventoryLocation.Shop)
            {
                //没有这个物品 
                if (index == -1)
                {
                    //在玩家背包中查找物品
                    foreach (var slot in playerSlots)
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
                Vector3 originPos = new Vector3(UnityEngine.Random.Range(coinRectTran.anchoredPosition.x - 50f, coinRectTran.anchoredPosition.x + 50f), UnityEngine.Random.Range(coinRectTran.anchoredPosition.y - 50f, coinRectTran.anchoredPosition.y + 50f));
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
                num = 4;
            }
            if(InventoryManager.Instance.ModifySellBoxValue() > 500)
            {
                num = 5;
            }
            if (InventoryManager.Instance.ModifySellBoxValue() > 1000)
            {
                num = 8;
            }
            if (InventoryManager.Instance.ModifySellBoxValue() > 5000)
            {
                num = 10;
            }
            for (int i = 0; i < num; i++)
            {
                GameObject coin = Instantiate(coinPrefab, allValueText.transform.position, Quaternion.identity, coinParent);
                if(coinRectTran == null)
                {
                    coinRectTran = coin.GetComponent<RectTransform>();
                    coinRectTran.anchoredPosition = coinParent.InverseTransformPoint(allValueText.transform.position);
                }
                coin.GetComponent<CoinTween>().targetPos = coinParent.InverseTransformPoint(coinTargetPos.position);
                if (i == 0)
                {
                    coin.GetComponent<CoinTween>().isfirst = true;
                }
                coinList.Add(coin);
            }
            coinNum = num;
            MakeTween();
        }
        /// <summary>
        /// 退出建造模式
        /// </summary>
        private void ExitBuildMode()
        {
            EventHandler.CallBuildindModeEvent(null,null,false);
            buildShopPanel.SetActive(true);
        }
        private void ExitAnimalMode()
        {
            EventHandler.CallBuildindModeEvent(null, null, false);
            animalShopUI.SetActive(true);
        }
        private void QuitBuildingShopUI()
        {
            buildShopPanel.SetActive(false);
            EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
        }
       
        /// <summary>
        /// 显示材料不足提示
        /// </summary>
        public void ShowResouceLackText()
        {
            Instantiate(resourceLock, Input.mousePosition, Quaternion.identity, coinParent);
        }
        /// <summary>
        /// 打开动物询问数量UI
        /// </summary>
        public void OpenAnimalAskUI(Sprite sprite)
        {
            animalAskUI.SetActive(true);
            askImage.sprite = sprite;
            currentAskAmount = 1;
            askAmountText.text = currentAskAmount.ToString();

        }
        /// <summary>
        /// 关闭动物询问数量UI
        /// </summary>
        public void QuitAnimalAskUI()
        {
            animalAskUI.SetActive(false);
        }
        /// <summary>
        /// 点击增加动物数量按钮
        /// </summary>
        public void ClickAnimalIncreaseButton()
        {
            currentAskAmount++;
            askAmountText.text = currentAskAmount.ToString();
        }
        /// <summary>
        /// 点击减少动物数量按钮
        /// </summary>
        public void ClickAnimalDecreaseButton()
        {
            if(currentAskAmount >= 2)
            {
                currentAskAmount--;
                askAmountText.text = currentAskAmount.ToString();
            }
        }
        public void ClickAnimalCommitAmountButton()
        {
            EventHandler.CallInstantiateAnimalInScene(currentAskAmount);
            animalAskUI.SetActive(false);
        }
        /// <summary>
        /// 关闭动物商店
        /// </summary>
        public void QuitAnimalShopUI()
        {
            animalShopUI.SetActive(false);
            EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
        }
    }
}


