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
        public TextMeshProUGUI currentTradeValueText;
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
        //当前这个物品交易金额
        private int currentTradeValue; 
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
            if (isSell)
            {
                maxAmount = amount;
                currentTradeValue = (int)(item.itemPrice * item.sellPercentage * 1);
                currentTradeValueText.text = currentTradeValue.ToString();
            }
            else
            {
                maxAmount = 99;
                currentTradeValue = (int)(item.itemPrice);
                currentTradeValueText.text = currentTradeValue.ToString();
            }
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
            transform.parent.gameObject.SetActive(false);
        }
        /// <summary>
        /// 点击增加按钮,调用在IncreaseButton按钮上
        /// </summary>
        public void ClickIncreaseButton()
        {
            //卖
            if (isSellTrade)
            {
                if (tradeAmount < maxAmount)
                {
                    tradeAmount++;
                }
                tradeAmountText.text = tradeAmount.ToString();
                currentTradeValue = (int)(item.itemPrice * item.sellPercentage * tradeAmount);
                currentTradeValueText.text = currentTradeValue.ToString();
            }
            //买
            else
            {
                if (item.itemPrice * (tradeAmount + 1) <= InventoryManager.Instance.playerMoney && tradeAmount < maxAmount)
                {
                    tradeAmount++;
                }
                tradeAmountText.text = tradeAmount.ToString();
                currentTradeValue = (int)(item.itemPrice * tradeAmount);
                currentTradeValueText.text = currentTradeValue.ToString();
            }
        }
        /// <summary>
        /// 点击减少按钮，调用在DecreaseButton按钮上
        /// </summary>
        public void ClickDecreaseButton()
        {
            //卖
            if (isSellTrade)
            {
                if (tradeAmount > 1)
                {
                    tradeAmount--;
                }
                tradeAmountText.text = tradeAmount.ToString();
                currentTradeValue = (int)(item.itemPrice * item.sellPercentage * tradeAmount);
                currentTradeValueText.text = currentTradeValue.ToString();
            }
            //买
            else
            {
                if (tradeAmount > 1)
                {
                    tradeAmount--;
                }
                tradeAmountText.text = tradeAmount.ToString();
                currentTradeValue = (int)(item.itemPrice * tradeAmount);
                currentTradeValueText.text = currentTradeValue.ToString();
            }
        }
        /// <summary>
        /// 双击增加10个数量，调用在调用在IncreaseButton按钮上
        /// </summary>
        public void DoubleClickInCreaseButton()
        {
            //卖
            if (isSellTrade)
            {
                if (tradeAmount < maxAmount)
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
                currentTradeValue = (int)(item.itemPrice * item.sellPercentage * tradeAmount);
                currentTradeValueText.text = currentTradeValue.ToString();
            }
            //买
            else
            {
                if(tradeAmount < 99)
                {
                    if (item.itemPrice * (tradeAmount + 10) <= InventoryManager.Instance.playerMoney)
                    {
                        if(tradeAmount + 10 < 99)
                        {
                            tradeAmount += 10;
                        }
                        else
                        {
                            tradeAmount = 99;
                        }
                    }
                    else
                    {
                        int gap = item.itemPrice * (tradeAmount + 10) - InventoryManager.Instance.playerMoney;
                        int gapAmount = gap / item.itemPrice;
                        if(tradeAmount + gapAmount < 99)
                        {
                            tradeAmount += gapAmount;
                        }
                        else
                        {
                            tradeAmount = 99;
                        }
                        
                    }
                }
                tradeAmountText.text = tradeAmount.ToString();
                currentTradeValue = (int)(item.itemPrice * tradeAmount);
                currentTradeValueText.text = currentTradeValue.ToString();
            }
           
        }
        /// <summary>
        /// 双击减少10个数量，调用在调用在DecreaseButton按钮上
        /// </summary>
        public void DoubleClickDeCreaseButton()
        {
            if (tradeAmount > 1)
            {
                if (tradeAmount - 10 > 0)
                {
                    tradeAmount -= 10;
                }
                else
                {
                    tradeAmount = 1;
                }
            }
            if (isSellTrade)
            {
                tradeAmountText.text = tradeAmount.ToString();
                currentTradeValue = (int)(item.itemPrice * item.sellPercentage * tradeAmount);
                currentTradeValueText.text = currentTradeValue.ToString();
            }
            else
            {
                tradeAmountText.text = tradeAmount.ToString();
                currentTradeValue = (int)(item.itemPrice * tradeAmount);
                currentTradeValueText.text = currentTradeValue.ToString();
            }

        }
        /// <summary>
        /// 点击全部交易按钮
        /// </summary>
        public void ClickAllIncreaseButton()
        {
            if (isSellTrade)
            {
                tradeAmount = maxAmount;
                tradeAmountText.text = tradeAmount.ToString();
                currentTradeValue = (int)(item.itemPrice * item.sellPercentage * tradeAmount);
                currentTradeValueText.text = currentTradeValue.ToString();
            }
            else
            {
                if(tradeAmount < 99)
                {
                    if (item.itemPrice * maxAmount <= InventoryManager.Instance.playerMoney)
                    {
                        tradeAmount = maxAmount;
                        tradeAmountText.text = tradeAmount.ToString();
                        currentTradeValue = (int)(item.itemPrice * tradeAmount);
                        currentTradeValueText.text = currentTradeValue.ToString();
                    }
                    else
                    {
                        tradeAmount = InventoryManager.Instance.playerMoney / item.itemPrice;
                        tradeAmountText.text = tradeAmount.ToString();
                        currentTradeValue = (int)(item.itemPrice * tradeAmount);
                        currentTradeValueText.text = currentTradeValue.ToString();
                    }
                }
               
            }
          
        }
    }
}

