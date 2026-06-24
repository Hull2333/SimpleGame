using MFarm.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueOptionUI : MonoBehaviour //调用预制体OptionButton
{
    public Text optionText;
    private Button thisButton;
    private DialoguePieces currentPiece;
    //下一条对话
    private string nextPieceID;
    private InventoryBag_SO bag_SO;
    private BuildingBagData_SO buildingBag;
    private AnimalBagData_SO animalBag;
    private int friendlinessValue;
    private DialogueOptionType optionType;
    private void Awake()
    {
        thisButton = GetComponent<Button>();
        thisButton.onClick.AddListener(OnOptionClicked);
    }
    /// <summary>
    /// 设置optionButton的要素
    /// </summary>
    /// <param name="piece"></param>
    /// <param name="option"></param>
    public void UpdateOption(DialoguePieces piece,DialogueOption option)
    {
        currentPiece = piece;
        optionText.text = option.text;
        nextPieceID = option.targetID;
        optionType = option.optionType;
        bag_SO = option.bag_SO;
        buildingBag = option.buildingBag;
        friendlinessValue = option.friendlinessValue;
        animalBag = option.animalBag;
    }
    /// <summary>
    /// 当optionButton点击时
    /// </summary>
    public void OnOptionClicked()
    {
        if(currentPiece.quest != null)
        {
            var newTask = new QuestManager.QuestTask
            {
                questData = Instantiate(currentPiece.quest)
            };
            //添加到任务列表
            //是否已经有相同任务
            if (QuestManager.Instance.HaveQuest(newTask.questData))
            {
                //判断是否完成给予奖励
                if (QuestManager.Instance.GetTask(newTask.questData).IsComplete)
                {
                    newTask.questData.GiveRewards();
                    if (newTask.questData.questType == QuestType.Plot)
                    {
                        //将任务状态改为IsFinish
                        QuestManager.Instance.GetTask(newTask.questData).IsFinshed = true;
                    }
                    else
                    {
                        //如果接受任务列表中任务类型是公告栏，就移除接受任务列表不需要改变状态
                        for (int i = 0; i < QuestManager.Instance.tasks.Count; i++)
                        {
                            if (newTask.questData.questName == QuestManager.Instance.tasks[i].questData.questName)
                            {
                                QuestManager.Instance.tasks.Remove(QuestManager.Instance.tasks[i]);
                            }
                        }

                    }
                }
                else
                {
                    //将任务添加到接受任务列表中并修改任务状态
                    QuestManager.Instance.tasks.Add(newTask);
                    QuestManager.Instance.GetTask(newTask.questData).IsStarted = true;
                    foreach (var requireItem in newTask.questData.RequireTargetItemID())
                    {
                        InventoryManager.Instance.CheckQuestItemInBag(requireItem);
                    }
                }
            }

        }
        //有对话选项触发NPC背包
        if (optionType == DialogueOptionType.ItemShop)
        {
            EventHandler.CallBaseBagOpenEvent(SlotType.Shop, bag_SO);
            DialogueUI.Instance.QuitDialogueUI();
            EventHandler.CallUpdateGameStateEvent(GameState.Pause);
            return;
        }
        //有对话选项触发Build背包
        if (optionType == DialogueOptionType.BuildShop)
        {
            EventHandler.CallOpenBuildShopEvent(buildingBag);
            DialogueUI.Instance.QuitDialogueUI();
            EventHandler.CallUpdateGameStateEvent(GameState.Pause);
            return;
        }
        //有对话选项触发Animal背包
        if(optionType == DialogueOptionType.AnimalShop)
        {
            EventHandler.CallOpenAnimalShopEvent(animalBag);
            DialogueUI.Instance.QuitDialogueUI();
            EventHandler.CallUpdateGameStateEvent(GameState.Pause);
            return;
        }
        //点击此选项后没有对话那就结束对话
        if (nextPieceID == null || nextPieceID == "0")
        {
            DialogueUI.Instance.QuitDialogueUI();
            //增加好感度
            EventHandler.CallIncreaseFriendliness(friendlinessValue, currentPiece.name.ToString());
            return;
        }
        //有对话就显示对应的piece
        else
        {
            //增加好感度
            EventHandler.CallIncreaseFriendliness(friendlinessValue,currentPiece.name.ToString());
            if (DialogueUI.Instance.currentData.dialogueIndex[nextPieceID].shakeImage)
            {
                DialogueUI.Instance.ShakePartraitImage();
            }
            DialogueUI.Instance.UpdateMainDialogue(DialogueUI.Instance.currentData.dialogueIndex[nextPieceID]);
        }
    }
}
