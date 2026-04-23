using MFarm.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestUI : Singleton<QuestUI> //调用在InventoryUI上
{
    [Header("Elements")]
    public GameObject questPanel;
    [Header("Quest Name")]
    //任务按钮列表
    public RectTransform[] questListTransforms;
    //任务信息按钮
    public QuestNameButton questNameButton;
    private QuestNameButton currentQuestNameButton;
    private int currentQuestPageIndex;
    private int maxQuestPageIndex;
    public Text currentPageText;
    public Text maxPageText;
    public GameObject noThingToDo;
    //任务上下页
    public Button questPreviousPage, questNextPage;
    [Header("Text Content")]
    public GameObject questRightPanel;
    public Text questContentText;
    [Header("Requirement")]
    public RectTransform requireTransform;
    public QuestRequirement requirement;
    [Header("Reward Panel")]
    public RectTransform rewardTransform;
    public SlotUI rewardSlotUI;
    [Header("任务天数")]
    public Text questDay;
   /// <summary>
   /// 生成并显示玩家任务按钮列表
   /// </summary>
    public void SetUpQuestList()
    {
        //清空任务详细信息文本
        questContentText.text = string.Empty;
        //清空任务面板左边的任务按钮
        foreach (Transform questListTransform in questListTransforms)
        {
            foreach (Transform item in questListTransform)
            {
                Destroy(item.gameObject);
            }
        }
        //清空任务面板右侧的奖励Slot
        foreach(Transform item in rewardTransform)
        {
            Destroy(item.gameObject);
        }
        //清空任务面板右侧的任务进程
        foreach (Transform item in requireTransform)
        {
            Destroy(item.gameObject);
        }
        //当没有接受任务时
        if(QuestManager.Instance.tasks.Count <= 0)
        {
            noThingToDo.SetActive(true);
        }
        else
        {
            noThingToDo.SetActive(false);
        }
        questRightPanel.SetActive(false);
        if(QuestManager.Instance.tasks.Count <= 5)
        {
            maxQuestPageIndex = 0;
        }
        if (QuestManager.Instance.tasks.Count > 5)
        {
            maxQuestPageIndex = 1;
        }
        if(QuestManager.Instance.tasks.Count > 10)
        {
            maxQuestPageIndex = 2;
        }
        maxPageText.text ="/  " + (maxQuestPageIndex + 1).ToString();
        //根据questListTransform下子物体的数量来重新生成新的已接受任务按钮
        foreach (var task in QuestManager.Instance.tasks)
        {
            if (questListTransforms[0].childCount <= 5)
            {
                currentQuestNameButton = Instantiate(questNameButton, questListTransforms[0]);
                currentQuestNameButton.SetUpNameButton(task.questData);
                currentQuestNameButton.questContentText = questContentText;
            }
            else
            {
                if (questListTransforms[1].childCount <= 5)
                {
                    currentQuestNameButton = Instantiate(questNameButton, questListTransforms[1]);
                    currentQuestNameButton.SetUpNameButton(task.questData);
                    currentQuestNameButton.questContentText = questContentText;
                }
                else
                {
                    currentQuestNameButton = Instantiate(questNameButton, questListTransforms[2]);
                    currentQuestNameButton.SetUpNameButton(task.questData);
                    currentQuestNameButton.questContentText = questContentText;
                }
               
            }
        }
        
    }
    /// <summary>
    /// 设置显示当前任务的所需物品
    /// </summary>
    /// <param name="questData"></param>
    public void SetUpRequireList(QuestData_OS questData)
    {
        foreach (Transform item in requireTransform)
        {
            Destroy(item.gameObject);
        }
        //当前任务的每一个所需物品
        foreach(var require in questData.questRequires)
        {
            var q = Instantiate(requirement, requireTransform);
            //显示所需物品名称
            string itemName = InventoryManager.Instance.GetItemDetails(require.ItemID).itemName;
            if (questData.isFinshed)
            {
                q.SetUpRequirement(itemName, true);
            }
            q.SetUpRequirement(itemName, require.requireAmount, require.currentAmount);
        }

    }
    /// <summary>
    /// 显示任务奖励物品
    /// </summary>
    /// <param name="ID"></param>
    /// <param name="amount"></param>
    public void SetUpRewardItem(int ID,int amount)
    {
        var itemDetails = InventoryManager.Instance.GetItemDetails(ID);
        var item = Instantiate(rewardSlotUI, rewardTransform);
        item.UpdateSlot(itemDetails, amount);
        item.GetComponent<Button>().interactable = false;
    }
    /// <summary>
    /// 显示当前任务的天数
    /// </summary>
    /// <param name="questData"></param>
    public void SetUpQuestDay(QuestData_OS questData)
    {
        questDay.text = questData.currentQuestDay.ToString() + " 天";
        //当天数不足2天时，显示红色
        if (questData.currentQuestDay <= 2)
        {
            questDay.color = new Color(0.915f, 0.263f, 0.263f, 1f);
            questDay.GetComponentInParent<Text>().color = new Color(0.915f, 0.263f, 0.263f, 1f);
        }
        else
        {
            questDay.color = Color.white;
            questDay.GetComponentInParent<Text>().color = Color.white;
        }
    }
    /// <summary>
    /// 控制任务上下页开关
    /// </summary>
    private void SwitchQuestPageButton()
    {
        if(currentQuestPageIndex <= 0)
        {
            questPreviousPage.interactable = false;
        }
        else
        {
            questPreviousPage.interactable = true;
        }
        if (currentQuestPageIndex >= maxQuestPageIndex)
        {
            questNextPage.interactable = false;
        }
        else
        {
            questNextPage.interactable = true;
        }
        currentPageText.text = (currentQuestPageIndex + 1).ToString();
    }
    /// <summary>
    /// 点击任务列表上一页按钮
    /// </summary>
    public void ClickQuestPreviousPageButton()
    {
        questListTransforms[currentQuestPageIndex].gameObject.SetActive(false);
        questListTransforms[currentQuestPageIndex - 1].gameObject.SetActive(true);
        currentQuestPageIndex --;
        SwitchQuestPageButton();
    }
    /// <summary>
    /// 点击任务列表下一页按钮
    /// </summary>
    public void ClickQuestNextPageButton()
    {
        questListTransforms[currentQuestPageIndex].gameObject.SetActive(false);
        questListTransforms[currentQuestPageIndex + 1].gameObject.SetActive(true);
        currentQuestPageIndex ++;
        SwitchQuestPageButton();
    }
    /// <summary>
    ///  重置任务列表页数
    /// </summary>
    public void ResetQuestListPage()
    {
        currentQuestPageIndex = 0;
        questListTransforms[0].gameObject.SetActive(true);
        questListTransforms[1].gameObject.SetActive(false);
        questListTransforms[2].gameObject.SetActive(false);
        SwitchQuestPageButton();
    }
}
