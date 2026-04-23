using DG.Tweening;
using UnityEngine;
//确保该对象一定有SpriteRenderer组件,没有会自动添加
[RequireComponent(typeof(SpriteRenderer))]
public class ItemFader : MonoBehaviour  //调用在Tree1对象上
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    /// <summary>
    /// 逐渐恢复颜色
    /// </summary>
    public void FadeIn()    
    {
        Color targetColor = new Color(1, 1, 1, 1);
        //在Settings.fadeDuration期间颜色逐渐变成targetColor
        spriteRenderer.DOColor(targetColor, Settings.itemFadeDuration);
    }
    /// <summary>
    ///  逐渐半透明
    /// </summary>
    public void FadeOut()   
    {
        Color targetColor = new Color(1, 1, 1, Settings.targetAlpha);
        spriteRenderer.DOColor(targetColor, Settings.itemFadeDuration);
    }
}
