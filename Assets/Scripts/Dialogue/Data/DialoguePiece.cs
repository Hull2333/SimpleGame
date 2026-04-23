using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace MFarm.Dialogue
{
    [System.Serializable]
    public class DialoguePiece
    {
        [Header("对话详情")]
        public Sprite faceImage;
        public bool onLeft;
        public string name;
        [TextArea]
        public string dialogueText;
        public bool hasToPause;
        //当前这句对话是否播放结束
        [HideInInspector]public bool isDone;
        //对话后需要发生的事件
        public UnityEvent afterTalkEvent;
    }
}

