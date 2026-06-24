using System.Collections;
using System.Collections.Generic;
using TMPro;
using static AnimalData_SO;
using UnityEngine;

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
    private void OnEnable()
    {
        EventHandler.GameDayEvent += OnGameDayEvent;
    }
    private void OnDisable()
    {
        EventHandler.GameDayEvent -= OnGameDayEvent;
    }

    private void OnGameDayEvent(int arg1, Season season)
    {
        if(currentGrowthDay <= animalDetails.growthDay)
        {
            currentGrowthDay++;
            //已成年
            if(currentGrowthDay == animalDetails.growthDay)
            {
                transform.GetChild(0).gameObject.SetActive(false);
                transform.GetChild(1).gameObject.SetActive(true);
                anim = transform.GetChild(1).GetComponent<Animator>();
            }
        }
    }

    public void FixedUpdate()
    {
        if (!isMoving)
        {
            anim.SetBool("walking", isMoving);
            anim.SetFloat("X", dir.x);
            nextPos = GetRandomPosInArea();
            idleTime -= Time.fixedDeltaTime;
            if(idleTime <= 0f)
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
        transform.position = Vector2.MoveTowards(
            transform.position,
            nextPos,
            moveSpeed * Time.fixedDeltaTime
        );
        if (Vector2.Distance(transform.position, nextPos) <= 0.1f)
        {
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
        transform.position = GetRandomPosInArea();
        if(currentGrowthDay < animalDetails.growthDay)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            anim = transform.GetChild(0).GetComponent<Animator>();
            transform.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            transform.GetChild(0).gameObject.SetActive(false);
            anim = transform.GetChild(1).GetComponent<Animator>();
            transform.GetChild(1).gameObject.SetActive(true);
        }
    }
}
