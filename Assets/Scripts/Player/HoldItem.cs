using MFarm.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldItem : MonoBehaviour //调用在Player的HoldItem对象上
{
    /// <summary>
    /// 在HoldItem的收获动画帧中运行
    /// </summary>
   public void StartEvent()
    {
        //取消玩家举起动画
        EventHandler.CallItemSelectEvent(InventoryManager.Instance.currentSelectedItem, false);
    }
}
