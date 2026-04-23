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

    private void Awake()
    {
        currentLight = GetComponent<Light2D>();
    }
    /// <summary>
    /// 在lightChangeDuration时间内切换灯光颜色和强度
    /// </summary>
    /// <param name="season"></param>
    /// <param name="lightShift"></param>
    /// <param name="timeDifference"></param>
    public void ChangeLightShift(Season season ,LightShift lightShift,float timeDifference)
    {
        currentLightDetails = lightData.GetLightDetails(season, lightShift);
        //还有时间切换颜色
        if(timeDifference < Settings.lightChangeDuration)
        {
            //当前颜色与目标颜色的差值
            var colorOffset = (currentLightDetails.lightColor - currentLight.color) / Settings.lightChangeDuration * timeDifference;
            currentLight.color += colorOffset;
            //颜色开始逐渐切换
            DOTween.To(() => currentLight.color, c => currentLight.color = c, currentLightDetails.lightColor, Settings.lightChangeDuration - timeDifference);
            DOTween.To(() => currentLight.intensity, i => currentLight.intensity = i, currentLightDetails.lightAmount, Settings.lightChangeDuration - timeDifference);
        }
        //切换时间已到,直接切换到目标颜色
        if(timeDifference >= Settings.lightChangeDuration)
        {
            currentLight.color = currentLightDetails.lightColor;
            currentLight.intensity = currentLightDetails.lightAmount;
        }
    }
}
