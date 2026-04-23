using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AttackState : IState
{
    private FSM manager;
    private Parameter parameter;

    private AnimatorStateInfo info;
    public AttackState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {

    }

    public void OnUpdate()
    {
        if (parameter.isFainting)
        {
            manager.TransitionState(EnemyStateType.Fainting);
        }
        //敌人受伤
        if (parameter.isHurted)
        {
            manager.TransitionState(EnemyStateType.Hurted);
        }
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        //攻击动画播放完再次进入追击状态还是继续攻击
        //if (info.normalizedTime >= 0.95f)
        //{
        //    if (Vector2.Distance(manager.transform.position, parameter.target.position) <= parameter.attackArea)
        //    {
        //        OnEnter();
        //    }
        //    if (Vector2.Distance(manager.transform.position, parameter.target.position) > parameter.attackArea)
        //    {
        //        manager.TransitionState(EnemyStateType.Chase);
        //    }
        //}
        //在攻击范围内
        if(Vector2.Distance(manager.transform.position, parameter.target.position) <= parameter.attackArea)
        {
            //攻击不在CD中
            if(parameter.inAttackCD == false)
            {
                manager.SetFaceDirection(parameter.target);
                parameter.animator.SetFloat("FaceX", parameter.faceX);
                parameter.animator.SetFloat("FaceY", parameter.faceY);
                parameter.animator.Play("Attack");
                parameter.inAttackCD = true;
            }
           
        }
        //在攻击范围外
        else if(info.normalizedTime >= 0.95f)
        {
            manager.TransitionState(EnemyStateType.Chase);
        }
    }

    public void OnExit()
    {

    }

    public void OnFixUpdate()
    {
        
    }
}
