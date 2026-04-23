using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : Singleton<NPCManager> //调用在NPCManager对象上
{
    public SceneRouteDataList_SO sceneRouteData;
    public List<NPCPosition> npcPositionList;

    private Dictionary<string,SceneRoute> sceneRouteDict = new Dictionary<string,SceneRoute>();


    protected override void Awake()
    {
        base.Awake();
        InitSceneRouteDict();
    }

    private void OnEnable()
    {
        //新游戏开始时需要重置的数据
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }

    private void OnStartNewGameEvent(int obj)
    {
        foreach(var character in npcPositionList)
        {
            character.npc.position = character.position;
            character.npc.GetComponent<NPCMovement>().StartScene = character.startScene;
        }
    }

    /// <summary>
    /// 初始化路径字典
    /// </summary>
    private void InitSceneRouteDict()
    {
        if(sceneRouteData.sceneRouteList.Count > 0 )
        {
            foreach(SceneRoute route in sceneRouteData.sceneRouteList)
            {
                var key = route.fromSceneName + route.gotoSceneName;
                //如果字典里已经有这个key
                if (sceneRouteDict.ContainsKey(key))
                {
                    continue;
                }
                //如果字典里没有这个key,就把key和其匹配的路径加到字典中
                else
                {
                    sceneRouteDict.Add(key, route);
                }
            }
        }
    }
    /// <summary>
    /// 从跨场景字典中查找并获取两个场景之间的路径
    /// </summary>
    /// <param name="fromSceneName">起始场景</param>
    /// <param name="gotoSceneName">目标场景</param>
    /// <returns></returns>
    public SceneRoute GetSceneRoute(string fromSceneName,string gotoSceneName)
    {
        return sceneRouteDict[fromSceneName + gotoSceneName];
    }
}
