using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : Singleton<HealthBar>  //调用在StateBar对象上
{
    
    public Image hpImage;
    public Image bufferHpImage;
    public Image stminaImage;
    private PlayerController playerController => FindObjectOfType<PlayerController>();
    //血量缓冲时间
    private float bufferTime = 0.5f;
    //血量缓冲协程
    private Coroutine updateCoroutine;
    public void OnEnable()
    {
        EventHandler.StartNPCEvent += OnStartNPCEvent;
        EventHandler.EndNPCEvent += OnEndNPCEvent;
    }
    public void OnDisable()
    {
        EventHandler.StartNPCEvent -= OnStartNPCEvent;
        EventHandler.EndNPCEvent -= OnEndNPCEvent;
    }
    private void Update()
    {
        UpdateHealthBar();
        UpdateStminaBar();
    }
    private void OnStartNPCEvent()
    {
        GetComponent<Canvas>().enabled = false;
    }

    private void OnEndNPCEvent()
    {
        GetComponent<Canvas>().enabled = true;
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
