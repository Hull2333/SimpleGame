using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFarm.Inventory
{
    public class Box : MonoBehaviour    //调用在自己建造的BOX预制体上
    {
        //空的box数据
        public InventoryBag_SO boxBagTemplate;
        public InventoryBag_SO boxBagData;
        public GameObject mouseIcon;
        private bool canOpen = false;
        //箱子是否已经打开
        private bool isOpen;
        //箱子编号
        public int index;

        //OnEnable,该物品创建一次会执行一次和场景刷新时也会执行一次
        private void OnEnable()
        {
            if(boxBagData == null)
            {
                boxBagData = Instantiate(boxBagTemplate);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                canOpen = true;
                mouseIcon.SetActive(true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                canOpen = false;
                mouseIcon.SetActive(false);
            }
        }

        private void Update()
        {
            //玩家进入箱子互动范围且点击鼠标右键打开箱子
            if (!isOpen && canOpen && Input.GetMouseButtonDown(1))
            {
                //打开箱子
                EventHandler.CallBaseBagOpenEvent(SlotType.Box, boxBagData);
                isOpen = true;
            }
            if(!canOpen && isOpen)
            {
                //关闭箱子
                EventHandler.CallBaseBagCloseEvent(SlotType.Box, boxBagData);
                isOpen= false;
            }
            if(isOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                //关闭箱子
                EventHandler.CallBaseBagCloseEvent(SlotType.Box, boxBagData);
                isOpen = false;
            }
        }
        /// <summary>
        /// 初始化箱子编号
        /// </summary>
        public void InitBox(int boxIndex)
        {
            index = boxIndex;
            var key = this.name + index;
            //游戏中已有的箱子
            if (InventoryManager.Instance.GetBoxDataList(key) != null)
            {
                boxBagData.itemList = InventoryManager.Instance.GetBoxDataList(key);
            }
            //新建箱子
            else
            {
                InventoryManager.Instance.AddBoxDataDict(this);
            }
        }
    }
}

