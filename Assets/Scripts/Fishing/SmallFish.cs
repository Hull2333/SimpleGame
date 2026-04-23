using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SmallFish : MonoBehaviour  //调用在SmallFish对象上
{
    //捉鱼进度条
    private Image fishProgress;
    public GameObject player;
    [Header("新添加")]
    public FishingGame fishingGame;
    private Rigidbody2D rb;
    private float moveSpeed = 3f;
    // 初始向左移动
    private Vector2 moveDirection = Vector2.left;
    private bool isLeft;
    
    private void Start()
    {
        rb = transform.GetComponent<Rigidbody2D>();
        //fishCurrentProgress = 1f;
    }
    private void OnEnable()
    {
        moveDirection = Vector2.left;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        isLeft = true;
    }
    private void FixedUpdate()
    {
        //游戏未暂停时移动
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

        if (other.CompareTag("EdgeBox") && !fishingGame.fishGamePause)
        {
            // 反转X轴方向（左右切换）
            moveDirection.x *= -1;
            if (isLeft)
            {
                transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                isLeft = false;
            }
            else
            {
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                isLeft = true;
            }
        }
    }
    //private void OnTriggerStay2D(Collider2D other)
    //{
        
    //    if (other.CompareTag("FishCatch"))
    //    {
    //        fishCurrentProgress += Time.deltaTime * 3;
    //    }
    //}
    //private void OnTriggerExit2D(Collider2D other)
    //{
    //    if (other.CompareTag("FishCatch"))
    //    {
    //        isProgressIncrease = false;
    //    }
    //}
    //public void Update()
    //{
    //    if (!isProgressIncrease && fishCurrentProgress != 0)
    //    {
    //        fishCurrentProgress -= Time.deltaTime;
    //        //当进度掉到0时，钓鱼失败
    //        if(fishCurrentProgress < 0.1)
    //        {
    //            player.GetComponent<PlayerController>().ExitFishing();
    //            fishCurrentProgress = 1f;
    //        }
    //    }
    //    //钓鱼进度已满
    //    if(fishCurrentProgress >= Settings.fishCatchProgress)
    //    {
    //        player.GetComponent<PlayerController>().compliteFishGame = true;
    //        fishCurrentProgress = 1f;
    //    }
    //    fishProgress.fillAmount = fishCurrentProgress / Settings.fishCatchProgress;
    //}
}
