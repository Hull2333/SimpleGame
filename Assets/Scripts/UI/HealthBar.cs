using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static AnimalData_SO;
public class HealthBar : Singleton<HealthBar>  //调用在StateBar对象上
{
    public GameObject HealthBg;
    public GameObject StminaBg;
    public Image hpImage;
    public Image bufferHpImage;
    public Image stminaImage;
    private Canvas stateCanvas => GetComponent<Canvas>();
    private PlayerController playerController => FindObjectOfType<PlayerController>();
    //血量缓冲时间
    private float bufferTime = 0.5f;
    //血量缓冲协程
    private Coroutine updateCoroutine;
    // 玩家离摄像机下边界小于 3 时，血条和体力条上移 240
    private const float bgRaiseThreshold = 3f;
    private const float bgRaiseOffset = 240f;
    private Vector2 healthBgOriginalPos;
    private Vector2 stminaBgOriginalPos;
    public void OnEnable()
    {
        EventHandler.StartNPCEvent += OnStartNPCEvent;
        EventHandler.EndNPCEvent += OnEndNPCEvent;
        EventHandler.BuildindModeEvent += OnBuildindModeEvent;
    }
    public void OnDisable()
    {
        EventHandler.StartNPCEvent -= OnStartNPCEvent;
        EventHandler.EndNPCEvent -= OnEndNPCEvent;
        EventHandler.BuildindModeEvent -= OnBuildindModeEvent;
    }

    private void Awake()
    {
        stateCanvas.worldCamera = FindAnyObjectByType<Camera>();
        stateCanvas.sortingLayerName = "Collision";
        // 记录血条和体力条的原位置，贴近摄像机下边界时上移，离开后复原
        if (HealthBg != null)
        {
            healthBgOriginalPos = HealthBg.GetComponent<RectTransform>().anchoredPosition;
        }
        if (StminaBg != null)
        {
            stminaBgOriginalPos = StminaBg.GetComponent<RectTransform>().anchoredPosition;
        }
    }

    private void Update()
    {
        UpdateHealthBar();
        UpdateStminaBar();
        UpdateBgPosition();
    }

    /// <summary>
    /// 玩家离摄像机下边界小于 100 时，血条和体力条上移 240，否则回到原位置
    /// </summary>
    private void UpdateBgPosition()
    {
        Camera cam = Camera.main;
        if (cam == null || playerController == null)
        {
            return;
        }
        // 摄像机下边界的世界 Y
        float bottomY = cam.ViewportToWorldPoint(new Vector3(0.5f, 0f)).y;
        bool nearBottom = playerController.transform.position.y - bottomY < bgRaiseThreshold;
        if (HealthBg != null)
        {
            RectTransform rect = HealthBg.GetComponent<RectTransform>();
            rect.anchoredPosition = nearBottom
                ? new Vector2(healthBgOriginalPos.x, healthBgOriginalPos.y + bgRaiseOffset)
                : healthBgOriginalPos;
        }
        if (StminaBg != null)
        {
            RectTransform rect = StminaBg.GetComponent<RectTransform>();
            rect.anchoredPosition = nearBottom
                ? new Vector2(stminaBgOriginalPos.x, stminaBgOriginalPos.y + bgRaiseOffset)
                : stminaBgOriginalPos;
        }
    }
    private void OnStartNPCEvent()
    {
        stateCanvas.enabled = false;
    }

    private void OnEndNPCEvent()
    {
        stateCanvas.enabled = true;
    }
    private void OnBuildindModeEvent(BuildingDetails details,AnimalDetails animal, bool startMode)
    {
        hpImage.transform.parent.gameObject.SetActive(!startMode);
        stminaImage.transform.parent.gameObject.SetActive(!startMode);
    }
    /// <summary>
    /// 更新玩家血条
    /// </summary>
    public void UpdateHealthBar()
    {
        hpImage.fillAmount = playerController.currentHealth / playerController.maxHealth;
        //避免出现协程混乱，当有协程在执行时，不执行接下来的协程
        if(updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
        }
        updateCoroutine = StartCoroutine(UpdateHpBuffer());
    }

    public void UpdateStminaBar()
    {
        stminaImage.fillAmount = playerController.currentStmina / playerController.maxStmina;
    }
    /// <summary>
    /// 血条缓冲效果
    /// </summary>
    /// <returns></returns>
    private IEnumerator UpdateHpBuffer()
    {
        //缓冲血量
        float bufferlength = bufferHpImage.fillAmount - hpImage.fillAmount;
        //缓冲计时器
        float elapsedTime = 0f;
        if( elapsedTime < bufferTime && bufferlength != 0)
        {
            elapsedTime += Time.deltaTime;
            //Lerp，在起点hpImage.fillAmount + bufferlength与终点 hpImage.fillAmount之间，通过插值elapsedTime / bufferTime返回起点或终点
            bufferHpImage.fillAmount = Mathf.Lerp(hpImage.fillAmount + bufferlength, hpImage.fillAmount, elapsedTime / bufferTime);
            yield return null;
        }
        //避免白色缓冲血条超过当前血条
        bufferHpImage.fillAmount = hpImage.fillAmount;
    }
}
