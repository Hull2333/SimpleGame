using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour //调用在场景的Bird对象上
{
    public Animator anim;
    private float speed = 1f;
    private float flySpeed = 10f;
    private bool isMove;
    private bool isFly;
    private float idleTime;
    private float moveTime;
    private float otherAnimTime;
    private float flyTime;
    private float faceX;
    private Vector2 flyDir;
    public ParticalEffectType effectType;
    public void OnEnable()
    {
        flyTime = 0;
        idleTime = 0;
        moveTime = 0;
        otherAnimTime = 0;
        isFly = false;
        faceX = 1f;
    }
    public void Update()
    {
        anim.SetFloat("MoveX", faceX);
        //anim.SetBool("isFly", isFly);
        anim.SetBool("isMove", isMove);
        if (!isFly)
        {
            if (!isMove)
            {
                idleTime += Time.deltaTime;
                otherAnimTime += Time.deltaTime;
                if (otherAnimTime >= 2f)
                {
                    otherAnimTime = 0;
                    int randomNum = Random.Range(0, 2);
                    if (randomNum == 0)
                    {
                        anim.SetTrigger("isIdle1");
                    }
                    if (randomNum == 1)
                    {
                        anim.SetTrigger("isIdle2");
                    }
                }
            }
            if (idleTime >= 3f)
            {
                isMove = true;
                faceX = Random.Range(-1, 2);
                if (faceX == 0)
                {
                    faceX = -1;
                }
                idleTime = 0;
            }

            if (isMove)
            {
                if (faceX < 0)
                {
                    transform.Translate(Vector2.left * speed * Time.deltaTime);
                }
                else
                {
                    transform.Translate(Vector2.right * speed * Time.deltaTime);
                }
                moveTime += Time.deltaTime;
                if (moveTime >= 2f)
                {
                    moveTime = 0;
                    isMove = false;
                }
            }
            
        }
        else
        {
            flyTime += Time.deltaTime;
            transform.Translate(flyDir * flySpeed * Time.deltaTime);
            if(flyTime >= 10f)
            {
                gameObject.SetActive(false);
            }
        }


    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            EventHandler.CallParticleEffectEvent(effectType, transform.position,Vector2.zero);
            isMove = false;
            isFly = true;
            //在玩家左边
            if(other.transform.parent.position.x > transform.position.x)
            {
                faceX = -1;
                flyDir = new Vector2(-1, 1).normalized;
            }
            else
            {
                faceX = 1;
                flyDir = new Vector2(1, 1).normalized;
            }
            anim.SetTrigger("isFly");
            
        }
    }
}
