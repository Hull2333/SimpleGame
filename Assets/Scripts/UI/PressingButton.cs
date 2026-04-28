using MFarm.Inventory;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Lifetime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PressingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,IPointerClickHandler,IPointerExitHandler//调用在所有需要长按功能的按钮上
{
    //点击和长按事件 
    public UnityEvent OnPress;
    public UnityEvent OnClick;
    public UnityEvent OnDoubleClick;
    //是否按下
    private bool isDown;
    //是否长按
    private bool isPressing = false;
    //按下的时间
    private float downTime;
    //计时器，多少时间执行一次press
    private float timer;
    //按下按钮多久才算进入长按
    private float pressDurationTime = 0.5f;
    //间隔多少时间内算双击
    private float doubleClockIntervalTime = 0.2f;
    //按下开始计时的间隔
    private float clickIntervalTime;
    //点击的次数
    private int clickTimes;
    public void Update()
    {
        if (isDown)
        {
            downTime += Time.deltaTime;
            if(downTime > pressDurationTime)
            {
                isPressing = true;
                if(timer <= 0)
                {
                    OnPress?.Invoke();
                    timer = 0.05f;
                }
                else
                {
                    timer -= Time.deltaTime;
                }
            }
        }
        if(clickTimes >= 1)
        {
            clickIntervalTime += Time.deltaTime;
            if(clickIntervalTime >= doubleClockIntervalTime)
            {
                if(clickTimes >= 2)
                {
                    OnDoubleClick?.Invoke();
                }
                else
                {
                    OnClick?.Invoke();
                }
                clickTimes = 0;
                clickIntervalTime = 0;
            }
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        isDown = true;
        downTime = 0;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDown = false;
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isPressing)
        {
            clickTimes++;
        }
        else
        {
            isPressing = false;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isDown = false;
        isPressing = false;
    }
}
