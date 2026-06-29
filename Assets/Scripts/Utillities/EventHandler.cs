using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MFarm.Inventory;
using MFarm.Dialogue;
using static AnimalData_SO;
public static class EventHandler
{
    //注册事件，当触发该事件UpdateInventoryUI时，会同时调用InventoryLocation和List<InventoryItem>
    public static event Action<InventoryLocation, List<InventoryItem>,int> UpdateInventoryUI;
    /// <summary>
    /// 调用事件
    /// </summary>
    /// <param name="location"></param>
    /// <param name="list"></param>
    public static void CallUpdateInventoryUI(InventoryLocation location, List<InventoryItem> list,int difference)
    {   //if简写，当UpdateInventoryUI != null时调用
        UpdateInventoryUI?.Invoke(location, list, difference);
    }
    //在指定位置生成物品的事件
    public static event Action<int, Vector3> InstantiateItemInScene;
    public static void CallInstantiateItemInScene(int ID, Vector3 pos)
    {
        InstantiateItemInScene?.Invoke(ID, pos);
    }
    //丢弃物品的事件
    public static event Action<int, Vector3, ItemType> DropItemEvent;
    public static void CallDropItemEvent(int ID, Vector3 pos, ItemType itemType)
    {
        DropItemEvent?.Invoke(ID, pos, itemType);
    }

    //注册点击slot物品时，头顶出现对应Item的图片
    public static event Action<ItemDetails, bool> ItemSelectEvent;
    public static void CallItemSelectEvent(ItemDetails itemDetails, bool isSelected)
    {
        ItemSelectEvent?.Invoke(itemDetails, isSelected);
    }
    public static event Action<int> SlotUISelectIndexEvent;
    //此时点击选择的物品在背包的序号位置事件
    public static void CallSlotUISelectIndexEvent(int index)
    {
        SlotUISelectIndexEvent?.Invoke(index);
    }
    //每分每时所执行的事件
    public static event Action<int, int, int, Season> GameMinuteEvent;
    public static void CallGameMinuteEvent(int minute, int hour, int day, Season season)
    {
        GameMinuteEvent?.Invoke(minute, hour, day, season);
    }
    //每天每季节对应的事件
    public static event Action<int, Season> GameDayEvent;
    public static void CallGameDayEvent(int day, Season season)
    {
        GameDayEvent?.Invoke(day, season);
    }
   
