
using Unity.VisualScripting;
using UnityEngine;

public class ChaseState : IState
{
    private FSM manager;
    private Parameter parameter;
    public ChaseState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {
       
    }
    public void OnUpdate()
    {
        //敌人受伤
        if (parameter.isHurted)
        {
            manager.TransitionState(EnemyStateType.Hurted);
        }
        manager.SetFaceDirection(parameter.target);
       
        if (parameter.target != null)
        {
            if (Vector2.Distance(parameter.playerTransform.position, parameter.parentPos.position) < 10f)
            {
                //视野内有Player，开始以追击速度冲向Player
                if (manager.ShoutNoticeRay())
                {
                    manager.transform.position = Vector2.MoveTowards(manager.transform.position, parameter.target.position, parameter.chaseSpeed * Time.deltaTime);
                    parameter.animator.SetBool("isMoving", parameter.isMoving);
                    parameter.animator.SetFloat("MoveX", parameter.moveX);
                    parameter.animator.SetFloat("MoveY", parameter.moveY);
                    //玩家进入敌人攻击范围时，敌人开始攻击
                    if (Vector2.Distance(manager.transform.position, parameter.target.position) <= parameter.attackArea)
                    {
                        manager.TransitionState(EnemyStateType.Attack);
                    }
                }
                else
                {
                    parameter.target = null;
                    manager.TransitionState(EnemyStateType.Idle);
                }
            }
            //超出追击范围
            else
            {
                parameter.target = null;
                manager.TransitionState(EnemyStateType.Idle);
            }
            
        }       
         
     
    }

    public void OnExit()
    {

    }

    public void OnFixUpdate()
    {

    }
}
