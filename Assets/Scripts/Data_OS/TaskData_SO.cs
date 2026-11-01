using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFarm.Task
{
    [CreateAssetMenu(fileName = "TaskDataList_OS", menuName = "Task/TaskDataList")]
    public class TaskData_SO : ScriptableObject
    {
        public List<TaskDetails> taskDetailsList;
        /// <summary>
        /// 根据任务ID获取taskDetails
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public TaskDetails GetTaskDetails(int ID)  
        {
            return taskDetailsList.Find(i => i.taskID == ID);
            
        }
    }
}

