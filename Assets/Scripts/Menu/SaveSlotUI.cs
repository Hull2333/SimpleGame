using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MFarm.Save;
using TMPro;

public class SaveSlotUi : MonoBehaviour //调用在SaveSlot对象上
{
    public TextMeshProUGUI dateTime, dateScene;
    private Button currentButton;
    private DataSlot currentData;
    //获取当前点击按钮的序号，根据子物体来分
    private int Index => transform.GetSiblingIndex();
    private void Awake()
    {
        currentButton = GetComponent<Button>();
        //按钮被点击后执行LoadGameData方法
        currentButton.onClick.AddListener(LoadGameData);
    }
    private void OnEnable()
    {
        SetupSlotUI();
    }


    /// <summary>
    /// 显示存档UI的详情
    /// </summary>
    private void SetupSlotUI()
    {
        currentData = SaveLoadManager.Instance.dataSlots[Index];
        if(currentData != null)
        {
            dateTime.text = currentData.DataTime;
            dateScene.text = currentData.DataScene;
        }
        else
        {
            dateTime.text = "这个世界还没开始";
            dateScene.text = "梦还没开始";
        }
    }
    /// <summary>
    /// 点击存档开始游戏
    /// </summary>
    private void LoadGameData()
    {
        //不为空，表示当前有进度
        if (currentData != null)
        {
            SaveLoadManager.Instance.Load(Index);
        }
        else
        {
            EventHandler.CallStartNewGameEvent(Index);
        }
    }
}
