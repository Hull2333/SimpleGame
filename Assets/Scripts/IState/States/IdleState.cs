using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//必须执行接口IState里一样的方法，否则报错
public class IdleState : IState
{
    private FSM manager;
    private Parameter parameter;
    //敌人到巡逻点的停留时间
    private float idleTimer;
    public IdleState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    //待机状态开始
    public void OnEnter()
    {
        //设置一开始进入待机状态的动画和面朝方向
        parameter.isMoving = false;
        parameter.animator.SetBool("isMoving", parameter.isMoving);
        parameter.animator.SetFloat("MoveX", 0);
        parameter.animator.SetFloat("MoveY", 0);
        //打开检测触碰圈
        parameter.chaseArae.enabled = true;
    }
    public void OnUpdate()
    {
        //敌人受伤
        if (parameter.isHurted)
        {
            manager.TransitionState(EnemyStateType.Hurted);
        }
        idleTimer += Time.deltaTime;
        //发现目标时转为追击状态
        if(parameter.target != null )
        {
            if (manager.ShoutNoticeRay() && Vector2.Distance(parameter.target.position, parameter.parentPos.position) < 10f)
            {
                manager.TransitionState(EnemyStateType.Chase);
            }
        }
        if(idleTimer >= parameter.idleTime)
        {
            manager.TransitionState(EnemyStateType.Patrol);
        }

    }
    public void OnFixUpdate()
    {
       
    }
    public void OnExit()
    {
        idleTimer = 0;
    }

 
}
