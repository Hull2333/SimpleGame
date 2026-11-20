using UnityEngine;

public class DeathState : IState
{
    private FSM manager;
    private Parameter parameter;

    public DeathState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {
        EventHandler.CallNextTeleportAppearEvent();
        parameter.animator.Play("Death");
        parameter.collider2D.enabled = false;
        //飞行怪死亡后停止移动，避免残留速度继续飞行
        if (parameter.enemyType != EnemyType.Ground)
        {
            parameter.rigidbody2D.velocity = Vector2.zero;
        }
    }

    public void OnUpdate()
    {

    }
    public void OnExit()
    {

    }

    public void OnFixUpdate()
    {
       
    }
}
