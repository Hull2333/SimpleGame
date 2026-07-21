
using MFarm.Inventory;
using MFarm.Map;
using MFarm.Save;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using static UnityEngine.RuleTile.TilingRuleOutput;
using Transform = UnityEngine.Transform;
using UnityEngine.SceneManagement;
using MFarm.AStar;
using System.Linq;
public class PlayerController : MonoBehaviour, ISaveable  //调用在Player对象上
{
    [Header("玩家属性")]
    public float speed;
    public float tiredSpeed;
    public float maxHealth;
    public float currentHealth;
    public float maxStmina;
    private int defenceValue;
    [HideInInspector] public float currentStmina;
    //剑防御和处决CD
    private float swordDefenceCD = 1f;
    public Collider2D playerCollider;
    public PlayerAttack playerAttack;
    [Header("玩家等级")]
    //种植、养殖、钓鱼、战斗、探索
    //等级、最大经验值、当前经验值
    [HideInInspector] public int plantingSkill;
    [HideInInspector] public int plantingMaxEXP;
    [HideInInspector] public int plantingCurrentEXP;
    [HideInInspector] public int cultivetionSkill;
    [HideInInspector] public int cultivetionMaxEXP;
    [HideInInspector] public int cultivetionCurrentEXP;
    [HideInInspector] public int fishingSkill;
    [HideInInspector] public int fishingMaxEXP;
    [HideInInspector] public int fishingCurrentEXP;
    [HideInInspector] public int fightSkill;
    [HideInInspector] public int fightMaxEXP;
    [HideInInspector] public int fightCurrentEXP;
    [HideInInspector] public int exploreSkill;
    [HideInInspector] public int exploreMaxEXP;
    [HideInInspector] public int exploreCurrentEXP;
    [Header("-------------------------------------------------------")]
    [Header("受击参数")]
    private bool invulnerable;
    public float invulnerableDuration;
    //受击的颜色变化
    private SpriteRenderer[] sr;
    private Color originalColor;
    public Rigidbody2D rb;
    //受击后退的距离、时间、移动程度
    private float knockbackDistance = 1f;
    private float knockbackDuration = 0.3f;
    public AnimationCurve knockbackCurve;
    [HideInInspector] public bool isKnocking = false;
    [SerializeField] private GameObject damageNumPrefab;
    [Header("移动相关")]
    //获取玩家输入的移动
    private float inputX;
    private float inputY;
    //获取玩家输入的移动方向
    private Vector2 movementInput;
    //玩家是否正在移动
    [HideInInspector] public bool isMoving;
    [HideInInspector] public bool inputDisable;
    //动画使用工具面朝方向
    private float mouseX;
    private float mouseY;
    //获取该对象的GUID
    public string GUID => GetComponent<DataGUID>().guid;
    //人物面朝方向
    private bool faceUp, faceDown, faceLeft, faceRight;
    public GameObject cursorManager;
    private CursorManager cM;
    //玩家动作组
    public GameObject[] playerAnimatorGroup;
    //获取Player上所有的Animator
    [HideInInspector] public List<Animator> animators = new List<Animator>();
    [Header("钓鱼系统")]
    //鱼钩生成位置和落点、鱼钩抛物线中点、鱼线中点
    public Transform baitAppearPos;
    private Vector3 baitEndPos;
    private Vector3 baitParabolaMidPos;
    private Vector3 fishlineMidPos;
    //是否在抛竿中
    public bool isBait = false;
    //鱼钩所在瓦片
    private Grid baitGrid;
    //钓鱼中
    private bool isFishing;
    //钓鱼感叹号
    public GameObject exclamatonMark;
    public bool canCatchFish = false;
    private bool exclamationEnable;
    //钓鱼随机时间
    private float fishingRandomTime;
    //钓鱼计时器
    private float currentFishingTime;
    //钓鱼小游戏
    public GameObject fishBg;
    private int randomFishIndex;
    //是否正在钓鱼游戏
    private bool isFishingGame;
    public GameObject readyText;
    [Header("鱼与抛物线")]
    private GameObject fish;
    //是否正在收鱼
    public bool fishMove = false;
    //是否显示鱼线
    private bool isFishLine = false;
    //鱼线
    public LineRenderer fishLine;
    //蓄力条画布
    public GameObject fishPowerProcessCanvas;
    //鼠标长按时间
    public float mouseDownTimer;
    //鼠标长按时间是否增加
    private bool isMouseDownTimerIncrease;
    //鼠标蓄力条
    public UnityEngine.UI.Image mouseDownProcess;
    //鱼钩游戏对象
    public GameObject baitGameObject;
    //鱼钩抛出速度
    public float baitSpeed;
    //鱼钩抛物线路径点列表
    private List<Vector3> baitParabolaPointList = new List<Vector3>();
    //鱼的抛物线路径的列表
    private List<Vector3> fishParabolaPointList = new List<Vector3>();
    //鱼钩抛物线的长度
    public float totalBaitParabolaLength;
    //鱼的抛物线长度
    private float totalFishParabolaLength;
    //当前鱼钩已抛出的长度
    private float currentBaitParabolaLength = 0;
    //当前鱼已抛出的长度
    private float currentFishParabolaLength = 0;
    //当前鱼钩到达的路径点
    public int baitParabolaPointIndex = 0;
    //当前鱼到达的路径的
    private int fishParabolaPintIndex = 0;
    //鱼钩位置
    private Vector3 baitPos;
    //鱼的位置
    private Vector3 fishPos;
    //鱼钩移动方向
    private Vector3 baitDir;
    //鱼的方向
    private Vector3 fishDir;
    //从抛竿开始计时
    public float baitInSkyTime;
    //鱼在空中时开始计时
    public float fishInSkyTime;
    //鱼钩抛出时的旋转角度
    public AnimationCurve baitRotationCurve;
    //生成的鱼
    public GameObject fishPrefab;
    //鱼的图片
    public GameObject fishItem;
    //生成鱼的信息
    private ItemDetails fishDetails;
    //确认渔获
    private bool confirmFishGet = false;
    [Header("慢动作设置")]
    public float normalTimeScale = 1f;
    public float slowMotionTimeScale = 0.1f;
    public float transtionDuration = 0.5f;
    public float currentTransitionTime;
    public bool isSlowMotionTime;
    //防御碰撞特效
    public GameObject[] defenceEffects;
    public int effectIndex;
    //正在继续NPC事件
    public bool isNpcEvent;
    //NPC开始时的位置
    private Vector2 npcEventOriginPos;
    public void Awake()
    {

        foreach (var playerAnim in playerAnimatorGroup)
        {
            animators.Add(playerAnim.GetComponent<Animator>());
        }
        rb = GetComponent<Rigidbody2D>();
        //获取所有子物体下的Animator组件
        inputDisable = true;
        cM = cursorManager.GetComponent<CursorManager>();
        //获取所有子物体下的SpriteRenderer组件
        sr = GetComponentsInChildren<SpriteRenderer>();
    }

