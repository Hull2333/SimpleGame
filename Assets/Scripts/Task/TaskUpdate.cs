using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
namespace MFarm.Task 
{
    public class TaskUpdate : MonoBehaviour //调用在PlayerTaskPanel UI上
    {
        //无任务提示UI
        public GameObject noTaskTip;
        /// <summary>
        /// 每次开启玩家任务面板时启动一次
        /// </summary>
        public void OnEnable()
        {
            if (TaskManager.Instance.playerAcceptTaskDetails.Count > 0)
            {
                noTaskTip.SetActive(false);
                EventHandler.CallTaskStartEvent();
            }
            else
            {
                noTaskTip.SetActive(true);
            }
            
        }
        /// <summary>
        /// 清空玩家任务面板的所以任务Image
        /// </summary>
        //private void RemoveAllChildrenInPlayerTaskPanel()
        //{
        //    Transform transform;
        //    for(int i = 0; i < gameObject.transform.childCount; i++)
        //    {
        //        transform = gameObject.transform.GetChild(i);
        //        GameObject.Destroy(transform.gameObject);
        //    }
        //}
    }
}


