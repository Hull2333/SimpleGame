using MFarm.Map;
using MFarm.Transition;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingItem : MonoBehaviour //调用在可建造建筑预制体上
{
    public int buildID;
    public SpriteRenderer spriteRenderer;
    //接受的动物尺寸
    public AnimalSizeType acceptSize;
    //随机的建筑识别码,用于和其建筑下的动物绑定
    public int buildCodeID;
    //建筑序号
    public int buildIndex;
    private BoxCollider2D collider2D => gameObject.transform.GetChild(0).GetComponent<BoxCollider2D>();
    [Header("建造阶段")]
    public int[] buildingDays;
    public Sprite[] buildSprite;
    //当前已建造天数
     public int currentBuildingDay;
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
    //未放置
    public bool isSet = false;
    //已建成
    public bool isDone;
    public Collider2D animalArea;
    //建筑箭头图片
    public GameObject arrowIcon;
    [Header("新建场景相关")]
    public string tempSceneName;
    [HideInInspector] public string buildsceneName;
    public Vector3 positionToGo;
    public Teleport entrance;
    private void OnEnable()
    {
        EventHandler.GameDayEvent += OnGameDayEvent;
        EventHandler.DisplayBuildingArrowIcon += OnDisplayBuildingArrowIcon;
    }
    private void OnDisable()
    {
        EventHandler.GameDayEvent -= OnGameDayEvent;
        EventHandler.DisplayBuildingArrowIcon -= OnDisplayBuildingArrowIcon;
    }
  
    private void OnGameDayEvent(int arg1, Season season)
    {
        Building(false);
    }
    private void OnDisplayBuildingArrowIcon(AnimalSizeType type, bool isShow)
    {
        if (isShow)
        {
            if (acceptSize == type && isDone)
            {
                
                arrowIcon.SetActive(true);
            }
        }
        else
        {
            arrowIcon.SetActive(false);
        }
       
        
    }
    private void Awake()
    {
        animalArea = transform.GetChild(1).GetComponent<Collider2D>();
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
        if(currentBuildingDay >= totalBuildingDays && !isDone)
        {
            isDone = true;
            entrance.gameObject.SetActive(true);
            entrance.code = buildCodeID;
            entrance.sceneToGo = tempSceneName;
            entrance.positionToGo = positionToGo;
        }
    }

   
}
