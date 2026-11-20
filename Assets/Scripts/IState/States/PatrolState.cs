using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : IState
{
    private FSM manager;
    private Parameter parameter;
    //位置变化小于该值视为没有移动
    private const float stuckMoveThreshold = 0.1f;
    //巡逻移动中持续没有位移多久就换下一个巡逻点
    private const float stuckTimeout = 2f;
    private float stuckTimer;
    private Vector2 lastPos;

    public PatrolState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnEnter()
    {
        parameter.chaseArae.enabled = true;
        //重置卡住检测
        stuckTimer = 0f;
        lastPos = manager.transform.position;
        //注册到达回调
        manager.onMoveComplete += OnArrivePatrolPoint;

        //陷阱型敌人：没有巡逻点，离落点远则先 A* 走回蛛网落点，否则直接驻守
        if (parameter.isTrapEnemy)
        {
            if (Vector2.Distance(manager.transform.position, parameter.parentPos.position) > 0.1f)
            {
                parameter.isMoving = true;
                manager.SetFaceDirection(parameter.parentPos);
                parameter.animator.SetBool("isMoving", true);
                parameter.animator.SetFloat("MoveX", parameter.moveX);
                parameter.animator.SetFloat("MoveY", parameter.moveY);
                manager.StartMoveTo(parameter.parentPos.position, parameter.moveSpeed);
            }
            else
            {
                HoldPosition();
            }
            return;
        }

        parameter.isMoving = true;
        //设置朝向下一个巡逻点
        manager.SetFaceDirection(parameter.patrolPoints[parameter.patrolPosition]);
        parameter.animator.SetBool("isMoving", parameter.isMoving);
        parameter.animator.SetFloat("MoveX", parameter.moveX);
        parameter.animator.SetFloat("MoveY", parameter.moveY);
        //启动 A* 移动到当前巡逻点
        Vector3 targetPos = parameter.patrolPoints[parameter.patrolPosition].position;
        manager.StartMoveTo(targetPos, parameter.moveSpeed);
    }

    public void OnUpdate()
    {
        //敌人受伤，进入受伤状态
        if (parameter.isHurted)
        {
            manager.TransitionState(EnemyStateType.Hurted);
            return;
        }
        //发现玩家则切换为追击状态：飞行近战怪无视视野与领地，地面怪要求视线无遮挡且在领地范围内
        if (parameter.target != null
            && (parameter.enemyType == EnemyType.FlyingMelee
                || (manager.ShoutNoticeRay()
                    && Vector2.Distance(parameter.target.position, parameter.parentPos.position) < 10f)))
        {
            manager.TransitionState(EnemyStateType.Chase);
            return;
        }
        //移动中长时间没有位移（可能被其他碰撞体挡住）
        if (manager.isMoving)
        {
            if (Vector2.Distance(manager.transform.position, lastPos) < stuckMoveThreshold)
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer >= stuckTimeout)
                {
                    if (parameter.isTrapEnemy)
                    {
                        //陷阱型：走回落点途中被卡住就就地驻守
                        manager.StopMove();
                        HoldPosition();
                    }
                    else
                    {
                        //普通敌人：换到下一个巡逻点
                        GoToNextPatrolPoint();
                    }
                }
            }
            else
            {
                stuckTimer = 0f;
                lastPos = manager.transform.position;
            }
        }
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
        //陷阱型敌人到达蛛网落点后原地驻守，不切待机也不推进巡逻序列
        if (parameter.isTrapEnemy)
        {
            HoldPosition();
            return;
        }
        AdvancePatrolPosition();
        manager.TransitionState(EnemyStateType.Idle);
    }
    /// <summary>
    /// 陷阱型敌人驻守：原地站立，等待玩家进入追击圈
    /// </summary>
    private void HoldPosition()
    {
        parameter.isMoving = false;
        parameter.animator.SetBool("isMoving", false);
    }
    /// <summary>
    /// 卡住时放弃当前巡逻点，直接前往下一个巡逻点
    /// </summary>
    private void GoToNextPatrolPoint()
    {
        AdvancePatrolPosition();
        stuckTimer = 0f;
        lastPos = manager.transform.position;
        Transform nextPoint = parameter.patrolPoints[parameter.patrolPosition];
        manager.SetFaceDirection(nextPoint);
        parameter.animator.SetFloat("MoveX", parameter.moveX);
        parameter.animator.SetFloat("MoveY", parameter.moveY);
        //StartMoveTo 内部会先停止当前卡住的移动
        manager.StartMoveTo(nextPoint.position, parameter.moveSpeed);
    }
    /// <summary>
    /// 沿巡逻点序列前后移动，到达两端后往回走
    /// </summary>
    private void AdvancePatrolPosition()
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
    }
}
