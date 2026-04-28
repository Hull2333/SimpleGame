using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SellJoyButton : MonoBehaviour //调用在SellJoyButton对象上
{
    private Animator anim;
    private Button button;
    public void Start()
    {
        anim = GetComponent<Animator>();
        button = GetComponent<Button>();
        button.onClick.AddListener(ClickSellJoyButton);
    }
    /// <summary>
    /// 点击出售摇杆时
    /// </summary>
    private void ClickSellJoyButton()
    {
        button.interactable = false;
        anim.SetTrigger("Pressed");
        StartCoroutine(RestoreSellJoyButtonInter());
    }
    private IEnumerator RestoreSellJoyButtonInter()
    {
        //获取当前的播放动画的时长
        AnimatorStateInfo currentInfo = anim.GetCurrentAnimatorStateInfo(0);
        //确保现在播放的动画时点击动画
        while (!currentInfo.IsName("Pressed"))
        {
            yield return null;
            currentInfo = anim.GetCurrentAnimatorStateInfo(0);
        }
        while (currentInfo.normalizedTime < 1f)
        {
            yield return null;
            currentInfo = anim.GetCurrentAnimatorStateInfo(0); 
        }
        button.interactable = true;
    }
}
