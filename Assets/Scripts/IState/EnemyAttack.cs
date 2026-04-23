using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
public class EnemyAttack : MonoBehaviour//调用在敌人下的AttackBox对象上
{
    private PlayerController player;
    private void Start()
    {
        player = GameObject.FindAnyObjectByType<PlayerController>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
       
        if (other.CompareTag("DefenceBox"))
        {
            //获取相对自己对于other的最近的点
            Vector2 closestPointOnSelf = GetComponent<Collider2D>().ClosestPoint(other.transform.position);
            //获取相对与other离自己最近的点
            Vector2 closestPointOnOther = other.ClosestPoint(transform.position);
            //近似碰撞点
            Vector2 collisionPoint = (closestPointOnSelf + closestPointOnOther) / 2f;
            transform.GetComponentInParent<FSM>().parameter.isFainting = true;
            player.PlayDefenceGetAnim(transform.parent);
            player.PlayDefenceEffect(collisionPoint);
        }
        if (other.CompareTag("Player"))
        {
            transform.GetComponentInParent<FSM>().AttackPlayer(other);
        }
    }
}
