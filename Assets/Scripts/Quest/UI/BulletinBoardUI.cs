using MFarm.Inventory;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static QuestManager;

public class BulletinBoardUI : Singleton<BulletinBoardUI> //调用在BulletinBoardUI对象上
{
    public GameObject bulletinBoardPanel;
    public Button quitButton;
    public Button acceptButton;
    public TextMeshProUGUI bulletinBoardDes;
    public TextMeshProUGUI NPCname;
    public TextMeshProUGUI questDay;
    public GameObject IngImage;
    public QuestTask currentQuest;
    //已接受该任务
    public bool isAccepted;
    /// <summary>
    /// 打开公告栏
    /// </summary>
    public void OpenBulletinBoardUI()
    {
        bulletinBoardPanel.SetActive(true);
        //时间暂停
        EventHandler.CallUpdateGameStateEvent(GameState.Pause);
        if (QuestManager.Instance.bulletinBoardTask != null)
        {
            currentQuest = QuestManager.Instance.bulletinBoardTask;
        }
        bulletinBoardDes.text = currentQuest.questData.description;
        NPCname.text = currentQuest.questData.questman.ToString();
        questDay.text = currentQuest.questData.questDay.ToString() + " 天";
        //改变公告栏显示状态
        if (isAccepted)
        {
            IngImage.gameObject.SetActive(true);
            acceptButton.gameObject.SetActive(false);
        }
        else
        {
            IngImage.gameObject.SetActive(false);
            acceptButton.gameObject.SetActive(true);
        }
        
    }
    /// <summary>
    /// 关闭公告栏
    /// </summary>
    public void QuitBulletinBoardUI()
    {
        bulletinBoardPanel.SetActive(false);
        EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
    }
    /// <summary>
    /// 点击接受任务按钮接受任务
    /// </summary>
    public void AcceptQuest()
    {
        //添加到任务列表
        //是否已经有相同任务
        if (QuestManager.Instance.HaveQuest(currentQuest.questData))
        {
            //判断是否完成给予奖励
            if (QuestManager.Instance.GetTask(currentQuest.questData).IsComplete)
            {
                currentQuest.questData.GiveRewards();
                //将任务状态改为IsFinish
                QuestManager.Instance.GetTask(currentQuest.questData).IsFinshed = true;
            }
        }
        else
        {
            //将任务添加到接受任务列表中并修改任务状态
            QuestManager.Instance.tasks.Add(currentQuest);
            QuestManager.Instance.GetTask(currentQuest.questData).IsStarted = true;
            //设置任务天数
            QuestManager.Instance.GetTask(currentQuest.questData).questData.currentQuestDay = QuestManager.Instance.GetTask(currentQuest.questData).questData.questDay;
            foreach (var requireItem in currentQuest.questData.RequireTargetItemID())
            {
                InventoryManager.Instance.CheckQuestItemInBag(requireItem);
            }
        }
        isAccepted = true;
        IngImage.gameObject.SetActive(true);
        acceptButton.gameObject.SetActive(false);
    }
    /// <summary>
    /// 更新公告栏显示状态
    /// </summary>
    public void UpdateIngImage()
    {
        if (QuestManager.Instance.HaveQuest(currentQuest.questData))
        {
            IngImage.gameObject.SetActive(true);
            acceptButton.gameObject.SetActive(false);
        }
        else
        {
            IngImage.gameObject.SetActive(false);
            acceptButton.gameObject.SetActive(true);
        }
    }
}



