using MFarm.Inventory;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace MFarm.Task
{
    public class TaskImage : MonoBehaviour //调用在TaskImage预制体上
    {
        private TextMeshProUGUI taskDes, taskDay, taskNum;
        public int taskID;
        public TaskData_SO taskData;
        //此预制体的TaskDetail
        public TaskDetails taskDetail;
        public InventoryBag_SO playerBag;
        //任务剩余天数
        public int taskRemainingDay;
        //第一次的显示
        public bool isFristSet;
        
        private void OnEnable()
        {
            if (isFristSet == false)
            {
                CheckTaskItemNum();
                taskDay.text = (taskRemainingDay.ToString() + "天");
                //扣除任务天数
                if (taskRemainingDay <= 0)
                {
                    TaskManager.Instance.DeleteTaskDetailsInPlayerAcceptTaskDetails(taskID);
                }
                if (taskDetail.canDestory)
                {
                    Destroy(gameObject);
                }
            }
        }
        /// 设置Task Image的显示内容
        /// </summary>
        /// <param name="currentTaskDetails"></param>
        public void SetTaskImageContent()
        {
            if(taskDetail.taskID == 0)
            {
                taskDetail = taskData.GetTaskDetails(taskID);
            }
            taskDes = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            taskDay = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            taskNum = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            CheckTaskItemNum();
            taskDes.text = taskDetail.taskDescriptionInPlayerTaskBar;
            taskRemainingDay = taskDetail.finishDay;
            taskDay.text = (taskRemainingDay.ToString() + "天");
        }
        /// <summary>
        /// 显示任务物品数量
        /// </summary>
        public void CheckTaskItemNum()
        {

            //每次开启玩家任务面板都获取一次InventoryUI上的playerSlot
            var inventoryUI = GameObject.FindWithTag("InventoryUI").gameObject.GetComponent<InventoryUI>();
            //判断玩家背包是否有任务物品
            if (InventoryManager.Instance.GetItemIndexInBag(taskDetail.itemID, InventoryManager.Instance.playerBag.itemList) == -1)
            {
                taskNum.text = ("0/" + taskDetail.finishNum.ToString());
            }
            //已有任务物品就显示数量
            else
            {
                for (int i = 0; i < inventoryUI.playerSlots.Length; i++)
                {
                    if (inventoryUI.playerSlots[i].itemDetails != null)
                    {
                        if (inventoryUI.playerSlots[i].itemDetails.itemID == taskDetail.itemID)
                        {
                            taskNum.text = (inventoryUI.playerSlots[i].itemAmount + "/" + taskDetail.finishNum.ToString());
                            //当获取数量大于等于任务所需数量，修改任务状态
                            if(inventoryUI.playerSlots[i].itemAmount >= taskDetail.finishNum)
                            {
                                taskDetail.isFinshing = true;
                            }
                        }
                    }
                }
            }
        }
       
    }
}
    



