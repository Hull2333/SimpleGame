using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using MFarm.CropPlant;
using MFarm.AStar;
/// <summary>
/// 敌人各种属性
/// </summary>
[Serializable]
public class Parameter
{
    [Header("敌人属性")]
    public int health;
    public float moveSpeed;
    public float chaseSpeed;
    public float idleTime;
    [Header("巡逻点")]
    //巡逻点
    public Transform[] patrolPoints;
    //当前巡逻点索引，供 PatrolState 和 ChaseState 共享（追击失败后回到巡逻序列的下一个点）
    [HideInInspector] public int patrolPosition = 0;
    //是否原地返回巡逻
    [HideInInspector] public bool isBack = false;
    [Header("追击相关")]
     public Transform parentPos;
    //与玩家距离小于该值且视线无遮挡时，改用直线追击，不再走 A*（防止玩家站在障碍标记格上时怪物永远差一格）
    public float directChaseRange = 2.5f;
    [HideInInspector] public CircleCollider2D chaseArae;
    //射线阻挡图层
    public LayerMask obstacleLayer;
    //追击目标
    [HideInInspector] public Transform playerTransform;
     public Transform target;
    [HideInInspector] public Animator animator;
    //敌人对着目标方向
    [HideInInspector] public float moveX;
    [HideInInspector] public float moveY;
    [HideInInspector] public float faceX;
    [HideInInspector] public float faceY;
    [HideInInspector] public bool isMoving;
    [HideInInspector] public bool isHurted;
    [HideInInspector] public bool isDeath;
    [Header("攻击相关")]
    //需要攻击目标所在的图层,攻击范围
    public int enemyDamage;
    [HideInInspector] public float attackArea = 0.8f;
    //各类攻击框
    public Collider2D[] attackBox;
    //攻击冷却
    [HideInInspector] public float attackCD;
    [HideInInspector] public bool inAttackCD = false;
    //是否眩晕
    public bool isFainting;
    [Header("受击相关")]
    [HideInInspector] public SpriteRenderer spriteRenderer;
    //敌人是否处于击退、击退的力、击退持续时间
    [HideInInspector] public bool isKnockback;
    [HideInInspector] public float knockbackForce;
    [HideInInspector] public float knockbackDuration = 0.3f;
    [HideInInspector] public Rigidbody2D rigidbody2D;
    //敌人碰撞体
    [HideInInspector] public Collider2D collider2D;
    //伤害数字对象预制体
    public GameObject damageNumPrefab;
    [Header("战利品")]
    public int[] enemyProduceItemID;
    //生成的最大最小数量和确定生成数量
    public int[] minProduceNum;
    public int[] maxPriduceNum;
    public int randomPriduceNum;
}
//有限状态机，直接添加到Enemy01对象上
public class FSM : MonoBehaviour
{
    public Parameter parameter;
    //当前状态，仅用于在 Inspector 中查看怪物处于什么状态（运行时可实时观察）
    [Header("当前状态（运行时查看）")]
    public EnemyStateType currentStateType;
    private IState currentState;
    //字典，键，值对集合，通过前面的键，获得后面的值value
    private Dictionary<EnemyStateType,IState> states = new Dictionary<EnemyStateType,IState>();
    public ReapItem reapItem;
    //敌人闪白后减弱速度
    private float blinkDecaySpeed = 5f;
    //敌人材质
    private Material material;
    //白色程度
    private float blinkFactor;
    //是否在闪白
    public bool isBlink;
    private GameState gameState;
    //A* 寻路相关
    private Grid grid;
    //当前移动协程
    private Coroutine moveRoutine;
    //是否正在沿 A* 路径移动
    public bool isMoving;
    //最近一次 StartMoveTo 是否成功找到路径，供状态判断目标是否可达
    public bool lastPathFound;
    //路径走完后的回调
    public Action onMoveComplete;

