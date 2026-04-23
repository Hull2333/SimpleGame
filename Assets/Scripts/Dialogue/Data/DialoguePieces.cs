using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class DialoguePieces 
{
    public string ID;
    public Sprite Image;
    public bool shakeImage;
    public NPCname name;
    //文本区域
    [TextArea]
    public string text;
    //此对话的任务
    public QuestData_OS quest;
    public List<DialogueOption> options = new List<DialogueOption>();
    [Header("选完选项之后的对话是否结束")]
    //当没有对话选项时是否结束对话
    public bool isEnd;
}
