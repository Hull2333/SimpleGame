using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FishingGame : MonoBehaviour
{
    public PlayerController playerController;
    //鼠标点击CD
    private float mouseCoolDownTime = 0.5f;
    //下一轮等待时间
    private float nextGameTime = 1f;
    //是否可以点击左键
    private bool canMouseLeft = true;
    //钓鱼游戏是否暂停
    public bool fishGamePause;
    //是否可以减少游戏时间
    public bool timeActive;
    //捉鱼进度条
    public GameObject fishBarBg;
    public Image fishProgress;
    private float fishCurrentProgress;
    public GameObject fishGameBg;
    //捕网
    public Transform catchBox;
    public GameObject smallFish;
    public bool isLeft;
    //小鱼与“捕网”接触
    public bool isCatchChance;
    //小鱼生成位置位置
    public Transform smallFishStartPos;
    //捕网的生成世界坐标范围X
    private float catchBoxWorldleftPosX,catchBoxWorldRightPosX;
    //捕鱼游戏的边缘碰撞体
    public Transform edgeBoxLeft, edgeBoxRight;
    [Header("捕网抖动参数")]
    //振幅
    public float amplitude = -0.05f;
    //震动时间
    public float durationPerShake = 0.05f;
    // 抖动次数
    public int loopCount = 1;
    //捕鱼游戏过关次数
    private int fishGamePassCount;
    public Transform fishGameCountParent;
    private List<GameObject> fishGameCountList = new List<GameObject>();
    public void OnEnable()
    {
        fishBarBg.SetActive(true);
        for (int i = 0; i < fishGameCountParent.childCount; i++)
        {
            fishGameCountList.Add(fishGameCountParent.GetChild(i).gameObject);
            fishGameCountParent.GetChild(i).GetChild(0).gameObject.SetActive(false);

        }
        //根据捕网的宽度来决定捕网的生成位置
        catchBoxWorldleftPosX = edgeBoxLeft.position.x + edgeBoxLeft.localScale.x / 2 + catchBox.localScale.x / 2;
        catchBoxWorldRightPosX = edgeBoxRight.position.x - edgeBoxRight.localScale.x / 2 - catchBox.localScale.x / 2;
        //捕鱼游戏时间
        fishCurrentProgress = Settings.fishCatchProgress;
        canMouseLeft = false;
        timeActive = false;
        fishGamePause = true;
        fishGamePassCount = 0;
        StartCoroutine(NextFishGameStart());
        fishGamePassCount = 0;


    }
    public void Update()
    {
        fishProgress.fillAmount = fishCurrentProgress / Settings.fishCatchProgress;
        //持续减少钓鱼游戏时间
        if(timeActive)
        {
            fishCurrentProgress -= 0.75f * Time.deltaTime;
            //钓鱼进度条缩减到30%开始抖动
            if(fishCurrentProgress / Settings.fishCatchProgress < 0.3f)
            {
                fishBarBg.GetComponentInChildren<Animator>().Play("FishBarShake");
            }
            //游戏时间耗尽
            if(fishCurrentProgress <= 0)
            {
                FishGameFailed();
                playerController.ExitFishing();
            }
        }
       
        if (Input.GetKeyDown(KeyCode.Mouse0) && canMouseLeft)
        {
            //按中小鱼
            if (isCatchChance)
            {
                timeActive = false;
                fishGamePause = true;
                canMouseLeft = false;
                fishGamePassCount++;
                //点亮次数
                fishGameCountList[fishGamePassCount - 1].transform.GetChild(0).gameObject.SetActive(true);
                smallFish.transform.GetChild(0).GetComponent<Animator>().SetTrigger("Bingo");
                catchBox.transform.GetChild(0).GetComponent<Animator>().SetTrigger("DisAppear");
                if(fishGamePassCount >= 3)
                {
                    StartCoroutine(FishGamePass());
                    return;
                }
                fishCurrentProgress += 0.5f;
                StartCoroutine(NextFishGameStart());

            }
            else
            {
                CatchBoxShake();
                StartCoroutine(MosueLeftCooldownRoutine());

            }
            

        }
        
        
        ////将leftUpPos、rightDownPos的局部坐标转换为世界坐标
        //Vector3 leftUpInWorldPos = transform.TransformPoint(chaseLeftUpPos.localPosition);
        //Vector3 rightDownInWorldPos = transform.TransformPoint(chaseRightDownPos.localPosition);
        ////当鼠标坐标在钓鱼游戏物体内，将“渔网”物体跟随当前鼠标的世界坐标
        //if (cM.mouseWorldPos.x > leftUpInWorldPos.x && cM.mouseWorldPos.x < rightDownInWorldPos.x && cM.mouseWorldPos.y > rightDownInWorldPos.y && cM.mouseWorldPos.y < leftUpInWorldPos.y)
        //{
        //    fishCatchObj.transform.position = cM.mouseWorldPos;
        //}
        ////小鱼随机移动
        //smallFishPos.position = Vector3.MoveTowards(smallFishPos.position, nextMovePos.position, speed * Time.deltaTime);
        //if (Vector3.Distance(smallFishPos.position, nextMovePos.position) < 0.1f)
        //{
        //    waitTime += Time.deltaTime;
        //    if (waitTime >= 0.5f)
        //    {
        //        nextMovePos.position = GetRandomPos();
        //        waitTime = 0;
        //    }
        //}

    }

   /// <summary>
   /// 捕网震动
   /// </summary>
    private void CatchBoxShake()
    {
        fishGamePause = true;
        float originalY = catchBox.localPosition.y;
        // 停止当前可能存在的动画，避免冲突
        catchBox.DOKill();
        // 执行抖动动画
        catchBox.DOLocalMoveY(originalY + amplitude, durationPerShake)
            .SetEase(Ease.Linear)
            // 一次往返算 2 次循环
            .SetLoops(loopCount, LoopType.Yoyo)
            .OnComplete(() => {
                // 结束后复位
                catchBox.localPosition = new Vector3(
                    catchBox.localPosition.x,
                    originalY,
                    catchBox.localPosition.z
                );
            });
    }
    /// <summary>
    /// 等待后才可以再次按下左键
    /// </summary>
    private IEnumerator MosueLeftCooldownRoutine()
    {
        canMouseLeft = false;
        yield return new WaitForSeconds(mouseCoolDownTime);
        canMouseLeft = true;
        fishGamePause = false;      
    }
    /// <summary>
    ///  等一会开始下一轮钓鱼游戏
    /// </summary>
    /// <returns></returns>
    private IEnumerator NextFishGameStart()
    {
        //等待小鱼和捕网消失的动画结束
        yield return new WaitForSeconds(nextGameTime);
        //初始化小鱼位置
        smallFish.transform.position = smallFishStartPos.position;
        //随机生成捕网位置
        catchBox.position = new Vector3(Random.Range(catchBoxWorldleftPosX, catchBoxWorldRightPosX), edgeBoxLeft.position.y);
        Debug.Log("catchBoxWorldleftPosX:  " + catchBoxWorldleftPosX + "catchBoxWorldRightPosX:  " + catchBoxWorldRightPosX + "catchBox.position.x :  " + catchBox.position.x);
        smallFish.transform.GetChild(0).GetComponent<Animator>().enabled = true;
        smallFish.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
        catchBox.transform.GetChild(0).GetComponent<Animator>().enabled = true;
        catchBox.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
        yield return new WaitForSeconds(nextGameTime);
        timeActive = true;
        fishGamePause = false;
        canMouseLeft = true;
    }
    /// <summary>
    /// 捕鱼游戏过关
    /// </summary>
    private IEnumerator FishGamePass()
    {
        yield return new WaitForSeconds(nextGameTime);
        playerController.compliteFishGame = true;
        fishBarBg.SetActive(false);
        gameObject.SetActive(false);
    }
    /// <summary>
    /// 捕鱼游戏失败
    /// </summary>
    private void FishGameFailed()
    {
        smallFish.transform.GetChild(0).GetComponent<Animator>().enabled = false;
        smallFish.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        catchBox.transform.GetChild(0).GetComponent<Animator>().enabled = false;
        catchBox.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        playerController.compliteFishGame = false;
        fishBarBg.SetActive(false);
        gameObject.SetActive(false);
    }
   
}
