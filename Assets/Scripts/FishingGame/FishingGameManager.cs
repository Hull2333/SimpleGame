using UnityEngine;
using UnityEngine.UI;

public class FishingGameManager : Singleton<FishingGameManager> //调用在FishBg对象上,管理钓鱼游戏的进行
{
    public Animator rodAnim;
    public Animator smallFishAnim;
    public Animator prograssImageAnim;
    public GameObject smallFish;
    private PlayerController playerController;
    public GameObject clickMouseTip;
    [Header("连接线参数")]
    //鱼竿和小鱼的连接线
    public LineRenderer line;
    public Transform rodPos;
    public Transform smallFishPos;
    //连接线的中点
    private Vector3 lineMidPos;
    [Header("其他")]
    public Image fishProgress;
    [HideInInspector] public float currentFishProgress;
    public Rigidbody2D smallFishRD;
    //小鱼一开始的位置
    public Transform smallFishStartPos;
    //左右边界点
    public Transform LeftEdgePos;
    public Transform RightEdgePos;
    //钓鱼进度条UI
    public GameObject fishProgressCanvas;
    public void OnEnable()
    {
        fishProgressCanvas.SetActive(true);
        playerController = GetComponentInParent<PlayerController>();
        line.positionCount = 50;
        smallFish.transform.position = smallFishStartPos.position;
        currentFishProgress = 1.5f;
    }
    private void Update()
    {
        fishProgress.fillAmount = currentFishProgress / Settings.fishCatchProgress;
        currentFishProgress -= 0.5f * Time.deltaTime;
        //进度条颤抖
        if (currentFishProgress / Settings.fishCatchProgress < 0.3f)
        {
            prograssImageAnim.SetBool("isShaking", true);
        }
        else
        {
            prograssImageAnim.SetBool("isShaking", false);
        }
        //效果动画
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            rodAnim.SetBool("isClicked", true);
            clickMouseTip.SetActive(false);
            smallFishAnim.SetBool("isPull", true);
        }
        if(Input.GetKeyUp(KeyCode.Mouse0))
        {
            rodAnim.SetBool("isClicked", false);
            clickMouseTip.SetActive(true);
            smallFishAnim.SetBool("isPull", false);
        }
        //失败
        if(smallFish.transform.position.x < LeftEdgePos.position.x || smallFish.transform.position.x > RightEdgePos.position.x || currentFishProgress <= 0)
        {
            fishProgressCanvas.SetActive(false);
            playerController.ExitFishing();
            gameObject.SetActive(false);

        }
        //成功
        if(currentFishProgress >= Settings.fishCatchProgress)
        {
            fishProgressCanvas.SetActive(false);
            playerController.FinishFishGame();
            gameObject.SetActive(false);
        }
    }
    private void FixedUpdate()
    {
        lineMidPos = playerController.GetMidPoint(rodPos.position, smallFishPos.position, Settings.fishLinePercent, Settings.fishLineOffsetY);
        //绘制连接线
        for (int i = 0; i < 50; i++)
        {
            Vector3 point = playerController.CalculateBezierPoint(rodPos.position, lineMidPos, smallFishPos.position, (i / 50.0f));
            line.SetPosition(i, point);

        }
        //给小鱼施加力
        if (Input.GetKey(KeyCode.Mouse0))
        {
            smallFishRD.AddForce(Vector2.left * (2f- Random.Range(0.5f, 1f)));
        }
        else
        {
            smallFishRD.velocity = new Vector2(Random.Range(0.5f, 1f), smallFishRD.velocity.y);
        }
        
    }

}
