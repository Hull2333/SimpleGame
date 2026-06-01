using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SellHaveItemTip : MonoBehaviour //调用在SellTipImage预制体的Image对象上
{
    /// <summary>
    /// 调用在动画最后一帧，"还有东西没卖掉"动画
    /// </summary>
   public void SellHaveItemTipAnim()
    {
        Destroy(transform.parent.gameObject);
    }
}