    //每小时，天，月，年,季节的事件
    public static event Action<int, int, int, int, Season> GameDateEvent;
    public static void CallGameDateEvemnt(int hour, int day, int month, int year, Season season)
    {
        GameDateEvent?.Invoke(hour, day, month, year, season);
    }
    //新场景传送事件
    public static event Action<string, Vector3> TransitionEvent;
    public static void CallTransitionEvent(string sceneName, Vector3 pos)
    {
        TransitionEvent?.Invoke(sceneName, pos);
    }
    //获取当前建筑场景的code
    public static event Action<int, Vector3> GetCurrentBuildCode;
    public static void CallGetCurrentBuildCode(int code, Vector3 pos)
    {
        GetCurrentBuildCode?.Invoke(code, pos);
    }
    //切换场景之前的事件
    public static event Action BeforeSceneUnloadEvent;
    public static void CallBeforeSceneUnloadEvent()
    {
        BeforeSceneUnloadEvent?.Invoke();
    }
    //切换场景之后的事件
    public static event Action AfterSceneLoadedEvent;
    public static void CallAfterSceneLoadedEvent()
    {
        AfterSceneLoadedEvent?.Invoke();
    }
    //场景加载好后人物所在的位置事件
    public static event Action<Vector3> MoveToPosition;
    public static void CallMoveToPosition(Vector3 targetPosition)
    {
        MoveToPosition?.Invoke(targetPosition);
    }
    //鼠标点击物品之后一系列操作
    public static event Action<Vector3, ItemDetails> MouseClickedEvent;
    public static void CallMouseClickedEvent(Vector3 pos, ItemDetails itemDetails)
    {
        MouseClickedEvent?.Invoke(pos, itemDetails);
    }
    //鼠标长按触发的事件
    public static event Action<Vector3, ItemDetails> MouseHoldEvent;
    public static void CallMouseHoldEvent(Vector3 pos,ItemDetails itemDetails)
    {
        MouseHoldEvent?.Invoke(pos, itemDetails);
    }
    //鼠标长按后松开事件
    public static event Action MouseUpEvent;
    public static void CallMouseUpEvent()
    {
        MouseUpEvent?.Invoke();
    }
    //恢复为默认鼠标图片事件
    public static event Action RestoreNormalCursorImageEvent;
    public static void CallRestoreNormalCursorImageEvent()
    {
        RestoreNormalCursorImageEvent?.Invoke();
    }
    //在玩家执行完对应动画之后执行的操作
    public static event Action<Vector3, ItemDetails> ExecuteActionAfterAnimation;
    public static void CallExecuteActionAfterAnimation(Vector3 pos, ItemDetails itemDetails)
    {
        ExecuteActionAfterAnimation?.Invoke(pos, itemDetails);
    }
    //播种的事件,int种子ID，TileDetails当前选择瓦片的信息 
    public static event Action<int, TileDetails> PlantSeedEvent;
    public static void CallPlantSeedEvent(int ID, TileDetails tile)
    {
        PlantSeedEvent?.Invoke(ID, tile);
    }
    //收获农作物或材料时的事件  
    public static event Action<int> HarvestAtPlayerPosition;
    public static void CallHarvestAtPlayerPosition(int ID)
    {
        HarvestAtPlayerPosition?.Invoke(ID);
    }
    //刷新当前地图
    public static event Action RefreshCurrentMap;
    public static void CallRefreshCurrentMap()
    {
        RefreshCurrentMap?.Invoke();
    }
    //调用粒子特效的事件
    public static event Action<ParticalEffectType, Vector3 , Vector2> ParticleEffectEvent;
    public static void CallParticleEffectEvent(ParticalEffectType effectType, Vector3 pos , Vector2 dirction)
    {
        ParticleEffectEvent?.Invoke(effectType, pos , dirction);
    }
    //预生成Crop事件
    public static event Action GenerateCropEvent;
    public static void CallGenerateCropEvent()
    {
        GenerateCropEvent?.Invoke();
    }
    //对话显示事件
    public static event Action<DialoguePiece> ShowDialogueEvent;
    public static void CallShowDialogueEvent(DialoguePiece piece)
    {
        ShowDialogueEvent?.Invoke(piece);
    }

    //商店开启
    public static event Action<SlotType, InventoryBag_SO> BaseBagOpenEvent;
    public static void CallBaseBagOpenEvent(SlotType slotType, InventoryBag_SO bag_SO)
    {
        BaseBagOpenEvent?.Invoke(slotType, bag_SO);
    }
    //商店关闭
    public static event Action<SlotType, InventoryBag_SO> BaseBagCloseEvent;
    public static void CallBaseBagCloseEvent(SlotType slotType, InventoryBag_SO bag_SO)
    {
        BaseBagCloseEvent?.Invoke(slotType, bag_SO);
    }
    //游戏状态设置
    public static event Action<GameState> UpdateGameStateEvent;
    public static void CallUpdateGameStateEvent(GameState gameState)
    {
        UpdateGameStateEvent?.Invoke(gameState);
    }

