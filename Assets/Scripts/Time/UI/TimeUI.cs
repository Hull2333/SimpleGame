using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEditor.Scripting;
using static AnimalData_SO;

public class TimeUI : MonoBehaviour //调用在GameTime对象上
{
    public Canvas timeCanvas;
    public GameObject timeUIBg;
    public Image seasonImage;
    public Image weatherImage;
    public Text dayText;
    public Text yearText;
    public Text timeText;
    //四个季节的图片
    public Sprite[] seasonSprites;
    //星期图片
    public Sprite[] weekDaySprites;
    //天气图片
    public Sprite[] weatherSprites;
    public Image weekImage;

    private void OnEnable()
    {
        EventHandler.GameMinuteEvent += OnGameMinuteEvent;
        EventHandler.GameDateEvent += OnGameDateEvent;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.StartNPCEvent += OnStartNPCEvent;
        EventHandler.EndNPCEvent += OnEndNPCEvent;
        EventHandler.BuildindModeEvent += OnBuildindModeEvent;
    }

    private void OnDisable()
    {
        EventHandler.GameMinuteEvent -= OnGameMinuteEvent;
        EventHandler.GameDateEvent -= OnGameDateEvent;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.StartNPCEvent -= OnStartNPCEvent;
        EventHandler.EndNPCEvent -= OnEndNPCEvent;
        EventHandler.BuildindModeEvent -= OnBuildindModeEvent;
    }

    private void Awake()
    {
        timeCanvas.worldCamera = FindAnyObjectByType<Camera>();
        timeCanvas.sortingLayerName = "Collision";
    }

    /// <summary>
    /// 小时分钟UI显示
    /// </summary>
    /// <param name="mintue"></param>
    /// <param name="hour"></param>
    private void OnGameMinuteEvent(int mintue, int hour, int day, Season season)
    {
        //"00"以两位数显示
        timeText.text = hour.ToString("00") + ":" + mintue.ToString("00");
    }
    /// <summary>
    /// 年月日UI显示
    /// </summary>
    /// <param name="hour"></param>
    /// <param name="day"></param>
    /// <param name="month"></param>
    /// <param name="year"></param>
    /// <param name="season"></param>
    private void OnGameDateEvent(int hour, int day, int month, int year, Season season)
    {
        //天数与星期图片显示
        RefreshDayUI(day, TimeManager.Instance.gameWeekday);
        yearText.text = "Year" + " " + year.ToString("00");
        //季节图片显示
        seasonImage.sprite = seasonSprites[(int)season];
    }
    /// <summary>
    /// 游戏开始或读档后场景加载完成，立即刷新一次天数和星期显示
    /// </summary>
    private void OnAfterSceneLoadedEvent()
    {
        RefreshDayUI(TimeManager.Instance.gameDay, TimeManager.Instance.gameWeekday);
    }
    /// <summary>
    /// 刷新天数文本、星期图片和天气图片
    /// </summary>
    private void RefreshDayUI(int day, Weekday weekday)
    {
        dayText.text = "Day" + " " + day.ToString("00");
        //weekDaySprites的顺序与Weekday枚举一致，可直接用枚举值索引
        weekImage.sprite = weekDaySprites[(int)weekday];
        //weatherSprites的顺序与Weather枚举一致，可直接用枚举值索引
        weatherImage.sprite = weatherSprites[(int)TimeManager.Instance.currentWeather];
    }

    private void OnStartNPCEvent()
    {
        transform.GetComponentInParent<Canvas>().enabled = false;
    }
    private void OnEndNPCEvent()
    {
        transform.GetComponentInParent<Canvas>().enabled = true;
    }
    private void OnBuildindModeEvent(BuildingDetails details, AnimalDetails animal, bool startMode)
    {
        timeUIBg.SetActive(!startMode);
    }
}
