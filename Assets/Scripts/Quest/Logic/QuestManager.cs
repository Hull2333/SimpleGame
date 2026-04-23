using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;

public class QuestManager : Singleton<QuestManager> //调用在QuestManager对象上
{
    [System.Serializable]
    public class QuestTask
    {
        public QuestData_OS questData;
        public bool IsStarted { get { return questData.isStarted; }set { questData.isStarted = value; } }
        public bool IsComplete { get { return questData.isComplete; } set { questData.isComplete = value; } }
        public bool IsFinshed { get { return questData.isFinshed; } set { questData.isFinshed = value; } }
    }
    //玩家接受任务列表
    public List<QuestTask> tasks = new List<QuestTask>();
    [Header("公告栏任务列表")]
    //公告栏发布任务列表
    public List<QuestTask> bulletinBoardTaskList;
    [HideInInspector] public QuestTask bulletinBoardTask;
    //临时存储公告栏任务信息
    private QuestTask lastbulletinBoardTask;
    //上一个公告栏任务的序列号
    private int lastRandomNum;
    //刷新公告栏任务天数
    private int waitQuestDay;
    public void OnEnable()
    {
        EventHandler.UpdateQuestProgressEvent += OnUpdateQuestProgressEvent;
        EventHandler.GameDayEvent += OnGameDayEvent;
    }
    public void OnDisable()
    {
        EventHandler.UpdateQuestProgressEvent -= OnUpdateQuestProgressEvent;
        EventHandler.GameDayEvent -= OnGameDayEvent;
    }


    private void OnUpdateQuestProgressEvent(int itemID, int amount)
    {
        foreach(var task in tasks)
        {
            var matchTask = task.questData.questRequires.Find(r => r.ItemID == itemID);
            if (matchTask != null)
            {
                matchTask.currentAmount += amount;
            }
            task.questData.CheckQuestProgress();
        }
    }
    private void OnGameDayEvent(int arg1, Season season)
    {
        InstantiateBulletinBoardQuest();
        UpdateQuestDay();
    }
    /// <summary>
    /// 查找是否重复接受任务
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool HaveQuest(QuestData_OS data)
    {
        if (data != null)
        {
            //Linq，在tasks列表中查找与当前data相同的questName，有相同返回true
            return tasks.Any(q => q.questData.questName == data.questName);
        }
        else
        {
            return false;
        }
    }
    /// <summary>
    /// 查找接受任务列表中的任务
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public QuestTask GetTask(QuestData_OS data)
    {
        return tasks.Find(q => q.questData.questName == data.questName);
        
    }
    /// <summary>
    /// 随机生成公告栏任务
    /// </summary>
    /// <returns></returns>
    public void InstantiateBulletinBoardQuest()
    {
        //每天减少任务刷新时间
        waitQuestDay --;
        if(waitQuestDay <= 0)
        {
            //随机生成新任务的天数为4~5天
            waitQuestDay = Random.Range(4, 6);
            //BulletinBoardUI.Instance.isAccepted = false;
            int randomNum = Random.Range(0, bulletinBoardTaskList.Count);
            //不让这次发出的任务和上一次任务相同
            do
            {
                randomNum = Random.Range(0, bulletinBoardTaskList.Count);
            }
            while (randomNum == lastRandomNum);
            lastRandomNum = randomNum;
            for (int i = 0; i < bulletinBoardTaskList.Count; i++)
            {
                bulletinBoardTask = bulletinBoardTaskList[randomNum];
            }
          
        }
        else
        {
            bulletinBoardTask = null;
        }
       
    }
    /// <summary>
    /// 更新从公告栏接受任务中天数
    /// </summary>
    public void UpdateQuestDay()
    {
        for(int t = 0; t < tasks.Count; t++)
        {
            if (tasks[t].questData.questType == QuestType.BullteinBoard)
            {
                tasks[t].questData.currentQuestDay--;
                if (tasks[t].questData.currentQuestDay <= 0)
                {
                    tasks.Remove(tasks[t]);
                }
            }
        }
    }
}
