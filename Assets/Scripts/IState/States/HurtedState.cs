using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtedState : IState
{
    private FSM manager;
    private Parameter parameter;

    public HurtedState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {
        manager.isBlink = true;
        manager.Blink();
        parameter.isFainting = false;
        if(parameter.target != null)
        {
            manager.Knockback(parameter.target.position);
            //敌人受伤特效
            EventHandler.CallParticleEffectEvent(ParticalEffectType.EnemyHit01, manager.transform.position , manager.transform.position - parameter.playerTransform.position);
        }
        
    }
    public void OnUpdate()
    {

        //血量归0时死亡，否则一被攻击就开始追击玩家
        if (parameter.health <= 0)
        {
            manager.TransitionState(EnemyStateType.Death);
        }
        else
        {
            parameter.target = parameter.playerTransform;
            manager.TransitionState(EnemyStateType.Chase);
        }
    }
    public void OnExit()
    {
        parameter.isHurted = false;
    }

    public void OnFixUpdate()
    {
        
    }
}
