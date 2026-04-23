using UnityEngine;
[System.Serializable]
public class TaskDetails 
{
    //任务的ID、类型、物品ID、完成数量、任务描述、任务完成天数
    public int taskID;
    public TaskType taskType;
    public int itemID;
    public int finishNum;
    public string taskDescriptionInBulletinBoard;
    public string taskDescriptionInPlayerTaskBar;
    public int finishDay;
    public NPCname taskName;
    public Sprite manIcon;
    public bool isInstantiate = false;
    public bool isFinshing;
    public bool canDestory;
}
