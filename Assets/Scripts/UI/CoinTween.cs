using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinTween : MonoBehaviour //调用在Coin预制体上
{
    private Image image;
    public Vector2 targetPos;
    private void Awake()
    {
        image = GetComponent<Image>();
    }
    
    /// <summary>
    /// 金币开始滑行
    /// </summary>
    /// <param name="delay">延迟</param>
    /// <param name="originPos">初始位置</param>
    public void PlayTween(float delay , Vector3 originPos)
    {
        //OnComplete(() = >{});指待前面的方法完成开始接下来的方法
        //开始移动到originPos
        image.rectTransform.DOAnchorPos(originPos, 0.5f).SetEase(Ease.OutQuad).SetDelay(delay).OnComplete(() =>
        {
            //开始移动到targetPos
            image.rectTransform.DOAnchorPos(targetPos, 0.6f).SetEase(Ease.OutQuad).SetDelay(delay).OnComplete(() =>
            {
                //金币开始缩小到0
                transform.DOScale(0, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    //最后摧毁该物体
                    Destroy(gameObject);
                    //TODO:金钱增加
                });
            });
        });
        //金币放大到1
        //transform.DOScale(1, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
        //{
           
        //});

    }

}
