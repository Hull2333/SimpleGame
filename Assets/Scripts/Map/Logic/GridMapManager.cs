using MFarm.CropPlant;
using MFarm.Inventory;
using MFarm.Save;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;
namespace MFarm.Map
{
    //Singleton单例模式，就算切换场景也不会改变里面的脚本
    public class GridMapManager : Singleton<GridMapManager>,ISaveable//调用在GridMapManager对象上
    {
        private Transform playerTransform => FindObjectOfType<PlayerController>().transform;
        [Header("种地瓦片切换信息")]
        public RuleTile digTile;
        public RuleTile waterTile;
        public RuleTile availableTilemap;
        public RuleTile unAvailableTilemap;
        private Tilemap digTilemap;
        private Tilemap waterTilemap;
        private Tilemap valueMap;
        //该Tile是否可以改变为耕地Tile
        public bool canSetTile;
        [Header("地图信息")]
        public List<MapData_SO> mapDataList;
        //根据地图名+Tile坐标作为关键字加入到字典中
        public Dictionary<string,TileDetails> tileDetailsDict = new Dictionary<string,TileDetails>();
        //string场景名称，bool是否为第一次加载
        public Dictionary<string,bool> firstLoadDict = new Dictionary<string,bool>();
        private Grid currentGrid;
        private Season currentSeason;
        [Header("可收割场景")]
        //杂草预制体
        public GameObject[] weedsPrefabs;
        //杂草预制体序号
        private int weedsNumber;
        private TileDetails aroundWeedsTileDetail,aroundBigRockTileDetail,aroundTreeTileDetail;
        //石矿预制体
        public GameObject[] rocksPrefabs;
        //石矿预制体序号
        private int rocksNumber;
        //树预制体
        public GameObject[] treePrefab;
        private int treesNumber;
        //大石矿预制体
        public GameObject bigRockPrefab;
        //可敲击、可砍伐图层
        public LayerMask knockableLayer,axeableLayer;
        //可收获物碰撞体
        public Collider2D ableCollider;
        public string GUID => GetComponent<DataGUID>().guid;

        public bool canGetPlayerPos;
        public void Start()
        {
            ISaveable saveable = this;
            saveable.RegisterSaveable();
           
            foreach (var mapData in mapDataList)
            {
                //游戏一开始，所有场景都是第一次加载
                firstLoadDict.Add(mapData.sceneName, true);
                InitTileDetailsDict(mapData);
            }
            
        }

