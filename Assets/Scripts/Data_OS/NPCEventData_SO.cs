
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;
[CreateAssetMenu(fileName = "NPCEventData_SO", menuName = "NPC/NPCEventData")]
public class NPCEventData_SO : ScriptableObject
{
    public List<NPCEvent> NPCEventList;
    /// <summary>
    /// 获取NPC好感事件
    /// </summary>
    /// <param name="frendliness"></param>
    public NPCEvent GetfriendlinessEvent(float frendliness,string currentScene)
    {
        foreach (var events in NPCEventList)
        {
            //当前场景和好感度都符合发生条件，就返回对应的NPC事件
            if (!events.isHappened && frendliness >= events.friendliness)
            {
                if (events.startScene == currentScene && events.startScene == SceneManager.GetActiveScene().name && TimeManager.Instance.gameHour >= events.eventStartHour && TimeManager.Instance.gameHour <= events.eventEndHour)
                {
                    return events;
                }
            }
        }
        return null;
    }
   
}
