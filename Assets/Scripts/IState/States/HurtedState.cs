using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtedState : IState
{
    private FSM manager;
    private Parameter parameter;
    //受伤硬直计时，达到击退持续时间后再回到追击
    private float hurtTimer;

    public HurtedState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {
        hurtTimer = 0f;
        manager.isBlink = true;
        manager.Blink();
        parameter.isFainting = false;
        //受击特效不依赖是否有目标：被投射物等任何来源击中都要播放
        Vector2 hurtDirection = manager.transform.position - parameter.playerTransform.position;
        //原受伤特效：始终播放，保持原来的 LookRotation 朝向
        EventHandler.CallParticleEffectEvent(ParticalEffectType.EnemyHit01, manager.transform.position, Quaternion.LookRotation(hurtDirection));
        //额外配置的受击特效：None 或与原特效相同时不重复播放
        if (parameter.hurtEffectType != ParticalEffectType.None
            && parameter.hurtEffectType != ParticalEffectType.EnemyHit01)
        {
            //可配置的受击特效在 2D 面内旋转（预制体默认朝上），绕 Z 轴对准受击方向
            float hurtAngleZ = Mathf.Atan2(hurtDirection.y, hurtDirection.x) * Mathf.Rad2Deg - 90f;
            EventHandler.CallParticleEffectEvent(parameter.hurtEffectType, manager.transform.position, Quaternion.Euler(0f, 0f, hurtAngleZ));
        }
        //只有存在攻击目标时才击退
        if (parameter.target != null)
        {
            //飞行怪被击飞前先清零冲刺速度，保证被反方向打飞的方向只由武器击退力决定
            if (parameter.enemyType != EnemyType.Ground)
            {
                parameter.rigidbody2D.velocity = Vector2.zero;
            }
            manager.Knockback(parameter.target.position);
        }
    }
    public void OnUpdate()
    {
        //血量归0时死亡
        if (parameter.health <= 0)
        {
            manager.TransitionState(EnemyStateType.Death);
            return;
        }
        //受伤硬直期间保持 Hurt 状态，不移动，让击退有表现时间
        hurtTimer += Time.deltaTime;
        if (hurtTimer < parameter.knockbackDuration)
        {
            return;
        }
        //硬直结束后一被攻击就开始追击玩家
        parameter.target = parameter.playerTransform;
        manager.TransitionState(EnemyStateType.Chase);
    }
    public void OnExit()
    {
        parameter.isHurted = false;
    }

    public void OnFixUpdate()
    {
        
    }
}