    //商品买卖事件
    public static event Action<ItemDetails, bool,int,int,int,InventoryLocation,InventoryLocation,bool> ShowTradeUI;
    public static void CallShowTradeUI(ItemDetails item, bool isSell,int maxAmount, int startIndex,int endIndex,InventoryLocation startLocation,InventoryLocation endLocation, bool isToSellBox)
    {
        ShowTradeUI?.Invoke(item, isSell, maxAmount, startIndex, endIndex, startLocation, endLocation, isToSellBox);
    }
    //点击残存出售箱UI的确认按钮事件
    public static event Action<List<InventoryItem>> ClickHaveItemYesEvent;
    public static void CallClickHaveItemYesEvent(List<InventoryItem> haveItemList)
    {
        ClickHaveItemYesEvent?.Invoke(haveItemList);
    }
    //获取当前的家具信息
    public static event Action<BluPrintDetails,Transform,List<TileDetails>> GetCurrentBluPrintDetails;
    public static void CallGetCurrentBluPrintPrefab(BluPrintDetails bluPrintDetails,Transform parent, List<TileDetails> list)
    {
        GetCurrentBluPrintDetails?.Invoke(bluPrintDetails, parent, list);
    }
    //建造物品生成事件
    public static event Action<BluPrintDetails, Vector3 , Transform> BuildFurnitureEvent;
    public static void CallBuildFurnitureEvent(BluPrintDetails bluPrintDetails, Vector3 pos,Transform parent)
    {
        BuildFurnitureEvent?.Invoke(bluPrintDetails, pos, parent);
    }
    //打开建筑商店事件
    public static event Action<BuildingBagData_SO> OpenBuildShopEvent;
    public static void CallOpenBuildShopEvent(BuildingBagData_SO buildingBag)
    {
        OpenBuildShopEvent?.Invoke(buildingBag);
    }
    //建造模式事件
    public static event Action<BuildingDetails ,AnimalDetails, bool> BuildindModeEvent;
    public static void CallBuildindModeEvent(BuildingDetails buildingDetails, AnimalDetails animal,bool startMode)
    {
        BuildindModeEvent?.Invoke(buildingDetails, animal, startMode);
    }
    //获取当前建筑信息
    public static event Action<BuildingDetails, Transform, List<TileDetails>> GetCurrentBuildingDetails;
    public static void CallGetCurrentBuildingDetails(BuildingDetails buildingDetails,Transform parent, List<TileDetails> tiles)
    {
        GetCurrentBuildingDetails?.Invoke(buildingDetails, parent, tiles);
    }
    //放置建筑事件
    public static event Action<BuildingDetails, Vector3, Transform> InstantiateBuildingOnMapEvent;
    public static void CallInstantiateBuildingOnMapEvent(BuildingDetails buildingDetails,Vector3 pos,Transform parent)
    {
        InstantiateBuildingOnMapEvent?.Invoke(buildingDetails, pos, parent);
    }
    //打开动物商店事件
    public static event Action<AnimalBagData_SO> OpenAnimalShopEvent;
    public static void CallOpenAnimalShopEvent(AnimalBagData_SO animalShop)
    {
        OpenAnimalShopEvent?.Invoke(animalShop);
    }
   //生成动物在场景中
    public static event Action<int> InstantiateAnimalInScene;
    public static void CallInstantiateAnimalInScene(int amount)
    {
        InstantiateAnimalInScene?.Invoke(amount);
    }
    //显示或关闭建筑箭头图片
    public static event Action<AnimalSizeType,bool> DisplayBuildingArrowIcon;
    public static void CallDisplayBuildingArrowIcon(AnimalSizeType buildingSize, bool isShow)
    {
        DisplayBuildingArrowIcon?.Invoke(buildingSize, isShow);
    }
    //切换灯光模式事件
    public static event Action<Season, LightShift, float> LightShiftChangeEvent;
    public static void CallLightShiftChangeEvent(Season season, LightShift lightShift, float timeDifference)
    {
        LightShiftChangeEvent?.Invoke(season, lightShift, timeDifference);
    }

