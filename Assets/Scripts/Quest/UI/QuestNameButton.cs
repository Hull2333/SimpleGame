using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestNameButton : MonoBehaviour // 调用在QuestNameButton预制体上
{
    public Text questNameText;
    public QuestData_OS currentData;
    public Text questContentText;
    private Color originColor;
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(UpdateQuestContent);
        originColor = questNameText.color;
    }
    /// <summary>
    /// 任务按钮点击之后显示任务信息
    /// </summary>
    private void UpdateQuestContent()
    {
        questContentText.text = currentData.description;
        QuestUI.Instance.questRightPanel.SetActive(true);
        QuestUI.Instance.SetUpRequireList(currentData);
        QuestUI.Instance.SetUpQuestDay(currentData);
        //开启高亮
        foreach (Transform questListTransform in QuestUI.Instance.questListTransforms)
        {
            foreach (Transform item in questListTransform)
            {
                item.gameObject.transform.GetChild(0).gameObject.SetActive(false);
            }
        }
        transform.GetChild(0).gameObject.SetActive(true);
        //先删除所有奖励内容再显示任务奖励
        foreach (Transform item in QuestUI.Instance.rewardTransform)
        {
            Destroy(item.gameObject);
        }
        foreach(var item in currentData.rewards)
        {
            QuestUI.Instance.SetUpRewardItem(item.itemID, item.itemAmount);
            Debug.Log(item.itemID);
        }
    }
    /// <summary>
    /// 设置任务按钮内容
    /// </summary>
    /// <param name="questData"></param>
    public void SetUpNameButton(QuestData_OS questData)
    {
        currentData = questData;
        if (questData.isComplete)
        {
            //字体变为绿色
            questNameText.text = questData.questName + "(完成)";
            questNameText.color = new Color(0.645f, 0.914f, 0.263f, 1f);
        }
        else
        {
            //回到初始颜色
            questNameText.text = questData.questName;
            questNameText.color = originColor;
        }
    }
}
