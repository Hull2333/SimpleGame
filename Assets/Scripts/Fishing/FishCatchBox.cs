using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishCatchBox : MonoBehaviour //调用在CatchBox对象上
{
    public FishingGame fishingGame;
    private float moveSpeed = 2f;
    private Rigidbody2D rb;
    // 初始向左移动
    private Vector2 moveDirection = Vector2.left;
    
    void Start()
    {
        rb = transform.GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (!fishingGame.fishGamePause)
        {
            rb.velocity = moveDirection * moveSpeed;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }

    }

   
    private void OnTriggerEnter2D(Collider2D other)
    {
       
        if (other.CompareTag("EdgeBox"))
        {
            // 反转X轴方向（左右切换）
            moveDirection.x *= -1;
        }
        if (other.CompareTag("SmallFish"))
        {
            fishingGame.isCatchChance = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("SmallFish"))
        {
            fishingGame.isCatchChance = false;
        }
    }

}
