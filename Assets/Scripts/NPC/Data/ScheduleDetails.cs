using System;
using UnityEngine;
[System.Serializable]
public class ScheduleDetails : IComparable<ScheduleDetails>
{
    public int hour, minute, day;
    //优先级越小优先执行
    public int priority;
    public Season season;
    public string targetScene;
    public Vector2Int targetGridPosition;
    //特殊事件执行的动作
    public AnimationClip clipAtStop;
    //是否可以互动
    public bool interactable;
    //构造函数
    public ScheduleDetails(int hour, int minute, int day, int priority, Season season, string targetScene, Vector2Int targetGridPosition, AnimationClip clipAtStop, bool interactable)
    {
        this.hour = hour;
        this.minute = minute;
        this.day = day;
        this.priority = priority;
        this.season = season;
        this.targetScene = targetScene;
        this.targetGridPosition = targetGridPosition;
        this.clipAtStop = clipAtStop;
        this.interactable = interactable;
    }
    public int Time => (hour * 100) + minute;
    /// <summary>
    /// 当前的ScheduleDetails与other的ScheduleDetails对比方法里的内容
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    public int CompareTo( ScheduleDetails other)
    {
        //如果双方时间相等
        if(Time == other.Time)
        {
            if(priority > other.priority)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
        else if(Time > other.Time)
        {
            return 1;
        }
        else if(Time < other.Time)
        {
            return -1;
        }
        return 0;
    }
}

