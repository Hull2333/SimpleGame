using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : IState
{
    private FSM manager;
    private Parameter parameter;
    public PatrolState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnEnter()
    {
        parameter.isMoving = true;
        parameter.chaseArae.enabled = true;
        //设置朝向下一个巡逻点
        manager.SetFaceDirection(parameter.patrolPoints[parameter.patrolPosition]);
        parameter.animator.SetBool("isMoving", parameter.isMoving);
        parameter.animator.SetFloat("MoveX", parameter.moveX);
        parameter.animator.SetFloat("MoveY", parameter.moveY);
        //注册到达巡逻点回调，启动 A* 移动到当前巡逻点
        manager.onMoveComplete += OnArrivePatrolPoint;
        Vector3 targetPos = parameter.patrolPoints[parameter.patrolPosition].position;
        manager.StartMoveTo(targetPos, parameter.moveSpeed);
    }

    public void OnUpdate()
    {
        //敌人受伤，进入受伤状态
        if (parameter.isHurted)
        {
            manager.TransitionState(EnemyStateType.Hurted);
        }
        //发现玩家且视线无遮挡则切换为追击状态
        else if (parameter.target != null
            && manager.ShoutNoticeRay()
            && Vector2.Distance(parameter.target.position, parameter.parentPos.position) < 10f)
        {
            manager.TransitionState(EnemyStateType.Chase);
        }
        //移动交给 A* 协程，到达后由 OnArrivePatrolPoint 处理
    }

    public void OnFixUpdate()
    {
    }

    public void OnExit()
    {
        parameter.isMoving = false;
        parameter.animator.SetBool("isMoving", parameter.isMoving);
        manager.onMoveComplete -= OnArrivePatrolPoint;
        //切换状态时停止当前路径移动
        manager.StopMove();
    }
    /// <summary>
    /// 到达巡逻点后推进巡逻序列，切回待机状态
    /// </summary>
    private void OnArrivePatrolPoint()
    {
        //从最后一个点往回走
        if (parameter.patrolPosition >= parameter.patrolPoints.Length - 1)
        {
            parameter.isBack = true;
        }
        if (parameter.patrolPosition <= 0)
        {
            parameter.isBack = false;
        }
        if (parameter.isBack)
        {
            parameter.patrolPosition--;
        }
        else
        {
            parameter.patrolPosition++;
        }
        manager.TransitionState(EnemyStateType.Idle);
    }
}
