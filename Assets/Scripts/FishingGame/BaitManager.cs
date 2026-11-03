using UnityEngine;

//挂在Player上，负责鱼钩的显示、位置、移动方向、动画以及鱼线的绘制
public class BaitManager : MonoBehaviour
{
    [Header("鱼钩与鱼线")]
    //鱼钩物体
    public GameObject baitGameObject;
    //鱼线
    public LineRenderer fishLine;
    //鱼钩当前的移动方向（实时更新，供动画使用）
    [HideInInspector] public float baitX;
    [HideInInspector] public float baitY;
    //鱼钩动画
    private Animator baitAnimator;
    //鱼竿位置与贝塞尔曲线
    private PlayerController playerController;
    //鱼线中点
    private Vector3 fishlineMidPos;
    //是否显示鱼线
    private bool isBaitLineShow;

    public void Awake()
    {
        baitAnimator = baitGameObject.GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }
    /// <summary>
    /// 显示鱼钩和鱼线，并按钓鱼朝向设置排序层（朝上时在玩家身后）
    /// </summary>
    public void ShowBait(Vector2 dir)
    {
        baitGameObject.SetActive(true);
        string sortingLayer = dir.y > 0f ? "Instance2" : "Instance";
        baitGameObject.GetComponent<SpriteRenderer>().sortingLayerName = sortingLayer;
        fishLine.sortingLayerName = sortingLayer;
        fishLine.gameObject.SetActive(true);
        fishLine.enabled = true;
        fishLine.positionCount = 50;
        isBaitLineShow = true;
    }
    /// <summary>
    /// 隐藏鱼钩和鱼线
    /// </summary>
    public void HideBait()
    {
        baitGameObject.SetActive(false);
        fishLine.enabled = false;
        fishLine.gameObject.SetActive(false);
        isBaitLineShow = false;
        baitX = 0f;
        baitY = 0f;
    }
    /// <summary>
    /// 设置鱼钩位置
    /// </summary>
    public void SetBaitPosition(Vector3 pos)
    {
        baitGameObject.transform.position = pos;
    }
    /// <summary>
    /// 设置鱼钩移动方向与移动动画（方向为0时回到待机动画）
    /// </summary>
    public void SetBaitMoveDirection(Vector2 dir)
    {
        baitX = dir.x;
        baitY = dir.y;
        baitAnimator.SetBool("BaitMove", dir != Vector2.zero);
    }
    /// <summary>
    /// 设置钓鱼面朝方向（鱼抵抗动画使用，整个小游戏期间不变）
    /// </summary>
    public void SetFishFaceDirection(Vector2 dir)
    {
        baitAnimator.SetFloat("FishFaceX", dir.x);
        baitAnimator.SetFloat("FishFaceY", dir.y);
    }
    /// <summary>
    /// 切换鱼的抵抗动画：鱼体力下降时播放，其余情况不播放
    /// </summary>
    public void SetFishResist(bool isResisting)
    {
        baitAnimator.SetBool("Resist", isResisting);
    }
    //鱼钩到达可钓鱼瓦片后播放待机动画
    public void PlayIdle()
    {
        baitAnimator.SetTrigger("BaitIdle");
    }
    //鱼咬钩动画
    public void PlayBiting()
    {
        baitAnimator.SetTrigger("Baiting");
    }
    private void LateUpdate()
    {
        if (!isBaitLineShow)
        {
            return;
        }
        //鱼线从鱼竿连到鱼钩当前位置
        fishlineMidPos = playerController.GetMidPoint(playerController.baitAppearPos.position, baitGameObject.transform.position, Settings.fishLinePercent, Settings.fishLineOffsetY);
        for (int i = 0; i < 50; i++)
        {
            Vector3 point = playerController.CalculateBezierPoint(playerController.baitAppearPos.position, fishlineMidPos, baitGameObject.transform.position, (i / 50.0f));
            fishLine.SetPosition(i, point);
        }
        //把实时移动方向传给动画
        baitAnimator.SetFloat("BaitX", baitX);
        baitAnimator.SetFloat("BaitY", baitY);
    }
}
