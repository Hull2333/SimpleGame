using MFarm.Dialogue;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(DialogueController))]
public class DialoguaGiver : MonoBehaviour //调用在NPC上
{
    private DialogueController controller;
    private QuestData_OS currentQuest;
    //接受任务、执行任务中、领取奖励、完成任务后对话内容
    public DialogueData_OS startDialogue;
    public DialogueData_OS progressDialogue;
    public DialogueData_OS completeDialogue;
    public DialogueData_OS commitQuest1Dialogue;
    public DialogueData_OS commitQuest2Dialogue;
    public DialogueData_OS morning1Dialogue;
    public DialogueData_OS morning2Dialogue;
    public DialogueData_OS morning3Dialogue;
    #region 获取玩家接受任务的状态
    public bool IsStarted
    {
        get
        {
            if (QuestManager.Instance.HaveQuest(currentQuest))
            {
                return QuestManager.Instance.GetTask(currentQuest).IsStarted;
            }
            else
            {
                return false;
            }
        }
    }
    public bool IsComplete
    {
        get
        {
            if (QuestManager.Instance.HaveQuest(currentQuest))
            {
                return QuestManager.Instance.GetTask(currentQuest).IsComplete;
            }
            else
            {
                return false;
            }
        }
    }
    public bool IsFinish
    {
        get
        {
            if (QuestManager.Instance.HaveQuest(currentQuest))
            {
                return QuestManager.Instance.GetTask(currentQuest).IsFinshed;
            }
            else
            {
                return false;
            }
        }
    }
    #endregion
    private void Awake()
    {
        controller = GetComponent<DialogueController>();
    }
    private void Start()
    {
        currentQuest = controller.currentData.GetQuest();
    }

    private void Update()
    {
        //根据任务状态改变不同的对话
        //if (IsStarted)
        //{
        //    if (IsComplete)
        //    {
        //        controller.currentData = completeDialogue;
        //    }
        //    else
        //    {
        //        controller.currentData = progressDialogue;
        //    }
        //}
        //if (IsFinish)
        //{
        //    controller.currentData = finishiDialogue;
        //}
    }
    /// <summary>
    /// 设置与玩家的对话
    /// </summary>
    public void TalkWithPlayer()
    {
        //每天的第一次对话
        if (controller.isFristTalk)
        {
            controller.currentData = morning1Dialogue;
            controller.isFristTalk = false;
        }
        //不是今天的第一次对话
        //玩家当前有任务时，且任务与该NPC相关，设置对应的对话
        else
        {
            foreach(var task in QuestManager.Instance.tasks)
            {
                if(task.questData.questman == controller.NPCname)
                {
                    //根据当前任务进度设置对应的对话
                    if (task.questData.isComplete)
                    {
                        currentQuest = task.questData;
                        if(currentQuest.questName == "收集8个杂草")
                        {
                            controller.currentData = commitQuest1Dialogue;
                            return;
                        }
                        if(currentQuest.questName == "收集5根木头")
                        {
                            controller.currentData = commitQuest2Dialogue;
                            return;
                        }
                        
                    }
                }
            }
            //无任务或不相关的对话
            int talkIndex = Random.Range(0, 2);
            if(talkIndex == 0)
            {
                controller.currentData = morning2Dialogue;
            }
            else
            {
                controller.currentData = morning3Dialogue;
            }
        }
    }
    
}
