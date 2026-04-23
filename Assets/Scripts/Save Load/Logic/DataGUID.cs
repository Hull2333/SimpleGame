using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//一直执行
[ExecuteAlways]
public class DataGUID : MonoBehaviour   //调用在需要存档的物体上，比如Player
{
    /// <summary>
    /// 16位字符串，用来和存档信息做匹配，保证存档信息的唯一性
    /// </summary>
    public string guid;

    private void Awake()
    {
        if(guid == string.Empty)
        {
            //把guid生成为一个16位的字符串
            guid = System.Guid.NewGuid().ToString();
        }
    }
}
