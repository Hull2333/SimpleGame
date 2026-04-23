using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour //调用在LightManager对象上
{

    private LightController[] sceneLights;
    private LightShift currentLightShift;
    private Season currentSeason;
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
        sceneLights = FindObjectsOfType<LightController>();

        foreach(LightController light in sceneLights)
        {
            //lightController 改变灯光的方法
            light.ChangeLightShift(currentSeason, currentLightShift, timeDifference);
        }
    }

    private void OnLightShiftChangeEvent(Season season, LightShift lightShift, float timeDifference)
    {
        currentSeason = season;
        this.timeDifference = timeDifference;
        if(currentLightShift != lightShift)
        {
            currentLightShift = lightShift;
            foreach (LightController light in sceneLights)
            {
                //lightController 改变灯光的方法
                light.ChangeLightShift(currentSeason, currentLightShift, timeDifference);
            }
        }
    }

    private void OnStartNewGameEvent(int obj)
    {
        currentLightShift = LightShift.Morning;
    }

}
