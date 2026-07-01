using MFarm.Transition;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static AnimalData_SO;

public class AnimalController : MonoBehaviour //调用在每个动物预制体上
{
    private Animator anim;
    //动物识别码
    public int animCodeID;
    //移动方向
    private Vector2 dir;
    //执行下一步的停留时间
    private float idleTime;
    //强制执行下一步的冗余时间
    private float nextStepTime = 8f;
    //活动范围
    public Collider2D activityArae;
    private float moveSpeed = 1f;
    private bool isMoving;
    //睡觉时间
    private bool sleepTime;
    private Vector2 nextPos;
    //当前的成长天数
    public int currentGrowthDay;
    public AnimalDetails animalDetails;
    [Header("动物生产相关")]
    private bool isBeingTouched;
    private float touchDistance = 3f;
    //今天已触摸
    public bool isTouch;
    //动物好感度
    public float animalFriendliness;
    public int produceItemID;
    //满好感度生产的稀有物品ID
    public int produceRareItemID;
    //已成年
    private bool isAdult;
    //生产在场景中
    public bool isProduceInScene;
    private Rigidbody2D rb;
    [Header("回舍设置")]
    [Tooltip("前往出口随机概率（0-1）")]
    private float goHomeChance = 0.8f;
    private bool isGoingHome;
    public Vector2 homePosition;
    private bool isAfter18;
    private bool isAfrer8;
    private bool hasDecidedNextPos;
    //在室外
    public bool isOutSide;
    //推开动物
    private bool isBeingPushed;
    private float pushDistance = 1f;
    [Header("动物气泡")]
    public Animator happleBubbleAnim;
    public Animator sleepBubbleAnim;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void OnEnable()
    {
        EventHandler.GameDayEvent += OnGameDayEvent;
        EventHandler.GameMinuteEvent += OnGameMinuteEvent;
    }
    private void OnDisable()
    {
        EventHandler.GameDayEvent -= OnGameDayEvent;
        EventHandler.GameMinuteEvent -= OnGameMinuteEvent;
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PushAwayFromPlayer(other.transform.position);
        }
    }


    private void OnGameDayEvent(int arg1, Season season)
    {
        isTouch = false;
        if (currentGrowthDay <= animalDetails.growthDay)
        {
            currentGrowthDay++;
            //已成年
            if (currentGrowthDay == animalDetails.growthDay)
            {
                isAdult = true;
                transform.GetChild(0).gameObject.SetActive(false);
                transform.GetChild(1).gameObject.SetActive(true);
                anim = transform.GetChild(1).GetComponent<Animator>();
            }
        }
        if (isAdult && isProduceInScene)
        {
            //每天随机生产，好感度越高，生产概率越大
            int produceOdd = Random.Range(1, 101);
            if(produceOdd + animalFriendliness * 10 >= 80)
            {
                //稀有物品生产概率
                int produceRareOdd = Random.Range(1, 101);
                //好感度到达5时，有概率生产稀有物品
                if (animalFriendliness >= 5 && produceRareOdd >= 50)
                {
                    EventHandler.CallInstantiateAniamlProduceItemEvent(animCodeID, produceRareItemID);
                    return;
                }
                EventHandler.CallInstantiateAniamlProduceItemEvent(animCodeID, produceItemID);
            }
           
        }

    }
    private void OnGameMinuteEvent(int minute, int hour, int day, Season season)
    {
        if (isOutSide)
        {
            //时间来到18:00
            isAfter18 = hour >= 18;
            //获取自己建筑的出口
            homePosition = activityArae.transform.parent.GetChild(0).transform.position;
        }
        if (!isOutSide)
        {
            // 计算新的睡觉状态
            bool newSleepTime = hour >= 19 || hour <= 7;
            // 状态发生变化时才处理
            if (newSleepTime != sleepTime)
            {
                sleepTime = newSleepTime;
                if (sleepTime)
                {
                    // 进入睡觉：立刻停下，播放睡觉动画
                    isMoving = false;
                    hasDecidedNextPos = false;
                    anim.SetBool("walking", false);
                    anim.SetBool("isSleep", true);
                }
                else
                {
                    // 离开睡觉：重置睡觉动画，恢复正常
                    anim.SetBool("isSleep", false);
                    hasDecidedNextPos = false;
                }
            }
            //时间来到8:00
            isAfrer8 = hour >= 8 && hour < 12;
            homePosition = FindAnyObjectByType<Teleport>().transform.position;
        }
    }
    public void FixedUpdate()
    {
        if (!sleepTime && !isBeingPushed && !isBeingTouched)
        {
            if (!isMoving)
            {
                anim.SetBool("walking", isMoving);
                anim.SetFloat("X", dir.x);
                if (!hasDecidedNextPos)
                {
                    nextPos = GetRandomPosInArea();
                    hasDecidedNextPos = true;
                }
                idleTime -= Time.fixedDeltaTime;
                if (idleTime <= 0f)
                {
                    isMoving = true;
                }
            }
            else
            {
                MoveToNextPos();
            }
        }
    }
    /// <summary>
    /// 在活动范围内随机获取下一目标点
    /// </summary>
    /// <returns></returns>
    public Vector2 GetRandomPosInArea()
    {
        //18:00后有一定概率回舍，第四次必须回舍
        if (isAfter18 || isAfrer8)
        {
            if (ShouldGoHome())
            {
                isGoingHome = true;
                return homePosition;
            }
            
        }
        // 获取 Collider 边界
        Bounds bounds = activityArae.bounds;
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomY = Random.Range(bounds.min.y, bounds.max.y);
        Vector2 randomPoint = new Vector2(randomX, randomY);
        return randomPoint;
    }
    /// <summary>
    /// 朝下一点移动
    /// </summary>
    public void MoveToNextPos()
    {
        dir = (nextPos - (Vector2)transform.position).normalized;
        anim.SetBool("walking", isMoving);
        anim.SetFloat("X", dir.x);
        // 向目标位置移动
        rb.MovePosition(Vector2.MoveTowards(
            transform.position,
            nextPos,
            moveSpeed * Time.fixedDeltaTime
        ));
        if (Vector2.Distance(transform.position, nextPos) <= 0.3f)
        {
            //到达鸡舍出口
            if (isGoingHome)
            {
                isMoving = false;
                isAfter18 = false;
                isAfrer8 = false;
                isGoingHome = false;
                //从室外回到室内
                if (isOutSide)
                {
                    isOutSide = false;
                    SceneAnimal animal = new SceneAnimal
                    {
                        animalDetails = animalDetails,
                        friendliness = animalFriendliness,
                        isTouch = isTouch,
                        animalCode = animCodeID,
                        growthDay = currentGrowthDay,
                        isOutSide = isOutSide
                    };
                    EventHandler.CallAnimalArrivedAtHomeEvent(animCodeID, animal);
                    Destroy(gameObject);
                    return;
                   
                }
                //从室内回到室外
                else
                {
                    isOutSide = true;
                    EventHandler.CallAnimalExitCoopEvent(animCodeID,
                    new SceneAnimal { animalDetails = animalDetails, animalCode = animCodeID, growthDay = currentGrowthDay, isOutSide = true });
                    Destroy(gameObject);
                    // 销毁后不再执行后续逻辑
                    return;
                }
            }
            isMoving = false;
            int actionIndex = Random.Range(1, 4);
            switch (actionIndex)
            {
                case 1:
                    anim.SetTrigger("action01");
                    break;
                case 2:
                    anim.SetTrigger("action02");
                    break;
                case 3:
                    break;
            }
            idleTime = Random.Range(2.5f, 4f);
           
            hasDecidedNextPos = false;
        }
    }
    /// <summary>
    /// 设置初始状态
    /// </summary>
    public void SetStartState(bool isBuy)
    {
        //是刚刚购买
        if (isBuy)
        {
            currentGrowthDay = 0;
        }
        sleepTime = false;
        SetAnimalSprite();
        transform.position = GetRandomPosInArea();
    }
    /// <summary>
    /// 设置此时动物的图片
    /// </summary>
    private void SetAnimalSprite()
    {
        if (currentGrowthDay < animalDetails.growthDay)
        {
            isAdult = false;
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(1).gameObject.SetActive(false);
            anim = transform.GetChild(0).GetComponent<Animator>();
        }
        else
        {
            isAdult = true;
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(true);
            anim = transform.GetChild(1).GetComponent<Animator>();
        }
    }
    /// <summary>
    /// 判断是否需要回舍
    /// </summary>
    /// <returns></returns>
    private bool ShouldGoHome()
    {
        // 随机判定
        return Random.value < goHomeChance;
    }
    /// <summary>
    /// 从玩家位置推开动物
    /// </summary>
    /// <param name="playerPos"></param>
    public void PushAwayFromPlayer(Vector2 playerPos)
    {
        if (!sleepTime || isBeingPushed) return;
        StartCoroutine(PushAwayCoroutine(playerPos));
    }
    /// <summary>
    /// 推开动物的协程
    /// </summary>
    /// <param name="playerPos"></param>
    /// <returns></returns>
    private IEnumerator PushAwayCoroutine(Vector2 playerPos)
    {
        isBeingPushed = true;
        // 计算远离玩家的方向
        Vector2 pushDir = ((Vector2)transform.position - playerPos).normalized;
        dir = pushDir;
        Vector2 targetPos = (Vector2)transform.position + pushDir * pushDistance;
        // 移动到目标点
        // 先退出睡觉动画，再播放行走动画
        anim.SetBool("isSleep", false);
        anim.SetBool("walking", true);
        anim.SetFloat("X", dir.x);
        while (Vector2.Distance(transform.position, targetPos) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(
                transform.position, targetPos, moveSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        // 恢复睡觉
        isMoving = false;
        isBeingPushed = false;
        anim.SetBool("walking", false);
        anim.SetBool("isSleep", true);
    }
    /// <summary>
    /// 增加动物好感度
    /// </summary>
    /// <param name="addValue"></param>
    public void AddAnimalFriendliness(float addValue,Vector2 playerPos)
    {
       
        //今日已触摸且不在睡觉状态，且玩家在触摸范围内
        if (!sleepTime && !isTouch && Vector2.Distance(transform.position, playerPos) <= touchDistance)
        {
            isBeingTouched = true;
            // 停止移动
            isMoving = false;
            //今日已触摸过，不能再增加好感度
            isTouch = true;
            animalFriendliness += addValue;
            // 面向玩家
            float facingDir = playerPos.x > transform.position.x ? 1f : -1f;
            anim.SetFloat("X", facingDir);
            anim.SetBool("walking", false);
            StartCoroutine(PlayHappyBubble(facingDir));
        }
        //睡觉时播放睡觉气泡
        if (sleepTime && Vector2.Distance(transform.position, playerPos) <= touchDistance)
        {
            StartCoroutine(PlaySleepBubble());
        }
       
    }
    private IEnumerator PlayHappyBubble(float faceX)
    {
        happleBubbleAnim.gameObject.SetActive(true);
        happleBubbleAnim.SetFloat("X", faceX);
        // 等待动画播放完成
        yield return new WaitForSeconds(happleBubbleAnim.GetCurrentAnimatorStateInfo(0).length);
        happleBubbleAnim.gameObject.SetActive(false);
        isBeingTouched = false;       
        hasDecidedNextPos = false;    
    }
    private IEnumerator PlaySleepBubble()
    {
        sleepBubbleAnim.gameObject.SetActive(true);
        sleepBubbleAnim.SetFloat("X", dir.x);
        // 等待动画播放完成
        yield return new WaitForSeconds(sleepBubbleAnim.GetCurrentAnimatorStateInfo(0).length);
        sleepBubbleAnim.gameObject.SetActive(false);
    }
}