using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using MFarm.Dialogue;
using System;
[Serializable]
public class DialogueBehaviour : PlayableBehaviour
{
    private PlayableDirector director;
    public DialoguePiece dialoguePiece;
    public override void OnPlayableCreate(Playable playable)
    {
        director = (playable.GetGraph().GetResolver() as PlayableDirector);
    }
    /// <summary>
    /// 当开始播放DialogueBehaviour片段
    /// </summary>
    /// <param name="playable"></param>
    /// <param name="info"></param>
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        //呼叫启动Dialogue UI
        EventHandler.CallShowDialogueEvent(dialoguePiece);
        if(Application.isPlaying)
        {
            if(dialoguePiece.hasToPause)
            {
                //暂停Timeline
                TimelineManager.Instance.PauseTimeline(director);
            }
            else
            {
                //关闭剧情对话
                EventHandler.CallShowDialogueEvent(null);
            }
        }
    }
    /// <summary>
    /// 在Timeline播放期间每帧执行
    /// </summary>
    /// <param name="playable"></param>
    /// <param name="info"></param>
    /// <param name="playerData"></param>
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if(Application.isPlaying)
        {
            TimelineManager.Instance.IsDone = dialoguePiece.isDone;
        }
    }
    /// <summary>
    /// Timeline的对话结束后关闭对话UI,OnBehaviourPause所有DialogueClip结束后执行一次
    /// </summary>
    /// <param name="playable"></param>
    /// <param name="info"></param>
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        EventHandler.CallShowDialogueEvent(null);
    }
    /// <summary>
    /// Playable播放时暂停时间，禁止人物移动
    /// </summary>
    /// <param name="playable"></param>
    public override void OnGraphStart(Playable playable)
    {
        EventHandler.CallUpdateGameStateEvent(GameState.Pause);
    }
    /// <summary>
    /// Playable播放完后恢复时间和人物移动
    /// </summary>
    /// <param name="playable"></param>
    public override void OnGraphStop(Playable playable)
    {
        EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
    }
}
