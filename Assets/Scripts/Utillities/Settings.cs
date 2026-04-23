using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings //不需要调用在任何物体上
{
    //设置物体颜色变化幅度,越小变化幅度越大
    public const float itemFadeDuration = 0.35f;
    //物体透明度
    public const float targetAlpha = 0.45f;
    //游戏1s的时间，值越小越快
    public const float secondThreshold = 0.1f;
    //时间相关
    public const int secondHold = 59;
    public const int minuteHold = 59;
    public const int hourHold = 23;
    public const int dayHold = 30;
    public const int seasonHold = 3;
    //加载界面浮现的时间
    public const float fadeDuration = 1.5f;
    //NPC网格移动
    public const float gridCellSize = 1;
    public const float gridCellDiagonalSize = 1.41f;
    //20*20 20/1
    public const float pixelSize = 0.05f;
    //动画执行等待时间
    public const float animationBreakTime = 5f;
    //最大网格尺寸
    public const int maxGridSize = 9999;

    //灯光
    //灯光变化的时间
    public const float lightChangeDuration = 25f;
    //早上和晚上的时间戳
    public static TimeSpan morningTime = new TimeSpan(5, 0, 0);
    public static TimeSpan nightTime = new TimeSpan(19, 0, 0);
    //主角初始坐标
    public static Vector3 playerStartPos = new Vector3(3.5f,-7f,0);
    //初始金额
    public const int playerStartMoney = 100;
    //捉鱼进度
    public const float fishCatchProgress = 3f;
    //鱼钩抛物线中点比例
    public const float fishParabolaPercent = 0.4f;
    //鱼钩抛物线偏移量
    public const float fishaParabolaOffsetY = -4f;
    //鱼线的中点比例
    public const float fishLinePercent = 0.2f;
    //鱼线的偏移量
    public const float fishLineOffsetY = 0.5f;
    //鼠标长按倍率
    public const float downPower = 0.8f;
    //鼠标蓄力时间
    public const float mouseDownHoldTime = 5f;
    
    
}
