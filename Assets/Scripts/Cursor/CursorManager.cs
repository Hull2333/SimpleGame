
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MFarm.Map;
using MFarm.CropPlant;
using MFarm.Inventory;
using Unity.VisualScripting;


public class CursorManager : MonoBehaviour  //调用在CursorManager对象上
{
    public Image cursorImage;
    private RectTransform cursorCanvas;
    //建造图标跟随
    private Image buildImage;
    //其他图标跟随
    private Image otherCursorImage;
    //用于鼠标检测的变量
    private Camera mainCamera;
    private Grid currentGrid;
    public Vector3 mouseWorldPos;
    private Vector3Int mouseGridPos;
    private Vector3Int playerGridPos;
    public bool cursorEnable;
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
    //是否可以检测鼠标的世界坐标
    public bool canDetectionMouseWorldPos;
    private void OnEnable()
    {
        EventHandler.ItemSelectEvent += OnItemSelectEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.ItemUselessEvent += OnItemUselessEvent;
        EventHandler.RestoreNormalCursorImageEvent += OnRestoreNormalCursorImageEvent;
    }


    private void OnDisable()
    {
        EventHandler.ItemSelectEvent -= OnItemSelectEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.ItemUselessEvent -= OnItemUselessEvent;
        EventHandler.RestoreNormalCursorImageEvent -= OnRestoreNormalCursorImageEvent;
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
        //拿到建造光标
        buildImage = cursorCanvas.GetChild(2).GetComponent<Image>();
        buildImage.gameObject.SetActive(false);
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
        //根据摄像机的位置来获取鼠标的世界坐标
        if (canDetectionMouseWorldPos)
        {
            mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));
        }
        if (currentGrid != null)
        {
            //再根据获取的世界坐标转化为地图坐标
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
        if (!InteractWithUI() && cursorEnable )
        {
            CheckCursorValid();
            CheckPlayerInput();
        }
        else if(InteractWithUI() && !cursorEnable)
        {
            
            cursorImage.gameObject.SetActive(true);
            //关闭建造图片、动态鼠标图片
            buildImage.gameObject.SetActive(false);
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
        }
        if (Input.GetMouseButtonUp(0))
        {
            cursorAnim.SetBool("Clicked", false);
        }
        EmptyHandHatvestCrop();

    }
    /// <summary>
    /// 鼠标点击物品时执行的操作
    /// </summary>
    public void CheckPlayerInput()
    {
        //当所有UI都未开启时才可以使用道具
        if (isUIOpened == false) 
        {
            if (currentItem.itemType == ItemType.FishingRod)
            {
                //长按鼠标左键
                if (Input.GetMouseButton(0) && fishingEventDisable == false)
                {
                    canDetectionMouseWorldPos = false;
                    EventHandler.CallMouseHoldEvent(mouseWorldPos, currentItem);
                }
                //松开鼠标长按
                if (Input.GetMouseButtonUp(0) && fishingEventDisable == false)
                {
                    canDetectionMouseWorldPos = true;
                    EventHandler.CallMouseUpEvent();
                }
            }
            if(currentItem.itemType == ItemType.Sword)
            {
                //点击左键攻击
                if (Input.GetMouseButtonDown(0))
                {
                    EventHandler.CallMouseClickedEvent(mouseWorldPos, currentItem);
                }
                //点击右键防御
                if(Input.GetMouseButtonDown(1) && !playerController.isKnocking)
                {
                    StartCoroutine(playerController.PlayerDefence(mouseWorldPos));
                }
                //按下F处决
                if (Input.GetKeyDown(KeyCode.F) && CheckRoundEnemy())
                {
                    StartCoroutine(playerController.PlayerExecution());
                }
            }
            else
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
                        //吃东西
                        EventHandler.CallUseItemRecoverEvent(currentItem, playerController);
                    }
                }
            }
        }
    }
    /// <summary>
    /// 获取当前地图的瓦片位置
    /// </summary>
    private void OnAfterSceneLoadedEvent()
    {
       currentGrid = FindObjectOfType<Grid>();
    }
    private void OnBeforeSceneUnloadEvent()
    {
        cursorEnable = false;
        GridMapManager.Instance.canGetPlayerPos = false;
    }

    private void OnRestoreNormalCursorImageEvent()
    {
        cursorAnim.runtimeAnimatorController = cursorAnimList[0].cursorController;
        canCheck = false;
    }
    /// <summary>
    /// 设置鼠标图片
    /// </summary>
    /// <param name="sprite"></param>
    public void SetCursorImage(Sprite sprite)
    {
        cursorImage.sprite = sprite;
        cursorImage.color = new Color(1, 1, 1, 1);
    }
    #region 设置鼠标样式
    /// <summary>
    /// 设置鼠标可用状态
    /// </summary>
    //private void SetCursorValid()
    //{
    //    cursorImage.color = new Color(1, 1, 1, 1);
    //    buildImage.color = new Color(1, 1, 1, 0.5f);
    //}
    /// <summary>
    /// 设置鼠标不可用状态
    /// </summary>
    //private void SetCursorInvalid()
    //{
    //    cursorImage.color = new Color(1,0,0,0.4f);
    //    buildImage.color = new Color(1, 0, 0, 0.5f);
    //}
    /// <summary>
    /// 设置鼠标可用但颜色变为不可以
    /// </summary>
    //private void SetCursorIncompleteInvalid()
    //{
    //    cursorImage.color = new Color(1, 0, 0, 0.4f);
    //    buildImage.color = new Color(1, 0, 0, 0.5f);
    //}
   
    #endregion
    /// <summary>
    /// 设置鼠标随物品类型而变化
    /// </summary>
    /// <param name="itemDetails"></param>
    /// <param name="isSelected"></param>
    private void OnItemSelectEvent(ItemDetails itemDetails,bool isSelected)
    {
        
        if (!isSelected)
        {
            //当没有选择物品时，动画为Normal
            cursorAnim.runtimeAnimatorController = cursorAnimList[0].cursorController;
            //currentSprite = normal;
            cursorEnable = false;
            currentItem = null;
            buildImage.gameObject.SetActive(false);
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
            //当选择家具图纸物品时，显示建造家具图片
            if(itemDetails.itemType == ItemType.Furniture)
            {
                buildImage.gameObject.SetActive(true);
                buildImage.sprite = itemDetails.itemOnWorldSprite;
                //强制设置家具图片为默认大小
                buildImage.SetNativeSize();
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
                if (checkCollider.CompareTag("NPC") || checkCollider.CompareTag("BulletinBoard"))
                {
                    cursorAnim.runtimeAnimatorController = cursorAnimList[1].cursorController;
                    canCheck = true;
                }
                if (checkCollider.CompareTag("Door"))
                {
                    cursorAnim.runtimeAnimatorController = cursorAnimList[4].cursorController;
                    canPat = true;
                }
            }
            else
            {
                cursorAnim.runtimeAnimatorController = cursorAnimList[0].cursorController;
                canCheck = false;
                canPat = false;
            }
        }
        

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
                    if (clickCrop != null && clickCrop.canHarvest)
                    {
                        //播放玩家收获动画
                        EventHandler.CallDisplayCollectItemSprite(clickCrop.cropDetails.producedItemID[0]);
                        clickCrop.ProcessCropAction();
                    }
                }
            }
        }
       
    }
    /// <summary>
    /// 检查鼠标是否可用
    /// </summary>
    private void CheckCursorValid()
    {
        
        //建造图片跟随鼠标移动
        buildImage.rectTransform.position = Input.mousePosition;
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
        //if(currentItem != null)
        //{
        //    SetCursorValid();
        //}
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
                case ItemType.cooked:
                    //GridMapManager.Instance.QuitDigAvailableGround();
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
                    //GridMapManager.Instance.QuitDigAvailableGround();
                    buildImage.gameObject.SetActive(true);
                    //拿到当前选择的家具蓝图
                    var bluePrintDetails = InventoryManager.Instance.bluPrintData.GetBluPrintDetails(currentItem.itemID);

                    if (currentTile.canPlaceFurniture && InventoryManager.Instance.CheckStock(currentItem.itemID) && !HaveFurnitrueInRadius(bluePrintDetails))
                    {
                        //SetCursorValid();
                    }
                    break;
                case ItemType.FishingRod:
                    //GridMapManager.Instance.QuitDigAvailableGround();
                    if (currentTile.canLaterFishing || currentTile.canSeaFishing)
                    {
                       // SetCursorValid();
                    }
                    else
                    {
                        //SetCursorIncompleteInvalid();
                    }
                    break;   
            }
            //采集时鼠标可用
            //if (currentCrop != null)
            //{
            //    if (currentTile.growthDays >= currentCrop.totalGrowthDays)
            //    {
            //        SetCursorValid();
                    
            //    }
            //    else
            //    {
            //        SetCursorInvalid();
            //    }
            //}
        }
        //当该瓦片上什么信息都没有时，直接调用鼠标不可用
        //if(currentTile == null)
        //{
        //    SetCursorInvalid();
        //}
    }
    /// <summary>
    /// 检查建造范围内是否有其他家具
    /// </summary>
    /// <param name="bluePrintDetails"></param>
    /// <returns></returns>
    private bool HaveFurnitrueInRadius(BluPrintDetails bluePrintDetails)
    {
        var buildItem = bluePrintDetails.buildPrefab;
        Vector2 point = mouseWorldPos;
        var size = buildItem.GetComponent<BoxCollider2D>().size;
        //起点，范围，角度,OverlapBox也会检测Trigger的coll，想要忽略被OverlapBox检测，把layer改为Ignore Raycast
        var otherColl = Physics2D.OverlapBox(point, size, 0);
        if(otherColl != null)
        {
            return otherColl.GetComponent<Furniture>();
        }
        return false;
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
   
}
