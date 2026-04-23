using MFarm.Map;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;

public class KnockableItem : MonoBehaviour //调用在可收割物体预制体上
{
    public Animator anim;
    //收割后产生的item
    public int[] produceItemID;
    //生成的最大最小数量和确定生成数量
    public int[] minProduceNum;
    public int[] maxPriduceNum;
    private int randomPriduceNum;
    //粒子特效
    public ParticalEffectType rockEffectType;
    public ParticalEffectType treeEffectType;
    public ParticalEffectType trunkEffectType;
    private Grid interactionGrid;
    //当前生成地图杂草的地图坐标
    private Vector3Int interactionGridPos;
    //互动次数
    public int interactionCount;
    private Transform playerPosition => FindObjectOfType<PlayerController>().transform;
    //转换后的预制体
    public GameObject convertGameObject;
    //没有动画物体的可销毁选项
    public bool canDestroy;
    public TreeGrowing treeGrowing;
    private void Start()
    {
        //获取与当前生成地图杂草的地图坐标
        interactionGrid = FindObjectOfType<Grid>();
        interactionGridPos = interactionGrid.WorldToCell(transform.position);
    }
    /// <summary>
    /// 敲击物体
    /// </summary>
    public void KnockItems(TileDetails rockTile)
    {
        interactionCount --;
        SwitchRockShake();
        //石子粒子特效
        EventHandler.CallParticleEffectEvent(rockEffectType, transform.position + Vector3.up * 0.25f, Vector2.zero);
        //物体已被敲碎
        if (interactionCount <= 0)
        {
            //生成对应的成果和数量
            for (int i = 0; i < produceItemID.Length; i++)
            {
                if(minProduceNum[i] < maxPriduceNum[i])
                {
                    randomPriduceNum = Random.Range(minProduceNum[i], maxPriduceNum[i] + 1);
                }
                else
                {
                    randomPriduceNum = minProduceNum[i];
                }
                if(randomPriduceNum <= 0)
                {
                    randomPriduceNum = 0;
                }
                for(int j = 0; j < randomPriduceNum; j++)
                {
                    EventHandler.CallInstantiateItemInScene(produceItemID[i], transform.position);
                }
            }
           
            //获取当前生成地图石块地块的TileDetail
            var rockTileDetails = GridMapManager.Instance.GetTileDetailsOnMousePosition(interactionGridPos);
            EventHandler.CallInCreaseExploreSkillEvent(5);
            PlayRockDestroyAnim();
        }
       
    }
    /// <summary>
    /// 砍伐树木
    /// </summary>
    public void AxeItems(TileDetails treeTile)
    {
        interactionCount--;
        //播放砍树和砍树桩的粒子特效
        if(trunkEffectType == ParticalEffectType.None)
        {
            EventHandler.CallParticleEffectEvent(treeEffectType, transform.position + Vector3.up * 2.5f, Vector2.zero);
        }
        else
        {
            EventHandler.CallParticleEffectEvent(trunkEffectType, transform.position + Vector3.up * 0.25f, Vector2.zero);
        }
        //树被砍倒
        if (interactionCount <= 0)
        {
            //如果转换预制体不为空，那就在原物体销毁前在原位置生成转换预制体
            if (convertGameObject != null)
            {
                Instantiate(convertGameObject, transform.position, Quaternion.identity, transform.parent);
                treeGrowing.isTreeGrow = false;
            }
            //当可以刷新周围的Tile
            if (canDestroy)
            {
                for (int x = treeTile.gridX - 2; x <= treeTile.gridX + 2; x++)
                {
                    for (int y = treeTile.gridY - 2; y <= treeTile.gridY + 2; y++)
                    {

                        var aroundTreeTileDetail = GridMapManager.Instance.GetTileDetailsOnMousePosition(new Vector3Int(x, y));
                        if (aroundTreeTileDetail != null)
                        {
                            aroundTreeTileDetail.haveTree = -1;
                        }
                    }
                }
            }
            SwitchTreeFall();
            //生成对应的成果和数量
            for (int i = 0; i < produceItemID.Length; i++)
            {
                int amountToProduce = Random.Range(minProduceNum[i], maxPriduceNum[i]);
                for (int j = 0; j < amountToProduce; j++)
                {
                    EventHandler.CallInstantiateItemInScene(produceItemID[i], transform.position);
                }
            }
            if (canDestroy)
            {
                DestroyGameObject();
            }
            //退出该方法，后续脚本不执行
            return;
        }
        SwitchTreeRotate();
       
    }
    /// <summary>
    /// 控制石矿抖动
    /// </summary>
    public void SwitchRockShake()
    {
        if(anim != null)
        {
            anim.SetTrigger("RockShake");
        }
        
    }
    public void PlayRockDestroyAnim()
    {
        if(anim != null)
        {
            anim.SetTrigger("RockDestroy");
        }
        
    }
    /// <summary>
    /// 调用在石头摧毁动画帧中
    /// </summary>
    public void DestroySelfGameObject()
    {
        Destroy(gameObject);
    }
    /// <summary>
    /// 摧毁父物体
    /// </summary>
    public void DestroyGameObject()
    {
        Destroy(transform.parent.gameObject);
    }
    /// <summary>
    /// 关闭自己，在树倒下动画帧中运行
    /// </summary>
    public void SetDownGameObject()
    {
        transform.gameObject.SetActive(false);
    }
    public void SwitchTreeRotate()
    {
        if (anim != null)
        {
            if (playerPosition.position.x < transform.position.x)
            {
                anim.SetTrigger("RotateRight");
            }
            else
            {
                anim.SetTrigger("RotateLeft");
            }
        }
       
    }
    public void SwitchTreeFall()
    {
        if (anim != null)
        {
            if (playerPosition.position.x < transform.position.x)
            {
                anim.SetTrigger("FallRight");
            }
            else
            {
                anim.SetTrigger("FallLeft");
            }
        }
       
    }
}
