using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace MFarm.CropPlant
{
    public class Crop : MonoBehaviour   //调用在CropBase预制体上
    {
        public CropDetails cropDetails;
        //收割工具使用次数的计时器
        private int harvestActionCount;
        public TileDetails tileDetails;
        public BoxCollider2D collider2D;
        //判断是否可以收割
        public bool canHarvest => tileDetails.growthDays >= cropDetails.totalGrowthDays;
        private Animator anim;
        private Transform playerPosition => FindObjectOfType<PlayerController>().transform;
        public Animator selfAnim;
        //作物图片是否反转
        private int isFlip;
       
        private void Start()
        {
            isFlip = Random.Range(0, 2);
            if(isFlip == 1)
            {
                transform.GetChild(0).transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
            }
           
        }
        /// <summary>
        /// 工具收获场景中物体的方法 
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="tile"></param>
        public void ProcessToolAction(ItemDetails tool, TileDetails tile)
        {
            tileDetails = tile;
            int requireActionCount = cropDetails.GetTotalRequireCount(tool.itemID);
            if(requireActionCount == -1)
            {
                return;
            }
            anim = GetComponentInChildren<Animator>();
            //点击计时器
            if (harvestActionCount < requireActionCount)
            {
                harvestActionCount ++;
                //判断是否有动画，树木
                if(anim != null && cropDetails.hasAnimation)
                {
                    //判断树木倒下的方向动画
                    if(playerPosition.position.x < transform.position.x)
                    {
                        anim.SetTrigger("RotateRight");
                    }
                    else
                    {
                        anim.SetTrigger("RotateLeft");
                    }
                }

                //TODO:播放粒子效果
                if(cropDetails.hasParticalEffect)
                {
                    EventHandler.CallParticleEffectEvent(cropDetails.effectType, transform.position + cropDetails.effectPos , Vector2.zero);
                }
                
                //TODO:播放声音
                if(cropDetails.soundEffect != SoundName.none)
                {
                    EventHandler.CallPlaySoundEvent(cropDetails.soundEffect);
                }
            }
            if (harvestActionCount>=requireActionCount)
            {
                if (cropDetails.generateAtPlayerPosition || !cropDetails.hasAnimation)
                {
                    //生成农作物
                    SpawnHarvestItems();
                }
                else if(cropDetails.hasAnimation) 
                {
                    if(playerPosition.position.x < transform.position.x)
                    {
                        anim.SetTrigger("FallRight");
                    }
                    else
                    {
                        anim.SetTrigger("FallLeft");
                    }
                    //播放树倒下的音效
                    EventHandler.CallPlaySoundEvent(SoundName.TreeFalling);
                    StartCoroutine(HarvestAfterAnimation());
                }
            }
        }
        /// <summary>
        /// 在树砍倒动画之后执行
        /// </summary>
        /// <returns></returns>
        private IEnumerator HarvestAfterAnimation()
        {
            //当未播放Animator中的都一个图层的End动画时，一直循环这个while
            while (!anim.GetCurrentAnimatorStateInfo(0).IsName("End"))
            {
                yield return null;
            }
            SpawnHarvestItems();
            //转换新物体
            if (cropDetails.transferItemID > 0)
            {
                CreateTransferCrop();
            }
        }
        /// <summary>
        /// 树木砍倒后转换为木桩
        /// </summary>
        private void CreateTransferCrop()
        {
            tileDetails.seedItemID = cropDetails.transferItemID;
            tileDetails.daysSinceLastHarvest = -1;
            tileDetails.growthDays = 0;

            EventHandler.CallRefreshCurrentMap();
        }
        /// <summary>
        /// 生成收获的农作物,或者各种材料
        /// </summary>
        public void SpawnHarvestItems()
        {
            //定义一个收获的数量
            int amountToProduce;
            for (int i = 0;i<cropDetails.producedItemID.Length;i++)
            {
                if (i == 0) 
                {
                    //如果最小和最大生成数相等，则收获数量等于最小值
                    if (cropDetails.producedMinAmount[i] == cropDetails.producedMaxAmount[i])
                    {
                        amountToProduce = cropDetails.producedMinAmount[i];
                    }
                    else
                    {
                        amountToProduce = Random.Range(cropDetails.producedMinAmount[i], cropDetails.producedMaxAmount[i] + 1);
                    }
                }
                else
                {
                    amountToProduce = Random.Range(cropDetails.producedMinAmount[i], cropDetails.producedMaxAmount[i] + 1);
                    
                    if(amountToProduce < cropDetails.producedMaxAmount[i] - cropDetails.producedMinAmount[i])
                    {
                        amountToProduce = 0;
                    }
                }
                
                //收获获取的物品数量
                for (int j = 0;  j < amountToProduce; j++)
                {
                    EventHandler.CallInstantiateItemInScene(cropDetails.producedItemID[i], transform.position);
                    //if (cropDetails.generateAtPlayerPosition)
                    //{
                    //    EventHandler.CallHarvestAtPlayerPosition(cropDetails.producedItemID[0]);
                    //}
                    //在地图上生成物品
                    //else
                    //{
                        //参照玩家位置判断物品生成的方向
                        //var dirX = transform.position.x > playerPosition.position.x ? 1 : -1;
                        //物品掉落生成位置的范围
                        //var spawnPos = new Vector3(transform.position.x + Random.Range(dirX, cropDetails.spawnRadius.x * dirX), transform.position.y + Random.Range(-cropDetails.spawnRadius.y, cropDetails.spawnRadius.y), 0);
                        
                        //EventHandler.CallInstantiateItemInScene(cropDetails.producedItemID[i],transform.position);
                   // }
                }
            }
            if (tileDetails != null)
            {
                tileDetails.daysSinceLastHarvest ++;
                //判断是否可以重复生长
                if (cropDetails.daysToRegrow > 0 && tileDetails.daysSinceLastHarvest < cropDetails.regrowTimes)
                {
                    tileDetails.growthDays = cropDetails.totalGrowthDays - cropDetails.daysToRegrow;
                    //刷新种子
                    EventHandler.CallRefreshCurrentMap();
                }
                //不可重复生长
                else
                {
                    tileDetails.daysSinceLastHarvest = -1;
                    tileDetails.seedItemID = -1;
                    //FIXME:自己设计
                    //tileDetails.daysSinceDug = -1;
                }

                Destroy(gameObject);
            }
        }
        public void ProcessToolsAction(ItemDetails tool)
        {

            int requireActionCount = cropDetails.GetTotalRequireCount(tool.itemID);
            if (requireActionCount == -1)
            {
                return;
            }
            anim = GetComponentInChildren<Animator>();
            //点击计时器
            if (harvestActionCount < requireActionCount)
            {
                harvestActionCount++;
                //判断是否有动画，树木
                if (anim != null && cropDetails.hasAnimation)
                {
                    //判断树木倒下的方向动画
                    if (playerPosition.position.x < transform.position.x)
                    {
                        anim.SetTrigger("RotateRight");
                    }
                    else
                    {
                        anim.SetTrigger("RotateLeft");
                    }
                }

                //TODO:播放粒子效果
                if (cropDetails.hasParticalEffect)
                {
                    EventHandler.CallParticleEffectEvent(cropDetails.effectType, transform.position + cropDetails.effectPos, Vector2.zero);
                }

                //TODO:播放声音
                if (cropDetails.soundEffect != SoundName.none)
                {
                    EventHandler.CallPlaySoundEvent(cropDetails.soundEffect);
                }
            }
            if (harvestActionCount >= requireActionCount)
            {
                if (cropDetails.generateAtPlayerPosition || !cropDetails.hasAnimation)
                {
                    //生成农作物
                    SpawnHarvestItems();
                }
                else if (cropDetails.hasAnimation)
                {
                    if (playerPosition.position.x < transform.position.x)
                    {
                        anim.SetTrigger("FallRight");
                    }
                    else
                    {
                        anim.SetTrigger("FallLeft");
                    }
                    //播放树倒下的音效
                    EventHandler.CallPlaySoundEvent(SoundName.TreeFalling);
                    StartCoroutine(HarvestAfterAnimation());
                }
            }
        }
        /// <summary>
        /// 收获农作物
        /// </summary>
        public void ProcessCropAction()
        {
            //TODO:播放粒子效果
            if (cropDetails.hasParticalEffect)
            {
                EventHandler.CallParticleEffectEvent(cropDetails.effectType, transform.position + cropDetails.effectPos, Vector2.zero);
            }
            //TODO:播放声音
            if (cropDetails.soundEffect != SoundName.none)
            {
                EventHandler.CallPlaySoundEvent(cropDetails.soundEffect);
            }
            //生成农作物
            SpawnHarvestItems();
            //if (cropDetails.generateAtPlayerPosition || !cropDetails.hasAnimation)
            //{
            //    //生成农作物
            //    SpawnHarvestItems();
            //}
            //else if (cropDetails.hasAnimation)
            //{
            //    if (playerPosition.position.x < transform.position.x)
            //    {
            //        anim.SetTrigger("FallRight");
            //    }
            //    else
            //    {
            //        anim.SetTrigger("FallLeft");
            //    }
            //    //播放树倒下的音效
            //    EventHandler.CallPlaySoundEvent(SoundName.TreeFalling);
            //    StartCoroutine(HarvestAfterAnimation());
            //}
            EventHandler.CallPlayerDecreaseStminaEvent(2);
        }
        
        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.transform.CompareTag("Player"))
            {
                if (other.transform.parent.transform.position.x >= transform.position.x)
                {
                    PlayLeftShakeAnim();
                }
                else
                {
                    PlayRightShakeAnim();
                }
            }
        }
        public void PlayLeftShakeAnim()
        {
            selfAnim.SetTrigger("LeftShake");
        }
        public void PlayRightShakeAnim()
        {
            selfAnim.SetTrigger("RightShake");
        }


    }

    
    
}
