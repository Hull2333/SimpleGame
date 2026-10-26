using MFarm.CropPlant;
using MFarm.Inventory;
using MFarm.Map;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
    private ParticalEffectType WeedFullEffect;
    private ParticalEffectType WeedDisappearEffect;
    private Grid weedGrid;
    //当前生成地图杂草的地图坐标
    private Vector3Int weedGridPos;
    //收割次数
    private int reapCount;
    //是否是杂草
    public bool isWeeds;
    //杂草春夏秋冬的图片
    public Sprite[] springWeedSprites;
    public Sprite[] summerWeedSprites;
    public Sprite[] AutumnWeedSprites;
    public Sprite[] winterWeedSprites;
    //当前季节的杂草
    public Season currentSeasonWeed;
    //是否是大杂草
    public bool isBigWeeds;
    public Sprite[] bigWeedSprites;
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
            if (isWeeds)
            {
                switch (currentSeasonWeed)
                {
                    case Season.春天:
                        WeedFullEffect = ParticalEffectType.WeedFall_Spring;
                        WeedDisappearEffect = ParticalEffectType.WeedDisappear_Spring;
                        break;
                    case Season.夏天:
                        WeedFullEffect = ParticalEffectType.WeedDisappear_Summer;
                        WeedDisappearEffect = ParticalEffectType.WeedDisappear_Summer;
                        break;
                    case Season.秋天:
                        WeedFullEffect = ParticalEffectType.WeedFall_Autumn;
                        WeedDisappearEffect = ParticalEffectType.WeedDisappear_Autumn;
                        break;
                    case Season.冬天:
                        WeedFullEffect = ParticalEffectType.WeedFall_Winter;
                        WeedDisappearEffect = ParticalEffectType.WeedDisappear_Winter;
                        break;
                };
                //杂草粒子特效
                EventHandler.CallParticleEffectEvent(WeedFullEffect, transform.position + Vector3.up, Vector2.zero);
                EventHandler.CallParticleEffectEvent(WeedDisappearEffect, transform.position + Vector3.up, Vector2.zero);
            }
            else
            {
                //杂草粒子特效
                EventHandler.CallParticleEffectEvent(ParticalEffectType.BigWeedFallEffect, transform.position, Vector2.zero);
            }
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
    /// <summary>
    /// 设置杂草的季节图片
    /// </summary>
    public void SetWeedSprite()
    {
        Sprite[] sprites = GetSeasonWeedSprites(TimeManager.Instance.gameSeason);
        if (sprites == null || sprites.Length == 0)
        {
            return;
        }
        GetComponentInChildren<SpriteRenderer>().sprite = sprites[Random.Range(0, sprites.Length)];
    }
    /// <summary>
    /// 根据季节来设置杂草的图片
    /// </summary>
    public void AccordingSeasonChangeWeedSprite()
    {
        if(TimeManager.Instance.gameSeason == currentSeasonWeed)
        {
            return;
        }
        SetWeedSprite();
    }
    /// <summary>
    /// 设置大杂草的图片
    /// </summary>
    public void SetBigWeedSprite()
    {
        GetComponentInChildren<SpriteRenderer>().sprite = bigWeedSprites[Random.Range(0, bigWeedSprites.Length)];
    }
    /// <summary>
    /// 根据当前季节返回对应的杂草图片数组
    /// </summary>
    private Sprite[] GetSeasonWeedSprites(Season season)
    {
        switch (season)
        {
            case Season.春天: 
                currentSeasonWeed = Season.春天; 
                return springWeedSprites;
            case Season.夏天:
                currentSeasonWeed = Season.夏天; 
                return summerWeedSprites;
            case Season.秋天:
                currentSeasonWeed = Season.秋天; 
                return AutumnWeedSprites;
            case Season.冬天: 
                currentSeasonWeed = Season.冬天; 
                return winterWeedSprites;
            default: return springWeedSprites;
        }
    }

  
}
