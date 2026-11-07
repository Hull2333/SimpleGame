using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using Unity.VisualScripting;

public class LightController : MonoBehaviour    //调用在所有光对象上
{
    public LightPattenList_SO lightData;
    private Light2D currentLight;
    private LightDetails currentLightDetails;
    //颜色和强度的过渡Tween，切换前需要先终止，避免多个Tween同时修改同一盏灯
    private DG.Tweening.Tween colorTween;
    private DG.Tweening.Tween strengthTween;

    private void Awake()
    {
        currentLight = GetComponent<Light2D>();
    }

    private void OnDestroy()
    {
        //对象销毁时终止Tween，避免回调继续访问已销毁的Light2D
        KillLightTweens();
    }
    /// <summary>
    /// 在lightChangeDuration时间内切换灯光颜色和强度
    /// </summary>
    /// <param name="season"></param>
    /// <param name="lightShift"></param>
    /// <param name="weather">当前天气，用于匹配天气专属灯光</param>
    /// <param name="timeDifference"></param>
    public void ChangeLightShift(Season season ,LightShift lightShift,Weather weather,float timeDifference)
    {
        currentLightDetails = lightData.GetLightDetails(season, lightShift, weather);
        //缺少对应季节/天气的配置时跳过这盏灯，避免空引用打断本次刷新
        if(currentLightDetails == null)
        {
            Debug.LogWarning($"{name} 缺少灯光配置：{season} {lightShift} {weather}", this);
            return;
        }
        //新建过渡前先停掉上一轮Tween，否则多个Tween会同时写入颜色和强度
        KillLightTweens();
        //还有时间切换颜色
        if(timeDifference < Settings.lightChangeDuration)
        {
            //当前颜色与目标颜色的差值
            var colorOffset = (currentLightDetails.lightColor - currentLight.color) / Settings.lightChangeDuration * timeDifference;
            currentLight.color += colorOffset;
            //颜色开始逐渐切换
            colorTween = DOTween.To(() => currentLight.color, c => currentLight.color = c, currentLightDetails.lightColor, Settings.lightChangeDuration - timeDifference);
            strengthTween = DOTween.To(() => currentLight.intensity, i => currentLight.intensity = i, currentLightDetails.lightStrength, Settings.lightChangeDuration - timeDifference);
        }
        //切换时间已到,直接切换到目标颜色
        if(timeDifference >= Settings.lightChangeDuration)
        {
            currentLight.color = currentLightDetails.lightColor;
            currentLight.intensity = currentLightDetails.lightStrength;
        }
    }

    /// <summary>
    /// 终止当前的颜色和强度过渡
    /// </summary>
    private void KillLightTweens()
    {
        colorTween?.Kill();
        strengthTween?.Kill();
        colorTween = null;
        strengthTween = null;
    }
}
