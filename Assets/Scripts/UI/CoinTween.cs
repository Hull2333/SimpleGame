using DG.Tweening;
using MFarm.Inventory;
using UnityEngine;
using UnityEngine.UI;

public class CoinTween : MonoBehaviour //调用在Coin预制体上
{
    private Image image;
    public Image imageChild;
    public Vector2 targetPos;
    //是否是第一个生成的金币
    public bool isfirst = false;
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
        //OnComplete(() =>{});指待前面的方法完成开始接下来的方法
        //开始移动到originPos
        image.rectTransform.DOAnchorPos(originPos, 0.5f).SetEase(Ease.OutQuad).SetDelay(delay).OnComplete(() =>
        {
            //开始移动到targetPos
            image.rectTransform.DOAnchorPos(targetPos, 0.6f).SetEase(Ease.OutQuad).SetDelay(delay).OnComplete(() =>
            {
                //金币开始缩小到0
                transform.DOScale(0, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    InventoryUI.Instance.playerMoneyUIAnim.Play("MoneyUIShake");
                    //最后摧毁该物体
                    Destroy(gameObject);
                    //在第一个金币到达时，金钱增加
                    if (isfirst)
                    {
                        InventoryManager.Instance.IncreasePlayerMoney(InventoryManager.Instance.ModifySellBoxValue());
                    }
                   
                });
            });
        });
        //金币放大到1
        //transform.DOScale(1, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
        //{
           
        //});

    }
    /// <summary>
    /// 先慢慢显性再移动到目标点
    /// </summary>
    /// <param name="delay"></param>
    public void PlayTweenOfAlpha(float delay)
    {
        imageChild.DOFade(1f, 0.3f).SetEase(Ease.InQuad).SetDelay(delay).OnComplete(() =>
        {
            image.rectTransform.DOAnchorPos(targetPos, 0.4f).SetEase(Ease.OutQuad);
        });
    }

}
