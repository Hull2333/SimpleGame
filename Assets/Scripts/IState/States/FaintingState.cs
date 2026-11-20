

using UnityEngine;

public class FaintingState : IState
{
    private FSM manager;
    private Parameter parameter;
    //获取正在播放的动画
    private AnimatorStateInfo info;

    public FaintingState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {
        
        parameter.animator.Play("Fainting");
        foreach (var box in parameter.attackBox)
        {
            box.enabled = false;
        }
        //飞行怪眩晕时停止飞行，眩晕结束后由追击状态重新加速
        if (parameter.enemyType != EnemyType.Ground)
        {
            parameter.rigidbody2D.velocity = Vector2.zero;
        }
    }

    public void OnUpdate()
    {
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (parameter.isHurted)
        {
            manager.TransitionState(EnemyStateType.Hurted);
        }
        //眩晕结束后进入追击状态
        if (info.normalizedTime >= 0.98f)
        {
            manager.TransitionState(EnemyStateType.Chase);
        }
    }
    public void OnExit()
    {
        parameter.isFainting = false;
    }

    public void OnFixUpdate()
    {
        
    }

}
