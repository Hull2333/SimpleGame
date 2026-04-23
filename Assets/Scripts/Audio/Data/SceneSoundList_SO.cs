using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "SceneSoundList_SO",menuName = ("Sound/SceneSoundList"))]
public class SceneSoundList_SO : ScriptableObject
{
    public List<SceneSoundItem> sceneSoundList;
    /// <summary>
    /// 查找获取场景音乐
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public SceneSoundItem GetSceneSoundItem(string name)
    {
        return sceneSoundList.Find(s => s.sceneName == name);
    }
}
[System.Serializable]
public class SceneSoundItem
{
    [SceneName] public string sceneName;
    //场景所在氛围
    public SoundName ambient;
    //场景音乐
    public SoundName music;
}
