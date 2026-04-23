using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BulletinBoardUI;
namespace MFarm.Task
{
    
    public class BulletinBoard : MonoBehaviour //调用在BulletinBoard对象上
    {
       
        private CursorManager cursorManager;
        public Sprite cursorTaskSprite;
        private void Start()
        {
            cursorManager = GameObject.FindWithTag("CursorManager").GetComponent<CursorManager>();
        }
        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                //打开公告栏
                if (Input.GetKey(KeyCode.Mouse1) && cursorManager.canCheck)
                {
                    BulletinBoardUI.Instance.OpenBulletinBoardUI();
                }
                //cursorManager = GameObject.FindWithTag("CursorManager").GetComponent<CursorManager>();
                // Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                //Collider2D boardCollider = Physics2D.OverlapPoint(mousePos);
                //右键点击公告栏显示公告栏任务UI
                //if (boardCollider != null)
                //{
                //    if(boardCollider.name == "BulletinBoardTrigger")
                //    {
                //        //显示动态鼠标
                //        EventHandler.CallCursorImagerChagerEvent("Check",true);
                        
                //    }
                //    else
                //    {
                //        EventHandler.CallCursorImagerChagerEvent("Check", false);
                //    }
                   
                //}
                //else
                //{
                //    EventHandler.CallCursorImagerChagerEvent("Check", false);
                //}
            }
            
        }
        //private void OnTriggerExit2D(Collider2D other)
        //{
        //    if (other.CompareTag("Player"))
        //    {
        //        EventHandler.CallCursorImagerChagerEvent("Check", false);
        //    }
        //}

    }

}