    private void Start()
    {

        currentHealth = maxHealth;
        currentStmina = maxStmina;
        ISaveable saveable = this;
        //执行ISaveable接口的方法
        saveable.RegisterSaveable();

    }
    void Update()
    {
        if (inputDisable == false)
        {
            PlayerInput();
        }
        else
        {
            //禁止对玩家操作时玩家停止移动
            isMoving = false;
        }
        SwitchAnimation();
        //执行慢动作
        if (isSlowMotionTime)
        {
            if(currentTransitionTime < transtionDuration)
            {
                currentTransitionTime += Time.unscaledDeltaTime;
                float t = currentTransitionTime / transtionDuration;
                t = Mathf.SmoothStep(0, 1, t);
                Time.timeScale = Mathf.Lerp(
                    slowMotionTimeScale,
                    normalTimeScale,
                    t
                    );
            }
            else
            {
                isSlowMotionTime = false;
            }
        }
        if (isBait)
        {
            BaitMoveAlongTheParabola();

            //ThrowBait();
        }
        //等待鱼上钩中
        if (isFishing)
        {
            //钓鱼中且不在钓鱼游戏时点击鼠标右键可中断钓鱼
            //if (Input.GetMouseButtonDown(1) && !isFishingGame)
            //{
            //    ExitFishing();
            //}
            if (exclamationEnable)
            {
                PlayerFishing();
            }
            StartFishGame();
        }
        //确认渔获后恢复站立状态
        if (confirmFishGet && Input.GetMouseButton(0))
        {
            foreach (var anim in animators)
            {
                anim.SetBool("Tool5", false);
            }
            fishItem.SetActive(false);
            inputDisable = false;
            fishMove = false;
            confirmFishGet = false;
            cM.fishingEventDisable = false;
            StartCoroutine(WaitTime());
            EventHandler.CallControlPlayerBagOpen(true);

        }
    }
    private void FixedUpdate()
    {
        if (inputDisable == false)
        {
            Movement();
        }
        if (fishMove)
        {
            FishMoveToPlayer();
        }
        if (isFishLine)
        {
            //随时获取鱼线中点
            fishlineMidPos = GetMidPoint(baitAppearPos.position, baitGameObject.transform.position, Settings.fishLinePercent, Settings.fishLineOffsetY);
            //fishLine.positionCount = 50;
            //绘制鱼线
            for (int i = 0; i < 50; i++)
            {
                Vector3 point = CalculateBezierPoint(baitAppearPos.position, fishlineMidPos, baitGameObject.transform.position, (i / 50.0f));
                fishLine.SetPosition(i, point);

            }
        }

    }
    private void OnEnable()
    {
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.MoveToPosition += OnMoveToPosition;
        EventHandler.MouseClickedEvent += OnMouseClickedEvent;
        EventHandler.MouseHoldEvent += OnMouseHoldEvent;
        EventHandler.MouseUpEvent += OnMouseUpEvent;
        EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
        //开始新游戏事件
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        //结束游戏的事件
        EventHandler.EndGameEvent += OnEndGameEvent;
        EventHandler.PlayerDecreaseStminaEvent += OnPlayerDecreaseStminaEvent;
        EventHandler.PlayerDecreaseHealthEvent += OnPlayerDecreaseHealthEvent;
        EventHandler.IncreasePlantingSkillEvent += OnIncreasePlantingSkillEvent;
        EventHandler.IncreaseCultivetionSkillEvent += OnIncreaseCultivetionSkillEvent;
        EventHandler.IncreaseFishingSkillEvent += OnIncreaseFishingSkillEvent;
        EventHandler.IncreaseFightSkillEvent += OnIncreaseFightSkillEvent;
        EventHandler.IncreaseExploreSkillEvent += OnIncreaseExploreSkillEvent;
        EventHandler.DisplayCollectItemSprite += OnDisplayCollectItemSprite;
        EventHandler.EquipArmorEvent += OnEquipArmorEven;
        EventHandler.StartNPCEvent += OnStartNPCEvent;
        EventHandler.EndNPCEvent += OnEndNPCEvent;
        EventHandler.PlayEatAnimEvent += OnPlayEatAnimEvent;
    }

   

    private void OnDisable()
    {
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.MoveToPosition -= OnMoveToPosition;
        EventHandler.MouseClickedEvent -= OnMouseClickedEvent;
        EventHandler.MouseHoldEvent -= OnMouseHoldEvent;
        EventHandler.MouseUpEvent -= OnMouseUpEvent;
        EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
        EventHandler.PlayerDecreaseStminaEvent -= OnPlayerDecreaseStminaEvent;
        EventHandler.PlayerDecreaseHealthEvent -= OnPlayerDecreaseHealthEvent;
        EventHandler.IncreasePlantingSkillEvent -= OnIncreasePlantingSkillEvent;
        EventHandler.IncreaseCultivetionSkillEvent -= OnIncreaseCultivetionSkillEvent;
        EventHandler.IncreaseFishingSkillEvent -= OnIncreaseFishingSkillEvent;
        EventHandler.IncreaseFightSkillEvent -= OnIncreaseFightSkillEvent;
        EventHandler.IncreaseExploreSkillEvent -= OnIncreaseExploreSkillEvent;
        EventHandler.DisplayCollectItemSprite -= OnDisplayCollectItemSprite;
        EventHandler.EquipArmorEvent -= OnEquipArmorEven;
        EventHandler.StartNPCEvent -= OnStartNPCEvent;
        EventHandler.EndNPCEvent -= OnEndNPCEvent;
        EventHandler.PlayEatAnimEvent -= OnPlayEatAnimEvent;
    }

    

