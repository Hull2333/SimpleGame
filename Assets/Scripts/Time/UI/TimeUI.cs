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
    public Text dayText;
    public Text yearText;
    public Text timeText;
    //四个季节的图片
    public Sprite[] seasonSprites;
    
    private void OnEnable()
    {
        EventHandler.GameMinuteEvent += OnGameMinuteEvent;
        EventHandler.GameDateEvent += OnGameDateEvent;
        EventHandler.StartNPCEvent += OnStartNPCEvent;
        EventHandler.EndNPCEvent += OnEndNPCEvent;
        EventHandler.BuildindModeEvent += OnBuildindModeEvent;
    }

    private void OnDisable()
    {
        EventHandler.GameMinuteEvent -= OnGameMinuteEvent;
        EventHandler.GameDateEvent -= OnGameDateEvent;
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
    private void OnGameMinuteEvent(int mintue, int hour,int day,Season season)
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
        dayText.text = "Day" + " "+ day.ToString("00");
        yearText.text = "Year" + " " + year.ToString("00");
        //季节图片显示
        seasonImage.sprite = seasonSprites[(int)season];
    }

    private void OnStartNPCEvent()
    {
        transform.GetComponentInParent<Canvas>().enabled = false;
    }
    private void OnEndNPCEvent()
    {
        transform.GetComponentInParent<Canvas>().enabled = true;
    }
    private void OnBuildindModeEvent(BuildingDetails details,AnimalDetails animal, bool isBuild)
    {
        timeUIBg.SetActive(!isBuild);
    }
}
