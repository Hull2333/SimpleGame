using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace MFarm.Inventory
{
    public class TradeUI : MonoBehaviour    //调用在TradeUI对象上
    {

        public Image itemIcon;
        public Text itemName;
        public TextMeshProUGUI tradeAmountText;
        public Button submitButton;
        public Button cancelButton;
        //增加减少选择物品数量
        public Button increaseButton;
        public Button decreaseButton;
        private ItemDetails item;
        private bool isSellTrade;
        //交易数量
        private int tradeAmount;
        //拥有的最大物品数
        private int maxAmount;
        //背包的slotindex和出售箱的index
        private int startIndex, endIndex;
        private InventoryLocation startLocation, endLocation;
        private void Awake()
        {
            //点击按钮触发其中的方法
            cancelButton.onClick.AddListener(CancelTrade);
            submitButton.onClick.AddListener(ClickSubmitButton);
            increaseButton.onClick.AddListener(ClickIncreaseButton);
            decreaseButton.onClick.AddListener(ClickDecreaseButton);
        }
        /// <summary>
        /// 设置TradeUI显示详情
        /// </summary>
        /// <param name="item"></param>
        /// <param name="isSell"></param>
        public void SetupTradeUI(ItemDetails item, bool isSell,int amount,int index1,int index2,InventoryLocation location1,InventoryLocation location2)
        {
            this.item = item;
            itemIcon.sprite = item.itemIcon;
            itemName.text = item.itemName;
            isSellTrade = isSell;
            tradeAmount = 1;
            maxAmount = amount;
            startIndex = index1;
            endIndex = index2;
            startLocation = location1;
            endLocation = location2;
            tradeAmountText.text = tradeAmount.ToString();
        }
        /// <summary>
        /// 点击交易提交按钮
        /// </summary>
        private void ClickSubmitButton() 
        {
            //将tradeAmount输入的文本数字转换为Int
            //var amount = Convert.ToInt32(tradeAmountText.text);
            InventoryManager.Instance.TradeItem(item, tradeAmount, isSellTrade,startIndex,endIndex, startLocation,endLocation);
            //交易结束后关闭TradeUI;
            CancelTrade();
        }
        private void CancelTrade()
        {
            this.gameObject.SetActive(false);
        }
        /// <summary>
        /// 点击增加按钮
        /// </summary>
        private void ClickIncreaseButton()
        {
            if(tradeAmount< maxAmount && tradeAmount < 99)
            {
                tradeAmount++;
            }
            tradeAmountText.text = tradeAmount.ToString();
        }
        /// <summary>
        /// 点击减少按钮
        /// </summary>
        private void ClickDecreaseButton()
        {
            if (tradeAmount > 1)
            {
                tradeAmount--;
            }
            tradeAmountText.text = tradeAmount.ToString();
        }
    }
}

