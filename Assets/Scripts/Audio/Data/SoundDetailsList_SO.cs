using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "SoundDetailsList_SO",menuName = "Sound/SoundDetailsList")]
public class SoundDetailsList_SO : ScriptableObject
{
    public List<SoundDetails> soundDetailsList;
    /// <summary>
    /// 查找音效
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public SoundDetails GetSoundDetails(SoundName name)
    {
        return soundDetailsList.Find(s => s.soundName == name);
    }

}
[System.Serializable]
public class SoundDetails
{
    public SoundName soundName;
    public AudioClip soundClip;
    //声音的音调
    [Range(0.1f, 1.5f)]
    public float soundPatchMin;
    [Range(0.1f, 1.5f)]
    public float soundPatchMax;
    //声音大小
    [Range(0.1f, 1f)]
    public float soundVolume;
}
