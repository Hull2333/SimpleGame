using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour //调用在LightManager对象上
{

    private LightController[] sceneLights;
    private LightShift currentLightShift;
    private Season currentSeason;
    private Weather currentWeather;
    private float timeDifference = Settings.lightChangeDuration;

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.LightShiftChangeEvent += OnLightShiftChangeEvent;
        //新游戏开始时需要重置的数据
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.LightShiftChangeEvent -= OnLightShiftChangeEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }
    /// <summary>
    /// 每次加载完地图之后获取场景中的所有灯光
    /// </summary>
    private void OnAfterSceneLoadedEvent()
    {
        //使用不排序的查找方式，避免已弃用API并减少场景加载时的排序开销
        sceneLights = FindObjectsByType<LightController>(FindObjectsSortMode.None);
        //场景加载时同步当前天气，避免读档后沿用旧天气
        currentWeather = TimeManager.Instance.currentWeather;

        RefreshAllLights();
    }

    private void OnLightShiftChangeEvent(Season season, LightShift lightShift, float timeDifference)
    {
        //天气只在跨天时变化，这里每分钟事件同步比较一次即可
        Weather weather = TimeManager.Instance.currentWeather;
        this.timeDifference = timeDifference;

        //季节、时段或天气任意一项变化都要立即刷新，否则换季/换天气时灯光会停在旧配置
        if(currentSeason != season || currentLightShift != lightShift || currentWeather != weather)
        {
            currentSeason = season;
            currentLightShift = lightShift;
            currentWeather = weather;
            RefreshAllLights();
        }
    }

    private void OnStartNewGameEvent(int obj)
    {
        //新游戏从春天早上开始，时长按完整值处理，让新场景的灯光直接到位
        currentSeason = Season.春天;
        currentLightShift = LightShift.Morning;
        timeDifference = Settings.lightChangeDuration;
    }

    /// <summary>
    /// 把当前季节、时段、天气应用到场景中的所有灯光
    /// </summary>
    private void RefreshAllLights()
    {
        if(sceneLights == null)
        {
            return;
        }

        foreach(LightController light in sceneLights)
        {
            //lightController 改变灯光的方法
            light.ChangeLightShift(currentSeason, currentLightShift, currentWeather, timeDifference);
        }
    }

}
