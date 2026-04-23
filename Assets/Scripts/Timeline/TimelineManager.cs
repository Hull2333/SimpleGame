using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineManager : Singleton<TimelineManager>    //调用在TimelineManager对象上
{

    //初始游戏是播放的PlayableDirector
    public PlayableDirector startDirector;
    //当前正在播放的PlayableDirector
    private PlayableDirector currentDirector;
    private bool isPause;
    //对话是否完成
    private bool isDone;
    public bool IsDone { set => isDone = value; }

    protected override void Awake()
    {
        base.Awake();
        currentDirector = startDirector;
    }

    private void Update()
    {
        //播放的Playable暂停且播放完后按空格键继续
        if(isPause && Input.GetKeyDown(KeyCode.Space) && isDone)
        {
            isPause = false;
            //恢复Playable原来的速度
            currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(1d);
        }
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
    }
    /// <summary>
    /// 场景加载后开始播放PlayableDirector动画
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void OnAfterSceneLoadedEvent()
    {
        currentDirector = FindObjectOfType<PlayableDirector>();
        if(currentDirector != null )
        {
            currentDirector.Play();
        }
    }

    /// <summary>
    /// 暂停当前的PlayableDirector
    /// </summary>
    /// <param name="director"></param>
    public void PauseTimeline(PlayableDirector director)
    {
        currentDirector = director;
        //将Playable速度调为0
        currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(0d);
        isPause = true;
    }
}
