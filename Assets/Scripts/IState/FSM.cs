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
    [Header("巡逻点,陷阱型敌人巡逻点设置为0")]
    //巡逻点
    public Transform[] patrolPoints;
    //当前巡逻点索引，PatrolState 用
    [HideInInspector] public int patrolPosition = 0;
    //是否原地返回巡逻
    [HideInInspector] public bool isBack = false;
    [Header("陷阱型敌人")]
    //陷阱型敌人（如蜘蛛）：由 TrapPlace 触发出场，无巡逻点，放弃追击后 A* 回到落点驻守
    public bool isTrapEnemy;
    //预警效果预制体（影子、光圈等）：TrapPlace 触发时先在落点实例化这个预制体，
    //等其效果播完后才让敌人从天而降。非陷阱型敌人忽略此字段。
    public GameObject appearPreviewPrefab;
    [Header("怪物类型")]
    //怪物类型，决定待机、巡逻、追击的移动方式
    public EnemyType enemyType = EnemyType.Ground;
    //飞行冲撞怪的冲刺加速度，最大速度沿用 chaseSpeed
    public float flyAcceleration = 10f;
    //飞行冲撞怪被击退时的力度倍率：速度驱动，击退冲量会完整保留，过大会被打飞很远
    [Range(0f, 1f)] public float flyKnockbackForceRate = 0.6f;
    [Header("追击相关")]
     public Transform parentPos;
    [HideInInspector] public CircleCollider2D chaseArae;
    //射线阻挡图层
    public LayerMask obstacleLayer;
    //追击目标
    [HideInInspector] public Transform playerTransform;
    //追击目标，运行时由 FindPlayer 赋值
    [HideInInspector] public Transform target;
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
    public float attackArea;
    //各类攻击框
    public Collider2D[] attackBox;
    //攻击冷却
    [HideInInspector] public float attackCD;
    [HideInInspector] public bool inAttackCD = false;
    [Header("远程攻击")]
    //是否为远程攻击：开启后进入攻击范围时不再播放近战攻击动画，而是朝玩家当前位置发射投射物
    public bool isRangedAttack = false;
    //远程攻击的投射物预制体
    public GameObject projectilePrefab;
    //投射物的生成点，发射方法传入的序号对应这里的下标
    public Transform[] projectileSpawnPoints;
    //发射投射物时在生成点播放的粒子特效预制体
    public GameObject projectileSpawnEffectPrefab;
    //是否眩晕，运行时由攻击框、防御框等逻辑设置
    [HideInInspector] public bool isFainting;
    [Header("受击相关")]
    //受击时播放的粒子特效类型（可在 Inspector 里为每种敌人单独指定）
    public ParticalEffectType hurtEffectType = ParticalEffectType.EnemyHit01;
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
    private float blinkDecaySpeed = 10f;
    //敌人材质
    private Material material;
    //白色程度
    private float blinkFactor;
    //是否在闪白，运行时状态
    [HideInInspector] public bool isBlink;
    private GameState gameState;
    //A* 寻路相关
    private Grid grid;
    //当前移动协程
    private Coroutine moveRoutine;
    //是否正在沿 A* 路径移动，运行时状态
    [HideInInspector] public bool isMoving;
    //最近一次 StartMoveTo 是否成功找到路径，供状态判断目标是否可达
    [HideInInspector] public bool lastPathFound;
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
        //暂停（如打开背包）时停止怪物的移动表现，恢复后按当前状态继续
        if (state == GameState.Pause)
        {
            parameter.animator.SetBool("isMoving", false);
            parameter.rigidbody2D.velocity = Vector2.zero;
        }
        else
        {
            parameter.animator.SetBool("isMoving", parameter.isMoving);
        }
    }
    private void Awake()
    {
        parameter.animator = GetComponent<Animator>();
        parameter.spriteRenderer = GetComponent<SpriteRenderer>();
        parameter.rigidbody2D = GetComponent<Rigidbody2D>();
        parameter.collider2D = GetComponent<Collider2D>();
        //追击检测圈按名字查找：不同敌人的子物体数量和顺序不同（如多方向攻击框），不能依赖固定下标
        Transform chaseAreaTransform = transform.Find("ChaseArea");
        if (chaseAreaTransform != null)
        {
            parameter.chaseArae = chaseAreaTransform.GetComponent<CircleCollider2D>();
        }
        else if (transform.childCount > 1)
        {
            parameter.chaseArae = transform.GetChild(1).GetComponent<CircleCollider2D>();
        }
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
    /// 朝玩家当前位置发射投射物，调用在远程敌人的攻击动画帧上
    /// spawnIndex 为生成点序号，对应 projectileSpawnPoints 的下标
    /// 投射物自身的运动与伤害逻辑后续实现
    /// </summary>
    public void FireProjectile(int spawnIndex)
    {
        if (parameter.projectilePrefab == null || parameter.target == null)
        {
            return;
        }
        //取对应序号的生成点，未配置或越界时回退到怪物自身位置
        Vector3 spawnPos = transform.position;
        if (parameter.projectileSpawnPoints != null
            && spawnIndex >= 0
            && spawnIndex < parameter.projectileSpawnPoints.Length
            && parameter.projectileSpawnPoints[spawnIndex] != null)
        {
            spawnPos = parameter.projectileSpawnPoints[spawnIndex].position;
        }
        Vector2 dir = (parameter.target.position - spawnPos).normalized;
        float angleZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        //在生成点播放发射粒子特效
        PlayProjectileSpawnEffect(spawnPos);
        //投射物预制体默认朝上（+Y），再减 90 度让它的朝向对准玩家
        GameObject projectileObj = Instantiate(parameter.projectilePrefab, spawnPos, Quaternion.Euler(0f, 0f, angleZ - 90f));
        //把当前敌人的攻击力传给投射物
        EnemyProjectile projectile = projectileObj.GetComponent<EnemyProjectile>();
        if (projectile != null)
        {
            projectile.damage = parameter.enemyDamage;
        }
    }
    /// <summary>
    /// 在投射物生成点播放发射粒子特效，播放完自动销毁
    /// </summary>
    private void PlayProjectileSpawnEffect(Vector3 spawnPos)
    {
        if (parameter.projectileSpawnEffectPrefab == null)
        {
            return;
        }
        GameObject effect = Instantiate(parameter.projectileSpawnEffectPrefab, spawnPos, Quaternion.identity);
        //按粒子总时长销毁，避免场景里堆积
        ParticleSystem particle = effect.GetComponent<ParticleSystem>();
        float lifeTime = particle != null ? particle.main.duration + particle.main.startLifetime.constantMax : 1f;
        Destroy(effect, lifeTime);
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
        //记录原质量，击退结束后恢复，避免把飞行怪等不同质量的敌人统一改回 500
        float originMass = parameter.rigidbody2D.mass;
        float knockbackForce = parameter.knockbackForce;
        if (parameter.enemyType == EnemyType.FlyingRam)
        {
            //飞行冲撞怪靠速度移动，击退冲量不会像地面怪那样被 MovePosition 立刻覆盖，按倍率削减击退距离
            knockbackForce *= parameter.flyKnockbackForceRate;
        }
        parameter.rigidbody2D.mass = 1;
        parameter.rigidbody2D.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(parameter.knockbackDuration);
        parameter.rigidbody2D.mass = originMass;
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
            while (Vector2.Distance(transform.position, targetWorld) > 0.02f)
            {
                //游戏暂停（如打开背包）时不移动，恢复后继续沿路径前进
                if (gameState != GameState.Gameplay)
                {
                    yield return null;
                    continue;
                }
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
