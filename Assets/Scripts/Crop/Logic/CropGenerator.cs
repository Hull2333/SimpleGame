using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFarm.Map;
namespace MFarm.CropPlant
{
    public class CropGenerator : MonoBehaviour  //调用在需要预生成的Crop预制体上
    {
        private Grid currentGrid;

        public int seedItemID;
        //预先加载的Crop成长的天数
        public int growthDays;

        private void Awake()
        {
            currentGrid = FindObjectOfType<Grid>();
        }
        private void OnEnable()
        {
            EventHandler.GenerateCropEvent += GenerateCrop;
        }
        private void OnDisable()
        {
            EventHandler.GenerateCropEvent -= GenerateCrop;
        }
        private void GenerateCrop()
        {
            //瓦片的世界坐标转换为网格坐标
            Vector3Int cropGridPos = currentGrid.WorldToCell(transform.position);

            if(seedItemID != null)
            {
                var tile = GridMapManager.Instance.GetTileDetailsOnMousePosition(cropGridPos);
                //如果该瓦片上什么都没有，则重新赋值新瓦片信息
                if(tile == null)
                {
                    tile = new TileDetails();
                    tile.gridX = cropGridPos.x;
                    tile.gridY = cropGridPos.y;
                }
                //重新对新tile赋值
                tile.gridX = cropGridPos.x;
                tile.gridY = cropGridPos.y;
                tile.daysSinceWatered = -1;
                tile.seedItemID = seedItemID;
                tile.growthDays = growthDays;

                GridMapManager.Instance.UpdateTileDetails(tile);
            }
        }
    }
}

