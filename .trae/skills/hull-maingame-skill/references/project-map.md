# SimpleGame 工程地图

## 环境

- Unity `2022.3.52f1c1`，URP 14.0.11，`com.unity.feature.2d`，Cinemachine 2.10.7
- DOTween（`Assets/Plugins/Demigiant`）、Addressables 1.22.3、Timeline 1.7.6、Newtonsoft.Json
- 输入：老 Input Manager（`UnityEngine.Input`），无新 Input System
- 唯一程序集 `Assembly-CSharp`（无 asmdef）

## 目录布局

```
Assets/
├─ Scripts/<系统>/{Data,Logic,UI}    # Data=数据类/SO，Logic=管理器，UI=界面
├─ GameData/<系统>                  # ScriptableObject 资源
├─ Prefabs/{Animal,BulidItem,Crop,Effect,Enemy,Environment,Item,NPC,Sound,Tree,UI,View}
├─ Scenes/                          # 44 个场景，见下
├─ TileMap/  Sprite Atlas/  Shader/  Setting/  Resources/  Plugins/
└─ ZZAdditons/Font                  # TMP 中文字体 SDF（Pixel_CN、HeiTi、KatiTi 等）
```

## 核心系统

| 系统 | 入口 | 数据 | 说明 |
|---|---|---|---|
| 地图 | `Map/Logic/GridMapManager.cs`（`MFarm.Map`，Singleton + ISaveable） | `MapData_SO` | `tileDetailsDict` 存地块状态；`firstLoadDict`；按天生成杂草/石头/树；传送点 |
| 种植 | `Crop/Logic/CropManager.cs`（`MFarm.CropPlant`） | `CropDetailsList_SO` | `GrowthDays` 倒序算阶段；`CropGenerator` 预生成随机作物 |
| 背包 | `Logic/InventoryManager.cs`（`MFarm.Inventory`） | `ItemDataList_SO`、`InventoryBag_SO` | `SlotType` × `InventoryLocation` 多背包；`InventoryUI`/`SlotUI` |
| 场景物品 | `Logic/ItemManager.cs`（`MFarm.Inventory`，ISaveable） | — | 按场景字典管理掉落物/灌木/家具/建筑/动物/可敲击物 |
| 玩家 | `Player/PlayerController.cs`（全局命名空间，非单例） | — | 移动/工具/钓鱼/血量疲劳/五项技能等级；`playerAnimatorGroup` 多 Animator |
| 时间 | `Time/Logic/TimeManager.cs`（Singleton + ISaveable） | — | 秒/分/时/日/月/年/季节/星期；天气预报 `weatherForecast` |
| 存档 | `Save Load/Logic/SaveLoadManager.cs`（`MFarm.Save`） | `GameSaveData`、`DataSlot` | 3 个存档位，JSON 存 `persistentDataPath/Save Data/` |
| 对话 | `Dialogue/Logic/DialogueController.cs` + `Timeline/` | `DialogueData_OS` | 选项可触发 Quest / 商店 / 建造 / 动物 |
| 任务 | `Quest/Logic/QuestManager.cs`、`Task/TaskManager.cs` | `QuestData_OS`、`TaskData_SO` | 剧情任务 + 告示板委托 |
| NPC | `NPC/Logic/NPCManager.cs`、`NPCMovement.cs` | `ScheduleDataList_SO`、`SceneRouteDataList_SO` | 日程驱动移动，寻路用 `AStar/` |
| 战斗 | `IState/`（FSM + States） | `EnemyDetails_SO` | Idle/Patrol/Chase/Attack/Hurted/Death/Fainting |
| 动物 | `Animal/AnimalController.cs`、`AnimalArea.cs` | `AnimalData_SO`、`AnimalBagData_SO` | 畜牧产出、回舍/出舍事件 |
| 交互光标 | `Cursor/CursorManager.cs`（全局命名空间） | — | 鼠标格子检测、`currentItem`/`currentTile`、光标动画 |
| 场景切换 | `Transition/TransitionManager.cs`、`Teleport.cs` | — | `TransitionEvent`，加载界面淡入淡出 |
| 音频 | `Audio/Logic/AudioManager.cs` | `SceneSoundList_SO`、`SoundDetailsList_SO` | 白名单 `SoundName` |
| 灯光 | `Light/Logic/LightManager.cs` | `LightPattenList_SO` | 按 `LightShift`（早/晚）与季节插值 |
| UI | `UI/`、`Menu/UIManager.cs` | — | `UIManager` 只负责暂停/睡觉/结算面板与返回菜单 |
| 对象池 | `ObjectPool/PoolManager.cs` | — | 雨效 `RainEffectController` |
| 钓鱼 | `FishingGame/` | — | 抛竿抛物线 → 小游戏 |
| 烹饪 | `Cooked/` | `CookedData_SO` | 菜谱 `CookedMenu` |

