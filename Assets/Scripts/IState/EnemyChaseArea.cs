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

    //用 Stay 而不是 Enter：放弃追击会清空 target 并重新打开触发圈，玩家仍在圈内站着不动时也能被重新发现
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            manager.FindPlayer(other.transform.parent);
        }
        
    }
}
