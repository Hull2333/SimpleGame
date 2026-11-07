using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;
[CreateAssetMenu(fileName ="LightPattenList_SO",menuName = "Light/Light Patten")]
public class LightPattenList_SO : ScriptableObject
{
    public List<LightDetails> lightPattenList;
    /// <summary>
    /// 根据季节和时间获取灯光细节
    /// </summary>
    /// <param name="season">季节</param>
    /// <param name="lightShift">时间</param>
    /// <param name="weather">天气</param>
    /// <returns></returns>
    public LightDetails GetLightDetails(Season season,LightShift lightShift,Weather weather)
    {
        //雷雨没有单独配置时统一按雨天灯光执行
        Weather targetWeather = weather == Weather.Thunder ? Weather.Rainy : weather;
        //先精确匹配季节、时段和天气
        LightDetails details = lightPattenList.Find(l => l.season == season && l.lightShift == lightShift && l.weather == targetWeather);
        if(details != null)
        {
            return details;
        }
        //缺少当前季节的配置时，回退到春天同一时段、同一天气；天气缺配置则返回 null 由调用方跳过
        return lightPattenList.Find(l => l.season == Season.春天 && l.lightShift == lightShift && l.weather == targetWeather);
    }

}
[System.Serializable]
public class LightDetails
{
    public Season season;
    public Weather weather;
    //灯光模式
    public LightShift lightShift;
    public Color lightColor;
    //灯光强度
    public float lightStrength;

}