工具类 `Utillities/`：`Singleton<T>`、`EventHandler`、`Settings`（常量）、`Enums`（全部枚举）、`DataCollection`（`ItemDetails`/`TileDetails`/`InventoryItem`/`GetItemTip`/`AnimatorType`/`CursorAnimator`）、`DataGUID`、`SceneNameAttribute`。

## 事件总线（`Utillities/EventHandler.cs`）

统一模式：`public static event Action<...> XxxEvent` + `public static void CallXxxEvent(...)`。

- 物品背包：`UpdateInventoryUI`、`InstantiateItemInScene`、`DropItemEvent`、`ItemSelectEvent`、`SlotUISelectIndexEvent`、`GetItemTipEvent`、`ItemFirstPos`、`ItemUselessEvent`、`BagFullTipEvent`、`RecerveItemEvent`、`QuitRecerveItemUIEvent`
- 鼠标交互：`MouseClickedEvent`、`MouseHoldEvent`、`MouseUpEvent`、`ExecuteActionAfterAnimation`、`RestoreNormalCursorImageEvent`
- 种植收获：`PlantSeedEvent`、`HarvestAtPlayerPosition`、`GenerateCropEvent`、`RemoveTreeSeedCount`、`DisplayCollectItemSprite`
- 时间：`GameMinuteEvent`、`GameDayEvent`、`GameDateEvent`、`LightShiftChangeEvent`
- 场景：`TransitionEvent`、`BeforeSceneUnloadEvent`、`AfterSceneLoadedEvent`、`MoveToPosition`、`RefreshCurrentMap`、`GetCurrentBuildCode`、`NextTeleportAppearEvent`
- 商店交易：`BaseBagOpenEvent`、`BaseBagCloseEvent`、`ShowTradeUI`、`ClickHaveItemYesEvent`、`OpenBuildShopEvent`、`OpenAnimalShopEvent`、`ShowSettleUIEvent`、`HideSettleUIEvent`、`SettleUIGenerateDoneEvent`、`SettleUIHideDoneEvent`
- 建造家具：`GetCurrentBluPrintDetails`、`BuildFurnitureEvent`、`BuildindModeEvent`、`GetCurrentBuildingDetails`、`InstantiateBuildingOnMapEvent`、`DisplayBuildingArrowIcon`
- 动物：`InstantiateAnimalInScene`、`InstantiateAniamlProduceItemEvent`、`AnimalArrivedAtHomeEvent`、`AnimalExitCoopEvent`
- 玩家：`PlayerDecreaseStminaEvent`、`PlayerDecreaseHealthEvent`、`EquipArmorEvent`、`EquipSlotEvent`、`DoTweenPlayerMoneyChageEvent`、`PlayEatAnimEvent`
- 技能：`IncreasePlantingSkillEvent`、`IncreaseCultivetionSkillEvent`、`IncreaseFishingSkillEvent`、`IncreaseFightSkillEvent`、`IncreaseExploreSkillEvent`
- 对话任务：`ShowDialogueEvent`、`TaskStartEvent`、`UpdateQuestProgressEvent`、`StartNPCEvent`、`PromoteNPCEvent`、`EndNPCEvent`、`IncreaseFriendliness`、`UpdateNPCFriendlinessUIPanel`
- 表现：`ParticleEffectEvent`（`ParticalEffectType`）、`PlaySoundEvent`、`InitSoundEffect`、`FishListInSpriteDayFreshEvent`
- 全局：`UpdateGameStateEvent`（Gameplay/Pause）、`StartNewGameEvent`、`EndGameEvent`、`ControlPlayerBagOpen`、`OpenSleepPanelEvent`、`SleepEvent`、`CheckCookedUIEvent`、`CookedMenuSetupEvent`

