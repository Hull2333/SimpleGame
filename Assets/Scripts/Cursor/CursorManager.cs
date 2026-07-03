
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MFarm.Map;
using MFarm.CropPlant;
using MFarm.Inventory;
using Unity.VisualScripting;
using System;
using Cinemachine;
using UnityEngine.SceneManagement;
using static AnimalData_SO;
using UnityEditor.Build.Pipeline.Utilities;
using System.Collections;

public class CursorManager : MonoBehaviour  //调用在CursorManager对象上
{
    public Image cursorImage;
    private RectTransform cursorCanvas;
    //其他图标跟随
    private Image otherCursorImage;
    //用于鼠标检测的变量
    private Camera mainCamera;
    private Grid currentGrid;
    public Vector3 mouseWorldPos;
    private Vector3Int mouseGridPos;
    private Vector3Int playerGridPos;
    public bool cursorEnable;
    //可以执行鼠标点击后方法
    private bool canExcuteMouse;
    //鼠标当前选择的物品
    public ItemDetails currentItem;
    public TileDetails currentTile;
    //找到挂了PlayerController脚本的对象位置，即玩家
    private Transform playerTransform => FindObjectOfType<PlayerController>().transform;
    private PlayerController playerController;
    //钓鱼的长按和松开事件不执行
    public bool fishingEventDisable = false;
    //角色背包或设置等UI是否开启
    private bool isUIOpened = false;
    //鼠标动画列表
    public List<CursorAnimator> cursorAnimList;
    //鼠标动画
    private Animator cursorAnim;
    //当前的鼠标类型
    private CursorType currentCursorType;
    //鼠标检测到的碰撞体
    public Collider2D checkCollider;
    //鼠标检查检测的图层
    public LayerMask checkLayer;
    private Collider2D[] roundColliders = new Collider2D[10];
    //可以检查或交谈
    public bool canCheck;
    //可以拍打
    public bool canPat;
    //可烹饪
    private bool canCooked;
    //是否可以检测鼠标的世界坐标
    public bool canDetectionMouseWorldPos;
    //鼠标长按的时间
    public float holdPressDuration;
    private GameState gameState;
    [Header("建筑和家具")]
    //建造模式摄像机位置
    public Transform buildModeCameraPos;
    public CinemachineVirtualCamera camera;
    //鼠标触碰摄像机边缘阈值
    private float cameraEdgeThreshold = 10f;
    //当前为建造模式
    public bool buildMode;
    //每个场景中放置建筑和家具的位置
    private Transform furnitureParent;
    private Transform buildingParent;
    //当前选择的家具和建筑Details
    public BluPrintData_SO bluPrintData;
    public BluPrintDetails currentBluPrintDetails;
    public BuildingDetails currentBuildingDetails;
    [HideInInspector] public GameObject bluPrintPrefab;
    //当前和上一帧的建筑占用Tile
    private List<TileDetails> bluPrintTileList = new List<TileDetails>();
    private List<TileDetails> lastBluPrintTileList = new List<TileDetails>();
    private Bounds lastBounds;
    //记录玩家当前场景和位置
    private string currentScene;
    private Vector3 currentPlayerPos;
    [Header("动物商店相关")]
    //动物选择建筑模式
    private bool animalSelectMode;
    private AnimalDetails currentAnimalDetails;
    private Transform animalParent;
    //当前的建筑活动范围
    [SerializeField] private Collider2D currentBuildingArea;
    //当前的活动区域
    [SerializeField] private int areaCode;
    //当前的激活场景协程
    private IEnumerator currentTranstion;
    private void OnEnable()
    {
        EventHandler.ItemSelectEvent += OnItemSelectEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.ItemUselessEvent += OnItemUselessEvent;
        EventHandler.RestoreNormalCursorImageEvent += OnRestoreNormalCursorImageEvent;
        EventHandler.CheckCookedUIEvent += OnCheckCookedUIEvent;
        EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
        EventHandler.BuildindModeEvent += OnBuildindModeEvent;
        EventHandler.InstantiateAnimalInScene += OnInstantiateAnimalInScene;
    }


