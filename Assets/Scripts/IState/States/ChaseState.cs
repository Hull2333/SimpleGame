using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : IState
{
    private FSM manager;
    private Parameter parameter;
    //进入追击后持续多久没有发生攻击就放弃追击
    private const float chaseTimeout = 8f;
    //飞行怪判定为“已停下”的速度阈值，低于该值重新朝玩家加速
    private const float flyCoastSpeedThreshold = 0.2f;
    private float chaseTimer;

    public ChaseState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnEnter()
    {
        chaseTimer = 0f;
        parameter.isMoving = true;
        parameter.animator.SetBool("isMoving", parameter.isMoving);
        manager.SetFaceDirection(parameter.target);
        parameter.animator.SetFloat("MoveX", parameter.moveX);
        parameter.animator.SetFloat("MoveY", parameter.moveY);
        //追击不使用 A*，进入时确保没有残留的寻路协程
        manager.StopMove();
        //飞行冲撞怪在追击期间常开攻击框（不播放攻击动画）
        if (parameter.enemyType == EnemyType.FlyingRam)
        {
            foreach (Collider2D attackBox in parameter.attackBox)
            {
                attackBox.enabled = true;
            }
        }
    }

    public void OnUpdate()
    {
        //受伤进入受伤状态
        if (parameter.isHurted)
        {
            manager.TransitionState(EnemyStateType.Hurted);
            return;
        }
        //攻击被格挡后进入眩晕（飞行怪追击时攻击框常开，需要在这里进入眩晕）
        if (parameter.isFainting)
        {
            manager.TransitionState(EnemyStateType.Fainting);
            return;
        }
        //目标丢失，回到待机
        if (parameter.target == null)
        {
            manager.TransitionState(EnemyStateType.Idle);
            return;
        }
        //飞行冲撞怪：一直直线冲撞追击，不进入攻击状态，也不会放弃追击
        if (parameter.enemyType == EnemyType.FlyingRam)
        {
            manager.SetFaceDirection(parameter.target);
            parameter.animator.SetFloat("MoveX", parameter.moveX);
            parameter.animator.SetFloat("MoveY", parameter.moveY);
            return;
        }
        //玩家进入攻击范围，开始攻击
        if (Vector2.Distance(manager.transform.position, parameter.target.position) <= parameter.attackArea)
        {
            manager.TransitionState(EnemyStateType.Attack);
            return;
        }
        //只有地面怪会因视线被遮挡、玩家超出追击领地或长时间没攻击到玩家而放弃追击；
        //飞行近战怪无视地形，一直追击
        if (parameter.enemyType == EnemyType.Ground)
        {
            if (!manager.ShoutNoticeRay()
                || Vector2.Distance(parameter.target.position, parameter.parentPos.position) >= 10f)
            {
                parameter.target = null;
                manager.TransitionState(EnemyStateType.Idle);
                return;
            }
            chaseTimer += Time.deltaTime;
            if (chaseTimer >= chaseTimeout)
            {
                parameter.target = null;
                manager.TransitionState(EnemyStateType.Idle);
                return;
            }
        }
        //直线朝玩家移动（射线追击，不使用 A*）
        manager.SetFaceDirection(parameter.target);
        parameter.animator.SetFloat("MoveX", parameter.moveX);
        parameter.animator.SetFloat("MoveY", parameter.moveY);
        Vector2 next = Vector2.MoveTowards(manager.transform.position, parameter.target.position, parameter.chaseSpeed * Time.deltaTime);
        parameter.rigidbody2D.MovePosition(next);
    }

    public void OnFixUpdate()
    {
        if (parameter.enemyType != EnemyType.FlyingRam || parameter.target == null)
        {
            return;
        }
        Rigidbody2D rb = parameter.rigidbody2D;
        Vector2 toPlayer = ((Vector2)parameter.target.position - rb.position).normalized;
        //冲过玩家后速度方向与目标方向相反，此时不再加速，靠刚体阻力自然减速
        if (rb.velocity.sqrMagnitude > flyCoastSpeedThreshold * flyCoastSpeedThreshold
            && Vector2.Dot(rb.velocity, toPlayer) < 0f)
        {
            return;
        }
        //朝玩家加速，最高不超过 chaseSpeed
        rb.velocity = Vector2.ClampMagnitude(
            rb.velocity + toPlayer * parameter.flyAcceleration * Time.fixedDeltaTime,
            parameter.chaseSpeed);
    }

    public void OnExit()
    {
        parameter.isMoving = false;
        parameter.animator.SetBool("isMoving", parameter.isMoving);
        //飞行冲撞怪离开追击（受伤、死亡等）时关闭常开的攻击框
        if (parameter.enemyType == EnemyType.FlyingRam)
        {
            foreach (Collider2D attackBox in parameter.attackBox)
            {
                attackBox.enabled = false;
            }
        }
    }
}
