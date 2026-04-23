using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFarm.Inventory
{
    [RequireComponent(typeof(SlotUI))]
    public class ActionBarButton : MonoBehaviour    //调用在Slot_Bag游戏对象不是预制体上
    {
        //键盘按钮1-0
        public KeyCode key;
        private SlotUI slotUI;
        //是否检测按键输入
        private bool canUse;

        private void Awake()
        {
            slotUI = GetComponent<SlotUI>();
        }

        private void OnEnable()
        {
            EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
        }

        private void OnDisable()
        {
            EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
        }
        /// <summary>
        /// 当打开交易界面时，禁止检测按键输入
        /// </summary>
        /// <param name="gameState"></param>
        private void OnUpdateGameStateEvent(GameState gameState)
        {
            canUse = gameState == GameState.Gameplay;
        }

        private void Update()
        {
            if (Input.GetKeyDown(key) && canUse)
            {
                if(slotUI.itemDetails!=null)
                {
                    slotUI.isSelected = !slotUI.isSelected;
                    if(slotUI.isSelected )
                    {
                        slotUI.inventoryUI.UpdateSlotHighlight(slotUI.slotIndex);
                    }
                    else
                    {
                        slotUI.inventoryUI.UpdateSlotHighlight(-1);
                    }
                    EventHandler.CallItemSelectEvent(slotUI.itemDetails,slotUI.isSelected);
                }
            }
        }
    }
}

