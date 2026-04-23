using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitCatchAndSmallFish : MonoBehaviour //调用在CatchBoxSprite和SmallFishSprite对象上
{
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    public FishingGame fishingGame;
    public void Start()
    {
        spriteRenderer = transform.GetComponent<SpriteRenderer>();
        animator = transform.GetComponent<Animator>();
    }
    /// <summary>
    /// 调用在动画帧上
    /// </summary>
    public void QuitSpriteAndAnimtor()
    {
        spriteRenderer.enabled = false;
        animator.enabled = false;
        
    }
    /// <summary>
    /// 调用在动画帧上,等待动画播放完
    /// </summary>
    public void WaitAnimatorPlay()
    {
        fishingGame.fishGamePause = false;
        fishingGame.timeActive = true;
    }
}
