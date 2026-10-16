using System;
using UnityEngine;
[System.Serializable]
public class ScheduleDetails : IComparable<ScheduleDetails>
{
    public int hour, minute, day;
    //优先级越小优先执行
    public int priority;
    //限定生效的季节，可以填多个，不填(长度为0)表示所有季节都生效
    public Season[] seasons;
    //限定生效的星期，可以填多个，不填(长度为0)表示每天都生效
    public Weekday[] weekdays;
    //限定生效的天气，可以填多个，不填(长度为0)表示所有天气都生效
    public Weather[] weathers;
    //需要达到的最低好感度，填0表示不限制
    public float minFriendliness;
    public string targetScene;
    public Vector2Int targetGridPosition;
    //特殊事件执行的动作
    public AnimationClip clipAtStop;
    //到达目的点后是否一直播放ClipAtStop动画，false表示每隔一段时间播放一次
    public bool alwaysPlayAnimation;
    //到达目的点后NPC的朝向，对应Animator的DirX、DirY
    public int faceDirX, faceDirY;
    //是否可以互动
    public bool interactable;
    //构造函数
    public ScheduleDetails(int hour, int minute, int day, int priority, Season[] seasons, string targetScene, Vector2Int targetGridPosition, AnimationClip clipAtStop, bool interactable)
    {
        this.hour = hour;
        this.minute = minute;
        this.day = day;
        this.priority = priority;
        this.seasons = seasons;
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

