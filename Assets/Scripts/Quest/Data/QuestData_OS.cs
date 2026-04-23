using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MFarm.Inventory;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest/Quest Data")]
public class QuestData_OS : ScriptableObject
{
    [System.Serializable]
    public class QuestRequire
    {
        public int ItemID;
        public int requireAmount;
        public int currentAmount;
    }
    public int questID;
    public string questName;
    public QuestType questType;
    [TextArea]
    public string description;
    public NPCname questman;
    public int questDay;
    [HideInInspector] public int currentQuestDay;
    //开始任务、领取奖励、完成任务
    public bool isStarted;
    public bool isComplete;
    public bool isFinshed;
    //接受任务列表
    public List<QuestRequire> questRequires = new List<QuestRequire>();
    //任务奖励列表
    public List<InventoryItem> rewards = new List<InventoryItem>();
    /// <summary>
    /// 检查任务进度
    /// </summary>
    public void CheckQuestProgress()
    {
        //finishRequire表示各个任务指标达到任务数量的列表
        var finishRequires = questRequires.Where(r => r.requireAmount <= r.currentAmount);
        //当finishRequire的数量等于任务指标的数量，则表示该任务可以提交了
        isComplete = finishRequires.Count() == questRequires.Count;
        if (isComplete)
        {
            Debug.Log("任务完成");
        }
    }
    /// <summary>
    /// 领取任务奖励
    /// </summary>
    public void GiveRewards()
    {
        foreach(var reward in rewards)
        {
            //获取任务奖励
            InventoryManager.Instance.AddRewardItem(reward.itemID, reward.itemAmount);
            ////扣除任务物品数量
            //if (reward.itemAmount < 0)
            //{
            //    //绝对值
            //    int requireCount = Mathf.Abs(reward.itemAmount);


            //}
            //else
            //{
            //    //获取任务奖励
            //    InventoryManager.Instance.AddRewardItem(reward.itemID, reward.itemAmount);
            //}
        }
    }
    /// <summary>
    /// 当前接受任务的所有需求物品/敌人的ID列表
    /// </summary>
    /// <returns></returns>
    public List<int> RequireTargetItemID()
    {
        List<int> targetItemIDList = new List<int>();
        foreach (var require in questRequires)
        {
            targetItemIDList.Add(require.ItemID);
        } 
        return targetItemIDList;
    }
}