        private void OnEnable()
        {
            EventHandler.ExecuteActionAfterAnimation += OnExecuteActionAfterAnimation;
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent += OnGameDayEvent;
            EventHandler.RefreshCurrentMap += OnRefreshCurrentMap;
        }
        private void OnDisable()
        {
            EventHandler.ExecuteActionAfterAnimation -= OnExecuteActionAfterAnimation;
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent -= OnGameDayEvent;
            EventHandler.RefreshCurrentMap -= OnRefreshCurrentMap;
        }
        private void OnAfterSceneLoadedEvent()
        {
            currentGrid = FindObjectOfType<Grid>();
            digTilemap =GameObject.FindGameObjectWithTag("Dig").GetComponent<Tilemap>();
            waterTilemap = GameObject.FindGameObjectWithTag("Water").GetComponent<Tilemap>();
            valueMap = GameObject.FindGameObjectWithTag("ValueTile").GetComponent<Tilemap>();
            canGetPlayerPos = true;
            if (firstLoadDict[SceneManager.GetActiveScene().name])
            {
                //预先生成Crop,再把第一次加载改为false
                EventHandler.CallGenerateCropEvent();
                firstLoadDict[SceneManager.GetActiveScene().name] = false;
            }
           
            OnRefreshCurrentMap();
        }
        /// <summary>
        /// 每天执行一次
        /// </summary>
        /// <param name="day"></param>
        /// <param name="season"></param>
        private void OnGameDayEvent(int day, Season season)
        {
     
            currentSeason = season;
            foreach(var tile in tileDetailsDict)
            {
                //如果已经种了种子且浇水那么种子的成长天数增加
                if (tile.Value.seedItemID != -1 && tile.Value.daysSinceWatered > -1)
                {
                    tile.Value.growthDays++;
                }
                //浇过水的瓦片过了一天需要继续浇水
                if (tile.Value.daysSinceWatered > -1)
                {
                    tile.Value.daysSinceWatered = -1;
                }
                if (tile.Value.daysSinceDug > -1)
                {
                    tile.Value.daysSinceDug++;
                }
                //挖过的坑超过两天且每种东西就消除
                if(tile.Value.daysSinceDug > 2 && tile.Value.seedItemID == -1)
                {
                    tile.Value.daysSinceDug = -1;
                    tile.Value.canDig = true;
                    tile.Value.growthDays = -1;
                }
               
            }
            RandomGenerateInterView();
            OnRefreshCurrentMap();
            //每日更新鱼类获取列表
            switch(currentSeason)
            {
                case Season.春天:
                    EventHandler.CallFishListInSpriteDayFreshEvent(InventoryManager.fishList);
                    break;
            }
        }
        /// <summary>
        /// 每天随机生成地图上的可收割事物
        /// </summary>
        public void RandomGenerateInterView()
        {
            
            foreach (var tile in tileDetailsDict)
            {
                //周围杂草列表
                List<TileDetails> aroundWeedsDetailsList = new List<TileDetails>();
                if (tile.Value.canWeeds)
                {
                    
                    //获取可生成杂草TileDetail
                    var weedsTileDetail = GetTileDetailsOnMousePosition(new Vector3Int(tile.Value.gridX, tile.Value.gridY));
                    if (weedsTileDetail != null)
                    {
                        //遍历可生成杂草Tile的周围8个Tile的位置
                        for (int x = weedsTileDetail.gridX - 1; x <= weedsTileDetail.gridX + 1; x++)
                        {
                            for (int y = weedsTileDetail.gridY - 1; y <= weedsTileDetail.gridY + 1; y++)
                            {
                                //忽略自身的Tile
                                if (x == weedsTileDetail.gridX && y == weedsTileDetail.gridY)
                                {
                                    continue;
                                }
                                //获取周围可种植杂草TileDetail
                                aroundWeedsTileDetail = GetTileDetailsOnMousePosition(new Vector3Int(x, y));
                                if (aroundWeedsTileDetail != null)
                                {
                                    aroundWeedsDetailsList.Add(aroundWeedsTileDetail);
                                }

                            }
                        }
                        if (aroundWeedsDetailsList != null)
                        {
                            //当前位置未生成杂草
                            if (tile.Value.haveWeeds == -1)
                            {
                                //获取可生成杂草地块的地图位置
                                Vector3 weedsPos = new Vector2(weedsTileDetail.gridX, weedsTileDetail.gridY);
                                int generateWeedsOdds = Random.Range(0, 101);
                                //随机生成杂草预制体
                                weedsNumber = Random.Range(0, 6);
                                //当周围没有杂草生成
                                if (IsAroundHaveWeeds(aroundWeedsDetailsList) == -1)
                                {
                                    //20%的概率生成杂草
                                    if (generateWeedsOdds <= 20)
                                    {
                                        Instantiate(weedsPrefabs[weedsNumber], new Vector2(Random.Range(weedsPos.x - 0.5f, weedsPos.x + 0.5f), Random.Range(weedsPos.y - 0.5f, weedsPos.y + 0.5f)), Quaternion.identity);
                                        //已生成杂草
                                        tile.Value.predictHaveWeeds++;
                                    }
                                }
                                //当周围有杂草生成
                                else if (IsAroundHaveWeeds(aroundWeedsDetailsList) == 1)
                                {
                                    //50%的概率生成杂草
                                    if (generateWeedsOdds <= 50)
                                    {
                                        Instantiate(weedsPrefabs[weedsNumber], new Vector2(Random.Range(weedsPos.x - 0.5f, weedsPos.x + 0.5f), Random.Range(weedsPos.y - 0.5f, weedsPos.y + 0.5f)), Quaternion.identity);
                                        //已生成杂草
                                        tile.Value.predictHaveWeeds++;
                                    }
                                }
                            }
                            //一个可生成杂草地块上可以长出两棵杂草
                            if (tile.Value.haveWeeds == 0)
                            {
                                int generateOdds = Random.Range(0, 101);
                                weedsNumber = Random.Range(0, 6);
                                if (generateOdds <= 50)
                                {
                                    Instantiate(weedsPrefabs[weedsNumber], new Vector2(Random.Range(tile.Value.gridX - 0.5f, tile.Value.gridX + 0.5f), Random.Range(tile.Value.gridY - 0.5f, tile.Value.gridY + 0.5f)), Quaternion.identity);
                                    tile.Value.predictHaveWeeds++;
                                }
                            }
                        }


                    }
                }
                    
                //生成石矿
                if (tile.Value.canRock && tile.Value.haveRock == -1)
                {
                    // 获取可生成石矿TileDetail
                    var rocksTileDetail = GetTileDetailsOnMousePosition(new Vector3Int(tile.Value.gridX, tile.Value.gridY));
                    if (rocksTileDetail != null)
                    {
                        //随机生成石矿预制体
                        rocksNumber = Random.Range(0, 2);
                        Instantiate(rocksPrefabs[rocksNumber], new Vector2(rocksTileDetail.gridX + 0.5f, rocksTileDetail.gridY + 0.5f), Quaternion.identity);
                        tile.Value.haveRock = 1;
                    }
                   
                }
                //生成大石矿
                if(tile.Value.canBigRock && tile.Value.haveBigRock == -1)
                {
                    List<TileDetails> aroundBigRockList = new List<TileDetails>();
                    TileDetails bigRockTile = GetTileDetailsOnMousePosition(new Vector3Int(tile.Value.gridX, tile.Value.gridY));
                    if (bigRockTile != null)
                    {
                        for (int x = bigRockTile.gridX; x <= bigRockTile.gridX + 1; x++)
                        {
                            for (int y = bigRockTile.gridY; y <= bigRockTile.gridY + 1; y++)
                            {
                                if (x == bigRockTile.gridX && y == bigRockTile.gridY)
                                {
                                    continue;
                                }
                                aroundBigRockTileDetail = GetTileDetailsOnMousePosition(new Vector3Int(x, y));
                                if (aroundBigRockTileDetail.canBigRock)
                                {
                                    aroundBigRockList.Add(aroundBigRockTileDetail);
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }
                        if (aroundBigRockList.Count == 3)
                        {
                            Instantiate(bigRockPrefab, new Vector2(bigRockTile.gridX + 1, bigRockTile.gridY + 1), Quaternion.identity);
                            bigRockTile.haveBigRock = 1;
                            for (int i = 0; i < aroundBigRockList.Count; i++)
                            {
                                aroundBigRockList[i].haveBigRock = 1;
                            }
                        }
                    }
                    
                }
                //生成树
                if(tile.Value.canTree && tile.Value.haveTree == -1)
                {
                    //List<TileDetails> aroundTreeList = new List<TileDetails>();
                    TileDetails treeTileDetail = GetTileDetailsOnMousePosition(new Vector3Int(tile.Value.gridX, tile.Value.gridY));
                    if (treeTileDetail != null)
                    {
                        int generateTreesOdds = Random.Range(0, 101);
                        //已生成树的附近两个Tile范围内不可再生成新的树
                        if (generateTreesOdds < 2)
                        {
                            for (int x = treeTileDetail.gridX - 2; x <= treeTileDetail.gridX + 2; x++)
                            {
                                for (int y = treeTileDetail.gridY - 2; y <= treeTileDetail.gridY + 2; y++)
                                {

                                    aroundTreeTileDetail = GetTileDetailsOnMousePosition(new Vector3Int(x, y));
                                    if (aroundTreeTileDetail != null)
                                    {
                                        aroundTreeTileDetail.haveTree = 1;
                                    }
                                }
                            }
                            treesNumber = Random.Range(0, 2);
                            Instantiate(treePrefab[treesNumber], new Vector2(treeTileDetail.gridX + 0.5f, treeTileDetail.gridY), Quaternion.identity);
                        }
                        else
                        {
                            continue;
                        }

                    }

                }
            }
            foreach (var tile in tileDetailsDict)
            {
                if (tile.Value.predictHaveWeeds == 1)
                {
                    tile.Value.haveWeeds = 1;
                    tile.Value.predictHaveWeeds = -1;
                }
            }
        }
        /// <summary>
        /// 判断未生成杂草地块周围是否有杂草生成
        /// </summary>
        /// <param name="tileDetailsList"></param>
        /// <returns></returns>
        private int IsAroundHaveWeeds(List<TileDetails> tileDetailsList)
        {
           
           foreach(var tileDetails in tileDetailsList)
            {
                if(tileDetails.haveWeeds >= 0)
                {
                    return 1;
                }
            }
            return -1;
        }
        /// <summary>
        /// 初始化地图字典
        /// </summary>
        /// <param name="mapData"></param>
        private void InitTileDetailsDict(MapData_SO mapData)
        {
            foreach(TileProperty tileProperty in mapData.tilePropertyes)
            {
                //生成字典中的每个地图块的X,Y坐标
                TileDetails tileDetails = new TileDetails
                {
                    gridX = tileProperty.tileCoordinate.x,
                    gridY = tileProperty.tileCoordinate.y
                };
                //设置字典的关键字格式
                string key = tileDetails.gridX + "X"+ tileDetails.gridY + "Y" +mapData.sceneName;
                //添加更新的瓦片到字典中
                if(GetTileDetails(key) != null )
                {
                    tileDetails = GetTileDetails(key);
                }
                switch (tileProperty.gridType)
                {
                    //如果tileProperty.gridTyp=GridType.Diggable，则tileDetails.canDig = tileProperty.boolTypeValue;
                    case GridType.Diggable:
                        tileDetails.canDig = tileProperty.boolTypeValue;
                        break;
                    case GridType.DropItem:
                        tileDetails.canDropItem = tileProperty.boolTypeValue;
                        break;
                    case GridType.PlaceFurniture:
                        tileDetails.canPlaceFurniture = tileProperty.boolTypeValue;
                        break;
                    case GridType.NPCObstalacle:
                        tileDetails.isNPCObstacle = tileProperty.boolTypeValue;
                        break;
                    case GridType.LaterFishing:
                        tileDetails.canLaterFishing = tileProperty.boolTypeValue;
                        break;
                    case GridType.SeaFishing:
                        tileDetails.canSeaFishing = tileProperty.boolTypeValue;
                        break;
                    case GridType.WeedsGrow:
                        tileDetails.canWeeds = tileProperty.boolTypeValue;
                        break;
                    case GridType.Rocks:
                        tileDetails.canRock = tileProperty.boolTypeValue;
                        break;
                    case GridType.BigRocks:
                        tileDetails.canBigRock = tileProperty.boolTypeValue;
                        break;
                    case GridType.Trees:
                        tileDetails.canTree = tileProperty.boolTypeValue;
                        break;
                }
                //字典原有地图更新
                if(GetTileDetails(key) != null)
                {
                    tileDetailsDict[key] = tileDetails;
                }
                //新地图更新
                else
                {
                    tileDetailsDict.Add(key, tileDetails);
                }
            }
        }
        /// <summary>
        /// 根据key返回瓦片信息
        /// </summary>
        /// <param name="key">x+y+地图名字</param>
        /// <returns></returns>
        public TileDetails GetTileDetails(string key)
        {
            if (tileDetailsDict.ContainsKey(key))
            {
                return tileDetailsDict[key];
            }
            return null;
        }
        /// <summary>
        /// 根据坐标返回瓦片信息
        /// </summary>
        /// <param name="mouseGridPos">鼠标网格信息</param>
        /// <returns></returns>
        public TileDetails GetTileDetailsOnMousePosition(Vector3Int mouseGridPos)
        {
            string key = mouseGridPos.x + "X" + mouseGridPos.y + "Y" + SceneManager.GetActiveScene().name;
            return GetTileDetails(key);
        }
        /// <summary>
        /// 执行实际工具过物品的功能
        /// </summary>
        /// <param name="mouseWorldPos">鼠标坐标</param>
        /// <param name="itemDetails">物品信息</param>
        private void OnExecuteActionAfterAnimation(Vector3 mouseWorldPos, ItemDetails itemDetails)
        {
            //玩家的地图坐标
            var playerGridPos = currentGrid.WorldToCell(playerTransform.position);
            //获取鼠标的地图坐标
            var mouseGridPos = currentGrid.WorldToCell(mouseWorldPos);
            var currentTile = GetTileDetailsOnMousePosition(mouseGridPos);
            if(currentTile != null)
            {
                Crop currentCrop = GetCropObject(mouseWorldPos);
                //WORKFOLLOW:物品使用的实际功能,不在需要判断物品是否具备丢弃或其他功能，因为只要执行到这里，都是符合对应的功能的
                switch (itemDetails.itemType)
                {
                    case ItemType.Seed:
                        if (canSetTile)
                        {
                            //当点击的瓦片有农作物成熟时，不种下种子而是收获该农作物
                            if (currentCrop != null && currentCrop.canHarvest)
                            {
                                currentCrop.ProcessCropAction();
                            }
                            //否则种下当前种子
                            else
                            {
                                var seedCropDetail = CropManager.Instance.GetCropDetails(itemDetails.itemID);
                                if (CropManager.Instance.SeasonAvailable(seedCropDetail))
                                {
                                    EventHandler.CallPlantSeedEvent(itemDetails.itemID, currentTile);
                                    //种下种子后，背包中的种子数量减一
                                    EventHandler.CallDropItemEvent(itemDetails.itemID, mouseWorldPos, itemDetails.itemType);
                                    //播放种植音效
                                    EventHandler.CallPlaySoundEvent(SoundName.Plant);
                                }
                                else
                                {
                                    Debug.Log("No season!!!");
                                }
                            }

                        }
                        
                        break;
                        //种下树种
                    case ItemType.TreeSeed:
                        if (canSetTile)
                        {
                            if (itemDetails.itemID == 1100)
                            {
                                GameObject treeCropPrefab0 = Instantiate(treePrefab[0], new Vector3(currentTile.gridX + 0.5f, currentTile.gridY), Quaternion.identity);
                                treeCropPrefab0.GetComponent<TreeGrowing>().InitTreeGrowDay();
                            }
                            if (itemDetails.itemID == 1102)
                            {
                                GameObject treeCropPrefab1 = Instantiate(treePrefab[1], new Vector3(currentTile.gridX + 0.5f, currentTile.gridY), Quaternion.identity);
                                treeCropPrefab1.GetComponent<TreeGrowing>().InitTreeGrowDay();
                            }
                            //已种下树种的Tile周围不可以再种或生成新的树
                            for (int x = currentTile.gridX - 2; x <= currentTile.gridX + 2; x++)
                            {
                                for (int y = currentTile.gridY - 2; y <= currentTile.gridY + 2; y++)
                                {

                                    aroundTreeTileDetail = GetTileDetailsOnMousePosition(new Vector3Int(x, y));
                                    if (aroundTreeTileDetail != null)
                                    {
                                        aroundTreeTileDetail.haveTree = 1;
                                    }
                                }
                            }
                            //种下种子后，背包中的种子数量减一
                            EventHandler.CallRemoveTreeSeedCount(itemDetails.itemID);
                        }
                        break;
                    //当丢弃商品类型的物品时，直接在鼠标的世界坐标位置生成一个丢弃商品
                    case ItemType.Commodity:
                        //执行收割方法
                        //currentCrop.ProcessToolAction(itemDetails, currentTile);
                        
                        if (currentCrop != null && currentCrop.canHarvest)
                        {
                            //播放玩家收获动画
                            EventHandler.CallDisplayCollectItemSprite(currentCrop.cropDetails.producedItemID[0]);
                            currentCrop.ProcessCropAction();
                        }
                        else
                        {
                            EventHandler.CallDropItemEvent(itemDetails.itemID, mouseWorldPos, itemDetails.itemType);
                        }
                        break;
                        //挖过的瓦片开始计时挖的天数，不可再挖，不可扔东西
                    case ItemType.HoeTool:
                        if(canSetTile)
                        {
                            SetDigGround(currentTile);
                            currentTile.daysSinceDug = 0;
                            currentTile.canDig = false;
                            currentTile.canDropItem = false;
                        }
                        else
                        {
                            break;
                        }
                        EventHandler.CallPlayerDecreaseStminaEvent(2);
                        EventHandler.CallParticleEffectEvent(ParticalEffectType.HoeEffect,new Vector3(mouseGridPos.x, mouseGridPos.y+0.5f,mouseGridPos.z), Vector2.zero);
                       
                        //音效
                        break;
                    case ItemType.WaterTool:
                        if (canSetTile)
                        {
                            SetWaterGround(currentTile);
                            currentTile.daysSinceWatered = 0;
                        }
                        EventHandler.CallPlayerDecreaseStminaEvent(1);
                       
                        //音效
                        break;
                    case ItemType.BreakTool:
                        if (Mathf.Abs(mouseGridPos.x - playerGridPos.x) <= itemDetails.itemUseRadius && Mathf.Abs(mouseGridPos.y - playerGridPos.y) <= itemDetails.itemUseRadius)
                        {
                            ableCollider = Physics2D.OverlapPoint(mouseWorldPos, knockableLayer);
                            if (ableCollider != null)
                            {
                                ableCollider.GetComponent<KnockableItem>().KnockItems(currentTile);
                            }
                            else
                            {
                                //播放锄到地上的粒子特效
                                EventHandler.CallParticleEffectEvent(ParticalEffectType.EarthenEffect, new Vector2(mouseGridPos.x + 0.5f,mouseGridPos.y + 0.5f), Vector2.zero);
                            }

                        }
                        break;
                    case ItemType.AxeTool:
                        if (Mathf.Abs(mouseGridPos.x - playerGridPos.x) <= itemDetails.itemUseRadius && Mathf.Abs(mouseGridPos.y - playerGridPos.y) <= itemDetails.itemUseRadius)
                        {
                            ableCollider = Physics2D.OverlapPoint(mouseWorldPos, axeableLayer);
                            if (ableCollider != null)
                            {
                                ableCollider.GetComponent<KnockableItem>().AxeItems(currentTile);
                            }
                            else
                            {
                                EventHandler.CallParticleEffectEvent(ParticalEffectType.EarthenEffect, new Vector2(mouseGridPos.x + 0.5f, mouseGridPos.y + 0.5f), Vector2.zero);
                            }

                        }
                        //currentCrop?.ProcessToolAction(itemDetails, currentCrop.tileDetails);
                        EventHandler.CallPlayerDecreaseStminaEvent(5);
                        break;
                        
                     case ItemType.Furniture:
                        //在地图上生成家具 ItemManager
                        //移除当前图纸 InventoryManager
                        //移除资源物品 InventoryManager
                        EventHandler.CallBuildFurnitureEvent(itemDetails.itemID,mouseWorldPos);
                        break;
                }   
                UpdateTileDetails(currentTile);
            }
        }
        /// <summary>
        /// 通过物理方法判断鼠标点击位置的农作物，用于收获
        /// </summary>
        /// <param name="mouseWorldPos"></param>
        /// <returns></returns>
        public Crop GetCropObject(Vector3 mouseWorldPos)
        {
            //获取鼠标点击位置的周围的所有碰撞体
            Collider2D[] colliders = Physics2D.OverlapPointAll(mouseWorldPos);
            Crop currentCrop = null;
            //并且获取碰撞体中的Crop脚本
            for(int i =0; i < colliders.Length; i++)
            {
                if (colliders[i].GetComponent<Crop>())
                {
                    currentCrop = colliders[i].GetComponent<Crop>();
                }
            }
            return currentCrop;
        }
        /// <summary>
        /// 鼠标选择范围内是否有可收割的场景
        /// </summary>
        /// <returns></returns>
        //public bool HaveReapableItemsInRadius(Vector3 playerPos,ItemDetails tool)
        //{
        //    //初始化杂草列表
        //    itemsInRadius = new List<ReapItem>();
        //    Collider2D[] colliders = new Collider2D[20];
        //    //Input.mousePosition检测坐标中心，Tool.itemUseRadius检测范围，colliders检测目标
        //    Physics2D.OverlapCircleNonAlloc(playerPos, tool.itemUseRadius, colliders);
        //    if(colliders.Length > 0)
        //    {
        //        for(int i = 0; i < colliders.Length; i++)
        //        {
        //            if (colliders[i] != null)
        //            {
        //                if (colliders[i].GetComponent<ReapItem>())
        //                {
        //                    var item = colliders[i].GetComponent<ReapItem>();
        //                    itemsInRadius.Add(item);
        //                }
        //            }
        //        }
        //    }
        //    //实时监控鼠标周围的杂草，当没有杂草时，方法一直在初始化杂草列表返回false，当有杂草在列表时，返回true
        //    return itemsInRadius.Count > 0;
        //}
        /// <summary>
        /// 获取鼠标周围可收割场景的动画
        /// </summary>
        /// <param name="mouseWorldPos"></param>
        /// <param name="tool"></param>
        /// <returns></returns>
        //public bool GetGrassBreakAnimator(Vector3 mouseWorldPos, ItemDetails tool)
        //{
        //    grassBreakAnims = new List<Animator>();
        //    Collider2D[] collider2Ds = new Collider2D[20];

        //    Physics2D.OverlapCircleNonAlloc(mouseWorldPos, tool.itemUseRadius, collider2Ds);
        //    if(collider2Ds.Length > 0)
        //    {
        //        for( int i = 0;i < collider2Ds.Length; i++)
        //        {
        //            if (collider2Ds[i] != null)
        //            {
        //                if (collider2Ds[i].GetComponent<Animator>())
        //                {
        //                    var grassAnim = collider2Ds[i].GetComponent<Animator>();
        //                    grassBreakAnims.Add(grassAnim);
        //                }
        //            }
        //        }
        //    }
        //    return itemsInRadius.Count > 0;
        //}
        /// <summary>
        /// 显示挖坑瓦片
        /// </summary>
        /// <param name="tile"></param>
        private void SetDigGround(TileDetails tile)
        {
            Vector3Int pos = new Vector3Int(tile.gridX, tile.gridY, 0);
            if(digTilemap!=null)
            {
                //再pos位置显示Rule Tile digTile
                digTilemap.SetTile(pos, digTile);
            }
        }
        /// <summary>
        /// 显示浇水瓦片
        /// </summary>
        /// <param name="tile"></param>
        private void SetWaterGround(TileDetails tile)
        {
            Vector3Int pos = new Vector3Int(tile.gridX, tile.gridY, 0);
            if (waterTilemap != null)
            {
                waterTilemap.SetTile(pos, waterTile);
            }
        }
       
       
        /// <summary>
        /// 存储瓦片挖坑的数据
        /// </summary>
        /// <param name="tileDetails"></param>
        public void UpdateTileDetails(TileDetails tileDetails)
        {
            //获取挖过坑的瓦片关键字
            string key = tileDetails.gridX + "X" + tileDetails.gridY + "Y" + SceneManager.GetActiveScene().name;
            if(tileDetailsDict.ContainsKey(key))
            {
                tileDetailsDict[key] = tileDetails;
            }
            else
            {
                tileDetailsDict.Add(key, tileDetails);
            }
        }
        /// <summary>
        /// 刷新地图信息
        /// </summary>
        private void OnRefreshCurrentMap()
        {
            if(digTilemap != null)
            {
                digTilemap.ClearAllTiles();
            }
            if (waterTilemap != null)
            {
                waterTilemap.ClearAllTiles();
            }
            //遍历场景中所有的带Crop脚本的物体并删除
            foreach(var crop in FindObjectsOfType<Crop>())
            {
                Destroy(crop.gameObject);
            }
            DisplayMap(SceneManager.GetActiveScene().name);
        }
        /// <summary>
        /// 进入场景后把存储好的瓦片信息重新加载出来到场景中
        /// </summary>
        /// <param name="sceneName"></param>
        private void DisplayMap(string sceneName)
        {
            foreach(var tile in tileDetailsDict)
            {
                var key = tile.Key;
                var tileDetails = tile.Value;
                if (key.Contains(sceneName))
                {
                    if(tileDetails.daysSinceDug > -1)
                    {
                        SetDigGround(tileDetails);
                    }
                    if(tileDetails.daysSinceWatered > -1)
                    {
                        SetWaterGround(tileDetails);
                    }
                    if(tileDetails.seedItemID > -1)
                    {
                        EventHandler.CallPlantSeedEvent(tileDetails.seedItemID,tileDetails);
                    }
                }
            }
        }
        /// <summary>
        /// 根据传进来的场景名字构建网格范围，输出范围和原点
        /// </summary>
        /// <param name="sceneName">场景名字</param>
        /// <param name="gridDimensions">网格范围</param>
        /// <param name="gridOrigin">地图左下角原点</param>
        /// <returns>是否有当前场景的信息</returns>
        public bool GetGridDimensions(string sceneName,out Vector2Int gridDimensions,out Vector2Int gridOrigin)
        {
            //初始化赋值
            gridDimensions = Vector2Int.zero;
            gridOrigin = Vector2Int.zero;
            foreach(var mapData in mapDataList)
            {
                if(mapData.sceneName == sceneName)
                {
                    gridDimensions.x = mapData.gridWidth;
                    gridDimensions.y = mapData.gridHeight;

                    gridOrigin.x = mapData.originX;
                    gridOrigin.y = mapData.originY;

                    return true;
                } 
            }
            return false;
        }
        /// <summary>
        /// 显示可互动瓦片
        /// </summary>
        /// <param name="mouseGridPos"></param>
        public void DisplayerAvailableGround(ItemDetails toolDetails, TileDetails currentTile)
        {
            
            var playerGridPos = currentGrid.WorldToCell(playerTransform.position);
            valueMap.gameObject.GetComponent<TilemapRenderer>().enabled = true;
            foreach (var tile in tileDetailsDict)
            {
                //鼠标此时选中的Tile
                if (tile.Value.gridX == currentTile.gridX && tile.Value.gridY == currentTile.gridY)
                {
     
                    //Tile在工具使用范围外
                    if (Mathf.Abs(currentTile.gridX - playerGridPos.x) > toolDetails.itemUseRadius || Mathf.Abs(currentTile.gridY - playerGridPos.y) > toolDetails.itemUseRadius)
                    {
                        
                        canSetTile = false;
                        valueMap.SetTile(new Vector3Int(tile.Value.gridX, tile.Value.gridY), null);
                    }
                    //Tile在工具使用范围内
                    else
                    {
                        switch (toolDetails.itemType) 
                        {
                            case ItemType.HoeTool:
                                //Tile可以挖掘
                                if (currentTile.canDig && currentTile.daysSinceDug == -1)
                                {
                                    canSetTile = true;
                                    valueMap.SetTile(new Vector3Int(tile.Value.gridX, tile.Value.gridY), availableTilemap);
                                }
                                //Tile不可以被挖掘
                                else
                                {
                                    canSetTile = false;
                                    valueMap.SetTile(new Vector3Int(tile.Value.gridX, tile.Value.gridY), unAvailableTilemap);
                                }
                                break;
                            case ItemType.WaterTool:
                                //Tile可以浇水
                                if (currentTile.daysSinceDug > -1 && currentTile.daysSinceWatered == -1)
                                {
                                    canSetTile = true;
                                    valueMap.SetTile(new Vector3Int(tile.Value.gridX, tile.Value.gridY), availableTilemap);
                                }
                                //Tile不可以浇水
                                else
                                {
                                    canSetTile = false;
                                    valueMap.SetTile(new Vector3Int(tile.Value.gridX, tile.Value.gridY), unAvailableTilemap);
                                }
                                break;
                            case ItemType.TreeSeed:
                                if (currentTile.canTree && currentTile.haveTree == -1)
                                {
                                    canSetTile = true;
                                    valueMap.SetTile(new Vector3Int(tile.Value.gridX, tile.Value.gridY), availableTilemap);
                                }
                                else
                                {
                                    canSetTile = false;
                                    valueMap.SetTile(new Vector3Int(tile.Value.gridX, tile.Value.gridY), unAvailableTilemap);
                                }
                                break;
                            case ItemType.Seed:
                               
                                if (currentTile.daysSinceDug > -1 && currentTile.seedItemID == -1)
                                {
                                    canSetTile = true;
                                    valueMap.SetTile(new Vector3Int(tile.Value.gridX, tile.Value.gridY), availableTilemap);
                                }
                                else
                                {
                                    canSetTile = false;
                                    valueMap.SetTile(new Vector3Int(tile.Value.gridX, tile.Value.gridY), unAvailableTilemap);
                                }
                                break;

                        }
                    }
                }
                //鼠标没选中的Tile
                else
                {
                    valueMap.SetTile(new Vector3Int(tile.Value.gridX, tile.Value.gridY), null);
                }
            }


        }
       

        /// <summary>
        /// 关闭可耕种瓦片
        /// </summary>
        public void QuitDigAvailableGround()
        {
            valueMap.gameObject.GetComponent<TilemapRenderer>().enabled = false;
        }
        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.tileDetailsDict = this.tileDetailsDict;
            saveData.firstLoadDict = this.firstLoadDict;
            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            this.tileDetailsDict = saveData.tileDetailsDict;
            this.firstLoadDict = saveData.firstLoadDict;
        }
    }
}