    //音效播放事件
    public static event Action<SoundDetails> InitSoundEffect;
    public static void CallInitSoundEffect(SoundDetails soundDetails)
    {
        InitSoundEffect?.Invoke(soundDetails);
    }
    //玩家动作音效
    public static event Action<SoundName> PlaySoundEvent;
    public static void CallPlaySoundEvent(SoundName soundName)
    {
        PlaySoundEvent?.Invoke(soundName);
    }
    //扣除或增加疲劳值事件
    public static event Action<float> PlayerDecreaseStminaEvent;
    public static void CallPlayerDecreaseStminaEvent(float value)
    {
        PlayerDecreaseStminaEvent?.Invoke(value);
    }
    //扣除或增加玩家血量
    public static event Action<float> PlayerDecreaseHealthEvent;
    public static void CallPlayerDecreaseHealthEvent(float value)
    {
        PlayerDecreaseHealthEvent?.Invoke(value);
    }
    //检查并打开烹饪UI的事件
    public static event Action<bool> CheckCookedUIEvent;
    public static void CallCheckCookedUIEvent(bool canCooked)
    {
        CheckCookedUIEvent?.Invoke(canCooked);
    }
    //菜谱激活的事件
    public static event Action CookedMenuSetupEvent;
    public static void CallCookedMenuSetupEvent()
    {
        CookedMenuSetupEvent?.Invoke();
    }
    //开始新游戏的事件
    public static event Action<int> StartNewGameEvent;
    public static void CallStartNewGameEvent(int index)
    {
        StartNewGameEvent?.Invoke(index);
    }

    //结束游戏的事件
    public static event Action EndGameEvent;
    public static void CallEndGameEvent()
    {
        EndGameEvent?.Invoke();
    }
    //拾取物品UI提示事件
    public static event Action<ItemDetails> GetItemTipEvent;
    public static void CallGetItemTipEvent(ItemDetails getitemDetail)
    {
        GetItemTipEvent?.Invoke(getitemDetail);
    }
    //Item出现时的起始位置
    public static event Action<Vector3> ItemFirstPos;
    public static void CallItemFirstPos(Vector3 itemFirstPos)
    {
        ItemFirstPos?.Invoke(itemFirstPos);
    }
    //春天白天渔获列表刷新时间
    public static event Action<List<ItemDetails>> FishListInSpriteDayFreshEvent;
    public static void CallFishListInSpriteDayFreshEvent(List<ItemDetails> fishList)
    {
        FishListInSpriteDayFreshEvent?.Invoke(fishList);
        
    }
    //增加种植经验值事件
    public static event Action<int> IncreasePlantingSkillEvent;
    public static void CallInCreasePlantingSkillEvent(int EXP)
    {
        IncreasePlantingSkillEvent?.Invoke(EXP);
    }
    public static event Action<int> IncreaseCultivetionSkillEvent;
    //增加养殖经验值事件
    public static void CallInCreaseCultivetionSkillEvent(int EXP)
    {
        IncreaseCultivetionSkillEvent?.Invoke(EXP);
    }
    //增加钓鱼经验值事件
    public static event Action<int> IncreaseFishingSkillEvent;
    public static void CallInCreaseFishingSkillEvent(int EXP)
    {
        IncreaseFishingSkillEvent?.Invoke(EXP);
    }
    //增加战斗经验值事件
    public static event Action<int> IncreaseFightSkillEvent;
    public static void CallInCreaseFightSkillEvent(int EXP)
    {
        IncreaseFightSkillEvent?.Invoke(EXP);
    }
    //增加探索经验值事件
    public static event Action<int> IncreaseExploreSkillEvent;
    public static void CallInCreaseExploreSkillEvent(int EXP)
    {
        IncreaseExploreSkillEvent?.Invoke(EXP);
    }
    //游戏UI开启时禁止鼠标使用道具
    public static event Action<bool> ItemUselessEvent;
    public static void CallItemUselessEvent(bool UIOpend)
    {
        ItemUselessEvent?.Invoke(UIOpend);
    }
    //开始任务事件
    public static event Action TaskStartEvent;
    public static void CallTaskStartEvent()
    {
        TaskStartEvent?.Invoke();
    }
    //更新任务进度事件，在怪物死亡，拾取物品时调用，不要忘记如果该任务物品可以使用，在使用的时候也要减去对应的任务进度数量
    public static event Action<int,int> UpdateQuestProgressEvent;
    public static void CallUpdateQuestProgressEvent(int itemID,int amount)
    {
        UpdateQuestProgressEvent?.Invoke(itemID,amount);
    }
    //检查装备框事件
    public static event Action<SlotUI, SlotUI> EquipSlotEvent;
    public static void CallEquipSlotEvent(SlotUI headSlot,SlotUI bodySlot)
    {
        EquipSlotEvent?.Invoke(headSlot, bodySlot);
    }
    //减少玩家背包中的树种子数量
    public static event Action<int> RemoveTreeSeedCount;
    public static void CallRemoveTreeSeedCount(int seedID)
    {
        RemoveTreeSeedCount?.Invoke(seedID);
    }
    //显示收获农作物的举起图片
    public static event Action<int> DisplayCollectItemSprite;
    public static void CallDisplayCollectItemSprite(int itemID)
    {
        DisplayCollectItemSprite?.Invoke(itemID);
    }
    //装备护甲的事件
    public static event Action<int> EquipArmorEvent;
    public static void CallEquipArmorEven(int value)
    {
        EquipArmorEvent?.Invoke(value);
    }
    //修改玩家金钱并“流动”变化的事件
    public static event Action<int, int> DoTweenPlayerMoneyChageEvent;
    public static void CallDoTweenPlayerMoneyChageEvent(int fromValue,int difference)
    {
        DoTweenPlayerMoneyChageEvent?.Invoke(fromValue, difference);
    }
    //开始NPC事件的事件
    public static event Action StartNPCEvent;
    public static void CallStartNPCEvent()
    {
        StartNPCEvent?.Invoke();
    }

