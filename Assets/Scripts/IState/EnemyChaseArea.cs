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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            manager.FindPlayer(other.transform.parent);
        }
        
    }
}
