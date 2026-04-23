using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class QuestRequirement : MonoBehaviour //调用在QuestRequirement预制体上
{
    private Text requireName;
    private Text progressNumber;
    private Color originColor;
    private void Awake()
    {
        requireName = GetComponent<Text>();
        progressNumber = transform.GetChild(0).GetComponent<Text>();
        originColor = requireName.color;
    }
    /// <summary>
    /// 设置任务所需物品的名字和数量
    /// </summary>
    /// <param name="name"></param>
    /// <param name="amount"></param>
    /// <param name="currentAmount"></param>
    public void SetUpRequirement(string name,int amount,int currentAmount)
    {
        requireName.text = name;
        progressNumber.text = currentAmount.ToString() + " / " + amount.ToString();
        //任务物品够数后显示绿色
        if(currentAmount >= amount)
        {
            requireName.color = new Color(0.645f, 0.914f, 0.263f, 1f);
            progressNumber.color = new Color(0.645f, 0.914f, 0.263f, 1f);
        }
        //回到初始颜色
        else
        {
            requireName.color = originColor;
            progressNumber.color = originColor;
        }
    }
    /// <summary>
    /// 任务完成后的任务显示
    /// </summary>
    /// <param name="name"></param>
    /// <param name="isFinished"></param>
    public void SetUpRequirement(string name,bool isFinished)
    {
        if (isFinished)
        {
            requireName.text = name;
            progressNumber.text = "完成";
            requireName.color = Color.gray;
            progressNumber.color = Color.gray;
        }
    }
}
