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
    /// <returns></returns>
    public LightDetails GetLightDetails(Season season,LightShift lightShift)
    {
        return lightPattenList.Find(l => l.season == season && l.lightShift == lightShift);
    }

}
[System.Serializable]
public class LightDetails
{
    public Season season;
    //灯光模式
    public LightShift lightShift;
    public Color lightColor;
    //灯光强度
    public float lightAmount;

}
