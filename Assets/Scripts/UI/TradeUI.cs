using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
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
        private ItemDetails item;
        //是否是交易
        private bool isSellTrade;
        //是否是直接拖到出售箱
        private bool isToSellBox;
        //交易数量
        private int tradeAmount;
        //拥有的最大物品数
        private int maxAmount;
        //全部添加按钮
        public Button allIncreaseButton;
        //背包的slotindex和出售箱的index
        private int startIndex, endIndex;
        private InventoryLocation startLocation, endLocation;
        private void Awake()
        {
            //点击按钮触发其中的方法
            allIncreaseButton.onClick.AddListener(ClickAllIncreaseButton);
            cancelButton.onClick.AddListener(CancelTrade);
            submitButton.onClick.AddListener(ClickSubmitButton);
           
        }
        /// <summary>
        /// 设置TradeUI显示详情
        /// </summary>
        /// <param name="item"></param>
        /// <param name="isSell"></param>
        public void SetupTradeUI(ItemDetails item, bool isSell,int amount,int index1,int index2,InventoryLocation location1,InventoryLocation location2,bool toSellBox)
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
            isToSellBox = toSellBox;
        }
        /// <summary>
        /// 点击交易提交按钮
        /// </summary>
        private void ClickSubmitButton() 
        {
            //将tradeAmount输入的文本数字转换为Int
            //var amount = Convert.ToInt32(tradeAmountText.text);
            InventoryManager.Instance.TradeItem(item, tradeAmount, isSellTrade,startIndex,endIndex, startLocation,endLocation, isToSellBox);
            //交易结束后关闭TradeUI;
            CancelTrade();
        }
        private void CancelTrade()
        {
            this.gameObject.SetActive(false);
        }
        /// <summary>
        /// 点击增加按钮,调用在IncreaseButton按钮上
        /// </summary>
        public void ClickIncreaseButton()
        {
            if(tradeAmount< maxAmount)
            {
                tradeAmount++;
            }
            tradeAmountText.text = tradeAmount.ToString();
        }
        /// <summary>
        /// 点击减少按钮，调用在DecreaseButton按钮上
        /// </summary>
        public void ClickDecreaseButton()
        {
            if (tradeAmount > 1)
            {
                tradeAmount--;
            }
            tradeAmountText.text = tradeAmount.ToString();
        }
        /// <summary>
        /// 双击增加10个数量，调用在调用在IncreaseButton按钮上
        /// </summary>
        public void DoubleClickInCreaseButton()
        {
            if(tradeAmount < maxAmount)
            {
                if (maxAmount - tradeAmount >= 10)
                {
                    tradeAmount += 10;
                }
                else
                {
                    tradeAmount += maxAmount - tradeAmount;
                }
            }
            tradeAmountText.text = tradeAmount.ToString();
        }
        /// <summary>
        /// 双击减少10个数量，调用在调用在DecreaseButton按钮上
        /// </summary>
        public void DoubleClickDeCreaseButton()
        {
            if (tradeAmount > 1)
            {
                Debug.Log(tradeAmount - 10);
                if (tradeAmount - 10 > 0)
                {
                    tradeAmount -= 10;
                }
                else
                {
                    tradeAmount = 1;
                }
            }
            tradeAmountText.text = tradeAmount.ToString();
        }
        /// <summary>
        /// 点击全部交易按钮
        /// </summary>
        public void ClickAllIncreaseButton()
        {
            tradeAmount = maxAmount;
            tradeAmountText.text = tradeAmount.ToString();
        }
    }
}

