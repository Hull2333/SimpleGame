using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFarm.CropPlant
{
    public class ReapItem : MonoBehaviour
    {
        private CropDetails cropDetails;
        private Transform playerPosition => FindObjectOfType<PlayerController>().transform;
        /// <summary>
        /// 获取Crop信息
        /// </summary>
        /// <param name="ID"></param>
        public void InitCropData(int ID)
        {
            cropDetails = CropManager.Instance.GetCropDetails(ID);
        }
        /// <summary>
        /// 生成果实
        /// </summary>
        public void SpawnHarvestItems()
        {
            
            //定义一个收获的数量
            int amountToProduce;
            for (int i = 0; i < cropDetails.producedItemID.Length; i++)
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
                //第二种战利品有概率生成
                else
                {
                    amountToProduce = Random.Range(cropDetails.producedMinAmount[i], cropDetails.producedMaxAmount[i] + 1);
                    if (amountToProduce < cropDetails.producedMaxAmount[i] - cropDetails.producedMinAmount[i])
                    {
                        amountToProduce = 0;
                    }
                }
                //收获获取的物品数量
                for (int j = 0; j < amountToProduce; j++)
                {
                    if (cropDetails.generateAtPlayerPosition)
                    {
                        EventHandler.CallHarvestAtPlayerPosition(cropDetails.producedItemID[i]);
                    }
                    //在地图上生成物品
                    else
                    {
                        //参照玩家位置判断物品生成的方向
                        var dirX = transform.position.x > playerPosition.position.x ? 1 : -1;
                        //物品掉落生成位置的范围
                        var spawnPos = new Vector3(transform.position.x + Random.Range(dirX, cropDetails.spawnRadius.x * dirX), transform.position.y + Random.Range(-cropDetails.spawnRadius.y, cropDetails.spawnRadius.y), 0);
                        EventHandler.CallInstantiateItemInScene(cropDetails.producedItemID[i], spawnPos);
                    }
                }
            }
           
        }
       
    }
}

