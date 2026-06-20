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
    [Header("对话选项类型")]
    public DialogueOptionType optionType;
    public InventoryBag_SO bag_SO;
    public BuildingBagData_SO buildingBag;
}
