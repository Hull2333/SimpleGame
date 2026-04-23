using DG.Tweening.Core.Easing;
using System.Collections;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;

public class ItemInterActive : MonoBehaviour    //调用在需要接触而晃动的场景上，列如杂草
{
    //是否正在晃动
    private bool isAnimating;
    private WaitForSeconds pause = new WaitForSeconds(0.04f);
    private Animator anim;

    private void Start()
    {
        anim = this.GetComponent<Animator>();
    }

    

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAnimating)
        {
            isAnimating = true;
            if (other != null)
            {
                anim.Play("Shaking");
            }
        }
        isAnimating = false;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isAnimating)
        {
            isAnimating = true;
            if (other != null)
            {
                anim.Play("Shaking");
            }
        }
        isAnimating = false;
    }
    /// <summary>
    /// 播放销毁动画并销毁
    /// </summary>
    public void PlayBreakAnim()
    {
        anim.Play("Break");
        StartCoroutine(WaitAnimDone());
        
    }
    /// <summary>
    /// 等待销毁动画播放完后销毁
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitAnimDone()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
    
}
