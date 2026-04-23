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
        public UnityEvent OnFinshEvent;
        public List<DialoguePiece> dialogueList = new List<DialoguePiece>();
        public List<DialoguePiece> taskDialogueList = new List<DialoguePiece>();
        private Stack<DialoguePiece> dialogueStack;
        private bool canTalk;
        //NPC现在是否正在说话
        public bool isTalking;
        //NPC可对话时头顶出现的提示图标
        private GameObject uiSign;
        public CursorManager cursorManager;
        public NPCname NPCname;
        //新添加
        public DialogueData_OS currentData;
        //是否为今天第一次和该NPC对话
        public bool isFristTalk;
        //对话库
        private DialoguaGiver dialoguaGiver;
  
        private void Awake()
        {
            
            cursorManager = GameObject.FindWithTag("CursorManager").GetComponent<CursorManager>();
            dialoguaGiver = GetComponent<DialoguaGiver>();
            uiSign = transform.GetChild(0).gameObject;
            FillDialogueStack(dialogueList);
            FillDialogueStack(taskDialogueList);
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

        private void Update()
        {
            //NPC头顶对话交互提示UI
            uiSign.SetActive(canTalk);
            //if (DialogueUI.Instance.talkWithNPC == false && canTalk && Input.GetKeyDown(KeyCode.Space))
            //{
            //    dialoguaGiver.TalkWithPlayer();
            //    OpenDialogue();
            //}
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
                        if (Input.GetKey(KeyCode.Mouse1) && DialogueUI.Instance.talkWithNPC == false && canTalk)
                        {

                            dialoguaGiver.TalkWithPlayer();
                            OpenDialogue();
                        }
                    }
                }
            }

        }
        /// <summary>
        /// 用堆栈的方式把每一句对话压如List中
        /// </summary>
        private void FillDialogueStack(List<DialoguePiece> dialoguePieceList) 
        {
            dialogueStack = new Stack<DialoguePiece>();
            for(int i = dialoguePieceList.Count - 1; i > -1; i--)
            {
                dialoguePieceList[i].isDone = false;
                dialogueStack.Push(dialoguePieceList[i]);
            }
        }
        /// <summary>
        /// 设置对话进程
        /// </summary>
        /// <returns></returns>
        private IEnumerator DialogueRoutine(List<DialoguePiece> dialoguePieceList)
        {
            isTalking = true;
            //TryPop尝试拿出stack中的第一个数值，该数值就在stack中没有了
            if(dialogueStack.TryPop(out DialoguePiece result))
            {
                //传到UI显示对话
                EventHandler.CallShowDialogueEvent(result);
                //开始对话时，禁止玩家移动
                EventHandler.CallUpdateGameStateEvent(GameState.Pause);
                //等待DialoguePiece中的isDone为true时，再往下执行
                yield return new WaitUntil(() => result.isDone);
                isTalking = false;
            }
            //当后续没有对话内容时，关闭对话框
            else
            {
                //结束对话后，恢复玩家移动
                EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
                EventHandler.CallShowDialogueEvent(null);
                //因为上面dialogueStack的TruPop方法拿出一条对话后会在堆栈中删除，所以希望后续还可以对话，需要重新填充堆栈
                FillDialogueStack(dialoguePieceList);
                isTalking = false;
                //如果正在执行对完话后的事件，比如打开商店，这是禁止再次对话
                if(OnFinshEvent != null)
                {
                    OnFinshEvent.Invoke();
                    canTalk = false;
                }
            }
        }
        /// <summary>
        /// 显示对话UI
        /// </summary>
        private void OpenDialogue()
        {
            DialogueUI.Instance.UpdateDialogueData(currentData);
            DialogueUI.Instance.UpdateMainDialogue(currentData.dialoguePieces[0]);
        }
    }

    
}
