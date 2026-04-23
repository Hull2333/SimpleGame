using MFarm.Inventory;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class ItemGetTipUI : MonoBehaviour //调用在Slot_ItemGetTip预制体上
{
    //UI显示时间
    private float showTimer;
    public int itemID;
    public Image itemSprite;
    public TextMeshProUGUI itemName;
    public Animator animator;
    public Text amountText;
    private int currentAmount;
    public void Update()
    {
        showTimer -= Time.deltaTime;
        if (showTimer <= 0)
        {
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// 修改拾取物品的UI显示
    /// </summary>
    /// <param name="item"></param>
    public void ModifyItemParameter(ItemDetails item)
    {
        showTimer = 3f;
        itemID = item.itemID;
        itemSprite.sprite = item.itemIcon;
        itemName.text = item.itemName;
        currentAmount = 1;
        amountText.text = currentAmount.ToString();
    }
    /// <summary>
    /// 修改拾取物品数量和播放动画
    /// </summary>
    public void ModifyItemAmount()
    {
        currentAmount ++;
        showTimer = 3f;
        amountText.text = currentAmount.ToString();
        animator.Play("ItemGetTip");
    }
}
