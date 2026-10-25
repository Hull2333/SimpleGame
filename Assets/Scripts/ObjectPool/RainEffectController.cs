using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RainEffectController : MonoBehaviour //根据TimeManager的天气和当前场景开关雨天粒子特效
{
    public GameObject[] rainEffects;
    public GameObject snowEffect;
    //允许显示雨天粒子特效的场景名
    public string[] showRainSceneName;

    private void OnEnable()
    {
        EventHandler.GameDayEvent += OnGameDayEvent;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
    }

    private void OnDisable()
    {
        EventHandler.GameDayEvent -= OnGameDayEvent;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
    }

    private void Start()
    {
        //进入场景时先按当前天气和场景设置一次
        RefreshRainEffects();
    }

    private void OnGameDayEvent(int day, Season season, Weekday weekday)
    {
        RefreshRainEffects();
    }

    private void OnAfterSceneLoadedEvent()
    {
        //切换场景后重新判断是否显示
        RefreshRainEffects();
    }

    /// <summary>
    /// 只有雨天/雷雨天，并且当前场景在允许显示的场景列表中，才显示粒子特效
    /// 冬季显示下雪特效，其他季节显示下雨特效
    /// </summary>
    private void RefreshRainEffects()
    {
        Weather weather = TimeManager.Instance.currentWeather;
        bool isRainy = weather == Weather.Rainy || weather == Weather.Thunder;
        bool isWinter = TimeManager.Instance.gameSeason == Season.冬天;
        string activeSceneName = SceneManager.GetActiveScene().name;
        bool isShowScene = false;
        for (int i = 0; i < showRainSceneName.Length; i++)
        {
            if (showRainSceneName[i] == activeSceneName)
            {
                isShowScene = true;
                break;
            }
        }
        bool isShow = isRainy && isShowScene;
        for (int i = 0; i < rainEffects.Length; i++)
        {
            if (rainEffects[i] != null)
            {
                rainEffects[i].SetActive(isShow && !isWinter);
            }
        }
        if (snowEffect != null)
        {
            snowEffect.SetActive(isShow && isWinter);
        }
    }
}
