using MFarm.Inventory;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace MFarm.Task
{
    public class Task : MonoBehaviour//IPointerEnterHandler, IPointerExitHandler //调用在TaskManager下的TaskPanel上
    {
        private TaskDetails currentTaskDetail;
        public TextMeshProUGUI taskDes;
        public TextMeshProUGUI taskDay;
        public TextMeshProUGUI taskManName;
        public GameObject acceptButton;
        public GameObject ingImage;
        //上一个任务ID
        private int lastTaskID;


        public void OnEnable()
        {
            ShowTaskInBulletinBoard();
           
        }
        /// <summary>
        /// 在公告栏上显示任务信息
        /// </summary>
        private void ShowTaskInBulletinBoard()
        {
            if(TaskManager.Instance.randomTaskDetail != null)
            {
                currentTaskDetail = TaskManager.Instance.randomTaskDetail;
                SetTaskDes();
            }
        }

        /// <summary>
        /// 设置公告栏任务显示详情
        /// </summary>
        private void SetTaskDes()
        {
            taskDes.text = currentTaskDetail.taskDescriptionInBulletinBoard;
            taskDay.text = ("时限:" + currentTaskDetail.finishDay.ToString());
            taskManName.text = ("--"+ currentTaskDetail.taskName);
            //显示接受任务按钮
            if(TaskManager.Instance.FindPlayerAcceptTaskDetails(currentTaskDetail) == -1)
            {
                acceptButton.SetActive(true);
                ingImage.SetActive(false);
            }
        }
        /// <summary>
        /// 传输当前TaskDetail,调用在接受委托按钮上
        /// </summary>
        public void GiveTaskDetail()
        {
            if (currentTaskDetail != null)
            {
                TaskManager.Instance.currentTaskDetails = currentTaskDetail;
                //玩家没有接受任何任务，直接添加到玩家接受任务列表
                if (TaskManager.Instance.playerAcceptTaskDetails.Count <= 0)
                {
                    TaskManager.Instance.playerAcceptTaskDetails.Add(currentTaskDetail);
                    currentTaskDetail.isFinshing = false;
                }
                //已有接受任务，判断是否为一样的任务，不一样再添加到任务列表
                else
                {
                    for(int i = 0;i < TaskManager.Instance.playerAcceptTaskDetails.Count; i++)
                    {
                        if (TaskManager.Instance.playerAcceptTaskDetails[i].taskID != currentTaskDetail.taskID)
                        {
                            TaskManager.Instance.playerAcceptTaskDetails.Add(currentTaskDetail);
                            currentTaskDetail.isFinshing = false;
                            currentTaskDetail.canDestory = false;
                        }
                       
                    }
                }
            }
            acceptButton.SetActive(false);
            ingImage.SetActive(true);
            
        }
    }
}

