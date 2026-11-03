using MFarm.Map;
using UnityEngine;
using UnityEngine.UI;

public class FishingGameManager : Singleton<FishingGameManager> //调用在FishProcessCanvas对象上,管理钓鱼游戏的进行
{
    [Header("钓鱼游戏")]
    public PlayerController playerController;
    //鱼逃脱临界线
    public GameObject fishAwayLine;
    //鱼钩移动方向（小游戏开始时记录）
    private Vector2 fishingDir;
    //鱼钩后退极限位置
    private Vector3 hookBackLimitPos;
    //鱼钩当前速度
    private float hookSpeed;
    //鱼钩状态：回升、按住拉回、鱼脱力按住拉回、鱼脱力停住
    private enum HookState { Rising, HoldPull, FishTiredPull, FishTiredIdle }
    //当前鱼钩状态（速度和动画都由它驱动）
    private HookState hookState;
    //鱼是否处于脱力恢复中（此期间鱼不拉钩）
    private bool isFishStaminaRecovering;
    //鱼钩越过逃脱线的累计时间
    private float awayLineOverTime;
    //鱼的最大速度
    private float fishMaxSpeed;
    //鱼的体力值
    private float fishStamina;
    //鱼当前的体力条
    private float currentFishStamina;
    //鱼的体力条
    public Image fishStaminaProgress;
    public Image fishStaminaBg;
    //提示拉鱼
    public GameObject pullFishTip;
    //玩家钓鱼体力值
    private float playerFishStamina = 5f;
    //玩家当前的钓鱼体力值
    private float currentPlayerFishStamina;
    //玩家钓鱼体力条
    public Image playerFishStaminaProgress;
    //鱼体力的下降速度
    private float fishStaminaDeclineSpeed = 0.5f;
    //玩家钓鱼体力变化速度
    private float playerFishStaminaChangeSpeed = 0.5f;
    //鱼体力下降的速度区间最小值
    private float staminaSpeedMin = -0.4f;
    //鱼体力下降的速度区间最大值
    private float staminaSpeedMax = 0.3f;
    [Header("鱼钩对象")]
    //鱼钩的移动方向
    private float baitX,baitY;
    public void OnEnable()
    {
        //鱼的最大速度和体力取自本次钓到的鱼
        fishMaxSpeed = playerController.FishDetails.fishMaxSpeed;
        fishStamina = playerController.FishDetails.fishStamina;
        // 鱼逃脱临界线：从鱼钩沿朝向逐格外推，遇到不可钓的格子就停在它的前一格（3格全可钓则取第3格）
        fishAwayLine.SetActive(true);
        fishingDir = playerController.FaceDirection;
        Vector3 hookPos = playerController.baitManager.baitGameObject.transform.position;
        Grid grid = FindObjectOfType<Grid>();
        Vector3Int hookCell = grid.WorldToCell(hookPos);
        Vector3Int dirCell = new Vector3Int(Mathf.RoundToInt(fishingDir.x), Mathf.RoundToInt(fishingDir.y), 0);
        int lineDistance = 3;
        for (int i = 1; i <= 3; i++)
        {
            TileDetails tile = GridMapManager.Instance.GetTileDetailsOnMousePosition(hookCell + dirCell * i);
            // 该格不可钓时，线停在前一格（i-1，为0则贴在鱼钩位置）
            if (tile == null || !(tile.canLaterFishing || tile.canSeaFishing))
            {
                lineDistance = i - 1;
                break;
            }
        }
        fishAwayLine.transform.position = hookPos + (Vector3)fishingDir * (lineDistance * Settings.gridCellSize);
        // 竖线在左右朝向时方向正确，上下朝向时旋转90度改为横线
        fishAwayLine.transform.rotation = (fishingDir.y != 0f) ? Quaternion.Euler(0f, 0f, 90f) : Quaternion.identity;
        // 鱼钩移动重置，并算出后退极限：从起点往玩家方向逐格回溯，取最远的连续可钓格；后方第一格不可钓则退回起点
        int backCells = 0;
        while (true)
        {
            TileDetails backTile = GridMapManager.Instance.GetTileDetailsOnMousePosition(hookCell - dirCell * (backCells + 1));
            if (backTile == null || !(backTile.canLaterFishing || backTile.canSeaFishing))
            {
                break;
            }
            backCells++;
        }
        hookBackLimitPos = hookPos - (Vector3)fishingDir * (backCells * Settings.gridCellSize);
        //鱼钩速度重置，开局从0升到最大速度
        hookSpeed = 0f;
        hookState = HookState.Rising;
        //体力与判定状态重置
        currentFishStamina = fishStamina;
        currentPlayerFishStamina = playerFishStamina;
        isFishStaminaRecovering = false;
        awayLineOverTime = 0f;
        //鱼抵抗动画：面朝方向在开局赋值，抵抗状态默认关闭
        playerController.baitManager.SetFishFaceDirection(fishingDir);
        playerController.baitManager.SetFishResist(false);
    }
    private void OnDisable()
    {
        //小游戏结束（成功或失败）后关闭逃脱线
        fishAwayLine.SetActive(false);
    }
    private void Update()
    {
        //鱼钩移动：按住左键每帧后退0.05（封顶-1）；松开后速度归零，从0用固定0.5秒升到fishMaxSpeed
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            hookSpeed = 0f;
        }
        bool holdingMouse = Input.GetKey(KeyCode.Mouse0);
        hookState = isFishStaminaRecovering
            ? (holdingMouse ? HookState.FishTiredPull : HookState.FishTiredIdle)
            : (holdingMouse ? HookState.HoldPull : HookState.Rising);
        switch (hookState)
        {
            case HookState.HoldPull:
            case HookState.FishTiredPull:
                hookSpeed = Mathf.Max(hookSpeed - 0.05f, -1f);
                break;
            case HookState.Rising:
                hookSpeed = Mathf.Min(hookSpeed + fishMaxSpeed / 0.5f * Time.deltaTime, fishMaxSpeed);
                break;
            case HookState.FishTiredIdle:
                //鱼脱力期间鱼不拉钩，松开时钩子停住
                hookSpeed = 0f;
                break;
        }
        //鱼钩动画方向由玩家输入和是否脱力决定：未脱力松手朝钓鱼方向，按住朝反方向；脱力松手时停住播待机
        Vector2 moveDir = holdingMouse ? -fishingDir : (isFishStaminaRecovering ? Vector2.zero : fishingDir);
        playerController.baitManager.SetBaitMoveDirection(moveDir);
        Vector3 hookNewPos = playerController.baitManager.baitGameObject.transform.position + (Vector3)fishingDir * (hookSpeed * Time.deltaTime);
        //速度为负时鱼钩往回走，最多退到起点后方最远的可钓格
        if (fishingDir.x > 0f)
        {
            hookNewPos.x = Mathf.Max(hookNewPos.x, hookBackLimitPos.x);
        }
        else if (fishingDir.x < 0f)
        {
            hookNewPos.x = Mathf.Min(hookNewPos.x, hookBackLimitPos.x);
        }
        else if (fishingDir.y > 0f)
        {
            hookNewPos.y = Mathf.Max(hookNewPos.y, hookBackLimitPos.y);
        }
        else
        {
            hookNewPos.y = Mathf.Min(hookNewPos.y, hookBackLimitPos.y);
        }
        playerController.baitManager.SetBaitPosition(hookNewPos);
        //鱼体力：速度保持在-0.2~0.2之间时下降，耗尽后每秒恢复20%
        if (isFishStaminaRecovering)
        {
            currentFishStamina = Mathf.Min(currentFishStamina + fishStamina * 0.2f * Time.deltaTime, fishStamina);
            if (currentFishStamina >= fishStamina)
            {
                //恢复满，鱼重新发力
                isFishStaminaRecovering = false;
            }
        }
        else if (hookSpeed >= staminaSpeedMin && hookSpeed <= staminaSpeedMax)
        {
            currentFishStamina = Mathf.Max(currentFishStamina - fishStaminaDeclineSpeed * Time.deltaTime, 0f);
            if (currentFishStamina <= 0f)
            {
                //鱼脱力，钩子不再被鱼拉动
                isFishStaminaRecovering = true;
            }
        }
        //鱼抵抗动画：仅在鱼体力下降时播放
        playerController.baitManager.SetFishResist(!isFishStaminaRecovering && hookSpeed >= staminaSpeedMin && hookSpeed <= staminaSpeedMax);
        //玩家体力：鱼未脱力时猛拉消耗、被鱼拉走恢复；鱼脱力恢复期间不变
        if (!isFishStaminaRecovering)
        {
            if (hookSpeed < staminaSpeedMin)
            {
                currentPlayerFishStamina = Mathf.Max(currentPlayerFishStamina - playerFishStaminaChangeSpeed * Time.deltaTime, 0f);
            }
            else if (hookSpeed > staminaSpeedMax)
            {
                //恢复速度是下降速度的1/5
                currentPlayerFishStamina = Mathf.Min(currentPlayerFishStamina + playerFishStaminaChangeSpeed / 5f * Time.deltaTime, playerFishStamina);
            }
        }
        //体力条显示
        fishStaminaProgress.fillAmount = currentFishStamina / fishStamina;
        playerFishStaminaProgress.fillAmount = currentPlayerFishStamina / playerFishStamina;
        //鱼体力为空（脱力）时背景变红并显示拉鱼提示
        bool isFishStaminaEmpty = isFishStaminaRecovering;
        fishStaminaBg.color = isFishStaminaEmpty ? new Color32(255, 57, 0, 255) : Color.white;
        pullFishTip.SetActive(isFishStaminaEmpty);
        //成功：鱼脱力恢复期间，玩家拉动鱼钩退到最后方的可钓格
        if (isFishStaminaRecovering && hookSpeed < 0f && Vector2.Dot((Vector2)(hookNewPos - hookBackLimitPos), fishingDir) <= 0f)
        {
            gameObject.SetActive(false);
            playerController.FinishFishGame();
            gameObject.SetActive(false);
            return;
        }
        //失败：玩家体力耗尽
        if (currentPlayerFishStamina <= 0f)
        {
            gameObject.SetActive(false);
            playerController.ExitFishing();
            gameObject.SetActive(false);
            return;
        }
        //失败：鱼钩越过逃脱线并停留超过0.5秒
        float beyondLine = Vector2.Dot((Vector2)(hookNewPos - fishAwayLine.transform.position), fishingDir);
        if (beyondLine >= 0f)
        {
            awayLineOverTime += Time.deltaTime;
            if (awayLineOverTime >= 0.5f)
            {
                gameObject.SetActive(false);
                playerController.ExitFishing();
                gameObject.SetActive(false);
            }
        }
        else
        {
            awayLineOverTime = 0f;
        }
    }
}
