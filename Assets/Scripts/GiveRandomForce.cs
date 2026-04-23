using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;


public class GiveRandomForce : MonoBehaviour
{
    //item抛出的最大力度、最小力度
    public float maxForce;
    public float minForce;
    //item出现时的y轴
    private float finalPosY;
    //item抛出的方向
    private Vector3 forceDir;
    private Transform player;
    private Rigidbody2D rigidbody2D => transform.GetComponent<Rigidbody2D>();
    public void Start()
    {

        finalPosY = transform.position.y;
        
        GiveItemForce();
       
        
    }
    public void Update()
    {
        //当Item掉落只已原位置y轴一样时静止
        if ( transform.position.y < finalPosY  )
        {
            rigidbody2D.gravityScale = 0;
            rigidbody2D.velocity = Vector2.zero;
            
        }
        //Item找到PLayer开始往Player移动
        if (player != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, 5f * Time.deltaTime);
        }
    }

    private void GiveItemForce()
    {
        //forceDir = Random.insideUnitCircle/2;
        //随机方向
        forceDir = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(0, 1f));
        //向量化
        forceDir.Normalize();
        //施加方向和随机的力
        rigidbody2D.AddForce(forceDir * Random.Range(minForce, maxForce), ForceMode2D.Impulse);
        
    }
    /// <summary>
    /// PLayer进入物体吸附范围
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Player")
        {
            player = other.transform;
        }
    }
}
