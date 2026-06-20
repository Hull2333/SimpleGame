using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingItem : MonoBehaviour //调用在可建造建筑预制体上
{
    public int buildID;
    public SpriteRenderer spriteRenderer;
    private BoxCollider2D collider2D => gameObject.transform.GetChild(0).GetComponent<BoxCollider2D>();
    [Header("建造阶段")]
    public int[] buildingDays;
    public Sprite[] buildSprite;
    //当前已建造天数
    [HideInInspector] public int currentBuildingDay;
    //总建造天数
    public int totalBuildingDays
    {
        get
        {
            int amount = 0;
            foreach (var days in buildingDays)
            {
                amount += days;
            }
            return amount;
        }
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
        Building(false);
    }
    public void SwitchCollider2D(bool active)
    {
        collider2D.enabled = active;
    }
    /// <summary>
    /// 开始建造
    /// </summary>
    /// <param name="isFirstDay">是否是刚开始建造</param>
    public void Building(bool isFirstDay)
    {
        if (!isFirstDay)
        {
            if(currentBuildingDay < totalBuildingDays)
            {
                currentBuildingDay++;
            }
        }
        int currentStage = 0;
        int buildStage = buildingDays.Length;
        int allDay = totalBuildingDays;
        for (int i = buildStage - 1; i >= 0; i--)
        {
            if(currentBuildingDay >= allDay)
            {
                currentStage = i;
                break;
            }
            allDay -= buildingDays[i];
        }
        //更改建筑的图片
        spriteRenderer.sprite = buildSprite[currentStage];

    }
}
