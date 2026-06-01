using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class DialogueOption 
{
    [Header("选项文本")]
    public string text;
    [Header("下一段对话ID")]
    public string targetID;
    [Header("好感度")]
    public int friendlinessValue;
    [Header("触发任务事件")]
    public bool takeQuest;
    [Header("触发背包事件")]
    public bool takeBag;
    public InventoryBag_SO bag_SO;
}
