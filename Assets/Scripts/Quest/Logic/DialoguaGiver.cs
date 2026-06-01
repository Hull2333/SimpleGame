using MFarm.Dialogue;
using MFarm.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(DialogueController))]
public class DialoguaGiver : MonoBehaviour //调用在NPC上
{
    //当前该NPC的好感度
    public float currentFriendliness;
    //喜欢的物品ID
    public List<int> favoriteItemIDList;
    //讨厌的物品ID
    public List<int> hatefulItemIDList;
    private DialogueController controller;
    private QuestData_OS currentQuest;
    [Header("任务相关对话")]
    //接受任务、执行任务中、领取奖励、完成任务后对话内容
    public DialogueData_OS startDialogue;
    public DialogueData_OS progressDialogue;
    public DialogueData_OS completeDialogue;
    public DialogueData_OS commitQuest1Dialogue;
    public DialogueData_OS commitQuest2Dialogue;
    [Header("送礼物相关对话")]
    public DialogueData_OS favorite1ItemDialogue;
    public DialogueData_OS favorite2ItemDialogue;
    public DialogueData_OS normal1ItemDialogue;
    public DialogueData_OS normal2ItemDialogue;
    public DialogueData_OS hateful1ItemDialogue;
    public DialogueData_OS hateful2ItemDialogue;
    public DialogueData_OS refuseGiftDialogue;
    [Header("日常对话")]
    public DialogueData_OS dailyfirst1Dialogue;
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

    private void OnEnable()
    {
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        EventHandler.IncreaseFriendliness += OnIncreaseFriendliness;
    }
    private void OnDisable()
    {
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        EventHandler.IncreaseFriendliness -= OnIncreaseFriendliness;
    }

   

    private void Awake()
    {
        controller = GetComponent<DialogueController>();
    }
    private void Start()
    {
        currentQuest = controller.currentData.GetQuest();
    }
    private void OnStartNewGameEvent(int obj)
    {
        currentFriendliness = 0;
    }
    private void OnIncreaseFriendliness(float Value,string name)
    {
        if (gameObject.name == name)
        {
            currentFriendliness += Value;
        }
        EventHandler.CallUpdateNPCFriendlinessUIPanel(gameObject.name, currentFriendliness);
    }
    /// <summary>
    /// 设置与玩家的对话
    /// </summary>
    public void TalkWithPlayer(int itemID)
    {
        //每天的第一次对话
        if (controller.isFristTalk)
        {
            //每天的第一次对话可以加好感度
            currentFriendliness += 0.5f;
            EventHandler.CallIncreaseFriendliness(currentFriendliness, gameObject.name);
            controller.isFristTalk = false;
            //玩家有拿物品
            if (itemID != 0)
            {
                SetFriendlinessDialogue(itemID);
            }
            //没送礼物
            else
            {
                controller.currentData = dailyfirst1Dialogue;
            }
           
        }
        //不是今天的第一次对话
        else
        {
            if (itemID != 0)
            {
                SetFriendlinessDialogue(itemID);
            }
            //没送礼物
            else
            {
                //玩家当前有任务时，且任务与该NPC相关，设置对应的对话
                foreach (var task in QuestManager.Instance.tasks)
                {
                    if (task.questData.questman == controller.NPCname)
                    {
                        //根据当前任务进度设置对应的对话
                        if (task.questData.isComplete)
                        {
                            currentQuest = task.questData;
                            if (currentQuest.questName == "收集8个杂草")
                            {
                                controller.currentData = commitQuest1Dialogue;
                                return;
                            }
                            if (currentQuest.questName == "收集5根木头")
                            {
                                controller.currentData = commitQuest2Dialogue;
                                return;
                            }

                        }
                    }
                }
                int talkIndex = Random.Range(0, 2);
                if (talkIndex == 0)
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
    /// <summary>
    /// 遍历NPC喜欢和讨厌的物品ID
    /// </summary>
    /// <param name="ID"></param>
    /// <returns>喜欢返回1，不喜欢返回-1，其他返回0</returns>
    private int CheckNPCItemList(int ID)
    {
        foreach (int itemID in favoriteItemIDList)
        {
            if (itemID == ID)
            {
                return 1;
            }
        }
        foreach (int itemID in hatefulItemIDList)
        {
            if (itemID == ID)
            {
                return -1;
            }
        }
        return 0;
    }
    /// <summary>
    /// 设置送礼物时的对话和好感度的上升
    /// </summary>
    /// <param name="itemID"></param>
    private void SetFriendlinessDialogue(int itemID)
    {
        //拒收礼物对话
        if (!controller.isTodayNotGetGift)
        {
            controller.currentData = refuseGiftDialogue;
            return;
        }
        //玩家送喜欢礼物的对话
        if (CheckNPCItemList(itemID) == 1)
        {
            //玩家送礼物品减一
            InventoryManager.Instance.RemoveItem(itemID, 1);
            if (currentFriendliness < 30 && controller.isTodayNotGetGift)
            {
                currentFriendliness += 2f;
                controller.isTodayNotGetGift = false;
            }
            int talkIndex = Random.Range(0, 2);
            if (talkIndex == 0)
            {
                controller.currentData = favorite1ItemDialogue;
            }
            else
            {
                controller.currentData = favorite2ItemDialogue;
            }
            return;
        }
        //玩家送其他礼物的对话
        if (CheckNPCItemList(itemID) == 0)
        {
            if (currentFriendliness < 30 && controller.isTodayNotGetGift)
            {
                currentFriendliness += 1f;
                controller.isTodayNotGetGift = false;
            }
            int talkIndex = Random.Range(0, 2);
            if (talkIndex == 0)
            {
                controller.currentData = normal1ItemDialogue;
            }
            else
            {
                controller.currentData = normal2ItemDialogue;
            }
            return;
        }
        //玩家送讨厌礼物的对话
        if (CheckNPCItemList(itemID) == -1)
        {
            int talkIndex = Random.Range(0, 2);
            if (talkIndex == 0)
            {
                controller.currentData = hateful1ItemDialogue;
            }
            else
            {
                controller.currentData = hateful2ItemDialogue;
            }
            return;
        }
        EventHandler.CallUpdateNPCFriendlinessUIPanel(gameObject.name, currentFriendliness);
    }
}