    private void OnDisable()
    {
        EventHandler.ItemSelectEvent -= OnItemSelectEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.ItemUselessEvent -= OnItemUselessEvent;
        EventHandler.RestoreNormalCursorImageEvent -= OnRestoreNormalCursorImageEvent;
        EventHandler.CheckCookedUIEvent -= OnCheckCookedUIEvent;
        EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
        EventHandler.BuildindModeEvent -= OnBuildindModeEvent;
        EventHandler.InstantiateAnimalInScene -= OnInstantiateAnimalInScene;
    }

  

    private void Start()
    {
        canDetectionMouseWorldPos = true;
        //关闭鼠标关闭
        Cursor.visible = false;
        playerController = GameObject.FindWithTag("Player").GetComponentInParent<PlayerController>();
        cursorCanvas = GameObject.FindGameObjectWithTag("CursorCanvas").GetComponent<RectTransform>();
        cursorImage = cursorCanvas.GetChild(0).GetComponent<Image>();
        cursorAnim = cursorCanvas.GetChild(0).GetComponent<Animator>();
        //拿到其他光标
        otherCursorImage = cursorCanvas.GetChild(3).GetComponent<Image>();
        //currentSprite = normal;
        //设置平时的光标图像
        cursorAnim.runtimeAnimatorController = cursorAnimList[0].cursorController;
        //SetCursorImage(normal);
        //获取Tag为MainCamera的摄像机
        mainCamera = Camera.main;


    }