## 存档结构（`GameSaveData`）

`dataSceneName`、`characterPosDict`、`sceneItemDict`、`sceneBushDict`、`sceneFurnitureDict`、`sceneBuildingDict`、`tileDetailsDict`、`firstLoadDict`、`inventoryDict`、`timeDict`、`playerMoney`、`targetScene`/`interactable`/`animationInstanceID`（NPC）。
各系统按自己的 key 前缀写入，读取时按 `GUID` 还原。

## 关键常量（`Utillities/Settings.cs`）

- `pixelSize = 0.05`（1 像素 = 0.05 世界单位）、`gridCellSize = 1`
- `secondThreshold = 0.1`（游戏内 1 秒的真实时长）、`dayHold = 28`、`seasonHold = 3`
- `itemFadeDuration = 0.35`、`targetAlpha = 0.45`、`animationBreakTime = 5`
- `morningTime = 05:00`、`nightTime = 19:00`、`lightChangeDuration = 25`
- `playerStartMoney = 10000`、`mouseDownHoldTime = 5`、`downPower = 0.8`

## 数据资产（`Assets/GameData`）

`Audio`、`Crop Data`、`Dialogue`、`Enemy`、`Inventory`、`LightPatten`、`Map Data`、`MineSceneName`、`NPC Schedule`、`NPCEvent`、`Quest Data`、`Scene Route`、`Task`、`TimeLine`。

## 场景（`Assets/Scenes`）

农场与城镇：`Farm`、`Town`、`TownEntrance`、`TownEastStreet`、`TownChurch`、`TownSchool`、`TownTavern`、`TownRanch`、`NorthBeach`、`NorthBeachPath`、`WestForest`、`WestForestEastPath`、`WestManor`、`WestManorPath`、`WestPath`、`WestAltar`、`WestMineEntrance`、`BusStop`、`OriginalGame`、`SceneTemplate`、`UI`。
房屋与室内：`PlayerHouse`、`AdolfHouse`、`AdolfHouseBasement`、`DaTuanHouse`、`DaTuanShop`、`FishShop`、`RanchShop`、`SandorBlacksmith`、`HaniesHouse`、`MaylinHouse`、`WillisHouse`、`HoHoApartmentFirstFloor`、`HoHoApartmentHall`、`HoHoApartment201`、`HoHoApartment202`、`ChickenCoop`、`CowCoop`。
矿洞：`Mine1_1` ~ `Mine1_6`。

## 已知遗留问题（只报告，不擅自修）

1. `Player/PlayerController.cs` 运行时脚本引用了 `UnityEditor`（第 7、11、12 行），打包会失败。
2. `Logic/ItemManager.cs` 的 `OnDisable` 中 `InstantiateAniamlProduceItemEvent` 用了 `+=`（第 99 行），应为 `-=`。
3. `Utillities/Singleton.cs` 写的是 `OnDestory()`（拼写错误，应为 `OnDestroy()`），因此单例引用在销毁时不会被清空。
4. 大量 `ItemManager` / `PlayerController` 单文件超过 40KB，改动时先用 Grep 定位方法，不要整文件通读。