    private void OnMouseClickedEvent(Vector3 mouseWorldPos, ItemDetails itemDetails)
    {
        //TODO:执行动画
        if (itemDetails.itemType != ItemType.Seed && itemDetails.itemType != ItemType.Commodity && itemDetails.itemType != ItemType.Furniture && itemDetails.itemType != ItemType.FishingRod && itemDetails.itemType != ItemType.Cooked)
        {
            //判断使用工具时面朝方向
            mouseX = mouseWorldPos.x - transform.position.x;
            mouseY = mouseWorldPos.y - (transform.position.y + 0.85f);
            //Abs绝对值判断，当X轴方向上的差异大于Y轴上的差异，优先判断X轴上的方向,反之，优先判断Y轴方向
            if (MathF.Abs(mouseX) > MathF.Abs(mouseY))
            {
                mouseY = 0;
            }
            else
            {
                mouseX = 0;
            }
            StartCoroutine(UseToolRoutine(mouseWorldPos, itemDetails));
        }
        //不是工具就直接执行
        else
        {
            EventHandler.CallExecuteActionAfterAnimation(mouseWorldPos, itemDetails);
        }
    }

    /// <summary>
    /// 长按鼠标左键执行动画和操作
    /// </summary>
    /// <param name="mouseWorldPos"></param>
    /// <param name="itemDetails"></param>
    private void OnMouseHoldEvent(Vector3 mouseWorldPos, ItemDetails itemDetails)
    {
        //长按时不让移动
        inputDisable = true;
        
        if (itemDetails.itemType == ItemType.FishingRod)
        {
            //判断使用工具时面朝方向
            mouseX = mouseWorldPos.x - transform.position.x;
            mouseY = mouseWorldPos.y - (transform.position.y + 0.85f);
            //Abs绝对值判断，当X轴方向上的差异大于Y轴上的差异，优先判断X轴上的方向,反之，优先判断Y轴方向
            if (MathF.Abs(mouseX) > MathF.Abs(mouseY))
            {
                mouseY = 0;
            }
            else
            {
                mouseX = 0;
            }
            fishPowerProcessCanvas.SetActive(true);
            PlayerFaceDirection();
            StartCoroutine(UseToolHoldRoutine());
            //控制鼠标蓄力时间在一定范围内
            if (isMouseDownTimerIncrease)
            {
                mouseDownTimer += 5f * Time.deltaTime;
                if (mouseDownTimer > Settings.mouseDownHoldTime)
                {
                    mouseDownTimer = Settings.mouseDownHoldTime;
                    isMouseDownTimerIncrease = false;
                }
            }
            else
            {
                mouseDownTimer -= 5f * Time.deltaTime;
                if (mouseDownTimer < 0)
                {
                    mouseDownTimer = 0;
                    isMouseDownTimerIncrease = true;
                }
            }
            mouseDownProcess.fillAmount = mouseDownTimer / Settings.mouseDownHoldTime;

        }
        else
        {
            EventHandler.CallExecuteActionAfterAnimation(mouseWorldPos, itemDetails);
        }
    }
    private void OnMouseUpEvent()
    {
        Debug.Log("MouseUp");
        cM.fishingEventDisable = true;
        isMouseDownTimerIncrease = true;
        fishPowerProcessCanvas.SetActive(false);
        //鱼竿抛出动作
        foreach (var anim in animators)
        {
            anim.SetTrigger("Tool2");
        }
        //显示鱼线
        fishLine.gameObject.SetActive(true);
        //根据面朝方向设置鱼钩落点
        if (faceDown)
        {
            baitEndPos = new Vector3(transform.position.x, transform.position.y - mouseDownTimer * Settings.downPower);
        }
        if (faceLeft)
        {
            baitEndPos = new Vector3(transform.position.x - mouseDownTimer * Settings.downPower, transform.position.y);
        }
        if (faceRight)
        {
            baitEndPos = new Vector3(transform.position.x + mouseDownTimer * Settings.downPower, transform.position.y);
        }
        if (faceUp)
        {
            baitEndPos = new Vector3(transform.position.x, transform.position.y + mouseDownTimer * Settings.downPower);
        }
        //获取鱼钩抛物线中点
        baitParabolaMidPos = GetMidPoint(baitAppearPos.position, baitEndPos, Settings.fishParabolaPercent, Settings.fishaParabolaOffsetY);
        baitParabolaPointList.Clear();
        AddParabolaPointList(baitAppearPos.position, baitParabolaMidPos, baitEndPos, baitParabolaPointList);
        totalBaitParabolaLength = GetTotalParabolaLength(baitParabolaPointList);
        currentBaitParabolaLength = 0;
        baitInSkyTime = 0;
        baitParabolaPointIndex = 0;
        isBait = true;
        mouseDownTimer = 0;
        //根据装备的鱼饵来减少钓鱼等待的时间
        if (InventoryUI.Instance.baitItemSlot.itemDetails != null)
        {
            var decreaseTime = InventoryUI.Instance.baitItemSlot.itemDetails.decreaseFishingTime;
            //鱼上钩的随机时间
            fishingRandomTime = UnityEngine.Random.Range(3f - decreaseTime, 8f - decreaseTime);
        }
        else
        {
            //鱼上钩的随机时间
            fishingRandomTime = UnityEngine.Random.Range(3f, 8f);
        }
       
    }