    private void Update()
    {
        switch (gameState)
        {
            case GameState.Gameplay:
                cursorEnable = true;
                break;
            case GameState.Pause:
                cursorEnable = false;
                break;
        }
        //根据摄像机的位置来获取鼠标的世界坐标
        if (canDetectionMouseWorldPos)
        {
            mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));
        }
        if (currentGrid != null)
        {
            //再根据获取的世界坐标转化为地图坐标
            mouseGridPos = currentGrid.WorldToCell(mouseWorldPos);
            mouseGridPos = currentGrid.WorldToCell(mouseWorldPos);
            playerGridPos = currentGrid.WorldToCell(playerTransform.position);
        }
        //当光标UI不存在时，不调用光标脚本
        if (cursorCanvas == null)
        {
            return;
        }
        //光标图片随鼠标位置移动
        cursorImage.transform.position = Input.mousePosition;
        otherCursorImage.rectTransform.position = Input.mousePosition;
        //当鼠标移动到任何UI的范围时，图片都变为normal
        if (!InteractWithUI() && cursorEnable)
        {
            CheckCursorValid();
            CheckPlayerInput();
        }
        else if(InteractWithUI() && !cursorEnable)
        {
            
            cursorImage.gameObject.SetActive(true);
            //关闭建造图片、动态鼠标图片
            otherCursorImage.gameObject.SetActive(false);
        }
        if(currentItem == null || currentItem.itemID == 0)
        {
            SwitchCursorImage();
        }
        //点击鼠标左键执行鼠标Clicked动画 
        if (Input.GetMouseButton(0))
        {
            cursorAnim.SetBool("Clicked",true);
            if (animalSelectMode && !InteractWithUI())
            {
                if (CheckBuildingAcceptAnimalSize(currentAnimalDetails.animSize))
                {
                   
                    InventoryUI.Instance.OpenAnimalAskUI(currentAnimalDetails.animalSprite);
                    Debug.Log("这个位置可以");
                }
                else
                {
                    Debug.Log("没法放在这个位置");
                }
            }
            //触摸动物
            if(canPat && !InteractWithUI())
            {
                if(checkCollider.GetComponent<AnimalController>() != null)
                {
                    checkCollider.GetComponent<AnimalController>().AddAnimalFriendliness(0.5f,playerTransform.position);
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            cursorAnim.SetBool("Clicked", false);
        }
        //建造模式下
        if (buildMode)
        {
            if (bluPrintPrefab != null)
            {
                
                bluPrintPrefab.transform.position = mouseGridPos;
                Bounds currentBounds = bluPrintPrefab.GetComponent<BoxCollider2D>().bounds;
                if (currentBounds != lastBounds)
                {
                    UpdateBluPrintCellList(currentBounds, lastBounds);
                    lastBounds = currentBounds;
                }
                GridMapManager.Instance.DisplayBluPrintAvaliableGround(bluPrintTileList, lastBluPrintTileList);
                MoveBuildModeCameraPos();
                if (Input.GetMouseButtonDown(0) && !InteractWithUI() && canExcuteMouse)
                {
                    EventHandler.CallInstantiateBuildingOnMapEvent(currentBuildingDetails, mouseGridPos, buildingParent);
                }
            }
            
        }
        EmptyHandHatvestCrop();

    }
   
    /// <summary>
    /// 获取当前地图的瓦片位置
    /// </summary>
    private void OnAfterSceneLoadedEvent()
    {
       currentGrid = FindObjectOfType<Grid>();
       furnitureParent = FindObjectOfType<FurnitureParent>().transform;
       buildingParent = FindObjectOfType<BuildingParent>().transform;
        animalParent = FindObjectOfType<AnimalParent>().transform;
        if (buildMode)
        {
            bluPrintPrefab = Instantiate(currentBuildingDetails.buildPrefab, buildingParent);
            bluPrintPrefab.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.75f);
            bluPrintPrefab.GetComponent<BuildingItem>().SwitchCollider2D(false);
            EventHandler.CallGetCurrentBuildingDetails(currentBuildingDetails, buildingParent, bluPrintTileList);
        }
    }
    private void OnBeforeSceneUnloadEvent()
    {
        cursorEnable = false;
        GridMapManager.Instance.canGetPlayerPos = false;
        //取消当前选择的物品和恢复鼠标图片为Normal
        InventoryManager.Instance.currentSelectedItem = null;
        cursorAnim.runtimeAnimatorController = cursorAnimList[0].cursorController;
        canCheck = false;
        canPat = false;
    }

    private void OnRestoreNormalCursorImageEvent()
    {
        cursorAnim.runtimeAnimatorController = cursorAnimList[0].cursorController;
        canCheck = false;
    }
   
    /// <summary>
    /// 设置鼠标随物品类型而变化
    /// </summary>
    /// <param name="itemDetails"></param>
    /// <param name="isSelected"></param>
    private void OnItemSelectEvent(ItemDetails itemDetails,bool isSelected)
    {
        canExcuteMouse = isSelected;
        if (bluPrintPrefab != null)
        {
            Destroy(bluPrintPrefab);
        }
        if (!isSelected)
        {
            //当没有选择物品时，动画为Normal
            cursorAnim.runtimeAnimatorController = cursorAnimList[0].cursorController;
            //currentSprite = normal;
            cursorEnable = false;
            currentItem = null;
        }
        //物品被选中才更改鼠标图标
        else
        {
            currentItem = itemDetails;
            
            //利用switch语句循环各种Item类型来返回不同的鼠标动画
            currentCursorType = itemDetails.itemType switch
            {
                ItemType.Seed => CursorType.Normal,
                ItemType.AxeTool => CursorType.Normal,
                ItemType.HoeTool => CursorType.Normal,
                ItemType.WaterTool => CursorType.Normal,
                ItemType.BreakTool => CursorType.Normal,
                ItemType.ReapTool => CursorType.Normal,
                ItemType.Furniture => CursorType.Normal,
                ItemType.Commodity => CursorType.Normal,
                ItemType.Sword => CursorType.Attack,
                ItemType.FishingRod => CursorType.Fishing,
                _ => CursorType.Normal
            };
            cursorEnable = true;
            for (int i = 0; i < cursorAnimList.Count; i++)
            {
                if(currentCursorType == cursorAnimList[i].cursorType)
                {
                    cursorAnim.runtimeAnimatorController = cursorAnimList[i].cursorController;
                }
            }
            //当选择家具物品时，预生成家具
            if(itemDetails.itemType == ItemType.Furniture)
            {
                currentBluPrintDetails = bluPrintData.GetBluPrintDetails(currentItem.itemID);
                bluPrintPrefab = Instantiate(currentBluPrintDetails.buildPrefab, furnitureParent);
                bluPrintPrefab.GetComponent<Furniture>().SetCollider(false);
                bluPrintPrefab.GetComponent<SpriteRenderer>().color = new Color(1,1,1,0.75f);
                EventHandler.CallGetCurrentBluPrintPrefab(currentBluPrintDetails, furnitureParent , bluPrintTileList);
            }
        }
        
    }
    /// <summary>
    /// 在UI开启时禁止使用道具
    /// </summary>
    /// <param name="isbool"></param>
    /// <exception cref="System.NotImplementedException"></exception>
    private void OnItemUselessEvent(bool isbool)
    {
        isUIOpened = isbool;
    }
    private void OnCheckCookedUIEvent(bool cooked)
    {
        canCooked = cooked;
    }
    private void OnUpdateGameStateEvent(GameState state)
    {
        gameState = state;
    }
    private void OnBuildindModeEvent(BuildingDetails building ,AnimalDetails animal, bool startMode)
    {
        if (startMode)
        {
            currentScene = SceneManager.GetActiveScene().name;
            currentPlayerPos = playerTransform.position;
            //切换场景到农场
            EventHandler.CallTransitionEvent("Farm", currentPlayerPos);
            buildModeCameraPos.position = new Vector2(35f, -25f);
            //设置摄像头的跟随和显示比例，生成预生成建筑在地图加载后的事件中
            camera.Follow = buildModeCameraPos;
            camera.m_Lens.OrthographicSize = 20;
            //建造模式
            if (building != null)
            {
                currentBuildingDetails = bluPrintData.GetBuildingDetails(building.ID);
                buildMode = true;
                return;
            }
            //动物选择建筑模式
            if (animal != null)
            {
                currentAnimalDetails = animal;
                animalSelectMode = true;
                buildMode = false;
                StartCoroutine(ShowBuindingIcon());
                return;
            }
        }
        else
        {
            if (bluPrintPrefab != null)
            {
                Destroy(bluPrintPrefab);
            }
            EventHandler.CallTransitionEvent(currentScene, currentPlayerPos);
            camera.Follow = playerTransform;
            camera.m_Lens.OrthographicSize = 7;
            currentBuildingDetails = null;
            buildMode = false;
            animalSelectMode = false;
        }
    }
    private IEnumerator ShowBuindingIcon()
    {
        yield return new WaitForSeconds(1f);
        EventHandler.CallDisplayBuildingArrowIcon(currentAnimalDetails.animSize, true);
    }
    private void OnInstantiateAnimalInScene(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            var animalInScene = Instantiate(currentAnimalDetails.animalPrefab, animalParent);
            animalInScene.GetComponent<AnimalController>().animalDetails = currentAnimalDetails;
            animalInScene.GetComponent<AnimalController>().animCodeID = areaCode;
            animalInScene.GetComponent<AnimalController>().activityArae = currentBuildingArea;
            animalInScene.GetComponent<AnimalController>().isOutSide = true;
            animalInScene.GetComponent<AnimalController>().SetStartState(true);
        }
    }
    /// <summary>
    /// 鼠标移动到对应物体切换成不一样的鼠标图片
    /// </summary>
    private void SwitchCursorImage()
    {
        //任何背包UI打开都不换检查图片
        if (!InventoryManager.Instance.anyBagOpened)
        {
            checkCollider = Physics2D.OverlapPoint(mouseWorldPos, checkLayer);
            if (checkCollider != null)
            {
                //NPC或者可调查事物
                if (checkCollider.CompareTag("NPC") || checkCollider.CompareTag("Investigable"))
                {
                    cursorAnim.runtimeAnimatorController = cursorAnimList[1].cursorController;
                    canCheck = true;
                    return;
                }
                //可互动事物
                if (checkCollider.CompareTag("Interactive") || checkCollider.CompareTag("Animal"))
                {
                    cursorAnim.runtimeAnimatorController = cursorAnimList[4].cursorController;
                    canPat = true;
                    return;
                }
                //可烹饪事物
                if (checkCollider.CompareTag("Cooked"))
                {
                    cursorAnim.runtimeAnimatorController = cursorAnimList[5].cursorController;
                    if (canCooked && Input.GetMouseButtonDown(0))
                    {
                        EventHandler.CallCookedMenuSetupEvent();
                    }
                    return;
                }
            }
            else
            {
                cursorAnim.runtimeAnimatorController = cursorAnimList[0].cursorController;
                canCheck = false;
                canPat = false;
            }

        }
        else
        {
            cursorAnim.runtimeAnimatorController = cursorAnimList[0].cursorController;
            canCheck = false;
            canPat = false;
        }
    }
    /// <summary>
    /// 检查点击的建筑是否是合适的动物尺寸
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public bool CheckBuildingAcceptAnimalSize(AnimalSizeType size)
    {
       
        checkCollider = Physics2D.OverlapPoint(mouseWorldPos, checkLayer);
        if(checkCollider != null && checkCollider.GetComponent<BuildingItem>())
        {
            if (checkCollider.GetComponent<BuildingItem>().acceptSize == size)
            {
                currentBuildingArea = checkCollider.GetComponent<BuildingItem>().animalArea;
                areaCode = checkCollider.GetComponent<BuildingItem>().buildCodeID;
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// 空手收获农作物
    /// </summary>
    private void EmptyHandHatvestCrop()
    {
        if(Mathf.Abs(mouseGridPos.x - playerGridPos.x) <= 2 && Mathf.Abs(mouseGridPos.y - playerGridPos.y) <= 2)
        {
            if (currentItem == null || currentItem.itemID == 0)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Crop clickCrop = GridMapManager.Instance.GetCropObject(mouseWorldPos);
                    ItemClick clickItem = GridMapManager.Instance.GetItemObject(mouseWorldPos);
                    if (clickCrop != null && clickCrop.canHarvest)
                    {
                        //播放玩家收获动画
                        EventHandler.CallDisplayCollectItemSprite(clickCrop.cropDetails.producedItemID[0]);
                        clickCrop.ProcessCropAction();
                        return;
                    }
                    if(clickItem != null)
                    {
                        //播放玩家收获动画
                        EventHandler.CallDisplayCollectItemSprite(clickItem.itemID);
                        clickItem.AddItemToBag();
                    }
                }
            }
        }
       
        
    }
    /// <summary>
    /// 鼠标点击物品时执行的操作
    /// </summary>
    public void CheckPlayerInput()
    {
        if (currentItem == null)
        {
            return;
        }
        //当所有UI都未开启时才可以使用道具
        if (isUIOpened == false)
        {
            if (currentItem.itemType == ItemType.Sword)
            {
                //点击左键攻击
                if (Input.GetMouseButtonDown(0))
                {
                    EventHandler.CallMouseClickedEvent(mouseWorldPos, currentItem);
                }
                //点击右键防御
                if (Input.GetMouseButtonDown(1) && !playerController.isKnocking)
                {
                    StartCoroutine(playerController.PlayerDefence(mouseWorldPos));
                }
                //按下F处决
                if (Input.GetKeyDown(KeyCode.F) && CheckRoundEnemy())
                {
                    StartCoroutine(playerController.PlayerExecution());
                }
            }
            if (currentItem.itemType == ItemType.FishingRod)
            {
                //长按鼠标左键
                if (Input.GetMouseButton(0) && fishingEventDisable == false)
                {
                    EventHandler.CallControlPlayerBagOpen(false);
                    holdPressDuration += Time.deltaTime;
                    if (holdPressDuration >= 0.2f)
                    {
                        canDetectionMouseWorldPos = false;
                        EventHandler.CallMouseHoldEvent(mouseWorldPos, currentItem);
                    }

                }
                //松开鼠标长按
                if (Input.GetMouseButtonUp(0) && fishingEventDisable == false)
                {
                    if (holdPressDuration >= 0.2f)
                    {
                        holdPressDuration = 0;
                        canDetectionMouseWorldPos = true;
                        EventHandler.CallMouseUpEvent();
                    }
                    else
                    {
                        EventHandler.CallControlPlayerBagOpen(true);
                        holdPressDuration = 0;
                    }

                }
               
            }
            //挤奶
            if (currentItem.itemType == ItemType.Bucket)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    checkCollider.GetComponent<AnimalController>().ProduceItemOnTransform(playerTransform.position);
                }
            }
            else
            {
                if (canExcuteMouse)
                {
                    //点击左键
                    if (Input.GetMouseButtonDown(0))
                    {
                        EventHandler.CallMouseClickedEvent(mouseWorldPos, currentItem);
                    }
                    //点击右键吃东西
                    if (Input.GetMouseButtonDown(1))
                    {
                        if (currentItem.canEat == true)
                        {
                            EventHandler.CallPlayEatAnimEvent(currentItem.itemID);
                        }
                    }
                }

            }
        }
    }
    private void UpdateBluPrintCellList(Bounds bounds , Bounds lastBounds)
    {
        bluPrintTileList.Clear();
        lastBluPrintTileList.Clear();
        Vector3Int minCell = currentGrid.WorldToCell(bounds.min);
        Vector3Int maxCell = currentGrid.WorldToCell(bounds.max);
        Vector3Int minLastCell = currentGrid.WorldToCell(lastBounds.min);
        Vector3Int maxLastCell = currentGrid.WorldToCell(lastBounds.max);
        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                TileDetails newBluPrintTile = GridMapManager.Instance.GetTileDetailsOnMousePosition(new Vector3Int(x, y, 0));
                if (newBluPrintTile != null)
                {
                    bluPrintTileList.Add(newBluPrintTile);
                }
                //检查该家具或建筑是否可以放置在这里
                foreach (var tile in bluPrintTileList)
                {
                    canExcuteMouse = true;
                    if (!tile.canPlaceFurniture || tile.havePlace == 1)
                    {
                        canExcuteMouse = false;
                        break;
                    }
                }

            }
        }
        for (int x = minLastCell.x; x <= maxLastCell.x; x++)
        {
            for (int y = minLastCell.y; y <= maxLastCell.y; y++)
            {
                TileDetails LastBluPrintTile = GridMapManager.Instance.GetTileDetailsOnMousePosition(new Vector3Int(x, y, 0));
                if (LastBluPrintTile != null)
                {
                    lastBluPrintTileList.Add(LastBluPrintTile);
                }

            }
        }
    }
    /// <summary>
    /// 检查鼠标是否可用
    /// </summary>
    private void CheckCursorValid()
    {
        
        //判断是否鼠标丢弃的位置超过该物品的使用范围
        //if(currentItem != null && currentItem.canDropped)
        //{
        //    if (Mathf.Abs(mouseGridPos.x - playerGridPos.x) > currentItem.itemUseRadius || Mathf.Abs(mouseGridPos.y - playerGridPos.y) > currentItem.itemUseRadius)
        //    {
        //        SetCursorInvalid();
        //        return;
        //    }
        //}
        currentTile = GridMapManager.Instance.GetTileDetailsOnMousePosition(mouseGridPos);
        //预生成家具跟随鼠标的Grid坐标
        if (bluPrintPrefab != null)
        {
            bluPrintPrefab.transform.position = mouseGridPos;
            Bounds currentBounds = bluPrintPrefab.GetComponent<BoxCollider2D>().bounds;
            if (currentBounds != lastBounds)
            {
                UpdateBluPrintCellList(currentBounds, lastBounds);
                lastBounds = currentBounds;
            }
        }
        //根据选择的物品类型来决定鼠标是否可用
        if (currentTile != null && currentItem != null)
        {
            //获取当前瓦片上的种子信息
            CropDetails currentCrop = CropManager.Instance.GetCropDetails(currentTile.seedItemID);
            //获取鼠标点击位置的Crop脚本组件
            Crop crop = GridMapManager.Instance.GetCropObject(mouseWorldPos);
            
            switch (currentItem.itemType)
            {
                case ItemType.Seed:
                    //GridMapManager.Instance.QuitDigAvailableGround();
                    GridMapManager.Instance.DisplayerAvailableGround(currentItem, currentTile);
                    //SetCursorValid();
                    break;
                case ItemType.Commodity:
                    //GridMapManager.Instance.QuitDigAvailableGround();
                    //当前选择的瓦片是可丢瓦片且选择的物品是可以丢弃的
                    if (currentTile.canDropItem && currentItem.canDropped)
                    {
                        //SetCursorValid();
                    }
                    break;
                case ItemType.TreeSeed:
                case ItemType.WaterTool:
                //判断选择的瓦片是否可以挖坑
                case ItemType.HoeTool:
                    GridMapManager.Instance.DisplayerAvailableGround(currentItem,currentTile);
                    //SetCursorValid();
                    break;
                case ItemType.AxeTool:
                    //GridMapManager.Instance.QuitDigAvailableGround();
                    if (crop != null)
                    {
                        if (crop.canHarvest == true && crop.cropDetails.CheckToolAvailable(currentItem.itemID)==true)
                        {
                            
                            //SetCursorValid();
                        }
                        else
                        {
                            //SetCursorIncompleteInvalid();
                        }
                    }
                    else
                    {
                        //SetCursorIncompleteInvalid();
                    }
                    break;
                case ItemType.ReapTool:
                case ItemType.Sword:
                case ItemType.BreakTool:
                    //SetCursorValid();
                    //GridMapManager.Instance.QuitDigAvailableGround();
                    break;
                //图纸
                case ItemType.Furniture:
                    GridMapManager.Instance.DisplayBluPrintAvaliableGround(bluPrintTileList, lastBluPrintTileList);
                    //GridMapManager.Instance.QuitDigAvailableGround();
                    break;
                case ItemType.FishingRod:
                    break;
                case ItemType.Bucket:
                    checkCollider = Physics2D.OverlapPoint(mouseWorldPos, checkLayer);
                    if (checkCollider.GetComponent<AnimalController>().isAdult)
                    {
                        cursorAnim.runtimeAnimatorController = cursorAnimList[4].cursorController;
                       
                    }
                    break;

            }
        }
    }
    /// <summary>
    /// 是否与UI互动
    /// </summary>
    /// <returns></returns>
    private bool InteractWithUI()
    {
       
        if (canDetectionMouseWorldPos)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return true;
            }
            return false;
        }
        return false;

    }
    /// <summary>
    /// 检测玩家周围是否有眩晕的敌人
    /// </summary>
    /// <returns></returns>
    private bool CheckRoundEnemy()
    {
        
        //Collider2D[] roundColliders = new Collider2D[10];
        Gizmos.color = Color.yellow;
        int count = Physics2D.OverlapCircleNonAlloc(playerTransform.position,5f,roundColliders);
        for (int i = 0; i < count; i++)
        {
            Collider2D collider = roundColliders[i];
            if (collider.GetComponent<FSM>() && collider.GetComponent<FSM>().parameter.isFainting)
            {
                return true;
            }
        }
        return false;
    }
   /// <summary>
   /// 建造模式下触碰摄像机边缘移动摄像机
   /// </summary>
    private void MoveBuildModeCameraPos()
    {
        Vector3 mousePos = Input.mousePosition;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        // 检测鼠标是否在屏幕边缘
        bool isLeftEdge = mousePos.x <= cameraEdgeThreshold;
        bool isRightEdge = mousePos.x >= screenWidth - cameraEdgeThreshold;
        bool isBottomEdge = mousePos.y <= cameraEdgeThreshold;
        bool isTopEdge = mousePos.y >= screenHeight - cameraEdgeThreshold;
        if (isLeftEdge)
        {
            buildModeCameraPos.Translate(Vector3.left * 20f * Time.deltaTime);
        }
        if (isRightEdge)
        {
            buildModeCameraPos.Translate(Vector3.right * 20f * Time.deltaTime);
        }
        if (isBottomEdge)
        {
            buildModeCameraPos.Translate(Vector3.down * 20f * Time.deltaTime);
        }
        if (isTopEdge)
        {
            buildModeCameraPos.Translate(Vector3.up * 20f * Time.deltaTime);
        }
    }

}