    private void OnEnable()
    {
        EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
    }
    private void OnDisable()
    {
        EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
    }
    /// <summary>
    /// 确保只有在游戏进行的情况下敌人才可以按照状态机执行脚本
    /// </summary>
    /// <param name="state"></param>
    private void OnUpdateGameStateEvent(GameState state)
    {
        gameState = state;
    }
    private void Awake()
    {
        parameter.animator = GetComponent<Animator>();
        parameter.spriteRenderer = GetComponent<SpriteRenderer>();
        parameter.rigidbody2D = GetComponent<Rigidbody2D>();
        parameter.collider2D = GetComponent<Collider2D>();
        parameter.chaseArae = transform.GetChild(1).GetComponent<CircleCollider2D>();
        parameter.playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        material = parameter.spriteRenderer.material;
        grid = FindObjectOfType<Grid>();
    }
    private void Start()
    {
        //TODO:给敌人添加新状态也要给字典添加
        //字典states中添加敌人Idle状态，键为EnemyStateType.Idle，值为IdleState()方法
        states.Add(EnemyStateType.Idle,new IdleState(this));
        states.Add(EnemyStateType.Patrol, new PatrolState(this));
        states.Add(EnemyStateType.Chase, new ChaseState(this));
        states.Add(EnemyStateType.Attack, new AttackState(this));
        states.Add(EnemyStateType.Hurted, new HurtedState(this));
        states.Add(EnemyStateType.Death, new DeathState(this));
        states.Add(EnemyStateType.Fainting, new FaintingState(this));
        //设置初始状态为Idle
        TransitionState(EnemyStateType.Idle);
    }