    /// <summary>
    /// 根据游戏状态来限制玩家
    /// </summary>
    /// <param name="gameState"></param>
    private void OnUpdateGameStateEvent(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.Gameplay:
                if (isNpcEvent)
                {
                    inputDisable = true;
                }
                else
                {
                    inputDisable = false;
                }
                break;
            case GameState.Pause:
                inputDisable = true;
                break;
        }
    }
    private void OnDisplayCollectItemSprite(int ID)
    {
        //播放玩家收获动画
        foreach (var anim in animators)
        {
            anim.SetTrigger("isCollect");
        }
    }
    


    /// <summary>
    /// 执行人物使用工具时的动画
    /// </summary>
    /// <param name="mouseWorldPos"></param>
    /// <param name="itemDetails"></param>
    /// <returns></returns>
    private IEnumerator UseToolRoutine(Vector3 mouseWorldPos, ItemDetails itemDetails)
    {
        cM.cursorEnable = false;
        inputDisable = true;
        //确保上面的代码执行完在执行下面的代码
        yield return null;
        foreach (var anim in animators)
        {
            anim.SetTrigger("useTool");
            //将鼠标点击世界坐标也传给角色，让角色的身体各个动画也随之转动到对应的面朝位置
            anim.SetFloat("InputX", mouseX);
            anim.SetFloat("InputY", mouseY);
        }
        //根据锄头锄到地上的动画时刻来决定时间
        yield return new WaitForSeconds(0.2f);
        EventHandler.CallExecuteActionAfterAnimation(mouseWorldPos, itemDetails);
        yield return new WaitForSeconds(0.5f);
        //工具使用动画结束后恢复未使用工具状态
        inputDisable = false;
        cM.cursorEnable = true;
    }
    /// <summary>
    /// 长按鼠标左键触发的动作
    /// </summary>
    /// <returns></returns>
    private IEnumerator UseToolHoldRoutine()
    {
        yield return null;
        foreach (var anim in animators)
        {
            anim.SetTrigger("useTool");
            //将鼠标点击世界坐标也传给角色，让角色的身体各个动画也随之转动到对应的面朝位置
            anim.SetFloat("InputX", mouseX);
            anim.SetFloat("InputY", mouseY);
        }
    }
    /// <summary>
    /// 鼠标点击后等播放动画结束后才可以再点击
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitTime()
    {
        cM.cursorEnable = false;
        yield return new WaitForSeconds(0.4f);
        cM.cursorEnable = true;
    }
    /// <summary>
    /// 获取鱼钩抛物线中点
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="percent"></param>
    /// <returns></returns>
    public Vector3 GetMidPoint(Vector3 start, Vector3 end, float percent, float OffsetY)
    {
        Vector3 normal = (end - start).normalized;
        float distance = Vector3.Distance(start, end);
        Vector3 point = normal * (distance * percent) + start;
        return new Vector3(point.x, point.y - OffsetY);

    }
    /// <summary>
    /// 获取一条抛物线线
    /// </summary>
    /// <param name="p0">起始点</param>
    /// <param name="p1">中点</param>
    /// <param name="p2">落点</param>
    /// <param name="t">线的弯曲比例</param>
    /// <returns></returns>
    public Vector3 CalculateBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        Vector3 p0p1 = Vector3.Lerp(p0, p1, t);
        Vector3 p1p2 = Vector3.Lerp(p1, p2, t);
        return Vector3.Lerp(p0p1, p1p2, t);
    }
    /// <summary>
    /// 添加抛物线上的点到抛物线列表中
    /// </summary>
    /// <param name="start"></param>
    /// <param name="mid"></param>
    /// <param name="end"></param>
    /// <param name="parabolaPointList">抛物线点列表</param>
    public void AddParabolaPointList(Vector3 start, Vector3 mid, Vector3 end, List<Vector3> parabolaPointList)
    {
        for (int i = 0; i < 50; i++)
        {
            //把获得的抛物线的点添加到抛物线列表
            Vector3 point = CalculateBezierPoint(start, mid, end, (i / 50.0f));
            parabolaPointList.Add(point);
        }
    }
    /// <summary>
    /// 获取抛物线总长度
    /// </summary>
    /// <param name="parabolaPointList"></param>
    /// <returns></returns>
    public float GetTotalParabolaLength(List<Vector3> parabolaPointList)
    {
        float ParabolaLength = 0;
        // 获取已添加好的抛物线长度
        for (int i = 1; i < parabolaPointList.Count; i++)
        {
            ParabolaLength += (parabolaPointList[i] - parabolaPointList[i - 1]).magnitude;
        }
        return ParabolaLength;
    }
    /// <summary>
    /// 利用曲线运动来控制物体选装
    /// </summary>
    /// <param name="curve"></param>
    /// <param name="thisGameObject"></param>
    /// <returns></returns>
    private IEnumerator RotationCurve(AnimationCurve curve, GameObject thisGameObject)
    {
        float rotationRate = curve.Evaluate(currentBaitParabolaLength / totalBaitParabolaLength);
        thisGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotationRate));
        yield return null;

    }
    /// <summary>
    /// 鱼钩沿着抛物移动
    /// </summary>
    public void BaitMoveAlongTheParabola()
    {
        baitInSkyTime += Time.deltaTime;
        baitGameObject.SetActive(true);
        isFishLine = true;
        fishLine.enabled = true;
        fishLine.positionCount = 50;
        float currentBaitJourney = (baitSpeed * 10 / 36) * baitInSkyTime;
        if (currentBaitParabolaLength < totalBaitParabolaLength)
        {
            for (int i = baitParabolaPointIndex; i < baitParabolaPointList.Count - 1; i++)
            {
                currentBaitParabolaLength += (baitParabolaPointList[i + 1] - baitParabolaPointList[i]).magnitude;
                if (currentBaitParabolaLength > currentBaitJourney)
                {
                    baitParabolaPointIndex = i;
                    currentBaitParabolaLength -= (baitParabolaPointList[i + 1] - baitParabolaPointList[i]).magnitude;
                    baitDir = (baitParabolaPointList[i + 1] - baitParabolaPointList[i]).normalized;
                    baitPos = baitParabolaPointList[i] + baitDir * (currentBaitJourney - currentBaitParabolaLength);
                    break;
                }
            }
            baitGameObject.transform.position = baitPos;
            //鱼钩抛出时旋转
            StartCoroutine(RotationCurve(baitRotationCurve, baitGameObject));

        }
        //鱼钩到达落点
        else
        {
            BaitGetPosition(baitPos);
        }
    }
    /// <summary>
    /// 鱼钩达到落点
    /// </summary>
    /// <param name="baitPos">鱼钩当前位置</param>
    private void BaitGetPosition(Vector3 baitPos)
    {
        //将当前鱼钩坐标转化为当前地图的Vector3Int Tile坐标
        Vector3Int baitGridPos = baitGrid.WorldToCell(baitPos);
        TileDetails baitTile = GridMapManager.Instance.GetTileDetailsOnMousePosition(baitGridPos);
        if (baitTile != null)
        {
            //落点为可钓鱼瓦片
            if (baitTile.canLaterFishing || baitTile.canSeaFishing)
            {
                if (baitTile.canLaterFishing)
                {
                    RollFishGetID(InventoryManager.Instance.laterFishList);
                }
                else
                {
                    RollFishGetID(InventoryManager.Instance.seaFishList);
                }
                isBait = false;
                //播放水花粒子效果
                EventHandler.CallParticleEffectEvent(ParticalEffectType.WaterEffect01, baitPos, Vector2.up);
                //播放玩家FishingIdle动画
                foreach (var anim in animators)
                {
                    anim.SetTrigger("Idle2");
                }
                isFishing = true;
                exclamationEnable = true;

            }
            //若鱼钩落点不可钓鱼
            if (!baitTile.canLaterFishing && !baitTile.canSeaFishing)
            {

                cM.fishingEventDisable = false;
                isBait = false;
                fishLine.gameObject.SetActive(false);
                EventHandler.CallControlPlayerBagOpen(true);
                //恢复移动和站立动画
                foreach (var anim in animators)
                {
                    anim.SetTrigger("toIdle");
                }
                inputDisable = false;
                //关闭鱼钩
                baitGameObject.SetActive(false);

            }
        }
        //若鱼钩落点的TileDetail为空
        else
        {
            cM.fishingEventDisable = false;
            isBait = false;
            fishLine.gameObject.SetActive(false);
            //恢复移动和站立动画
            foreach (var anim in animators)
            {
                anim.SetTrigger("toIdle");
            }
            inputDisable = false;
            //关闭鱼钩
            baitGameObject.SetActive(false);

        }
    }
    /// <summary>
    /// 有鱼上钩
    /// </summary>
    private void PlayerFishing()
    {
        currentFishingTime += Time.deltaTime;
        if (currentFishingTime > fishingRandomTime)
        {
            currentFishingTime = 0;
            //根据装备的鱼饵来减少钓鱼等待的时间
            if (InventoryUI.Instance.baitItemSlot.itemDetails != null)
            {
                var decreaseTime = InventoryUI.Instance.baitItemSlot.itemDetails.decreaseFishingTime;
                //鱼上钩的随机时间
                fishingRandomTime = UnityEngine.Random.Range(3f - decreaseTime, 8f - decreaseTime);
            }
            else
            {
                //鱼上钩的随机时间
                fishingRandomTime = UnityEngine.Random.Range(3f, 8f);
            }
            //显示感叹号
            exclamatonMark.SetActive(true);
            //播放鱼钩动画
            baitGameObject.GetComponent<Animator>().SetTrigger("baitShake");
            //播放玩家上钩动画
            foreach (var anim in animators)
            {
                anim.SetTrigger("Bait");
            }
        }
    }
    /// <summary>
    /// 随机获取渔获的信息
    /// </summary>
    /// <param name="canFishingList"></param>
    private void RollFishGetID(List<ItemDetails> canFishingList)
    {
        //while(fishDetails.likeBaits)
        List<ItemDetails> temporaryFishList = new List<ItemDetails>();

        for (int i = 0; i < canFishingList.Count; i++)
        {
            //有的鱼没写喜欢的鱼饵，那就装备如何一种鱼饵或者不装备鱼饵都可以钓到
            if (canFishingList[i].likeBaits.Length == 0)
            {
                temporaryFishList.Add(canFishingList[i]);
            }
            //有的鱼写了喜欢的鱼饵，那就必须用它其中喜欢的一种鱼饵才可以钓到
            else
            {
                for (int b = 0; b < canFishingList[i].likeBaits.Length; b++)
                {
                    if (InventoryUI.Instance.baitItemSlot.itemDetails != null &&canFishingList[i].likeBaits[b] == InventoryUI.Instance.baitItemSlot.itemDetails.itemID)
                    {
                        temporaryFishList.Add(canFishingList[i]);
                    }
                }
            }
        }
        for (int i = 0; i < temporaryFishList.Count; i++)
        {
            Debug.Log(temporaryFishList[i].itemID);
        }
        randomFishIndex = UnityEngine.Random.Range(0, temporaryFishList.Count);
        fishDetails = temporaryFishList[randomFishIndex];
    }
    /// <summary>
    /// 开始钓鱼游戏或是退出钓鱼
    /// </summary>
    private void StartFishGame()
    {

        if (Input.GetKey(KeyCode.Space) && !isFishingGame)
        {
            //减少当前使用的鱼饵数量
            if(InventoryUI.Instance.baitItemSlot.itemDetails != null)
            {
                InventoryManager.Instance.RemoveItem(InventoryUI.Instance.baitItemSlot.itemDetails.itemID, 1);
            }
            //在出现感叹号时按下空格触发抓鱼
            if (canCatchFish)
            {
                //播放鱼钩动画
                baitGameObject.GetComponent<Animator>().SetTrigger("bait");
                isFishingGame = true;
                EventHandler.CallUpdateGameStateEvent(GameState.Pause);
                StartCoroutine(WaitForReadyTextPlay());
                exclamationEnable = false;
            }
            //若没出现感叹号时按下空格就关闭钓鱼
            else
            {
                ExitFishing();
            }
        }
    }
    /// <summary>
    /// 等待钓鱼准备动画播放完
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitForReadyTextPlay()
    {
        readyText.SetActive(true);
        yield return new WaitForSeconds(1f);
        readyText.SetActive(false);
        //播放玩家拉鱼动画
        foreach (var anim in animators)
        {
            anim.SetTrigger("Tool3");
        }
        //开启钓鱼小游戏
        fishBg.SetActive(true);

    }
    /// <summary>
    /// 完成钓鱼游戏
    /// </summary>
    public void FinishFishGame()
    {
        EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
        fishLine.enabled = false;
        isFishLine = false;
        isFishingGame = false;
        //关闭鱼钩对象
        baitGameObject.SetActive(false);
        isFishing = false;
        //播放玩家接鱼动画
        foreach (var anim in animators)
        {
            anim.SetTrigger("Tool4");
        }
        //生成鱼item
        fishPrefab.GetComponent<Item>().itemID = fishDetails.itemID;
        var fishItems = Instantiate(fishPrefab, baitGameObject.transform.position, Quaternion.identity);
        fishItems.GetComponent<CircleCollider2D>().enabled = false;
        fish = fishItems;
        ////获取鱼的信息并生成
        //if (isLaterFishing)
        //{
        //    //fishDetails = InventoryManager.Instance.GetFishDetails(randomFishIndex, true);
        //    fishPrefab.GetComponent<Item>().itemID = fishDetails.itemID;
        //    var fishItems = Instantiate(fishPrefab, baitGameObject.transform.position, Quaternion.identity);
        //    fish = fishItems;
        //}
        //else
        //{
        //    //fishDetails = InventoryManager.Instance.GetFishDetails(randomSeaFishIndex, true);
        //    fishPrefab.GetComponent<Item>().itemID = fishDetails.itemID;
        //    var fishItems = Instantiate(fishPrefab, baitGameObject.transform.position, Quaternion.identity);
        //    fish = fishItems;
        //}
        //获取鱼抛物线的中点并将鱼的抛物线点添加到抛物线点列表中
        Vector3 fishParabolaMidPos = GetMidPoint(baitGameObject.transform.position, new Vector3(transform.position.x, transform.position.y + 1f), Settings.fishParabolaPercent, Settings.fishaParabolaOffsetY);
        fishParabolaPointList.Clear();
        AddParabolaPointList(baitGameObject.transform.position, fishParabolaMidPos, new Vector3(transform.position.x, transform.position.y + 1f), fishParabolaPointList);
        totalFishParabolaLength = GetTotalParabolaLength(fishParabolaPointList);
        fishInSkyTime = 0;
        currentFishParabolaLength = 0;
        fishParabolaPintIndex = 0;
        fishMove = true;
    }
    /// <summary>
    /// 鱼沿着抛物线移动到玩家身上
    /// </summary>
    private void FishMoveToPlayer()
    {
        fishInSkyTime += Time.deltaTime;
        float currentFishJourney = (baitSpeed * 10 / 36) * fishInSkyTime;
        if (currentFishParabolaLength < totalFishParabolaLength)
        {
            for (int i = fishParabolaPintIndex; i < fishParabolaPointList.Count - 1; i++)
            {
                currentFishParabolaLength += (fishParabolaPointList[i + 1] - fishParabolaPointList[i]).magnitude;
                if (currentFishParabolaLength > currentFishJourney)
                {
                    fishParabolaPintIndex = i;
                    currentFishParabolaLength -= (fishParabolaPointList[i + 1] - fishParabolaPointList[i]).magnitude;
                    fishDir = (fishParabolaPointList[i + 1] - fishParabolaPointList[i]).normalized;
                    fishPos = fishParabolaPointList[i] + fishDir * (currentFishJourney - currentFishParabolaLength);
                    break;
                }
            }
            fish.transform.position = fishPos;
        }
        else if (fish != null)
        {
            fish.GetComponent<CircleCollider2D>().enabled = true;
            StartCoroutine(ShowFishGetAnimation());
        }
    }

    /// <summary>
    /// 显示渔获动作
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShowFishGetAnimation()
    {
        //播放收鱼动画
        foreach (var anim in animators)
        {
            anim.SetBool("Tool5",true);
        }
        fishItem.SetActive(true);
        fishItem.GetComponent<SpriteRenderer>().sprite = fishDetails.itemOnWorldSprite;
        yield return new WaitForSeconds(0.5f);
        confirmFishGet = true;

    }
    /// <summary>
    /// 退出钓鱼
    /// </summary>
    public void ExitFishing()
    {
        fishBg.SetActive(false);
        isFishing = false;
        isFishingGame = false;
        isFishLine = false;
        currentFishingTime = 0;
        EventHandler.CallControlPlayerBagOpen(true);
        //恢复移动和站立动画
        foreach (var anim in animators)
        {
            anim.SetTrigger("toIdle");
        }
        StartCoroutine(WaitTime());
        inputDisable = false;
        fishLine.enabled = false;
        //关闭鱼钩对象
        baitGameObject.SetActive(false);
        cM.fishingEventDisable = false;
    }
    private void OnBeforeSceneUnloadEvent()
    {
        //切换场景时禁止对玩家的操作
        inputDisable = true;
    }
    private void OnAfterSceneLoadedEvent()
    {
        baitGrid = FindObjectOfType<Grid>();
        //切换场景后恢复对玩家的操作
        inputDisable = false;
    }

    private void OnMoveToPosition(Vector3 targetPosition)
    {
        //将玩家位置设置为targetPosition位置
        transform.position = targetPosition;
    }

    private void OnStartNewGameEvent(int obj)
    {
        //初始化玩家各项等级和经验
        plantingSkill = 1;
        plantingMaxEXP = 100;
        plantingCurrentEXP = 0;
        cultivetionSkill = 1;
        cultivetionMaxEXP = 100;
        cultivetionCurrentEXP = 0;
        fishingSkill = 1;
        fishingMaxEXP = 100;
        fishingCurrentEXP = 0;
        fightSkill = 1;
        fightMaxEXP = 100;
        fightCurrentEXP = 0;
        exploreSkill = 1;
        exploreMaxEXP = 100;
        exploreCurrentEXP = 0;
        inputDisable = false;
        transform.position = Settings.playerStartPos;
    }
    public void OnIncreasePlantingSkillEvent(int EXP)
    {
        plantingCurrentEXP = plantingCurrentEXP + EXP;
        if (plantingCurrentEXP >= plantingMaxEXP)
        {
            plantingSkill++;
        }
    }
    public void OnIncreaseCultivetionSkillEvent(int EXP)
    {
        cultivetionCurrentEXP = cultivetionCurrentEXP + EXP;
        if (cultivetionCurrentEXP >= cultivetionMaxEXP)
        {
            cultivetionSkill++;
        }
    }
    public void OnIncreaseFishingSkillEvent(int EXP)
    {
        fishingCurrentEXP = fishingCurrentEXP + EXP;
        if (fishingCurrentEXP >= fishingMaxEXP)
        {
            fishingSkill++;
        }
    }
    public void OnIncreaseFightSkillEvent(int EXP)
    {
        fightCurrentEXP = fightCurrentEXP + EXP;
        if (fightCurrentEXP >= fightMaxEXP)
        {
            fightSkill++;
        }
    }
    public void OnIncreaseExploreSkillEvent(int EXP)
    {
        exploreCurrentEXP = exploreCurrentEXP + EXP;
        if (exploreCurrentEXP >= exploreMaxEXP)
        {
            exploreSkill++;
        }
    }
    private void OnEndGameEvent()
    {
        inputDisable = true;
    }
    private void OnEquipArmorEven(int value)
    {
        defenceValue = value;
    }
   
    private void OnStartNPCEvent()
    {
        isNpcEvent = true;
        inputDisable = true;
        npcEventOriginPos = transform.position;
    }
    private void OnEndNPCEvent()
    {
        isNpcEvent = false;
        inputDisable = false;
        transform.position = npcEventOriginPos;
    }
    private void OnPlayEatAnimEvent(int ID)
    {
        EventHandler.CallUpdateGameStateEvent(GameState.Pause);
        EventHandler.CallControlPlayerBagOpen(false);
        //播放玩家吃东西动画
        foreach (var anim in animators)
        {
            anim.SetTrigger("isEat");
        }
    }
    /// <summary>
    /// 监测玩家的移动输入
    /// </summary>
    private void PlayerInput()
    {
        //inputX和inputY的值只会是-1、0、1
        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");
        //使玩家的斜方向移动速度不要比直方向的速度快
        if (inputX != 0 && inputY != 0)
        {
            inputX = inputX * 0.6f;
            inputY = inputY * 0.6f;
        }
        //改变玩家速度,GetKey按住按键
        if (Input.GetKey(KeyCode.LeftShift))
        {
            inputX = inputX * 0.4f;
            inputY = inputY * 0.4f;
        }
        //通过玩家输入来移动角色
        movementInput = new Vector2(inputX, inputY);
        //根据玩家是否输入移动指令来决定isMoving的bool值
        isMoving = movementInput != Vector2.zero;
    }
    /// <summary>
    /// 通过玩家移动的输入时角色动起来
    /// </summary>
    private void Movement()
    {
        //当体力值小于等于0时，速度减慢
        if (currentStmina <= 0)
        {
            rb.MovePosition(rb.position + movementInput * tiredSpeed * Time.deltaTime);
        }
        else
        {
            rb.MovePosition(rb.position + movementInput * speed * Time.deltaTime);
        }
    }
    /// <summary>
    /// 实现玩家移动和站立的动画切换
    /// </summary>
    private void SwitchAnimation()
    {
        foreach (var anim in animators)
        {
            //根据玩家是否移动来播放站立或者跑动动画
            anim.SetBool("isMoving", isMoving);
            anim.SetFloat("mouseX", mouseX);
            anim.SetFloat("mouseY", mouseY);
            if (isMoving)
            {
                //如果玩家在移动中，再根据移动速度来播放走路或跑动动画
                anim.SetFloat("InputX", inputX);
                anim.SetFloat("InputY", inputY);
            }

        }

    }
    /// <summary>
    /// 检测玩家此时面朝方向
    /// </summary>
    public void PlayerFaceDirection()
    {
        //人物朝右
        if (mouseX > 0)
        {
            faceLeft = false;
            faceRight = true;
            faceDown = false;
            faceUp = false;

        }
        //人物朝左
        if (mouseX < 0)
        {
            faceLeft = true;
            faceRight = false;
            faceDown = false;
            faceUp = false;

        }
        //人物朝上
        if (mouseX == 0 && mouseY > 0)
        {
            faceLeft = false;
            faceRight = false;
            faceDown = false;
            faceUp = true;

        }
        //人物朝下
        if (mouseX == 0 && mouseY < 0)
        {
            faceLeft = false;
            faceRight = false;
            faceDown = true;
            faceUp = false;

        }

    }
    /// <summary>
    /// 玩家受伤
    /// </summary>
    /// <param name="damage">敌人伤害</param>
    public void PlayerHurted(int damage, float depleteStmina, Transform emenyPos)
    {
        //检测玩家当前是否为无敌
        if (invulnerable)
        {
            return;
        }
        //根据玩家自身的防御力来扣除生命值
        int realDamage = damage - defenceValue;
        if (realDamage <= 0)
        {
            realDamage = 1;
        }
        currentHealth -= realDamage;
        //消耗体力值
        EventHandler.CallPlayerDecreaseStminaEvent(depleteStmina);
        if (damage > 0)
        {
            inputDisable = true;
            FlashColorAndKnockBack(emenyPos,Color.red);
            ShowDamageNumOfPlayer(emenyPos.position, realDamage);
        }
        //玩家死亡
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            inputDisable = true;
            Debug.Log("血量为零！！");
        }
        if (currentHealth >= maxHealth)
        {
            currentHealth = maxHealth;
        }
        if (currentStmina >= maxStmina)
        {
            currentStmina = maxStmina;
        }
        StartCoroutine(InvulnerableCoroutine());
    }
    /// <summary>
    /// 无敌状态切换
    /// </summary>
    /// <returns></returns>
    private IEnumerator InvulnerableCoroutine()
    {
        invulnerable = true;
        yield return new WaitForSeconds(invulnerableDuration);
        invulnerable = false;
    }
    /// <summary>
    /// 玩家受击特效和后退
    /// </summary>
    private void FlashColorAndKnockBack(Transform enemy,Color playerColor)
    {
        isKnocking = true;
        foreach (var sprite in sr)
        {
            originalColor = sprite.color;
            sprite.color = playerColor;
            StartCoroutine(ResetColor(sprite, originalColor));
        }
        PlayerHurdKnockback(enemy.position);

    }
    /// <summary>
    /// 玩家受击后退
    /// </summary>
    private void PlayerHurdKnockback(Vector3 emenyPos)
    {
        Vector2 knockbackDirection = (transform.position - emenyPos).normalized;
        StartCoroutine(KnockbackRoutine(knockbackDirection));
    }
    /// <summary>
    /// 玩家受击后退协程
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    IEnumerator KnockbackRoutine(Vector2 direction)
    {
        float elapsed = 0f;
        Vector2 startPosition = transform.position;
        Vector2 targetPosition = startPosition + direction * knockbackDistance;
        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / knockbackDuration;
            float curveValue = knockbackCurve.Evaluate(t);
            Vector2 newPosition = Vector2.Lerp(startPosition, targetPosition, curveValue);
            rb.MovePosition(newPosition);
            yield return null;
        }
        inputDisable = false;
        isKnocking = false;
        //var direction = (transform.position - targetPosition).normalized;
        //rb.AddForce(direction * knockbackForce, ForceMode2D.Force);
        //Debug.Log(direction * knockbackForce);
        //yield return new WaitForSeconds(knockbackDuration);
    }
    /// <summary>
    /// 恢复玩家原来颜色
    /// </summary>
    /// <param name="sprite"></param>
    /// <param name="originalColor"></param>
    /// <returns></returns>
    private IEnumerator ResetColor(SpriteRenderer sprite, Color originalColor)
    {
        yield return new WaitForSeconds(invulnerableDuration / 1.5f);
        sprite.color = originalColor;
    }
    /// <summary>
    /// 显示受伤伤害
    /// </summary>
    /// <param name="emenyPos"></param>
    private void ShowDamageNumOfPlayer(Vector3 emenyPos, int damage)
    {
        var damageNum = Instantiate(damageNumPrefab, transform.position, Quaternion.identity, transform);
        damageNum.GetComponent<DamageNum>().SetDamageNumText(damage);
        //根据与玩家的位置来播放对应的伤害数字动画
        if (MathF.Abs(transform.position.x - emenyPos.x) > 0.2f)
        {
            if (transform.position.x < emenyPos.x)
            {
                damageNum.GetComponent<Animator>().SetTrigger("isLeft");
            }
            else
            {
                damageNum.GetComponent<Animator>().SetTrigger("isRight");
            }
        }
        else
        {
            damageNum.GetComponent<Animator>().SetTrigger("isDown");
        }
    }
    /// <summary>
    /// 扣除玩家疲劳值
    /// </summary>
    /// <param name="value">扣除值</param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnPlayerDecreaseStminaEvent(float value)
    {
        currentStmina -= value;
        if (currentStmina < 0)
        {
            currentStmina = 0;
        }
        if (currentStmina >= maxStmina)
        {
            currentStmina = maxStmina;
        }
    }
    private void OnPlayerDecreaseHealthEvent(float value)
    {
        currentHealth -= value;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        if (currentHealth >= maxHealth)
        {
            currentHealth = maxHealth;
        }
    }
    /// <summary>
    /// 玩家防御
    /// </summary>
    public IEnumerator PlayerDefence(Vector3 mouseWorldPos)
    {
        inputDisable = true;
        cM.cursorEnable = false;
        //判断使用工具时面朝方向
        mouseX = mouseWorldPos.x - transform.position.x;
        mouseY = mouseWorldPos.y - (transform.position.y + 0.85f);
        //Abs绝对值判断，当X轴方向上的差异大于Y轴上的差异，优先判断X轴上的方向,反之，优先判断Y轴方向
        if (MathF.Abs(mouseX) > MathF.Abs(mouseY))
        {
            mouseY = 0;
        }
        else
        {
            mouseX = 0;
        }
        //执行防御动作
        foreach (var anim in animators)
        {
            //将鼠标点击世界坐标也传给角色，让角色的身体各个动画也随之转动到对应的面朝位置
            anim.SetFloat("InputX", mouseX);
            anim.SetFloat("InputY", mouseY);
            anim.SetTrigger("Tool2");
        }

        yield return new WaitForSeconds(swordDefenceCD);
        cM.cursorEnable = true;
    }
    /// <summary>
    /// 玩家处决
    /// </summary>
    /// <returns></returns>
    public IEnumerator PlayerExecution()
    {
        inputDisable = true;
        cM.cursorEnable = false;
        foreach (var anim in animators)
        {
            anim.SetTrigger("Tool3");
        }
        yield return new WaitForSeconds(swordDefenceCD);
        cM.cursorEnable = true;
    }
    /// <summary>
    /// 播放防御成功动画
    /// </summary>
    public void PlayDefenceGetAnim(Transform enemy)
    {
        currentTransitionTime = 0;
        isSlowMotionTime = true;
        FlashColorAndKnockBack(enemy,Color.white);
        playerCollider.enabled = true;
        foreach (var box in playerAttack.defenceBoxs)
        {
            box.enabled = false;
        }
        foreach (var anim in animators)
        {
            anim.SetTrigger("Idle2");
        }
    }
    /// <summary>
    /// 播放防御成功特效
    /// </summary>
    /// <param name="point"></param>
    public void PlayDefenceEffect(Vector2 point)
    {
        effectIndex = UnityEngine.Random.Range(0, 2);
        defenceEffects[effectIndex].gameObject.SetActive(true);
        defenceEffects[effectIndex].gameObject.transform.position = point;
    }
    /// <summary>
    /// 控制玩家移动到指定位置
    /// </summary>
    /// <param name="targetPos"></param>
    /// <param name="speed"></param>
    /// <returns></returns>
    public IEnumerator SetPlayerPos(Vector2 targetPos,float speed , Vector2 faceDir)
    {
        inputDisable = true;

        // A* 寻路
        string sceneName = SceneManager.GetActiveScene().name;
        var grid = FindObjectOfType<Grid>();
        Vector3Int currentGridPos = grid.WorldToCell(transform.position);
        Vector3Int targetGridPos = grid.WorldToCell(new Vector3(targetPos.x, targetPos.y, 0));
        Vector2 dir = Vector2.down;
        // 构建路径
        Stack<MovementStep> pathStack = new Stack<MovementStep>();
        AStar.Instance.buildPath(sceneName, (Vector2Int)currentGridPos, (Vector2Int)targetGridPos, pathStack);

        if (pathStack.Count == 0)
        {
            inputDisable = false;
            yield break;
        }

        // Stack 顶部是起点、底部是终点，逆序取出得到 起点→终点 的路径列表
        List<Vector2Int> path = new List<Vector2Int>();
        while (pathStack.Count > 0)
        {
            path.Add(pathStack.Pop().gridCoordinate);
        }

        // 沿路径逐格移动（跳过起点 i=0）
        for (int i = 1; i < path.Count; i++)
        {
            Vector3 worldPos = grid.CellToWorld(new Vector3Int(path[i].x, path[i].y, 0));
            worldPos = new Vector3(worldPos.x + Settings.gridCellSize / 2, worldPos.y + Settings.gridCellSize / 2, 0);
            Vector2 stepTarget = new Vector2(worldPos.x, worldPos.y);
            // 检查目标位置是否有门
            Door door = FindObjectsByType<Door>(FindObjectsSortMode.None)
     .OrderBy(d => Vector2.Distance(d.transform.position, stepTarget))
     .FirstOrDefault(d => Vector2.Distance(d.transform.position, stepTarget) < 0.7f);
            if (door != null && !door.isOpened)
            {
                door.OpenDoor();
                yield return new WaitForSeconds(0.4f);
            }
            while (Vector2.Distance(transform.position, stepTarget) > Settings.pixelSize)
            {
                dir = (stepTarget - (Vector2)transform.position).normalized;
                transform.position = Vector2.MoveTowards(transform.position, stepTarget, speed * Time.deltaTime);

                foreach (var anim in animators)
                {
                    anim.SetBool("isMoving", true);
                    anim.SetFloat("mouseX", dir.x);
                    anim.SetFloat("mouseY", dir.y);
                    anim.SetFloat("InputX", dir.x);
                    anim.SetFloat("InputY", dir.y);
                }

                yield return null;
            }
        }

        // 到达终点，停止移动动画
        foreach (var anim in animators)
        {
            anim.SetBool("isMoving", false);
            anim.SetFloat("InputX", faceDir.x);
            anim.SetFloat("InputY", faceDir.y);
        }
    }
    /// <summary>
    /// 保存SaveData
    /// </summary>
    /// <returns></returns>
    public GameSaveData GenerateSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        //存档当前玩家地图坐标
        saveData.characterPosDict = new Dictionary<string, SerializableVector3>();
        saveData.characterPosDict.Add(this.name, new SerializableVector3(transform.position));
        //再把存档好的SaveData返回
        return saveData;
    }
    /// <summary>
    /// 读取存储好的SaveData
    /// </summary>
    /// <param name="saveData"></param>
    public void RestoreData(GameSaveData saveData)
    {
        //再将上面存储好的saveData中的玩家坐标拿出来
        var targetPosition = saveData.characterPosDict[this.name].ToVector3();

        transform.position = targetPosition;
    }
}


