using MFarm.Map;
using MFarm.Save;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
namespace MFarm.Task
{
    public class TaskManager : Singleton<TaskManager> //调用在TaskManager对象上
    {
        public Transform taskPanel;
        public TaskData_SO taskData;
        //当前接受的任务
        [HideInInspector] public TaskDetails currentTaskDetails;
        //公告栏面板
        public GameObject bulletinBoardPanel;
        public GameObject playerTaskPanel;
        //随机获取公告栏任务ID、TaskDetail
        private int randomTaskID;
        [HideInInspector] public TaskDetails randomTaskDetail;
        //玩家接受任务列表
        [HideInInspector] public List<TaskDetails> playerAcceptTaskDetails = new List<TaskDetails>();
        //玩家面板接受任务预制体
        public GameObject taskImagePrefab;
        //接受任务预制体数组
        public GameObject[] playerTaskImages = new GameObject[20];
        //任务刷新天数
        private int taskFreshDay;

        //减少一天任务时间
        public bool canDeductTaskDay = false;
        public void OnEnable()
        {
            EventHandler.TaskStartEvent += OnTaskStartEvent;
            EventHandler.GameDayEvent += OnGameDayEvent;
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        }

        public void OnDisable()
        {
            EventHandler.TaskStartEvent -= OnTaskStartEvent;
            EventHandler.GameDayEvent -= OnGameDayEvent;
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        }

        private void OnStartNewGameEvent(int obj)
        {
            //清空玩家接受任务列表
            playerAcceptTaskDetails.Clear();
            //重置任务列表
            for(int t = 0;t< taskData.taskDetailsList.Count; t++)
            {
                taskData.taskDetailsList[t].isInstantiate = false;
            }
            
        }

        /// <summary>
        /// 生成TaskImage
        /// </summary>
        public void OnTaskStartEvent()
        {
            for (int i = 0; i < playerAcceptTaskDetails.Count; i++)
            {
                if (playerAcceptTaskDetails[i].isInstantiate == false)
                {
                    var taskAcceptImage = Instantiate(taskImagePrefab, taskPanel);
                    taskAcceptImage.GetComponent<TaskImage>().taskID = playerAcceptTaskDetails[i].taskID;
                    taskAcceptImage.GetComponent<TaskImage>().SetTaskImageContent();
                    playerAcceptTaskDetails[i].isInstantiate = true;
                    taskAcceptImage.GetComponent<TaskImage>().isFristSet = false;
                }
            }
            //查找已生成的接受任务预制体
            playerTaskImages = GameObject.FindGameObjectsWithTag("TaskImage");

        }

        private void OnGameDayEvent(int arg1, Season season)
        {
            ///公告栏任务刷新时间为3到4天
            if(taskFreshDay == 0)
            {
                randomTaskID = UnityEngine.Random.Range(5001, 5004);
                randomTaskDetail = taskData.GetTaskDetails(randomTaskID);
                taskFreshDay = UnityEngine.Random.Range(3, 5);
            }
            else
            {
                taskFreshDay --;
            }
            //if(playerAcceptTaskDetails.Count > 0)
            //{
            //    canDeductTaskDay = true;
            //}
            playerTaskPanel.SetActive(true);
            StartCoroutine(ReductTaskDay());
        }
        /// <summary>
        /// 扣除当前接受任务的天数
        /// </summary>
        /// <returns></returns>
        private IEnumerator ReductTaskDay()
        {
            
            if (playerTaskImages.Length > 0)
            {
                for (int i = 0; i < playerTaskImages.Length; i++)
                {
                    if(playerTaskImages[i] != null)
                    {
                        playerTaskImages[i].GetComponent<TaskImage>().taskRemainingDay--;
                    }
                }
            }
            playerTaskPanel.SetActive(false);
            //等待上面的代码完成
            yield return null;
            
        }
        /// <summary>
        /// 关闭任务公告栏面板
        /// </summary>
        public void QuitBulletinBoard()
        {
            bulletinBoardPanel.SetActive(false);
            
        }
        /// <summary>
        /// 打开公告栏面板
        /// </summary>
        public void OpenBulletinBoard()
        {
            bulletinBoardPanel.SetActive(true);
        }
        /// <summary>
        /// 查找PlayerAcceptTaskDetails列表中是否存在该任务
        /// </summary>
        /// <param name="task"></param>
        /// <returns>不存在返回-1</returns>
        public int FindPlayerAcceptTaskDetails(TaskDetails task)
        {
            for(int i = 0; i < playerAcceptTaskDetails.Count; i++)
            {
                if(playerAcceptTaskDetails[i].taskID == task.taskID)
                {
                    return i;
                }
            }
            return -1;
        }
        /// <summary>
        /// 在接受任务列表移除任务
        /// </summary>
        /// <param name="ID"></param>
        public void DeleteTaskDetailsInPlayerAcceptTaskDetails(int ID)
        {
            for(int i = 0;i< playerAcceptTaskDetails.Count; i++)
            {
                if (playerAcceptTaskDetails[i].taskID == ID)
                {
                    playerAcceptTaskDetails[i].isInstantiate = false;
                    playerAcceptTaskDetails[i].canDestory = true;
                    playerAcceptTaskDetails.Remove(playerAcceptTaskDetails[i]);
                    
                }
            }
        }
    }
}

