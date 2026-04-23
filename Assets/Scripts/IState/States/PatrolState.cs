using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : IState
{
    private FSM manager;
    private Parameter parameter;
    //怪物巡逻点
    private int patrolPosition;
    public PatrolState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {
        manager.SetFaceDirection(parameter.patrolPoints[patrolPosition]);
        parameter.isMoving = true;
        parameter.chaseArae.enabled = true;
        parameter.animator.SetBool("isMoving", parameter.isMoving);
        parameter.animator.SetFloat("MoveX", parameter.moveX);
        parameter.animator.SetFloat("MoveY", parameter.moveY);
    }
    public void OnUpdate()
    {
        //敌人受伤
        if (parameter.isHurted)
        {
            manager.TransitionState(EnemyStateType.Hurted);
        }
        //发现目标时转为追击状态
        if (parameter.target != null)
        {
            if (manager.ShoutNoticeRay() && Vector2.Distance(parameter.target.position, parameter.parentPos.position) < 10f)
            {
                manager.TransitionState(EnemyStateType.Chase);
            }
        }
        //敌人以moveSpeed面朝巡逻点移动
        manager.transform.position = Vector2.MoveTowards(manager.transform.position, parameter.patrolPoints[patrolPosition].position, parameter.moveSpeed * Time.deltaTime);
        manager.SetFaceDirection(parameter.patrolPoints[patrolPosition]);
        //到达巡逻点时进入到Idle状态
        if (Vector2.Distance(manager.transform.position, parameter.patrolPoints[patrolPosition].position) < 0.1f)
        {
            manager.TransitionState(EnemyStateType.Idle);
        }

    }
    public void OnExit()
    {
        //退出巡逻patrol状态时，更新下一个巡逻点
        //实现敌人原路往返
        if(patrolPosition >= parameter.patrolPoints.Length - 1)
        {
            parameter.isBack = true;
        }
        if(patrolPosition <= 0)
        {
            parameter.isBack = false;
        }
        if (parameter.isBack)
        {
            patrolPosition--;
        }
        else
        {
            patrolPosition++;
        }
    }

    public void OnFixUpdate()
    {
       
    }
}
