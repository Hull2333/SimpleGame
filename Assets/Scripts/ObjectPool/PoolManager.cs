using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour    //调用在PoolManager对象上
{
    //对象池列表
    public List<GameObject> poolPrefabs;
    //储存所有对象池的列表,上面的每一个poolPrefabs生成一个对象池ObjectPool
    private List<ObjectPool<GameObject>> poolEffectList = new List<ObjectPool<GameObject>>();
    //音效队列，先进先出
    private Queue<GameObject> soundQueue = new Queue<GameObject>();
    private void OnEnable()
    {
        //粒子特效播放事件
        EventHandler.ParticleEffectEvent += OnParticleEffectEvent;
        //音效播放事件
        EventHandler.InitSoundEffect += InitSoundEffect;
        
    }

    private void OnDisable()
    {
        EventHandler.ParticleEffectEvent -= OnParticleEffectEvent;
        EventHandler.InitSoundEffect -= InitSoundEffect;
       
    }


    private void Start()
    {
        CreatePool();
        
    }
    /// <summary>
    /// 创建和添加对象池到poolEffectList列表中
    /// </summary>
    private void CreatePool()
    {
        foreach(GameObject item in poolPrefabs)
        {
            //给对象池创建一个父物体
            Transform parent = new GameObject(item.name).transform;
            //同时parent又属于PoolManager对象的子物体
            parent.SetParent(transform);
            //e代表对象池中每一个游戏对象，取出，释放，销毁
            var newPool = new ObjectPool<GameObject>(
                //生成对象，例如粒子特效
                () => Instantiate(item,parent),
                e => { e.SetActive(true); },
                e => { e.SetActive(false); },
                e => { Destroy(e); }
                );
            //添加到存储对象池的列表中
            poolEffectList.Add(newPool);
        }
    }

    private void OnParticleEffectEvent(ParticalEffectType effectType, Vector3 pos, Vector2 dir)
    {
        //WORKFOLLOW：根据粒子特效补全
        ObjectPool<GameObject> objPool = effectType switch
        {
            //根据特效类型来显示不同序号的粒子效果
            ParticalEffectType.LeavesFall01 => poolEffectList[0],
            ParticalEffectType.LeavesFall02 => poolEffectList[1],
            ParticalEffectType.Rock => poolEffectList[2],
            ParticalEffectType.ReapableScenery => poolEffectList[3],
            ParticalEffectType.EnemyHit01 => poolEffectList[5],
            ParticalEffectType.Eat01 => poolEffectList[6],
            ParticalEffectType.HoeEffect => poolEffectList[7],
            ParticalEffectType.WeedFall => poolEffectList[8],
            ParticalEffectType.WaterEffect01 => poolEffectList[9],
            ParticalEffectType.EarthenEffect => poolEffectList[10],
            ParticalEffectType.WoodEffect01 => poolEffectList[11],
            ParticalEffectType.CropEffect01 => poolEffectList[12],
            ParticalEffectType.FeatherEffect01 => poolEffectList[13],
            ParticalEffectType.FeatherEffect02 => poolEffectList[14],
            ParticalEffectType.HoeEffect02 => poolEffectList[15],
            _ => null,
        };
        //在对象池中获取、释放对象
        GameObject obj = objPool.Get();
        obj.transform.position = pos;
        obj.transform.rotation = Quaternion.LookRotation(dir);
        StartCoroutine(ReleaseRoutine(objPool, obj));
    }
    
    /// <summary>
    /// 粒子特效持续的时间
    /// </summary>
    /// <param name="pool"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    private IEnumerator ReleaseRoutine(ObjectPool<GameObject> pool ,GameObject obj)
    {
        yield return new WaitForSeconds(1.5f);
        pool.Release(obj);
    }

    
    /// <summary>
    /// 生成音效
    /// </summary>
    /// <param name="soundDetails"></param>
    //private void InitSoundEffect(SoundDetails soundDetails)
    //{
    //    //获取对象池中的第四个对象
    //    ObjectPool<GameObject> pool = poolEffectList[4];
    //    var obj = pool.Get();

    //    obj.GetComponent<Sound>().SetSound(soundDetails);
    //    StartCoroutine(DisableSound(pool, obj, soundDetails));
    //}

    //private IEnumerator DisableSound(ObjectPool<GameObject> pool , GameObject obj , SoundDetails soundDetails)
    //{
    //    //等待该音效的实际播放时间
    //    yield return new WaitForSeconds(soundDetails.soundClip.length);
    //    pool.Release(obj);
    //}

    private void CreateSoundPool()
    {
        //音效对象生成位置
        var parent = new GameObject(poolPrefabs[4].name).transform;
        parent.SetParent(transform);
        //预先生成20个音效对象到队列中
        for(int i = 0; i < 20 ; i++)
        {
            GameObject newObj = Instantiate(poolPrefabs[4], parent);
            newObj.SetActive(false);
            //将对象池的第四个对象压入音效队列中
            soundQueue.Enqueue(newObj);
        }
    }
    /// <summary>
    /// 获取音效对象池中的音效
    /// </summary>
    /// <returns></returns>
    private GameObject GetPoolObject()
    {
        //一旦队列中对象少于2个，就重新生成对象池
        if(soundQueue.Count < 2)
        {
            CreateSoundPool();
        }
        //队列第一个启动
        return soundQueue.Dequeue();
    }
    /// <summary>
    /// 生成音效
    /// </summary>
    /// <param name="soundDetails"></param>
    private void InitSoundEffect(SoundDetails soundDetails)
    {
        var obj = GetPoolObject();
        obj.GetComponent<Sound>().SetSound(soundDetails);
        obj.SetActive(true);
        StartCoroutine(DisableSound(obj,soundDetails.soundClip.length));
    }
    /// <summary>
    /// 播放完音效后关闭
    /// </summary>
    /// <param name="obj">音效对象</param>
    /// <param name="duration">播放时间</param>
    /// <returns></returns>
    private IEnumerator DisableSound(GameObject obj,float duration)
    {
        yield return new WaitForSeconds(duration);
        obj.SetActive(false);
        soundQueue.Enqueue(obj);
    }
}
