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
    public class InventoryUI : Singleton<InventoryUI>    //??????Inventory??????
    {
        [Header("???????")]
        public Canvas tweenCanvas;
        public ItemTooltip itemTooltip;
        [Header("?????")]
        public Image dragItem;
        public TextMeshProUGUI dragItemAmount;
        [Header("??????UI")]
        [SerializeField] private GameObject bagUI;
        [SerializeField] private GameObject playerEquipUI;
        //????????????????
        [HideInInspector] public bool canOpenBag = true;
        public GameObject activeBagBar;
        public GameObject bagButton;
        private bool bagOpened;
        [Header("??????")]
        [SerializeField] private GameObject baseBag;
        //??????UI?ß÷??ß›???ﬁœ?
        public GameObject[] playerBagSupButtons;
        public GameObject shopSlotPrefab;
        public GameObject boxSlotPrefab;
        //???SlotUI????
        public Animator baitBarAnim;
        //??????
        public SlotUI baitItemSlot;
        [Header("???????????UI")]
        public GameObject tradeUIPanel;
        public TradeUI tradeUI;
        //??????????????UI
        public GameObject haveItemUIPanel;
        public Button yesButton;
        public Button noButton;
        public TextMeshProUGUI playerMoneyText;
        public Animator playerMoneyUIAnim;
        public SlotUI[] playerSlots;
        public SlotUI[] sellBoxSlots;
        [Header("NPC???UI")]
        public GameObject shopSlotHolder;
        public Button quitShopButton;
        public Button shopButton;
        [Header("NPC??????UI")]
        public GameObject sellBoxSlotHolder;
        public Button sellBoxButton;
        public GameObject sellJoy;
        public GameObject allValue;
        public TextMeshProUGUI allValueText;
        //??????ßÿ????????
        public GameObject sellHaveItemTipPrefab;
        //????????????
        public GameObject sellTip;
        [SerializeField] private List<SlotUI> baseBagSlots;
        //???????ß”ß’?????
        private List<InventoryItem> haveSellBoxItemList = new List<InventoryItem>();
        [Header("???")]
        private RectTransform coinRectTran;
        //????????
        public GameObject coinPrefab;
        //????????????Parent
        public Transform coinParent;
        private List<GameObject> coinList = new List<GameObject>();
        private int coinNum;
        //???????
        public Transform coinTargetPos;
        [Header("????????")]
        //??????UI????¶À??
        [SerializeField] private Transform itemGetBg;
        //??????UI
        public GameObject itemGetTipPrefab; 
        //?????UI
        public GameObject skillPanel;
        [HideInInspector] public bool skillBarOpened;
        [Header("??????")]
        public GameObject[] playerPanels;
        //?????
        public SlotUI equipHeadSlot, equipBodySlot;
        //???????????
        public Text attackNumber, defenseNumber;
        [Header("??????ß“?")] 
        //??????????
        public List<EquipImage> equipImageSprites = new List<EquipImage>();
        //????????????
        public List<Image> equipImageList = new List<Image>();
        public Dictionary<string, Image> equipImageDict = new Dictionary<string, Image>();
        [Header("??ßÿ?")]
        public Image[] friendlinessImages;
        [Header("????UI")]
        public SlotUI_Build buildSlotUI;
        public Transform buildSlotUIParent;
        public GameObject buildShopPanel;
        public GameObject buildModePanel;
        public Button buildModeExitButton;
        public Button animalModeExitButton;
        public Button buildingShopExitButton;
        //??????????
        public GameObject resourceLock;
        [Header("???????")]
        public GameObject animalShopUI;
        public Transform animalSlotParent;
        public AnimalSlotUI animalShopSlotUI;
        public Button quitAnimalShopButton;
        //????????????UI
        public GameObject animalAskUI;
        public Image askImage;
        public TextMeshProUGUI askAmountText;
        public Button increaseAmountButton;
        public Button decreaseAmountButton;
        public Button quitAskUIButton;
        //??????????
        public Button commitAmountButton;
        //???????????
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
        /// ?????????????????????
        /// </summary>
        private void OnBeforeSceneUnloadEvent()
        {
           
            UpdateSlotHighlight(-1);
        }
        private void OnAfterSceneLoadedEvent()
        {
            //??????????
            playerMoneyText.text = InventoryManager.Instance.playerMoney.ToString();
        }
        /// <summary>
        /// ??????????????
        /// </summary>
        /// <param name="slotType"></param>
        /// <param name="bag_SO"></param>
        private void OnBaseBagOpenEvent(SlotType slotType, InventoryBag_SO bagData)
        {
            //TODO:???????Prefab
            GameObject prefab = slotType switch
            {
                SlotType.Shop => shopSlotPrefab,
                SlotType.Box => boxSlotPrefab,
                _ => null,
            };
            //???????UI
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
                //????????????????????
                var slot = Instantiate(prefab, baseBag.transform.GetChild(0)).GetComponent<SlotUI>();
                //??????????????????
                slot.slotIndex = i;
                //????????slot?????baseBag?ß“???
                baseBagSlots.Add(slot);
            }
            //???????????????????????????????????????
            LayoutRebuilder.ForceRebuildLayoutImmediate(baseBag.GetComponent<RectTransform>());
            //?????????????????,?????????????¶À??
            if (slotType == SlotType.Shop)
            {
                bagUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(449.2f, 199.2f);
                bagUI.SetActive(true);
                bagOpened = true;
            }
            EventHandler.CallRestoreNormalCursorImageEvent();
            //????UI???
            OnUpdateInventoryUI(InventoryLocation.Box, bagData.itemList,0);

        }
        /// <summary>
        /// ?????????????
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
            //????????ß÷????
            InventoryManager.Instance.currentSelectedItem = null;
            EventHandler.CallItemSelectEvent(InventoryManager.Instance.currentSelectedItem, false);
            itemTooltip.gameObject.SetActive(false);
            UpdateSlotHighlight(-1);
            foreach (var slot in baseBagSlots)
            {
                Destroy(slot.gameObject);
            }
            baseBagSlots.Clear();
            //?????????????,???????¶À??
            if (slotType == SlotType.Shop)
            {
                bagUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(385.2f, 197);
                bagUI.SetActive(false);
                bagOpened = false;
            }
        }
        /// <summary> 
        /// ??????????
        /// </summary>
        /// <param name="item"></param>
        /// <param name="isSell"></param>
        private void OnShowTradeUI(ItemDetails item, bool isSell,int maxAmount,int fromIndex,int toIndex,InventoryLocation startLocation , InventoryLocation endLocation , bool isToSellBox)
        {
            if(toIndex < sellBoxSlots.Length)
            {
                //????????????????????????ßﬂ???UI???
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
        /// ??????????????????ItemTipList?ß“???
        /// </summary>
        /// <param name="getItemDetails"></param>
        private void OnGetItemTipEvent(ItemDetails getItemDetails)
        {
            if (getItemDetails != null)
            {
                //itemGetBg?????????
                ItemGetTipUI[] itemGetTipUIs;
                itemGetTipUIs = itemGetBg.GetComponentsInChildren<ItemGetTipUI>();
                //?????????
                if(itemGetTipUIs.Length <= 0)
                {
                    GameObject getItem = Instantiate(itemGetTipPrefab, itemGetBg);
                    getItem.GetComponent<ItemGetTipUI>().ModifyItemParameter(getItemDetails);
                }
                //????????
                else
                {
                    //?????????????????
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
                    //????????
                    else
                    {
                        GameObject getItem = Instantiate(itemGetTipPrefab, itemGetBg);
                        getItem.GetComponent<ItemGetTipUI>().ModifyItemParameter(getItemDetails);
                    }
                }
                //currentGetItemID = getItemDetails.itemID;
                ////?????????????ßÿ???????????ß“??ß›???????????????????
                //if (ItemListConditons(getItemDetails) == -1)
                //{
                //    itemGetTipList.Add(getItemDetails);
                //    Instantiate(itemGetTipPrefab, itemGetBg.transform);
                //    itemGetTipPrefab.transform.GetChild(0).GetComponent<Image>().sprite = getItemDetails.itemIcon;
                //    itemGetTipPrefab.transform.GetChild(1).GetComponent<Text>().recipeName = "X" + " " + "1";
                //}
                ////?????????????????????????????????????????
                //else
                //{
                //    //????????????????????????ß“???index
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
        /// ???????ß“????ß÷?????????????????????
        /// </summary>
        /// <param name="location">???????¶À??????</param>
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
                //Shop??Box??????????
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
                    //?????????????????
                    allValueText.text = InventoryManager.Instance.ModifySellBoxValue().ToString();
                    //???????????????????????
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
        /// ????????????????????????????Å£
        /// </summary>
        /// <param name="fromValue"></param>
        /// <param name="difference"></param>
        public void OnDoTweenPlayerMoneyChageEvent(int fromValue, int difference)
        {
            //????Å£????
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
        /// ????itemGetTipUIs?????????
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
        /// ?????????????????????????
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
            //???buildSlotUIParent?????????
            for (int i = 0; i < buildSlotUIParent.childCount; i++)
            {
                Destroy(buildSlotUIParent.GetChild(i).gameObject);
            }
            //?????????????
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
            //??????????????????
            for (int i = 0; i < playerSlots.Length; i++)
            {
                playerSlots[i].slotIndex = i;
            }
            //???????????Player Bag ??????
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
        }
      
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B) )
            {
                OpenBagUI();
            }
        }
       
        /// <summary>
        /// ???????UI?????
        /// </summary>
        public void OpenBagUI()
        {
            if (canOpenBag)
            {
                //???Bag Button?????????
                bagOpened = !bagOpened;
                InventoryManager.Instance.anyBagOpened = bagOpened;

                if (bagOpened)
                {
                    //Time.timeScale = 0f;
                    EventHandler.CallUpdateGameStateEvent(GameState.Pause);
                    //???¶¡????????????????????
                    playerPanels[0].SetActive(true);
                    //SwitchBagSlotHolderPageButton();
                }
                else
                {
                    //????ActionBar????????????????????????????????????????????????null
                    for (int i = 0; i < playerSlots.Length - 9; i++)
                    {
                        if (playerSlots[i].isSelected)
                        {
                            UpdateSlotHighlight(-1);
                            InventoryManager.Instance.currentSelectedItem = null;
                            //??¶…?????UI????????????Player??????????????
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
                    //??????UI????????????UI
                    itemTooltip.gameObject.SetActive(false);
                }
                playerEquipUI.SetActive(bagOpened);
                EventHandler.CallItemUselessEvent(bagOpened);
                EventHandler.CallRestoreNormalCursorImageEvent();
            }
           
        }

        /// <summary>
        /// ???????????ß⁄?????????????
        /// </summary>
        /// <param name="index">OnPointerClick????????????slotIndex</param>
        public void UpdateSlotHighlight(int index)
        {
            //???????playerSlots???????????slot
            foreach (var slot in playerSlots)
            {
                if (slot.isSelected && slot.slotIndex == index)
                {
                    slot.slotHighlight.gameObject.SetActive(true);
                }
                else
                {
                    //?????????ß÷?slot?????????????????????
                    slot.isSelected = false;
                    slot.slotHighlight.gameObject.SetActive(false);
                }
            }
        }
        /// <summary>
        /// ?????????????????,????????????UI????????
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
                    //?????????????
                    if(index == 1)
                    {
                        skillPanel.GetComponent<SkillBar>().UpdateSkillBar();
                    }
                    //????????????
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
        /// ??????????,??????PlayerEquipUI???QuitButton?????
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
            //??¶…?????UI????????????Player??????????????
            EventHandler.CallEquipSlotEvent(equipHeadSlot, equipBodySlot);
            EventHandler.CallItemSelectEvent(InventoryManager.Instance.currentSelectedItem, false);
            bagOpened = false;
            //Time.timeScale = 1f;
            EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
            EventHandler.CallItemUselessEvent(false);
            playerEquipUI.SetActive(false);
        }
        ///// <summary>
        ///// ?????????????
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
        ///// ?????????????
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
        ///// ????????????????????
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
        /// ????????????????
        /// </summary>
        /// <param name="itemDetails"></param>
        public void SetEquipImage(ItemDetails itemDetails,SlotUI targetSlot)
        {
            if (itemDetails != null)
            {
                //??????itemDetails????
                if (targetSlot.slotType == SlotType.Equipment_Head)
                {
                    //??????????????
                    foreach (EquipImage s in equipImageSprites)
                    {
                        if (s.itemID == itemDetails.itemID && s.equipPartName == PartName.Head)
                        {
                            equipImageDict["Head"].sprite = s.equipSprite;
                        }
                    }

                }
                //???????????
                else
                {
                    //??????????????
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
        /// ?????????
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
        /// ????????????????
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
        /// ????????????????
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
            //????????????
            EventHandler.CallEquipArmorEven(num);
            defenseNumber.text = num.ToString();
        }
        /// <summary>
        /// ????sellButton???????,??????sellButton?????
        /// </summary>
        public void ClickSellButton()
        {
            //??????????sellBox??UI
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
        /// ???????ß›???? 
        /// </summary>
        public void ClickShopSwitchButton()
        {
            foreach (var slot in sellBoxSlots)
            {
                //?????????????????????UI
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
        /// ??????????ß›????
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
        /// ??????????
        /// </summary>
        public void ClickQuitShopButton()
        {
            haveSellBoxItemList.Clear();
            //??????UI?????????????ß”ß’????
            foreach (var slot in sellBoxSlots)
            {
                if (slot.itemDetails != null)
                {
                    InventoryItem haveItem = new InventoryItem { itemID = slot.itemDetails.itemID, itemAmount = slot.itemAmount };
                    haveSellBoxItemList.Add(haveItem);
                }
            }
          
            //?????????????????????UI
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
        /// ?????????????????
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
        ///  ??????????????????index?????????index
        /// </summary>
        /// <param name="currentItemID">??????</param>
        /// <param name="lists">????????InventoryItem????</param>
        /// <param name="location">??????????¶À??</param>
        /// <returns></returns>
        public int GetEmptySellBoxIndex(int currentItemID,List<InventoryItem> lists,InventoryLocation location)
        {
            int index = -1;
            index = InventoryManager.Instance.GetItemIndexInBag(currentItemID, lists);
           
            if(location == InventoryLocation.Player)
            {
                //??????????????????????????index
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
                //????????
                else
                {
                    return index;
                }
            }
            if(location == InventoryLocation.SellBox || location == InventoryLocation.Shop)
            {
                //????????? 
                if (index == -1)
                {
                    //?????????ß”??????
                    foreach (var slot in playerSlots)
                    {
                        if (slot.itemDetails == null)
                        {
                            index = slot.slotIndex;
                            return index;
                        }
                    }
                }
                //????????
                else
                {
                    return index;
                }
            }
            return index;
        }
        /// <summary>
        /// ?????????????UI??
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
        /// ??????
        /// </summary>
        private void GenerateCoins()
        {
            coinList.Clear();
            //??????????
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
        /// ?????????
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
        /// ?????????????
        /// </summary>
        public void ShowResouceLackText()
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Instantiate(resourceLock, pos, Quaternion.identity, coinParent);
        }
        /// <summary>
        /// ????????????UI
        /// </summary>
        public void OpenAnimalAskUI(Sprite sprite)
        {
            animalAskUI.SetActive(true);
            askImage.sprite = sprite;
            currentAskAmount = 1;
            askAmountText.text = currentAskAmount.ToString();

        }
        /// <summary>
        /// ?????????????UI
        /// </summary>
        public void QuitAnimalAskUI()
        {
            animalAskUI.SetActive(false);
        }
        /// <summary>
        /// ?????????????????
        /// </summary>
        public void ClickAnimalIncreaseButton()
        {
            currentAskAmount++;
            askAmountText.text = currentAskAmount.ToString();
        }
        /// <summary>
        /// ?????????????????
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
        /// ?????????
        /// </summary>
        public void QuitAnimalShopUI()
        {
            animalShopUI.SetActive(false);
            EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
        }
    }
}


