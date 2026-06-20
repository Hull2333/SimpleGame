using MFarm.CropPlant;
using MFarm.Inventory;
using MFarm.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ReapableItem : MonoBehaviour  //调用在地图杂草、地图小麦对象上
{
    public GrassAnimController grassAnim;
    private Crop crop;
    //收割后产生的item
    public int produceItemID;
    //生成的最大最小数量
    public int minProduceNum;
    public int maxPriduceNum;
    //粒子特效
    public ParticalEffectType particalEffectType;
    private Grid weedGrid;
    //当前生成地图杂草的地图坐标
    private Vector3Int weedGridPos;
    //收割次数
    private int reapCount;
    //是否是杂草
    public bool isWeeds;
    //是否是大杂草
    public bool isBigWeeds;
    private void Start()
    {
        if (!isWeeds)
        {
            crop = GetComponent<Crop>();
        }
        //获取与当前生成地图杂草的地图坐标
        weedGrid = FindObjectOfType<Grid>();
        weedGridPos = weedGrid.WorldToCell(transform.position);
        if (!isBigWeeds)
        {
            reapCount = Random.Range(1, 4);
        }
        else
        {
            reapCount = 1;
        }
       
    }
   
    /// <summary>
    /// 生成杂草、小麦
    /// </summary>
    public void SpawnItems()
    {
        reapCount --;
        //大杂草不执行动画
        if (!isBigWeeds)
        {
            //判断播放杂草还是小麦动画
            if (isWeeds)
            {
                //让镰刀触碰杂草也摇晃一下
                grassAnim.PlayLeftShakeAnim();
            }
            else
            {
                crop.PlayLeftShakeAnim();
            }
        }
        if (reapCount <= 0)
        {
            int amountToProduce = Random.Range(minProduceNum, maxPriduceNum);
            //收获获取的物品数量
            for (int j = 0; j < amountToProduce; j++)
            {
                {
                    EventHandler.CallInstantiateItemInScene(produceItemID, transform.position);
                }
            }
            //杂草粒子特效
            EventHandler.CallParticleEffectEvent(particalEffectType, transform.position + Vector3.up, Vector2.zero);
            //获取当前生成地图杂草地块的TileDetail
            var currentTileDetails = GridMapManager.Instance.GetTileDetailsOnMousePosition(weedGridPos);
            if (!isBigWeeds)
            {
                if (isWeeds)
                {
                    //杂草已收割,可继续生成杂草
                    currentTileDetails.haveWeeds = -1;
                    currentTileDetails.predictHaveWeeds = -1;
                }
                else
                {
                    //小麦已收割，可继续种植
                    currentTileDetails.daysSinceLastHarvest = -1;
                    currentTileDetails.seedItemID = -1;
                }
            }
            else
            {
                //大杂草已收割，可继续长出大杂草
                currentTileDetails.haveBigWeeds = -1;
            }
            Destroy(gameObject);
        }
        
    }
  
}
