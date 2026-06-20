using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFarm.CropPlant
{
    public class CropManager : Singleton<CropManager>   //调用在CropManager对象上
    {
        public CropDetailsList_SO cropData;
        private Transform cropParent;
        private Grid currenGrid;
        private Season currentSeason;
        private void OnEnable()
        {
            EventHandler.PlantSeedEvent += OnPlantSeedEvent;
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent += OnGameDayEvent;
        }
        private void OnDisable()
        {
            EventHandler.PlantSeedEvent -= OnPlantSeedEvent;
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent -= OnGameDayEvent;
        }

        private void OnGameDayEvent(int day, Season season)
        {
            currentSeason = season;
        }

        private void OnAfterSceneLoadedEvent()
        {
            currenGrid = FindObjectOfType<Grid>();
            cropParent = GameObject.FindWithTag("CropParent").transform;
        }

        private void OnPlantSeedEvent(int ID, TileDetails tileDetails)
        {
            
            CropDetails currentCrop = GetCropDetails(ID);
            if(currentCrop != null)
            {
                //用于第一次种植
                if (tileDetails.seedItemID == -1)
                {
                    tileDetails.seedItemID = ID;
                    tileDetails.growthDays = 0;
                    DisplayCropPlant(tileDetails, currentCrop);

                }
                //用于刷新地图
                else if (tileDetails.seedItemID != -1)
                {
                    DisplayCropPlant(tileDetails, currentCrop);
                }
            }
            
        }
        /// <summary>
        /// 显示种下的农作物
        /// </summary>
        /// <param name="tileDetails">选中瓦片信息</param>
        /// <param name="cropDetails">选中种子信息</param>
        private void DisplayCropPlant(TileDetails tileDetails,CropDetails cropDetails)
        {
           
            //成长阶段
            int growthStages = cropDetails.growthDays.Length; 
            int currentStage = 0;
            int dayCounter = cropDetails.totalGrowthDays;
            //倒序计算当前的成长阶段
            for(int i = growthStages - 1; i >= 0; i--)
            {
                if (tileDetails.growthDays >= dayCounter)
                {  
                    currentStage = i;
                   
                    break;
                }
                dayCounter -= cropDetails.growthDays[i];
            }
            Debug.Log("currentStage:  " + currentStage);
            //获取当前阶段的Prefab和图片
            GameObject cropPrefab = cropDetails.growthPrefab[currentStage];
            Sprite cropSprite;
            if (currentStage == growthStages - 1)
            {
                //随机作物显示图片
                int spriteIndex = Random.Range(currentStage, currentStage + 1);
                cropSprite = cropDetails.growthSprites[spriteIndex];
            }
            else
            {
                cropSprite = cropDetails.growthSprites[currentStage];
            }
           
            //农作物生成的位置
            Vector3 pos = new Vector3(tileDetails.gridX + 0.5f, tileDetails.gridY + 0.5f, 0);
            GameObject cropInstance = Instantiate(cropPrefab,pos,Quaternion.identity,cropParent);
            cropInstance.GetComponentInChildren<SpriteRenderer>().sprite = cropSprite;
            //让农作物知道自己的农作物详情和所在瓦片的信息
            cropInstance.GetComponent<Crop>().cropDetails = cropDetails;
            cropInstance.GetComponent<Crop>().tileDetails = tileDetails;
            //设置农作物碰撞体是否开启
            if (cropDetails.openCollider)
            {
                cropInstance.GetComponent<Crop>().collider2D.enabled = true;
            }
            else
            {
                cropInstance.GetComponent<Crop>().collider2D.enabled = false;
            }
        }
        /// <summary>
        /// 通过物品ID查找种子信息
        /// </summary>
        /// <param name="ID">物品ID</param>
        /// <returns></returns>
        public CropDetails GetCropDetails(int ID)
        {
            return cropData.cropDetailsList.Find(c => c.seedItemID == ID);
        }
        /// <summary>
        /// 判断当前季节是否可以种植对应的种子
        /// </summary>
        /// <param name="crop">种子信息</param>
        /// <returns></returns>
        public bool SeasonAvailable(CropDetails crop)
        {
            for(int i =0;i<crop.seasons.Length;i++)
            {
                if (crop.seasons[i] == currentSeason)
                {
                    return true;
                }
                
            }
            return false;
        }
    }

}
