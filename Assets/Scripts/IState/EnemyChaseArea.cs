using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChaseArea : MonoBehaviour //调用在Enemy子物体上的ChaseArea对象上
{
    private FSM manager;
    private void Start()
    {
        manager = GetComponentInParent<FSM>();
    }

    //玩家每次进入触发圈时检测一次即可
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            manager.FindPlayer(other.transform.parent);
        }
        
    }
}
