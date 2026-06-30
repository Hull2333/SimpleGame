using MFarm.Transition;
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
    private Vector2 nextPos;
    //当前的成长天数
    public int currentGrowthDay;
    public AnimalDetails animalDetails;
    [Header("动物生产相关")]
    public int produceItemID;
    //已成年
    private bool isAdult;
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



    private void OnGameDayEvent(int arg1, Season season)
    {
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
        if (isAdult)
        {
            //每天开始生产物品
            EventHandler.CallInstantiateAniamlProduceItemEvent(animCodeID, produceItemID);
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
            //时间来到8:00
            isAfrer8 = hour >= 8 && hour < 12;
            homePosition = FindAnyObjectByType<Teleport>().transform.position;
        }
    }
    public void FixedUpdate()
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
            isMoving = false;
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

}