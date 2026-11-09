using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : IState
{
    private FSM manager;
    private Parameter parameter;
    //是否处于"追击失败后返回巡逻点"的模式
    private bool returningToPatrol;
    //是否处于近距离直线追击模式（此时不走 A*）
    private bool isDirectChasing;
    //追击寻路失败后，回巡逻途中允许重新追击的最短间隔（防止失败寻路每帧重启导致原地不动）
    private const float chaseRetryInterval = 1.5f;
    private float nextChaseRetryTime;

    public ChaseState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnEnter()
    {
        parameter.isMoving = true;
        returningToPatrol = false;
        isDirectChasing = false;
        nextChaseRetryTime = 0f;
        manager.SetFaceDirection(parameter.target);
        parameter.animator.SetBool("isMoving", parameter.isMoving);
        parameter.animator.SetFloat("MoveX", parameter.moveX);
        parameter.animator.SetFloat("MoveY", parameter.moveY);
        manager.onMoveComplete += OnMoveComplete;
        //以触发追击时玩家的位置为目标点做 A* 移动（Y+0.5：玩家pivot在脚下，取身体所在格作为目标格）
        manager.StartMoveTo(GetChaseTargetPos(), parameter.chaseSpeed);
    }

    public void OnUpdate()
    {
        //受伤进入受伤状态
        if (parameter.isHurted)
        {
            manager.TransitionState(EnemyStateType.Hurted);
            return;
        }
        //放弃追击后 target 已清空，回巡逻途中不再做追击相关判断
        if (parameter.target == null)
        {
            return;
        }
        float distToPlayer = Vector2.Distance(manager.transform.position, parameter.target.position);
        //进入攻击范围，停止寻路直接攻击
        if (distToPlayer <= parameter.attackArea)
        {
            manager.TransitionState(EnemyStateType.Attack);
            return;
        }
        //近距离且视线无遮挡：直线追击，不再依赖 A*（避免玩家站在障碍标记格上时永远无法贴身）
        if (distToPlayer <= parameter.directChaseRange && manager.ShoutNoticeRay())
        {
            if (!isDirectChasing)
            {
                isDirectChasing = true;
                manager.StopMove();
            }
            DirectChaseMove();
            return;
        }
        //距离重新拉远后恢复 A* 追击
        if (isDirectChasing)
        {
            isDirectChasing = false;
            manager.StartMoveTo(GetChaseTargetPos(), parameter.chaseSpeed);
            return;
        }
        //返回巡逻点途中，如果玩家重新满足追击条件，则继续追击（冷却内不重试，避免与失败回调互相打断）
        if (returningToPatrol && Time.time >= nextChaseRetryTime && CanChasePlayer())
        {
            returningToPatrol = false;
            manager.SetFaceDirection(parameter.target);
            manager.StartMoveTo(GetChaseTargetPos(), parameter.chaseSpeed);
        }
    }

    public void OnFixUpdate()
    {
    }

    public void OnExit()
    {
        parameter.isMoving = false;
        parameter.animator.SetBool("isMoving", parameter.isMoving);
        manager.onMoveComplete -= OnMoveComplete;
        manager.StopMove();
    }
    /// <summary>
    /// 玩家是否满足继续追击的条件：视线无遮挡 且 距离怪物出生点小于追击范围
    /// </summary>
    private bool CanChasePlayer()
    {
        return parameter.target != null
            && manager.ShoutNoticeRay()
            && Vector2.Distance(parameter.target.position, parameter.parentPos.position) < 10f;
    }
    /// <summary>
    /// 追击目标点：玩家位置 X 不变，Y 加 0.5（玩家 pivot 在脚下，取身体所在格作为 A* 目标格）
    /// </summary>
    private Vector3 GetChaseTargetPos()
    {
        return parameter.target.position + new Vector3(0f, 0.5f, 0f);
    }
    /// <summary>
    /// 近距离直线朝玩家移动，与 A* 移动一样走 Rigidbody2D，保持物理碰撞一致
    /// </summary>
    private void DirectChaseMove()
    {
        manager.SetFaceDirection(parameter.target);
        parameter.animator.SetFloat("MoveX", parameter.moveX);
        parameter.animator.SetFloat("MoveY", parameter.moveY);
        Vector2 next = Vector2.MoveTowards(manager.transform.position, parameter.target.position, parameter.chaseSpeed * Time.deltaTime);
        parameter.rigidbody2D.MovePosition(next);
    }
    /// <summary>
    /// A* 路径走完后的处理：
    /// 追击模式下判断是否继续追玩家，否则返回巡逻点；
    /// 返回模式下到达巡逻点后切回待机。
    /// </summary>
    private void OnMoveComplete()
    {
        //直线追击期间忽略 A* 完成/失败回调
        if (isDirectChasing)
        {
            return;
        }
        if (!returningToPatrol)
        {
            //路径失败说明玩家位置经 A* 检测不可达
            if (!manager.lastPathFound)
            {
                //玩家在直线追击范围内且之间没有障碍物：直接直线追击到玩家身边攻击
                if (parameter.target != null
                    && Vector2.Distance(manager.transform.position, parameter.target.position) <= parameter.directChaseRange
                    && manager.ShoutNoticeRay())
                {
                    isDirectChasing = true;
                    return;
                }
                //否则放弃追击，清空目标并回到待机，按 Idle 现有逻辑执行
                parameter.target = null;
                manager.TransitionState(EnemyStateType.Idle);
                return;
            }
            //到达追击目标点后，玩家仍可追击则以当前玩家位置继续 A*
            if (CanChasePlayer())
            {
                manager.SetFaceDirection(parameter.target);
                manager.StartMoveTo(GetChaseTargetPos(), parameter.chaseSpeed);
            }
            //否则 A* 返回巡逻序列的下一个巡逻点
            else
            {
                returningToPatrol = true;
                //放弃追击，清空目标，之后需要玩家重新进入触发圈才会再次被追击
                parameter.target = null;
                Vector3 patrolTarget = parameter.patrolPoints[parameter.patrolPosition].position;
                manager.StartMoveTo(patrolTarget, parameter.moveSpeed);
            }
        }
        else
        {
            //已回到巡逻点，切回待机（待机后会重新进入巡逻）
            manager.TransitionState(EnemyStateType.Idle);
        }
    }
}
