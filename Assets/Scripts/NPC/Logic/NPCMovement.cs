using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFarm.AStar;
using UnityEngine.SceneManagement;
using System;
using MFarm.Save;
using MFarm.Dialogue;
using Cinemachine;
using Unity.Mathematics;
using static UnityEngine.RuleTile.TilingRuleOutput;
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class NPCMovement : MonoBehaviour,ISaveable   //调用在NPC01对象上，所有NPC都要挂载
{
    //NPC行程表
    public ScheduleDataList_SO scheduleData;
    //排序并保证数据唯一性
    private SortedSet<ScheduleDetails> scheduleSet;
    private ScheduleDetails currentSchedule;
    //临时存储信息
    [SerializeField]private string currentScene;
    private string targetScene;
    //当前和目标网格位置
    private Vector3Int currentGridPosition;
    private Vector3Int targetGridPosition;
    private Vector3Int nextGridPosition;
    //下一步的世界坐标
    private Vector3 nextWorldPosition;
    //设置StartScene的值为currentScene的值
    public string StartScene { set => currentScene = value; }

    [Header("移动属性")]
    public float normalSpeed;
    private float minSpeed = 2f;
    private float maxSpeed = 6f;
    //移动方向
    private Vector2 dir;
    public bool isMoving;
    //NPC组件
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D coll;
    private Animator anim;
    //npc的路径堆栈
    private Stack<MovementStep> movementSteps;
    public Grid grid;
    //是否是第一次加载
    private bool isInitialised;
    //NPC是否移动
    private bool npcMove;
    //NPC是否可以互动
    public bool interactable;
    //是否第一次加载
    public bool isFirstLoad;
    //读取游戏季节
    public Season currentSeason;
    //场景是否正在加载
    private bool sceneLoaded;
    //动画计时器
    private float animationBreakTime;
    private bool canPlayStopAnimation;
    private AnimationClip stopAnimationClip;
    public AnimationClip blankAnimationClip;
    private AnimatorOverrideController animOverride;
    //游戏结束时的协程
    private Coroutine npcMoveRoutine;
    private TimeSpan gameTime => TimeManager.Instance.gameTime;

    public string GUID => GetComponent<DataGUID>().guid;
    //NPC好感度事件
    public NPCEventData_SO npcEventData;
    private DialoguaGiver dialoguaGiver;
    private DialogueController dialogueController;
    public NPCEvent currentNPCEvent;
    //事件进度
    private int NPCEventStep;
    //开始移动到下一个事件点
    public bool timeToMoveNextPos;
    public CinemachineVirtualCamera cameraFollow;
    public UnityEngine.Transform playerTransform;
    //NPC事件开始前NPC的位置
    private Vector2 originNPCPos;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        coll = transform.GetChild(1).GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        movementSteps = new Stack<MovementStep>();
        //通过反向赋值来初始化 AnimatorOverrideController
        animOverride = new AnimatorOverrideController(anim.runtimeAnimatorController);
        anim.runtimeAnimatorController = animOverride;
        scheduleSet = new SortedSet<ScheduleDetails>();
        dialoguaGiver = GetComponent<DialoguaGiver>();
        dialogueController = GetComponent<DialogueController>();
        //把scheduleData.scheduleList中的所有schedule按早晚的顺序放入scheduleSet列表中
        foreach (var schedule in scheduleData.scheduleList)
        {
            scheduleSet.Add(schedule);
        }
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.GameMinuteEvent += OnGameMinuteEvent;
        //游戏开始的事件
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        //游戏结束的事件
        EventHandler.EndGameEvent += OnEndGameEvent;
        EventHandler.PromoteNPCEvent += OnPromoteNPCEvent;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.GameMinuteEvent -= OnGameMinuteEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
        EventHandler.PromoteNPCEvent -= OnPromoteNPCEvent;
    }

   

    private void Start()
    {
        ISaveable saveable = this;
        saveable.RegisterSaveable();
    }
    private void Update()
    {
        if(sceneLoaded)
        {
            SwitchAnimation();
           
        }
        //计时器
        animationBreakTime -= Time.deltaTime;
        canPlayStopAnimation = animationBreakTime <= 0;
    }
    
    private void FixedUpdate()
    {
        //当场景加载后才npc才可以移动
        if (sceneLoaded)
        {
            Movement();
            if (timeToMoveNextPos)
            {
                ContinueNPCEvent();
            }
        }
    }
    /// <summary>
    /// NPC在指定时间段执行的事件
    /// </summary>
    /// <param name="minute"></param>
    /// <param name="hour"></param>
    /// <param name="season"></param>
    private void OnGameMinuteEvent(int minute, int hour, int day, Season season)
    {
        //游戏的实际时间
        int time = (hour * 100) + minute;
        currentSeason = season;
        ScheduleDetails matchSchedule = null;
        foreach (var schedule in scheduleSet)
        {
            if (schedule.Time == time)
            {
                //TODO:天气事件
                if (schedule.day != day && schedule.day != 0)
                {
                    continue;
                }
                if (schedule.season != season)
                {
                    continue;
                }
                matchSchedule = schedule;
            }
            //如果时间不对或者还没到就跳出循环
            else if (schedule.Time > time)
            {
                break;
            }
        }
        //根据游戏时间和NPC事件时间匹配的schedule开始构建路径
        if (matchSchedule != null)
        {
            BuildPath(matchSchedule);
        }
    }
    private void OnBeforeSceneUnloadEvent()
    {
       sceneLoaded = false;
    }
    private void OnAfterSceneLoadedEvent()
    {
        grid = FindObjectOfType<Grid>();
        CheckVisiable();
        if(!isInitialised)
        {
            InitNPC();
            isInitialised = true;
        }
       
        sceneLoaded = true;
        //读取存档时，重新加载NPC schedule使其可以正常移动
        if (!isFirstLoad)
        {
            currentGridPosition = grid.WorldToCell(transform.position);
            //重新生成NPC Schedule
            var schedule = new ScheduleDetails(0,0,0,0,currentSeason,targetScene,(Vector2Int)targetGridPosition,stopAnimationClip,interactable);
            BuildPath(schedule);
            isFirstLoad = true;
        }
        if (npcEventData.GetfriendlinessEvent(dialoguaGiver.currentFriendliness,currentScene) != null)
        {
            currentNPCEvent = npcEventData.GetfriendlinessEvent(dialoguaGiver.currentFriendliness, currentScene);
            StartNPCEvent();
        }
    }

    private void OnStartNewGameEvent(int obj)
    {
        isInitialised = false;
        isFirstLoad = true;
    }

    private void OnEndGameEvent()
    {
        sceneLoaded = false;
        npcMove = false;
        //在游戏随时结束时，停止npc此时正在移动的协程
        if(npcMoveRoutine!= null)
        {
            StopCoroutine(npcMoveRoutine);
        }
    }
    private void OnPromoteNPCEvent()
    {
        NPCEventStep ++;
        timeToMoveNextPos = true;
        anim.SetBool("ActionExit", true);
    }
    /// <summary>
    /// 根据NPC所在场景判断是否可见
    /// </summary>
    private void CheckVisiable()
    {
        if(currentScene == SceneManager.GetActiveScene().name)
        {
            SetActiveInScene();
        }
        else
        {
            SetInactiveInScene();
        }
    }

    /// <summary>
    /// 初始化NPC
    /// </summary>
    private void InitNPC()
    {
        targetScene = currentScene;
        //保证NPC在当前坐标的网格中心点
        currentGridPosition = grid.WorldToCell(transform.position);
        transform.position = new Vector3(currentGridPosition.x + Settings.gridCellSize / 2f, currentGridPosition.y + Settings.gridCellSize / 2f, 0);
        targetGridPosition = currentGridPosition;
    }
    /// <summary>
    /// NPC主要移动方法
    /// </summary>
    private void Movement()
    {
        if (!npcMove)
        {
            if (movementSteps.Count > 0)
            {
                //Pop移除第一个并拿出来
                MovementStep step = movementSteps.Pop();
                //判断当前场景是否是这一步所在的场景
                currentScene = step.sceneName;
                CheckVisiable();
                //下一步
                nextGridPosition = (Vector3Int)step.gridCoordinate;
                //获取当前的时间戳
                TimeSpan stepTime = new TimeSpan(step.hour, step.minute, step.second);

                MoveToGridPosition(nextGridPosition, stepTime);
            }
            else if(!isMoving && canPlayStopAnimation)
            {
                StartCoroutine(SetStopAnimation());
            }
        }
    }

    private void MoveToGridPosition(Vector3Int gridPos,TimeSpan stepTime)
    {
        //NPC移动的协程 
        npcMoveRoutine = StartCoroutine(MoveRoutine(gridPos, stepTime));
    }
    /// <summary>
    /// NPC移动的协程
    /// </summary>
    /// <param name="gridPos"></param>
    /// <param name="stepTime"></param>
    /// <returns></returns>
    private IEnumerator MoveRoutine(Vector3Int gridPos,TimeSpan stepTime)
    {
        npcMove = true;
        nextWorldPosition = GetWorldPosition(gridPos);
        if(stepTime > gameTime)
        {
            //用来移动的时间差，以秒为单位
            float timeToMove = (float)(stepTime.TotalSeconds - gameTime.TotalSeconds);
            //实际移动距离
            float distance = Vector3.Distance(transform.position, nextWorldPosition);
            //实际移动速度，Mathf.Max取最大值 
            float speed = Mathf.Max(minSpeed, (distance / timeToMove / Settings.secondThreshold));
            //还有时间来移动
            if(speed <= maxSpeed)
            {
                //当前未到达下一步的网格
                while(Vector3.Distance(transform.position, nextWorldPosition) > Settings.pixelSize)
                { 
                    dir = (nextWorldPosition - transform.position).normalized;
                    //沿x,y轴移动
                    Vector2 posOffset = new Vector2(dir.x * speed * Time.fixedDeltaTime,dir.y * speed * Time.fixedDeltaTime);
                    rb.MovePosition(rb.position + posOffset);
                    //每次FixedUpdate执行一次
                    yield return new WaitForFixedUpdate();
                }
            }
        }
        //如果时间到了就瞬移到目标点位
        rb.position = nextWorldPosition;
        currentGridPosition = gridPos;
        nextGridPosition = currentGridPosition;

        npcMove = false;
    }
    /// <summary>
    /// 根据Schedule构建路径
    /// </summary>
    /// <param name="schedule"></param>
    public void BuildPath(ScheduleDetails schedule)
    {
        //先清空堆栈
        movementSteps.Clear();
        currentSchedule = schedule;
        targetScene = schedule.targetScene;
        targetGridPosition = (Vector3Int)schedule.targetGridPosition;
        stopAnimationClip = schedule.clipAtStop;
        this.interactable = schedule.interactable;
        //同场景移动
        if(schedule.targetScene == currentScene)
        {
            AStar.Instance.buildPath(schedule.targetScene, (Vector2Int)currentGridPosition, schedule.targetGridPosition, movementSteps);
        }
        ///跨场景移动
        else if(schedule.targetScene != currentScene)
        {
            SceneRoute sceneRoute = NPCManager.Instance.GetSceneRoute(currentScene, schedule.targetScene);
            if(sceneRoute != null)
            {
                for(int i = 0;i<sceneRoute.scenePathList.Count;i++)
                {
                    //NPC的路径起始点和目标点
                    Vector2Int fromPos, gotoPos;
                    ScenePath path = sceneRoute.scenePathList[i];
                    //当SceenRoute中的fromGridCell大于9999，那么NPC跨场景时起始点就是现在自己的位置
                    if (path.fromGridCell.x >= Settings.maxGridSize || path.fromGridCell.y >= Settings.maxGridSize)
                    {
                        fromPos = (Vector2Int)currentGridPosition;
                    }
                    //否则起始点就是SceenRoute中的fromGridCell坐标
                    else
                    {
                        fromPos = path.fromGridCell;
                    }
                    //当SceenRoute中的gotoGridCell大于9999，那么NPC跨场景时目标点就是NPC自己schedule中的目标点
                    if (path.gotoGridCell.x >= Settings.maxGridSize || path.gotoGridCell.y >= Settings.maxGridSize)
                    {
                        gotoPos = (Vector2Int)schedule.targetGridPosition;
                    }
                    //否则终点就是SceenRoute中的gotoGridCel坐标
                    else
                    {
                        gotoPos= path.gotoGridCell;
                    }
                    Debug.Log( fromPos.ToString() + gotoPos.ToString());
                    AStar.Instance.buildPath(path.sceneName,fromPos,gotoPos,movementSteps);
                }
            }
        }
        if(movementSteps.Count > 1)
        {
            //更新每一步对应的时间戳
            UPdateTimeOnPath();
        }
    }
    /// <summary>
    /// 更新每一步对应的时间戳
    /// </summary>
    private void UPdateTimeOnPath()
    {
        //上一个MovementStep
        MovementStep previousStep = null;
        //时间戳
        TimeSpan currentGameTime = gameTime;

        foreach(MovementStep step in movementSteps)
        {
            if(previousStep == null)
            {
                previousStep = step;
            }
            step.hour = currentGameTime.Hours;
            step.minute = currentGameTime.Minutes;
            step.second = currentGameTime.Seconds;
            //走过每一个格子所需要的时间
            TimeSpan gridMovementStepTime;
            //判断是否为斜方向
            if (MoveInDiagonal(step, previousStep))
            {
                gridMovementStepTime = new TimeSpan(0, 0, (int)(Settings.gridCellDiagonalSize / normalSpeed / Settings.secondThreshold));
            }
            //横向移动
            else
            {
                gridMovementStepTime = new TimeSpan(0, 0, (int)(Settings.gridCellSize / normalSpeed / Settings.secondThreshold));
            } 
            //累加获得下一步的时间
            currentGameTime = currentGameTime.Add(gridMovementStepTime);
            //当前时间变为走完一格后的时间，并把当前步改为上一步,之后继续循环第二步
            previousStep = step;
        }
    }
    /// <summary>
    /// 判断走直线还是斜线
    /// </summary>
    /// <param name="currentStep"></param>
    /// <param name="nextStep"></param>
    /// <returns></returns>
    private bool MoveInDiagonal(MovementStep currentStep, MovementStep previousStep)
    {
        //当上一格的x,y坐标不等于当前的网格x,y轴，就判断为斜方向
        return (currentStep.gridCoordinate.x != previousStep.gridCoordinate.x) && (currentStep.gridCoordinate.y != currentStep.gridCoordinate.y);
    }

    /// <summary>
    /// 根据网格坐标返回世界坐标中心点
    /// </summary>
    /// <param name="gridPos"></param>
    /// <returns></returns>
    private Vector3 GetWorldPosition(Vector3Int gridPos)
    {
        
        //因为游戏默认的网格坐标是网格左下角的坐标
        Vector3 worldPos = grid.CellToWorld(gridPos);
        return new Vector3(worldPos.x + Settings.gridCellSize / 2, worldPos.y + Settings.gridCellSize / 2, 0);
    }

    private void SwitchAnimation()
    {
        isMoving = transform.position != GetWorldPosition(targetGridPosition);
        anim.SetBool("isMoving", isMoving);
        if(isMoving )
        {
            anim.SetBool("Exit", true);
            anim.SetFloat("DirX", dir.x);
            anim.SetFloat("DirY", dir.y);
        }
        else
        {
            anim.SetBool("Exit", false);
        }

    }
    private IEnumerator SetStopAnimation()
    {
        //强制面向镜头
        anim.SetFloat("DirX", 0);
        anim.SetFloat("DirY", -1);
        animationBreakTime = Settings.animationBreakTime;
        if(stopAnimationClip != null)
        {
            animOverride[blankAnimationClip] = stopAnimationClip;
            anim.SetBool("EventAnimation", true);
            yield return null;
            anim.SetBool("EventAnimation", false);
        }
        else
        {
            animOverride[stopAnimationClip] = blankAnimationClip;
            anim.SetBool("EventAnimation", false);
        }
    }
    #region 设置NPC显示情况
    private void SetActiveInScene()
    {
        spriteRenderer.enabled = true;
        coll.enabled = true;
        //TODO:影子关闭
        //transform.GetChild(0).gameObject.SetActive(true);
    }

    private void SetInactiveInScene()
    {
        spriteRenderer.enabled = false;
        coll.enabled = false;
        //TODO:影子关闭
        //transform.GetChild(0).gameObject.SetActive(false);
    }
    #endregion
    /// <summary>
    /// 开始NPC事件
    /// </summary>
    public void StartNPCEvent()
    {
        EventHandler.CallStartNPCEvent();
        if(NPCEventStep <= 0)
        {
            //cameraFollow.Follow = transform;
            //对焦NPC
            cameraFollow.Follow = GameObject.Find(currentNPCEvent.npcName).transform;
            
            anim.SetBool("ActionExit", false);
            //播放这段对话的动作
            anim.Play(currentNPCEvent.animClip[NPCEventStep].name);
            originNPCPos = transform.transform.position;
            //设置事件最开始的位置
            transform.transform.position = currentNPCEvent.nextPos[NPCEventStep];
            //设置对话
            dialogueController.currentData = currentNPCEvent.dialogueData[NPCEventStep];
            StartCoroutine(WaitForOpenStartDialue());
        }
    }
  
    /// <summary>
    /// 继续事件进度
    /// </summary>
    public void ContinueNPCEvent()
    {
        //TODO:结束事件
        if(NPCEventStep >= currentNPCEvent.dialogueData.Length)
        {
            transform.transform.position = originNPCPos;
            cameraFollow.Follow = playerTransform;
            currentNPCEvent.isHappened = true;
            timeToMoveNextPos = false;
            EventHandler.CallEndNPCEvent();
            return;
        }
        var newPos = Vector2.MoveTowards(transform.position, currentNPCEvent.nextPos[NPCEventStep], currentNPCEvent.normalSpeed * Time.fixedDeltaTime);
        dir = ((Vector3)currentNPCEvent.nextPos[NPCEventStep] - transform.position).normalized;
        anim.SetFloat("DirX", dir.x);
        anim.SetFloat("DirY", dir.y);
        rb.MovePosition(newPos);
        if (Vector2.Distance(transform.position, currentNPCEvent.nextPos[NPCEventStep]) <= 0.01f)
        {
            anim.SetBool("ActionExit", false);
            anim.Play(currentNPCEvent.animClip[NPCEventStep].name);
            //设置对话
            dialogueController.currentData = currentNPCEvent.dialogueData[NPCEventStep];
            //开始对话
            StartCoroutine(WaitForOpenStartDialue());
            timeToMoveNextPos = false;
        }
    }
    /// <summary>
    /// 等待场景加载出来在显示NPC事件对话
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitForOpenStartDialue()
    {
        yield return new WaitForSeconds(2f);
       
        //开始第一段对话
        dialogueController.OpenDialogue();
    }
    public GameSaveData GenerateSaveData()
    {
        isInitialised = true;
        GameSaveData saveData = new GameSaveData();
        saveData.characterPosDict = new Dictionary<string, SerializableVector3>();
        //存档NPC的目标位置，当前位置，目标场景，当前场景
        saveData.characterPosDict.Add("targetGridPosition", new SerializableVector3(targetGridPosition));
        saveData.characterPosDict.Add("currentPositon", new SerializableVector3(transform.position));
        saveData.dataSceneName = currentScene;
        saveData.targetScene = this.targetScene;
        //存档NPC的动作
        if(stopAnimationClip != null)
        {
            saveData.animationInstanceID = stopAnimationClip.GetInstanceID();
        }
        saveData.interactable = this.interactable;
        //存档季节
        saveData.timeDict = new Dictionary<string, int>();
        saveData.timeDict.Add("currentSeason", (int)currentSeason);
        return saveData;
    }

    public void RestoreData(GameSaveData saveData)
    {
        isInitialised = true;
        isFirstLoad = false;
        currentScene = saveData.dataSceneName;
        targetScene = saveData.targetScene;
        Vector3 pos = saveData.characterPosDict["currentPositon"].ToVector3();
        Vector3Int gridPos = (Vector3Int)saveData.characterPosDict["targetGridPosition"].ToVector2Int(); 
        
        transform.position = pos;
        targetGridPosition = gridPos;

        if(saveData.animationInstanceID != 0)
        {
            this.stopAnimationClip = Resources.InstanceIDToObject(saveData.animationInstanceID) as AnimationClip;
        }

        this.interactable = saveData.interactable;
        this.currentSeason = (Season)saveData.timeDict["currentSeason"];
    }
   
}