    private void Update()
    {
        switch (gameState)
        {
            case GameState.Gameplay:
                currentState.OnUpdate();
                break;
            case GameState.Pause:
                break;
        }
        //闪白渐弱
        if (isBlink)
        {
            if (blinkFactor <= 0f)
            {
                return;
            }
            blinkFactor = Mathf.Lerp(blinkFactor, 0, Time.deltaTime * blinkDecaySpeed);
            if (blinkFactor < 0.01f)
            {
                blinkFactor = 0f;
            }
            ApplyBlinkFactor();
        }

    }
    private void FixedUpdate()
    {
        switch (gameState)
        {
            case GameState.Gameplay:
                currentState.OnFixUpdate();
                //攻击CD计时
                if (parameter.inAttackCD)
                {
                    parameter.attackCD += Time.fixedDeltaTime;

                    if (parameter.attackCD >= 2)
                    {
                        parameter.inAttackCD = false;
                        parameter.attackCD = 0;
                    }
                }
                break;
            case GameState.Pause:
                break;
        }
       
    }
    /// <summary>
    /// 玩家进入追击范围
    /// </summary>
    /// <param name="other"></param>
   public void FindPlayer(Transform player)
    {
        parameter.target = player.transform;
        parameter.chaseArae.enabled = false;
    }
    /// <summary>
    /// 对玩家发射察觉射线,判断是否发现玩家
    /// </summary>
    public bool ShoutNoticeRay()
    {
        
        Vector2 direction = (parameter.target.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, parameter.target.position);
        //发射射线
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direction,
            distance,
            parameter.obstacleLayer
            );
        //可视化射线
        Debug.DrawRay(transform.position, direction * distance,
                 hit.collider != null ? Color.red : Color.green);
        //被障碍物遮挡
        if (hit.collider != null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    /// <summary>
    /// 切换状态
    /// </summary>
    /// <param name="type"></param>
    public void TransitionState(EnemyStateType type)
    {
        //退出当前状态
        if(currentState != null)
        {
            currentState.OnExit();
        }
        //改为切换后的状态
        currentState = states[type];
        currentStateType = type;
        currentState.OnEnter();
    }
    /// <summary>
    /// 设置敌人对着目标的方向
    /// </summary>
    /// <param name="target"></param>
    public void SetFaceDirection(Transform target)
    {
        if(target != null)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            parameter.moveX = direction.x;
            parameter.moveY = direction.y;
            //parameter.faceX = direction.x;
            //parameter.faceY = direction.y;
            parameter.faceX = target.transform.position.x - transform.position.x;
            parameter.faceY = target.transform.position.y - transform.position.y;
            if (Math.Abs(parameter.faceX) > Math.Abs(parameter.faceY))
            {
                parameter.faceY = 0;
            }
            else
            {
                parameter.faceX = 0;
            }
        }        
    }
    /// <summary>
    /// 攻击命中玩家
    /// </summary>
    /// <param name="player"></param>
    public void AttackPlayer(Collider2D player)
    {
        player.GetComponentInParent<PlayerController>().PlayerHurted(parameter.enemyDamage,5,transform);

    }
    /// <summary>
    /// 激活对应攻击框并开始检测攻击对象、调用在敌人攻击动画帧上
    /// </summary>
    /// <param name="boxIndex"></param>
    public void SetUpAttackBox(int boxIndex)
    {
        parameter.attackBox[boxIndex].enabled = true;
    }
    /// <summary>
    /// 关闭攻击框、调用在敌人攻击动画帧上
    /// </summary>
    public void SetDownAttackBox()
    {
        foreach (var box in parameter.attackBox)
        {
            box.enabled = false;
        }
    }
    /// <summary>
    /// 该方法为官方方法，可以在屏幕上绘制图像
    /// </summary>
    //private void OnDrawGizmos()
    //{
    //    //绘制一个空心圆attackPoint圆心，attackArea半径
    //    Gizmos.DrawWireSphere(parameter.attackPoint.position, parameter.attackArea);
    //}
    /// <summary>
    /// 敌人受伤
    /// </summary>
    public void EnemyHurted(int damage,float attackForce)
    {
        int powerfulProbability = UnityEngine.Random.Range(0, 101);
        int currentDamage;
        bool isPowerful;
        if(powerfulProbability > 50)
        {
            currentDamage = damage * 2;
            parameter.health -= damage * 2;
            isPowerful = true;
        }
        else
        {
            currentDamage = damage;
            parameter.health -= damage;
            isPowerful = false;
        }
        ShowDanageNumOfEmeny(currentDamage, isPowerful);
        parameter.knockbackForce = attackForce;
        parameter.isHurted = true;
    }
    /// <summary>
    /// 开始闪白
    /// </summary>
    public void ApplyBlinkFactor()
    {
        material.SetFloat("_BlinkFactor", blinkFactor);
        if (blinkFactor <= 0)
        {
            isBlink = false;
        }
    }
    /// <summary>
    /// 初始化闪白参数
    /// </summary>
    public void Blink()
    {
        blinkFactor = 3f;
        ApplyBlinkFactor();
    }
    /// <summary>
    /// 显示伤害数字
    /// </summary>
    public void ShowDanageNumOfEmeny(int damage,bool isPowerful)
    {
        
        var damageNum = Instantiate(parameter.damageNumPrefab,transform.position, Quaternion.identity,transform);
        damageNum.GetComponent<DamageNum>().SetDamageNumText(damage);
        damageNum.GetComponent<DamageNum>().SetDamageNumColor(isPowerful);
        //根据与玩家的位置来播放对应的伤害数字动画
        if (MathF.Abs(transform.position.x - parameter.playerTransform.position.x) > 0.2f)
        {
            if (transform.position.x < parameter.playerTransform.position.x)
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
    /// 敌人被击退
    /// </summary>
    /// <param name="pos"></param>
    public void Knockback(Vector3 pos)
    {
        StartCoroutine(KnockbackCoroutine(pos));
    }
    private IEnumerator KnockbackCoroutine(Vector3 pos)
    {
        var direction = (transform.position - pos).normalized;
        parameter.rigidbody2D.mass = 1;
        parameter.rigidbody2D.AddForce(direction * parameter.knockbackForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(parameter.knockbackDuration);
        parameter.rigidbody2D.mass = 500;
    }
    /// <summary>
    /// 在死亡动画最后一帧执行销毁敌人对象并生成道具
    /// </summary>
    public void EnemyDestroy()
    {
        Destroy(this.gameObject);
        //reapItem = GetComponent<ReapItem>();
        //reapItem.SpawnHarvestItems();
        //生成对应的成果和数量
        for (int i = 0; i < parameter.enemyProduceItemID.Length; i++)
        {
            if (parameter.minProduceNum[i] < parameter.maxPriduceNum[i])
            {
                parameter.randomPriduceNum = UnityEngine.Random.Range(parameter.minProduceNum[i], parameter.maxPriduceNum[i] + 1);
            }
            else
            {
                 parameter.randomPriduceNum = parameter.minProduceNum[i];
            }
            if ( parameter.randomPriduceNum <= 0)
            {
                 parameter.randomPriduceNum = 0;
            }
            for (int j = 0; j <  parameter.randomPriduceNum; j++)
            {
                EventHandler.CallInstantiateItemInScene(parameter.enemyProduceItemID[i], transform.position);
            }
        }
    }
    #region A* 寻路移动
    /// <summary>
    /// 以 A* 路径移动到目标世界坐标。到达后触发 onMoveComplete。
    /// 怪物追击进入攻击范围、受伤等情况需先调用 StopMove 打断。
    /// </summary>
    /// <param name="worldTarget">目标世界坐标</param>
    /// <param name="speed">移动速度（格/秒）</param>
    public void StartMoveTo(Vector3 worldTarget, float speed)
    {
        StopMove();
        if (grid == null)
        {
            grid = FindObjectOfType<Grid>();
        }
        if (grid == null)
        {
            moveRoutine = StartCoroutine(CompleteMoveDeferred());
            return;
        }
        Vector3Int startCell = grid.WorldToCell(transform.position);
        Vector3Int endCell = grid.WorldToCell(worldTarget);
        //起点与终点同格：无需寻路，直接视为到达
        if (startCell == endCell)
        {
            lastPathFound = true;
            moveRoutine = StartCoroutine(CompleteMoveDeferred());
            return;
        }
        string sceneName = SceneManager.GetActiveScene().name;
        Stack<MovementStep> path = new Stack<MovementStep>();
        AStar.Instance.buildPath(sceneName, new Vector2Int(startCell.x, startCell.y), new Vector2Int(endCell.x, endCell.y), path, false);
        if (path.Count == 0)
        {
            //目标确实不可达，标记失败并延迟回调交由状态决定下一步
            lastPathFound = false;
            moveRoutine = StartCoroutine(CompleteMoveDeferred());
            return;
        }
        lastPathFound = true;
        isMoving = true;
        moveRoutine = StartCoroutine(MoveAlongPath(path, speed));
    }
    /// <summary>
    /// 停止当前 A* 移动并清空路径。
    /// </summary>
    public void StopMove()
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }
        isMoving = false;
    }
    /// <summary>
    /// 延迟一帧触发完成回调，避免在 StartMoveTo 的调用栈里同步递归
    /// </summary>
    private IEnumerator CompleteMoveDeferred()
    {
        yield return null;
        isMoving = false;
        moveRoutine = null;
        onMoveComplete?.Invoke();
    }
    /// <summary>
    /// 沿 A* 路径逐格移动到终点
    /// </summary>
    private IEnumerator MoveAlongPath(Stack<MovementStep> path, float speed)
    {
        //先让出一帧，保证完成回调不会在 StartMoveTo 的调用栈里同步触发
        yield return null;
        while (path.Count > 0)
        {
            MovementStep step = path.Pop();
            Vector3 targetWorld = grid.CellToWorld(new Vector3Int(step.gridCoordinate.x, step.gridCoordinate.y, 0)) + new Vector3(0.5f, 0.5f, 0);
            //每格更新朝向与移动动画参数
            Vector2 dir = (targetWorld - transform.position).normalized;
            parameter.moveX = dir.x;
            parameter.moveY = dir.y;
            parameter.animator.SetFloat("MoveX", parameter.moveX);
            parameter.animator.SetFloat("MoveY", parameter.moveY);
            while (Vector2.Distance(transform.position, targetWorld) > 0.01f)
            {
                parameter.rigidbody2D.MovePosition(Vector2.MoveTowards(transform.position, targetWorld, speed * Time.deltaTime));
                yield return null;
            }
            //对齐到格子中心，避免浮点误差累积
            transform.position = targetWorld;
        }
        isMoving = false;
        moveRoutine = null;
        onMoveComplete?.Invoke();
    }
    #endregion

}
