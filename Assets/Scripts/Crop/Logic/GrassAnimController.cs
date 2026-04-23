using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassAnimController : MonoBehaviour //调用在可收割环境杂草预制体上
{
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("ItemBase"))
        {
            if (other.transform.position.x < transform.position.x)
            {
                PlayRightShakeAnim();
            }
            else
            {
                PlayLeftShakeAnim();
            }
        }
        
    }
   
    /// <summary>
    /// 播放杂草往左摇晃动画
    /// </summary>
    public void PlayLeftShakeAnim()
    {
        anim.SetTrigger("LeftShake");
    }
    /// <summary>
    /// 播放杂草往右摇晃动画
    /// </summary>
    private void PlayRightShakeAnim()
    {
        anim.SetTrigger("RightShake");
    }
}