    //推进NPC事件的事件
    public static event Action PromoteNPCEvent;
    public static void CallPromoteNPCEvent()
    {
        PromoteNPCEvent?.Invoke();
    }
    //增加NPC的好感度
    public static event Action<float,string> IncreaseFriendliness;
    public static void CallIncreaseFriendliness(float value,string name)
    {
        IncreaseFriendliness?.Invoke(value,name);
    }
    //结束NPC事件的事件
    public static event Action EndNPCEvent;
    public static void CallEndNPCEvent()
    {
        EndNPCEvent?.Invoke();
    }
 
    //更新好感度UI事件
    public static event Action<string, float> UpdateNPCFriendlinessUIPanel;
    public static void CallUpdateNPCFriendlinessUIPanel(string name,float value)
    {
        UpdateNPCFriendlinessUIPanel?.Invoke(name, value);
    }
    //什么时候能打开玩家背包事件
    public static event Action<bool> ControlPlayerBagOpen;
    public static void CallControlPlayerBagOpen(bool canOpen)
    {
        ControlPlayerBagOpen?.Invoke(canOpen);
    }
    //下一个传送点出现事件
    public static event Action NextTeleportAppearEvent;
    public static void CallNextTeleportAppearEvent()
    {
        NextTeleportAppearEvent?.Invoke();
    }
    //吃东西事件
    public static event Action<int> PlayEatAnimEvent;
    public static void CallPlayEatAnimEvent(int itemID)
    {
        PlayEatAnimEvent?.Invoke(itemID);
    }
    //动物生产的事件
    public static event Action<int, int> InstantiateAniamlProduceItemEvent;
    public static void CallInstantiateAniamlProduceItemEvent(int code,int itemID)
    {
        InstantiateAniamlProduceItemEvent?.Invoke(code, itemID);
    }
    //动物在回到家之后的事件
    public static event Action<int,SceneAnimal> AnimalArrivedAtHomeEvent;
    public static void CallAnimalArrivedAtHomeEvent(int buildCode, SceneAnimal animal)
    {
        AnimalArrivedAtHomeEvent?.Invoke(buildCode, animal);
    }
    //动物离开家之后的事件
    public static event Action<int, SceneAnimal> AnimalExitCoopEvent;
    public static void CallAnimalExitCoopEvent(int buildCode, SceneAnimal animal)
    {
        AnimalExitCoopEvent?.Invoke(buildCode, animal);
    }
}
