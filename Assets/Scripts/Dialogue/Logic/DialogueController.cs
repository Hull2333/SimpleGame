using MFarm.Inventory;
using MFarm.Task;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace MFarm.Dialogue
{
    [RequireComponent(typeof(NPCMovement))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class DialogueController : MonoBehaviour //调用在需要对话的NPC上
    {
        private NPCMovement npc => GetComponent<NPCMovement>();
        //public List<DialoguePiece> taskDialogueList = new List<DialoguePiece>();
        //private Stack<DialoguePiece> dialogueStack;
        private bool canTalk;
        //NPC现在是否正在说话
        public bool isTalking;
        public CursorManager cursorManager;
        public NPCname NPCname;
        //新添加
        public DialogueData_OS currentData;
        //是否为今天第一次和该NPC对话
        public bool isFristTalk;
        //今天未收到礼物
        public bool isTodayNotGetGift;
        //对话库
        private DialoguaGiver dialoguaGiver;
  
        private void Awake()
        {
            
            cursorManager = GameObject.FindWithTag("CursorManager").GetComponent<CursorManager>();
            dialoguaGiver = GetComponent<DialoguaGiver>();
           // FillDialogueStack(taskDialogueList);
        }
        public void OnEnable()
        {
            EventHandler.GameDayEvent += OnGameDayEvent;
        }
        public void OnDisable()
        {
            EventHandler.GameDayEvent -= OnGameDayEvent;
        }
        private void OnGameDayEvent(int arg1, Season season)
        {
            isFristTalk = true;
            isTodayNotGetGift = true;
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                canTalk = !npc.isMoving && npc.interactable;
            }
        }
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                canTalk = false;
            }
        }
        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                cursorManager = GameObject.FindWithTag("CursorManager").GetComponent<CursorManager>();
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Collider2D NPCCollider = Physics2D.OverlapPoint(mousePos);
                //右键点击NPC开始对话
                if (NPCCollider != null)
                {
                    if (NPCCollider.name == "NPCTrigger")
                    {
                        if (Input.GetKey(KeyCode.Mouse1) && DialogueUI.Instance.talkWithNPC == false && canTalk && !InventoryManager.Instance.anyBagOpened)
                        {
                            //玩家此时是否拿着物品
                            if (InventoryManager.Instance.currentSelectedItem.itemID != 0)
                            {
                                dialoguaGiver.TalkWithPlayer(InventoryManager.Instance.currentSelectedItem.itemID);
                            }
                            else
                            {
                                dialoguaGiver.TalkWithPlayer(0);
                            }
                            OpenDialogue();
                        }
                    }
                }
            }

        }
        /// <summary>
        /// 用堆栈的方式把每一句对话压如List中
        /// </summary>
        //private void FillDialogueStack(List<DialoguePiece> dialoguePieceList) 
        //{
        //    dialogueStack = new Stack<DialoguePiece>();
        //    for(int i = dialoguePieceList.Count - 1; i > -1; i--)
        //    {
        //        dialoguePieceList[i].isDone = false;
        //        dialogueStack.Push(dialoguePieceList[i]);
        //    }
        //}
        /// <summary>
        /// 显示对话UI
        /// </summary>
        public void OpenDialogue()
        {
            DialogueUI.Instance.UpdateDialogueData(currentData);
            DialogueUI.Instance.UpdateMainDialogue(currentData.dialoguePieces[0]);
        }
    }

    
}
