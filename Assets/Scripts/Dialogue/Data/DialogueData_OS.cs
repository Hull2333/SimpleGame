using MFarm.Dialogue;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData_OS : ScriptableObject
{
    public List<DialoguePieces> dialoguePieces = new List<DialoguePieces>();
    public Dictionary<string, DialoguePieces> dialogueIndex = new Dictionary<string, DialoguePieces>();
    //只在编辑模式下运行
#if UNITY_EDITOR
    /// <summary>
    /// 当DialogueData_OS下的任何数据被修改时就会执行一次
    /// </summary>
    private void OnValidate()
    {
        dialogueIndex.Clear();
        //添加dialoguePieces到字典dialogueIndex中
        foreach(var piece in dialoguePieces)
        {
            if (!dialogueIndex.ContainsKey(piece.ID))
            {
                dialogueIndex.Add(piece.ID, piece);
            }
        }
    }
#endif
    /// <summary>
    /// 查找整个对话内容中哪一句对话会下发任务，并返回该任务信息
    /// </summary>
    /// <returns></returns>
    public QuestData_OS GetQuest()
    {
        QuestData_OS currentQuest = null;
        foreach(var piece in dialoguePieces)
        {
            if(piece.quest != null)
            {
                currentQuest = piece.quest;
            }
        }
        return currentQuest;
    }
}
