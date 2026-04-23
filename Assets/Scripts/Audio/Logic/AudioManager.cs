using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : Singleton<AudioManager>   //调用在AudioManager对象上
{
    [Header("音乐数据库")]
    public SoundDetailsList_SO soundDetailsData;
    public SceneSoundList_SO sceneSoundData;
    [Header("Audio Source")]
    public AudioSource ambientSource;
    public AudioSource gameSource;
    //背景音乐开始播放时间
    public float MusicStartSecond => Random.Range(2.5f, 5f);
    //声明协程
    private Coroutine soundRoutine;
    [Header("Audio Mixer")]
    public AudioMixer audioMixer;
    [Header("Snapshots")]
    //平时声音大小
    public AudioMixerSnapshot normalSnapshot;
    //只有环境音效
    public AudioMixerSnapshot ambientSnapshot;
    //静音
    public AudioMixerSnapshot muteSnapshot;
    //背景音乐转换时间
    private float musicTransitionSecond = 8f;

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        //玩家动作音效事件
        EventHandler.PlaySoundEvent += OnPlaySoundEvent;
        //游戏结束的事件
        EventHandler.EndGameEvent += OnEndGameEvent;
    }
    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.PlaySoundEvent -= OnPlaySoundEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
    }

    private void OnPlaySoundEvent(SoundName soundName)
    {
        var soundDetails = soundDetailsData.GetSoundDetails(soundName);
        if(soundDetails != null)
        {
            //呼叫音效播放事件
            EventHandler.CallInitSoundEffect(soundDetails);
        }
    }

    /// <summary>
    /// 拿到游戏的音频
    /// </summary>
    private void OnAfterSceneLoadedEvent()
    {
       string currentScene = SceneManager.GetActiveScene().name;

        SceneSoundItem sceneSound = sceneSoundData.GetSceneSoundItem(currentScene);
        if(sceneSound == null)
        {
            return;
        }
        SoundDetails ambient = soundDetailsData.GetSoundDetails(sceneSound.ambient);
        SoundDetails music = soundDetailsData.GetSoundDetails(sceneSound.music);

        if(soundRoutine != null)
        {
            StopCoroutine(soundRoutine);
        }
        soundRoutine = StartCoroutine(PlaySoundRoutine(music, ambient));
    }

    private void OnEndGameEvent()
    {
        if(soundRoutine != null)
        {
            StopCoroutine(soundRoutine);
        }
        //静音
        muteSnapshot.TransitionTo(1f);
    }
    /// <summary>
    /// 切换场景是先播放环境音效，经过随机的时间后开始播放背景音乐
    /// </summary>
    /// <param name="music"></param>
    /// <param name="ambient"></param>
    /// <returns></returns>
    private IEnumerator PlaySoundRoutine(SoundDetails music, SoundDetails ambient)
    {
        if(music != null && ambient != null)
        {
            PlayAmbientClip(ambient,1f);
            yield return new WaitForSeconds(MusicStartSecond);
            PlayMusicClip(music,musicTransitionSecond);
        }
    }
    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="soundDetails"></param>
    private void PlayMusicClip(SoundDetails soundDetails,float transitionTime)
    {
        audioMixer.SetFloat("MusicVolume", ConvertSoundVolume(soundDetails.soundVolume));
        gameSource.clip = soundDetails.soundClip;
        if(gameSource.isActiveAndEnabled)
        {
            gameSource.Play();
        }
        //背景音乐在transitionTime时间内达到normalSnapshot快照的音量
        normalSnapshot.TransitionTo(transitionTime);
    }
    /// <summary>
    /// 播放环境音效
    /// </summary>
    /// <param name="soundDetails"></param>
    private void PlayAmbientClip(SoundDetails soundDetails,float transitionTime)
    {
        audioMixer.SetFloat("AmbientVolume", ConvertSoundVolume(soundDetails.soundVolume));
        ambientSource.clip = soundDetails.soundClip;
        if (ambientSource.isActiveAndEnabled)
        {
            ambientSource.Play();
        }
        //背景音乐在transitionTime时间内达到ambientSnapshot快照的音量
        ambientSnapshot.TransitionTo(transitionTime);
    }
    /// <summary>
    /// 将Audio Mixer的分贝值-80~20调整为0~1
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    private float ConvertSoundVolume(float amount)
    {
        return (amount * 100 - 80);
    }
    /// <summary>
    /// 将暂停菜单的音乐Slider与AudioMixer绑定
    /// </summary>
    /// <param name="value"></param>
    public void SetMasterVolume(float value)
    {
        audioMixer.SetFloat("MasterVolume", (value * 100 - 80));
    }
}
