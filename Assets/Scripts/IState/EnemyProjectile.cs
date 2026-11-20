using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人远程攻击的投射物，挂在投射物预制体上
/// 沿自身朝向（+X 轴）直线飞行，击中玩家造成伤害并播放击中特效
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("投射物属性")]
    //飞行速度
    public float speed = 5f;
    //击中玩家时播放的粒子特效预制体
    public GameObject hitEffectPrefab;

    //伤害值，由 FSM 发射时传入
    [HideInInspector] public int damage;
    //生成后存活时间，超时未击中也会销毁
    private const float lifeTime = 5f;
    //剩余存活时间，被玩家反弹后重新计时
    private float lifeTimer;
    //是否已被玩家攻击反弹，进入返回状态
    private bool isReturning;
    //返回途中是否已经命中怪物，避免同一帧多个碰撞体重复结算
    private bool hasHit;
    private Rigidbody2D rigidbody2D;

    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        //预制体默认朝上，因此沿自身 +Y 方向直线飞行，发射时已把朝向对准玩家
        rigidbody2D.velocity = transform.up * speed;
        //5 秒后无论是否击中都销毁
        lifeTimer = lifeTime;
    }
    private void Update()
    {
        //存活倒计时，被反弹返回时会在 Reverse 中重新计时
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        //返回途中：碰到任何怪物都造成投射物伤害并销毁
        if (isReturning)
        {
            if (hasHit)
            {
                return;
            }
            FSM enemy = other.GetComponentInParent<FSM>();
            if (enemy != null)
            {
                hasHit = true;
                //伤害沿用发射时敌人设置的攻击力
                enemy.EnemyHurted(damage, 5f);
                //在命中位置播放击中特效，朝向被击中的怪物
                PlayHitEffect(enemy.transform.position);
                Destroy(gameObject);
            }
            return;
        }
        if (other.CompareTag("Player"))
        {
            //伤害值取自发射该投射物的敌人的攻击力
            other.GetComponentInParent<PlayerController>().PlayerHurted(damage, 5, transform);
            //在当前位置播放击中特效，整体朝向玩家（含所有子物体）
            PlayHitEffect(other.transform.position);
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// 被玩家攻击反弹：从当前位置沿原飞行路线返回
    /// </summary>
    public void Reverse()
    {
        //已经在返回状态的投射物不重复反弹
        if (isReturning)
        {
            return;
        }
        isReturning = true;
        //掉头，沿原路线往回飞
        transform.Rotate(0f, 0f, 180f);
        rigidbody2D.velocity = transform.up * speed;
        //返回后销毁倒计时重新开始计算
        lifeTimer = lifeTime;
    }
    /// <summary>
    /// 生成击中特效，并让粒子及其子物体的朝向指向玩家
    /// </summary>
    private void PlayHitEffect(Vector3 playerPos)
    {
        if (hitEffectPrefab == null)
        {
            return;
        }
        //预制体默认朝上（+Y），角度在 +X 基准上再减 90 度
        Vector2 dir = (playerPos - transform.position).normalized;
        float angleZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.Euler(0f, 0f, angleZ));
        //特效播完后销毁，避免场景里堆积
        ParticleSystem particle = effect.GetComponent<ParticleSystem>();
        float lifeTime = particle != null ? particle.main.duration + particle.main.startLifetime.constantMax : 1f;
        Destroy(effect, lifeTime);
    }
}
