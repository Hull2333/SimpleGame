using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeGrowing : MonoBehaviour //调用在树预制体上
{
    //已生长的天数
    public int growthDays;
    public int to2ndDay, to3rdDay;
    public Transform first, second, third;
    //树是否可以生长
    public bool isTreeGrow = true;
    private void Awake()
    {
        growthDays = Random.Range(0, to2ndDay + to3rdDay + 1);
        TreeStartGrowing();
    }
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
        if (isTreeGrow)
        {
            if (growthDays <= to2ndDay + to3rdDay)
            {
                growthDays++;
            }
            TreeStartGrowing();
        }
    }
       
    /// <summary>
    /// 初始化树木生长天数
    /// </summary>
    public void InitTreeGrowDay()
    {
        
        growthDays = 0;
        TreeStartGrowing();
    }
    /// <summary>
    /// 树木生长
    /// </summary>
    public void TreeStartGrowing()
    {
        if (growthDays < to2ndDay)
        {
            first.gameObject.SetActive(true);
            second.gameObject.SetActive(false);
            third.gameObject.SetActive(false);

        }
        if (growthDays >= to2ndDay && growthDays < to2ndDay + to3rdDay)
        {
            first.gameObject.SetActive(false);
            second.gameObject.SetActive(true);
            third.gameObject.SetActive(false);

        }
        if (growthDays >= to2ndDay + to3rdDay)
        {
            first.gameObject.SetActive(false);
            second.gameObject.SetActive(false);
            third.gameObject.SetActive(true);

        }

    }
}
