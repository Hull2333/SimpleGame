using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class Sound : MonoBehaviour  //调用在Sound对象上
{
    [SerializeField] private AudioSource audioSource;
    /// <summary>
    /// 设置音效
    /// </summary>
    /// <param name="soundDetails"></param>
    public void SetSound(SoundDetails soundDetails)
    {
        audioSource.clip = soundDetails.soundClip;
        audioSource.volume = soundDetails.soundVolume;
        audioSource.pitch = Random.Range(soundDetails.soundPatchMin, soundDetails.soundPatchMax);
    }
}
