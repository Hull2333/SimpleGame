using MFarm.Map;
using MFarm.Save;
using MFarm.Transition;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace MFarm.Inventory
{
    public class ItemManager : MonoBehaviour, ISaveable    //调用在ItemManager对象上
    {

        public Item itemPerfab;
        public Item bounceItemPerfab;
        public KnockableItem knockableItemPerfab1;
        public KnockableItem knockableItemPerfab2;
        public KnockableItem knockableItemPerfab3;
        public KnockableItem knockableItemPerfab4;
        public ReapableItem weedItemPerfab;
        public ReapableItem bigWeedItemPerfab;
        private Transform itemParent;
        private Transform buildingParent;
        private Transform animalParent;
        private Transform furnitureParent;
        private Transform playerTransform => FindObjectOfType<PlayerController>().transform;
        //所有建造场景的名字
        public string[] coopSceneNames;
        public string GUID => GetComponent<DataGUID>().guid;
        public MineSceneDataList_SO mineData;
        //用字典的方式存储每个场景的物品信息，key为场景名，value为该场景的物品列表
        private Dictionary<string, List<SceneItem>> sceneItemDict = new Dictionary<string, List<SceneItem>>();
        //用字典的方式存储每个场景的家具信息，key为场景名，value为该场景的家具列表
        private Dictionary<string, List<SceneFurniture>> sceneFurnitureDict = new Dictionary<string, List<SceneFurniture>>();
        //用字典的方式存储每个场景的建筑信息，key为场景名，value为该场景的建筑列表
        private Dictionary<string, List<SceneBuilding>> sceneBuildingDict = new Dictionary<string, List<SceneBuilding>>();
        //用字典的方式存储每个场景的可敲击物品信息，key为场景名，value为该场景的可敲击物品列表
        public Dictionary<string, List<SceneKnockItem>> sceneKnockItemDict = new Dictionary<string, List<SceneKnockItem>>();
        //用字典的方式存储每个场景的可收割物品信息，key为场景名，value为该场景的可收割物品列表
        public Dictionary<string, List<SceneReapableItem>> sceneWeedItemDict = new Dictionary<string, List<SceneReapableItem>>();
        public Dictionary<string, List<SceneReapableItem>> sceneBigWeedItemDict = new Dictionary<string, List<SceneReapableItem>>();
        //用字典的方式存储每个场景的动物信息，key为场景名，value为该场景的动物列表
        public Dictionary<string, List<SceneAnimal>> sceneAnimalDict = new Dictionary<string, List<SceneAnimal>>();
        //用字典的方式存储每个建造场景的家具信息，key为建造场景的buildCode，value为该建造场景的家具列表
        public Dictionary<int, List<SceneFurniture>> buildFurnitureDict = new Dictionary<int, List<SceneFurniture>>();
        public int currentBuildCode;
        private Teleport teleport;
        private Vector3 buildSceneTeleportToGo;
        [Header("动物生成相关")]
        //动物生产的物品列表，存储每个建造场景的动物生产的物品信息
        public List<buildProduceItem> buildProduceItemList = new List<buildProduceItem>();
        //用字典的方式存储建造建筑场景的物品点击信息，key为建造场景的buildCode，value为该建造场景的物品点击列表
        public Dictionary<int, List<SceneItem>> buildItemClickDict = new Dictionary<int, List<SceneItem>>();
        public ItemClick itemClickPrefab;
        public MapData_SO ChickenCoopMap;
        [Header("建造建筑相关")]
        private HashSet<int> buildCodeIDs = new HashSet<int>();
        //记录每一个动物进入建造建筑的数量，key为建造建筑的buildCode，value为该建造建筑的动物列表
        public Dictionary<int, List<SceneAnimal>> buildAnimalCountDict = new Dictionary<int, List<SceneAnimal>>();
        //建造建筑的活动范围
        public List<BuildColliderArea> buildAreaList = new List<BuildColliderArea>();
        //今天是否已经生成过室外动物的标志位，防止在同一天内重复生成
        private bool hasSpawnedOutdoorToday;  
        private void OnEnable()
        {
            EventHandler.InstantiateItemInScene += OnInstantiateItemInScene;
            EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
            EventHandler.DropItemEvent += OnDropItemEvent;
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadeEvent;
            EventHandler.BuildFurnitureEvent += OnBuildFurnitureEvent;
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
            EventHandler.InstantiateBuildingOnMapEvent += OnInstantiateBuildingOnMapEvent;
            EventHandler.GetCurrentBuildCode += OnGetCurrentBuildCode;
            EventHandler.InstantiateAniamlProduceItemEvent += OnInstantiateAniamlProduceItemEvent;
            EventHandler.AnimalArrivedAtHomeEvent += OnAnimalArrivedAtHomeEvent;
            EventHandler.AnimalExitCoopEvent += OnAnimalExitCoopEvent;
            EventHandler.GameMinuteEvent += OnGameMinuteEvent;
        }



        private void OnDisable()
        {
            EventHandler.InstantiateItemInScene -= OnInstantiateItemInScene;
            EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
            EventHandler.DropItemEvent -= OnDropItemEvent;
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadeEvent;
            EventHandler.BuildFurnitureEvent -= OnBuildFurnitureEvent;
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
            EventHandler.InstantiateBuildingOnMapEvent -= OnInstantiateBuildingOnMapEvent;
            EventHandler.GetCurrentBuildCode -= OnGetCurrentBuildCode;
            EventHandler.InstantiateAniamlProduceItemEvent += OnInstantiateAniamlProduceItemEvent;
            EventHandler.AnimalArrivedAtHomeEvent -= OnAnimalArrivedAtHomeEvent;
            EventHandler.AnimalExitCoopEvent -= OnAnimalExitCoopEvent;
            EventHandler.GameMinuteEvent -= OnGameMinuteEvent;
        }

       

        private void Start()
        {
            ISaveable saveable = this;
            saveable.RegisterSaveable();
        }
        private void OnBuildFurnitureEvent(BluPrintDetails bluPrintDetails, Vector3 bluPrintPos, Transform parent)
        {
            var bluPrintGameObject = Instantiate(bluPrintDetails.buildPrefab, bluPrintPos, Quaternion.identity, parent);
            bluPrintGameObject.GetComponent<Furniture>().SetCollider(true);
            bluPrintGameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1f);
            //记录箱子的索引值，方便存储和读取箱子数据
            if (bluPrintGameObject.GetComponent<Box>())
            {
                bluPrintGameObject.GetComponent<Box>().index = InventoryManager.Instance.boxDataAmount;
                bluPrintGameObject.GetComponent<Box>().InitBox(bluPrintGameObject.GetComponent<Box>().index);
            }
        }

        private void OnBeforeSceneUnloadEvent()
        {
            if (ExcludeMineScene(SceneManager.GetActiveScene().name))
            {
                if (IsCurrentSceneOfCoop())
                {
                    GetAllBuildFurniture();
                    SaveBuildSceneItemClick();
                }
                else
                {
                    GetAllSceneItems();
                    GetAllSceneFurniture();
                    GetAllSceneBuilding();
                    GetAllSceneAnimal();
                    GetAllSceneKnockItem();
                    GetAllSceneReapableItem();
                }
            }

        }
        private void OnAfterSceneLoadeEvent()
        {
            itemParent = GameObject.FindWithTag("ItemParent").transform;
            buildingParent = FindAnyObjectByType<BuildingParent>().transform;
            animalParent = FindAnyObjectByType<AnimalParent>().transform;
            furnitureParent = FindAnyObjectByType<FurnitureParent>().transform;
            if (ExcludeMineScene(SceneManager.GetActiveScene().name))
            {
                if (IsCurrentSceneOfCoop())
                {
                    RecreateBuildFurniture();
                    teleport = FindAnyObjectByType<Teleport>();
                    teleport.positionToGo = buildSceneTeleportToGo;
                    //临时存储需要移除的物品列表，避免在遍历时修改原列表导致异常
                    List<buildProduceItem> toRemoveItem = new List<buildProduceItem>();
                    //遍历建造建筑的生产物品列表，找到当前建造建筑对应的生产物品，并在随机位置生成物品
                    foreach (buildProduceItem item in buildProduceItemList)
                    {
                        if (item.buildCode == currentBuildCode)
                        {
                            var producePos = GridMapManager.Instance.GetRandomPlaceFurnitureTile(ChickenCoopMap);
                            if (producePos != null)
                            {
                                var produceItem = Instantiate(itemClickPrefab, new Vector3(producePos.gridX + 0.5f, producePos.gridY + 0.5f, 0), Quaternion.identity, itemParent);
                                produceItem.itemID = item.itemID;
                                produceItem.SetItemSprite();
                            }
                            //删除当前建造建筑对应的生产物品，避免重复生成
                            toRemoveItem.Add(item);
                        }
                    }
                    foreach (var item in toRemoveItem)
                    {
                        buildProduceItemList.Remove(item);
                    }
                    RecreateBuildSceneItemClick();
                    RecreateBuildSceneAnimal();
                }
                else
                {
                    RecreateAllItem();
                    RebuilFurniture();
                    ReBuildBuilding();
                    RecreateAnimal();
                    RecreateKnockItem();
                    RecreateReapableItem();
                }

            }
        }
        /// <summary>
        /// 重新生成场景中的物品，主要用于场景切换后恢复物品状态
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="pos"></param>
        private void OnInstantiateItemInScene(int ID, Vector3 pos)
        {
            EventHandler.CallItemFirstPos(pos);
            var item = Instantiate(bounceItemPerfab, pos, Quaternion.identity, itemParent);
            item.itemID = ID;
        }

        private void OnDropItemEvent(int ID, Vector3 mousePos, ItemType itemType)
        {
            //种子类型的物品不需要生成弹跳物品，直接返回
            if (itemType == ItemType.Seed)
            {
                return;
            }
            //TODO:�Ӷ�����Ч��
            var item = Instantiate(bounceItemPerfab, playerTransform.position, Quaternion.identity, itemParent);
            item.itemID = ID;
            var dir = (mousePos - playerTransform.position).normalized;
            item.GetComponent<ItemBounce>().InitBounceItem(mousePos, dir);
        }

        private void OnStartNewGameEvent(int obj)
        {
            sceneItemDict.Clear();
            sceneFurnitureDict.Clear();
            sceneBuildingDict.Clear();
        }
        private void OnInstantiateBuildingOnMapEvent(BuildingDetails building, Vector3 pos, Transform transform)
        {
            var buildingInMap = Instantiate(building.buildPrefab, pos, Quaternion.identity, transform);
            buildingInMap.GetComponent<BuildingItem>().Building(true);
            //生成唯一的建筑物标识ID
            int buildCode;
            do
            {
                buildCode = Random.Range(0, 256);
            }
            while (buildCodeIDs.Contains(buildCode));
            buildCodeIDs.Add(buildCode);
            buildingInMap.GetComponent<BuildingItem>().buildCodeID = buildCode;
            buildingInMap.GetComponent<BuildingItem>().SwitchCollider2D(true);
            buildingInMap.GetComponent<BuildingItem>().isSet = true;
            buildingInMap.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1f);
        }
        private void OnGetCurrentBuildCode(int code, Vector3 toGoPos)
        {
            currentBuildCode = code;
            Debug.Log(toGoPos);
            buildSceneTeleportToGo = toGoPos;
        }
        private void OnInstantiateAniamlProduceItemEvent(int code, int itemID)
        {
            buildProduceItemList.Add(new buildProduceItem { buildCode = code, itemID = itemID });
        }
        private void OnAnimalArrivedAtHomeEvent(int buildCode, SceneAnimal animal)
        {
            if (!buildAnimalCountDict.ContainsKey(buildCode))
                buildAnimalCountDict[buildCode] = new List<SceneAnimal>();
            buildAnimalCountDict[buildCode].Add(animal);
        }
        private void OnAnimalExitCoopEvent(int buildCode, SceneAnimal animal)
        {
            // 遍历建筑中的动物
            if (buildAnimalCountDict.ContainsKey(buildCode))
            {
                var list = buildAnimalCountDict[buildCode];
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].animalCode == animal.animalCode && list[i].growthDay == animal.growthDay)
                    {
                        list.RemoveAt(i);
                        break; 
                    }
                }
            }
            // 来到Farm场景的动物添加到Farm场景的动物字典中
            if (!sceneAnimalDict.ContainsKey("Farm"))
                sceneAnimalDict["Farm"] = new List<SceneAnimal>();
            sceneAnimalDict["Farm"].Add(animal);
        }
        private void OnGameMinuteEvent(int minute, int hour, int day, Season season)
        {
            // 当前场景不是Farm就不执行该方法
            if (SceneManager.GetActiveScene().name != "Farm") return;
            // 8:00刚刚到动物开始从建筑中走出
            if (hour == 8 && minute == 0 && !hasSpawnedOutdoorToday)
            {
                hasSpawnedOutdoorToday = true;
                SpawnOutdoorAnimals();
            }
            // 12:00后重置hasSpawnedOutdoorToday
            if (hour >= 12)
            {
                hasSpawnedOutdoorToday = false;
            }
        }
        /// <summary>
        /// 排除矿洞场景
        /// </summary>
        /// <param name="sceneName">矿洞场景名</param>
        /// <returns></returns>
        private bool ExcludeMineScene(string sceneName)
        {
            foreach (var scene in mineData.mineSceneList)
            {
                if (scene.sceneName == sceneName)
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 获取所有场景的物品信息
        /// </summary>
        private void GetAllSceneItems()
        {
            //当前场景的所有物品信息
            List<SceneItem> currentSceneItems = new List<SceneItem>();
            foreach (var item in FindObjectsOfType<Item>())
            {
                SceneItem sceneItem = new SceneItem()
                {
                    itemID = item.itemID,
                    position = new SerializableVector3(item.transform.position)
                };
                currentSceneItems.Add(sceneItem);
            }
            //有当前场景的key就更新当前场景的物品信息，没有就添加新的key-value对
            if (sceneItemDict.ContainsKey(SceneManager.GetActiveScene().name))
            {
                sceneItemDict[SceneManager.GetActiveScene().name] = currentSceneItems;
            }
            else
            {
                sceneItemDict.Add(SceneManager.GetActiveScene().name, currentSceneItems);
            }

        }
        /// <summary>
        /// 获取所有场景的家具信息
        /// </summary>
        private void GetAllSceneFurniture()
        {
            //�洢��ǰ��������Ʒ
            List<SceneFurniture> currentSceneFurniture = new List<SceneFurniture>();
            foreach (var item in FindObjectsOfType<Furniture>())
            {
                SceneFurniture sceneFurniture = new SceneFurniture()
                {
                    itemID = item.itemID,
                    position = new SerializableVector3(item.transform.position)
                };
                //该家具是箱子的话，记录箱子的索引值，方便存储和读取箱子数据
                if (item.GetComponent<Box>())
                {
                    sceneFurniture.boxIndex = item.GetComponent<Box>().index;
                }
                currentSceneFurniture.Add(sceneFurniture);
            }
            if (sceneFurnitureDict.ContainsKey(SceneManager.GetActiveScene().name))
            {
                sceneFurnitureDict[SceneManager.GetActiveScene().name] = currentSceneFurniture;
            }
            else
            {
                sceneFurnitureDict.Add(SceneManager.GetActiveScene().name, currentSceneFurniture);
            }
        }
        /// <summary>
        /// 获取所有场景的建筑信息
        /// </summary>
        private void GetAllSceneBuilding()
        {
            List<SceneBuilding> currentSceneBuinding = new List<SceneBuilding>();
            foreach (var building in FindObjectsOfType<BuildingItem>())
            {
                if (building.isSet)
                {
                    SceneBuilding sceneBuinding = new SceneBuilding
                    {
                        buildID = building.buildID,
                        buildCodeID = building.buildCodeID,
                        buildDay = building.currentBuildingDay,
                        position = new SerializableVector3(building.transform.position),
                        acceptAnimalSize = building.acceptSize,
                        isDone = building.isDone
                    };
                    currentSceneBuinding.Add(sceneBuinding);
                }
            }
            if (sceneBuildingDict.ContainsKey(SceneManager.GetActiveScene().name))
            {
                sceneBuildingDict[SceneManager.GetActiveScene().name] = currentSceneBuinding;
            }
            else
            {
                sceneBuildingDict.Add(SceneManager.GetActiveScene().name, currentSceneBuinding);
            }
        }
        /// <summary>
        /// 获取所有场景的可敲击物品信息
        /// </summary>
        private void GetAllSceneKnockItem()
        {
            List<SceneKnockItem> currentSceneKnockItems = new List<SceneKnockItem>();
            foreach (var knockItem in FindObjectsOfType<KnockableItem>())
            {
                SceneKnockItem sceneItem = new SceneKnockItem()
                {
                    position = new SerializableVector3(knockItem.transform.position),
                    itemIndex = knockItem.rockIndex
                };
                currentSceneKnockItems.Add(sceneItem);
            }
            if (sceneItemDict.ContainsKey(SceneManager.GetActiveScene().name))
            {
                sceneKnockItemDict[SceneManager.GetActiveScene().name] = currentSceneKnockItems;
            }
            else
            {
                sceneKnockItemDict.Add(SceneManager.GetActiveScene().name, currentSceneKnockItems);
            }
        }
        /// <summary>
        /// 获取所有场景的可收割物品信息
        /// </summary>
        private void GetAllSceneReapableItem()
        {
            List<SceneReapableItem> currentSceneWeedItems = new List<SceneReapableItem>();
            List<SceneReapableItem> currentSceneBigWeedItems = new List<SceneReapableItem>();
            foreach (var reapableItem in FindObjectsOfType<ReapableItem>())
            {
                SceneReapableItem sceneItem = new SceneReapableItem()
                {
                    position = new SerializableVector3(reapableItem.transform.position),
                    sprite = reapableItem.GetComponentInChildren<SpriteRenderer>().sprite
                };
                if (reapableItem.isWeeds)
                {
                    currentSceneWeedItems.Add(sceneItem);
                }
                if (reapableItem.isBigWeeds)
                {
                    currentSceneBigWeedItems.Add(sceneItem);
                }
            }
            if (sceneItemDict.ContainsKey(SceneManager.GetActiveScene().name))
            {
                sceneWeedItemDict[SceneManager.GetActiveScene().name] = currentSceneWeedItems;
                sceneBigWeedItemDict[SceneManager.GetActiveScene().name] = currentSceneBigWeedItems;
            }
            else
            {
                sceneWeedItemDict.Add(SceneManager.GetActiveScene().name, currentSceneWeedItems);
                sceneBigWeedItemDict.Add(SceneManager.GetActiveScene().name, currentSceneBigWeedItems);
            }
        }
        /// <summary>
        /// 获取所有场景的动物信息
        /// </summary>
        private void GetAllSceneAnimal()
        {
            List<SceneAnimal> currentSceneAnimalList = new List<SceneAnimal>();
            foreach (var animal in FindObjectsOfType<AnimalController>())
            {
                SceneAnimal sceneAnimal = new SceneAnimal
                {
                    animalDetails = animal.animalDetails,
                    animalCode = animal.animCodeID,
                    growthDay = animal.currentGrowthDay,
                    isOutSide = animal.isOutSide
                };
                currentSceneAnimalList.Add(sceneAnimal);
            }
            if (sceneAnimalDict.ContainsKey(SceneManager.GetActiveScene().name))
            {
                sceneAnimalDict[SceneManager.GetActiveScene().name] = currentSceneAnimalList;
            }
            else
            {
                sceneAnimalDict.Add(SceneManager.GetActiveScene().name, currentSceneAnimalList);
            }
        }
        /// <summary>
        /// 获取当前建造场景的所有家具信息
        /// </summary>
        private void GetAllBuildFurniture()
        {
            List<SceneFurniture> currentBuildFurnitureList = new List<SceneFurniture>();
            foreach (var item in FindObjectsOfType<Furniture>())
            {
                SceneFurniture sceneFurniture = new SceneFurniture()
                {
                    itemID = item.itemID,
                    position = new SerializableVector3(item.transform.position)
                };
                if (item.GetComponent<Box>())
                {
                    sceneFurniture.boxIndex = item.GetComponent<Box>().index;
                }
                currentBuildFurnitureList.Add(sceneFurniture);
            }
            if (buildFurnitureDict.ContainsKey(currentBuildCode))
            {
                buildFurnitureDict[currentBuildCode] = currentBuildFurnitureList;
            }
            else
            {
                buildFurnitureDict.Add(currentBuildCode, currentBuildFurnitureList);
            }

        }
        /// <summary>
        /// 保存当前建造场景的可点击物品信息
        /// </summary>
        private void SaveBuildSceneItemClick()
        {
            List<SceneItem> currentItems = new List<SceneItem>();
            foreach (var item in FindObjectsOfType<ItemClick>())
            {
                currentItems.Add(new SceneItem
                {
                    itemID = item.itemID,
                    position = new SerializableVector3(item.transform.position)
                });
            }
            if (buildItemClickDict.ContainsKey(currentBuildCode))
                buildItemClickDict[currentBuildCode] = currentItems;
            else
                buildItemClickDict.Add(currentBuildCode, currentItems);
        }
        /// <summary>
        /// 重新加载当前场景的所有物品，主要用于场景切换后恢复物品状态
        /// </summary>
        private void RecreateAllItem()
        {
            List<SceneItem> currentSceneItems = new List<SceneItem>();
            if (sceneItemDict.TryGetValue(SceneManager.GetActiveScene().name, out currentSceneItems))
            {
                if (currentSceneItems != null)
                {
                    //先初始化当前场景的所有物品，避免重复生成
                    foreach (var item in FindObjectsOfType<Item>())
                    {
                        Destroy(item.gameObject);
                    }
                    //开始重新生成当前场景的所有物品
                    foreach (var item in currentSceneItems)
                    {
                        Item newItem = Instantiate(itemPerfab, item.position.ToVector3(), Quaternion.identity, itemParent);
                        newItem.Init(item.itemID);
                    }
                }
            }
        }
        /// <summary>
        /// 重新加载当前场景的所有家具，主要用于场景切换后恢复家具状态
        /// </summary>
        private void RebuilFurniture()
        {
            List<SceneFurniture> currentSceneFurniture = new List<SceneFurniture>();
            if (sceneFurnitureDict.TryGetValue(SceneManager.GetActiveScene().name, out currentSceneFurniture))
            {
                if (currentSceneFurniture != null)
                {
                    foreach (SceneFurniture sceneFurniture in currentSceneFurniture)
                    {
                        BluPrintDetails bluePrint = InventoryManager.Instance.bluPrintData.GetBluPrintDetails(sceneFurniture.itemID);
                        var buildItem = Instantiate(bluePrint.buildPrefab, sceneFurniture.position.ToVector3(), Quaternion.identity, furnitureParent);
                        //重新生成箱子
                        if (buildItem.GetComponent<Box>())
                        {
                            buildItem.GetComponent<Box>().InitBox(sceneFurniture.boxIndex);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 重新加载当前场景的所有建筑，主要用于场景切换后恢复建筑状态
        /// </summary>
        private void ReBuildBuilding()
        {
            buildAreaList.Clear();
            List<SceneBuilding> currentSceneBuilding = new List<SceneBuilding>();
            if (sceneBuildingDict.TryGetValue(SceneManager.GetActiveScene().name, out currentSceneBuilding))
            {
                if (currentSceneBuilding != null)
                {
                    foreach (SceneBuilding sceneBuilding in currentSceneBuilding)
                    {
                        BuildingDetails buildingDetails = InventoryManager.Instance.bluPrintData.GetBuildingDetails(sceneBuilding.buildID);
                        var buildItem = Instantiate(buildingDetails.buildPrefab, sceneBuilding.position.ToVector3(), Quaternion.identity, buildingParent);
                        buildItem.GetComponent<BuildingItem>().currentBuildingDay = sceneBuilding.buildDay;
                        buildItem.GetComponent<BuildingItem>().buildCodeID = sceneBuilding.buildCodeID;
                        buildItem.GetComponent<BuildingItem>().Building(true);
                        buildItem.GetComponent<BuildingItem>().isSet = true;
                        //并将建筑的活动范围添加到buildAreaList中
                        BuildColliderArea currentArea = new BuildColliderArea
                        {
                            code = sceneBuilding.buildCodeID,
                            area = buildItem.GetComponent<BuildingItem>().animalArea
                        };
                        buildAreaList.Add(currentArea);
                    }
                }
            }
        }
        /// <summary>
        /// 重新加载当前场景的所有可敲击物品，主要用于场景切换后恢复可敲击物品状态
        /// </summary>
        private void RecreateKnockItem()
        {

            List<SceneKnockItem> currentSceneItems = new List<SceneKnockItem>();
            if (sceneKnockItemDict.TryGetValue(SceneManager.GetActiveScene().name, out currentSceneItems))
            {
                if (currentSceneItems != null)
                {
                    //先初始化当前场景的所有可敲击物品，避免重复生成
                    foreach (var item in FindObjectsOfType<KnockableItem>())
                    {
                        Destroy(item.gameObject);
                    }
                    //根据可敲击物品的索引值，重新生成当前场景的所有可敲击物品
                    foreach (var item in currentSceneItems)
                    {
                        switch (item.itemIndex)
                        {
                            case 1:
                                Instantiate(knockableItemPerfab1, item.position.ToVector3(), Quaternion.identity);
                                break;
                            case 2:
                                Instantiate(knockableItemPerfab2, item.position.ToVector3(), Quaternion.identity);
                                break;
                            case 3:
                                Instantiate(knockableItemPerfab3, item.position.ToVector3(), Quaternion.identity);
                                break;
                            case 4:
                                Instantiate(knockableItemPerfab4, item.position.ToVector3(), Quaternion.identity);
                                break;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 重新加载当前场景的所有可收割物品，主要用于场景切换后恢复可收割物品状态
        /// </summary>
        private void RecreateReapableItem()
        {

            List<SceneReapableItem> currentWeedsItems = new List<SceneReapableItem>();
            List<SceneReapableItem> currentBigWeedsItems = new List<SceneReapableItem>();
            if (sceneWeedItemDict.TryGetValue(SceneManager.GetActiveScene().name, out currentWeedsItems))
            {
                if (currentWeedsItems != null)
                {
                    //先初始化当前场景的所有可收割物品，避免重复生成
                    foreach (var item in FindObjectsOfType<ReapableItem>())
                    {
                        Destroy(item.gameObject);
                    }
                    //生成当前场景的所有可收割物品
                    foreach (var item in currentWeedsItems)
                    {
                        var weedItem = Instantiate(weedItemPerfab, item.position.ToVector3(), Quaternion.identity);
                        weedItem.GetComponentInChildren<SpriteRenderer>().sprite = item.sprite;
                    }
                }
            }
            if (sceneBigWeedItemDict.TryGetValue(SceneManager.GetActiveScene().name, out currentBigWeedsItems))
            {
                if (currentBigWeedsItems != null)
                {
                    foreach (var item in currentBigWeedsItems)
                    {
                        var bigWeedItem = Instantiate(bigWeedItemPerfab, item.position.ToVector3(), Quaternion.identity);
                        bigWeedItem.GetComponentInChildren<SpriteRenderer>().sprite = item.sprite;
                    }
                }
            }
        }
        /// <summary>
        /// 重新加载当前场景的所有动物，主要用于场景切换后恢复动物状态
        /// </summary>
        private void RecreateAnimal()
        {
            List<SceneAnimal> currentSceneAnimal = new List<SceneAnimal>();
            if (sceneAnimalDict.TryGetValue(SceneManager.GetActiveScene().name, out currentSceneAnimal))
            {
                foreach (var animal in currentSceneAnimal)
                {
                    var animalInScene = Instantiate(animal.animalDetails.animalPrefab, animalParent);
                    animalInScene.GetComponent<AnimalController>().animalDetails = animal.animalDetails;
                    animalInScene.GetComponent<AnimalController>().currentGrowthDay = animal.growthDay;
                    animalInScene.GetComponent<AnimalController>().animCodeID = animal.animalCode;
                    animalInScene.GetComponent<AnimalController>().activityArae = GetBuildArea(animal.animalCode);
                    animalInScene.GetComponent<AnimalController>().isOutSide = animal.isOutSide;
                    animalInScene.GetComponent<AnimalController>().SetStartState(false);
                }
            }
        }
        /// <summary>
        /// 重新加载当前建造场景的所有家具，主要用于场景切换后恢复家具状态
        /// </summary>
        private void RecreateBuildFurniture()
        {
            List<SceneFurniture> currentBuildFurniture = new List<SceneFurniture>();
            if (buildFurnitureDict.TryGetValue(currentBuildCode, out currentBuildFurniture))
            {
                if (currentBuildFurniture != null)
                {
                    foreach (var furniture in currentBuildFurniture)
                    {
                        BluPrintDetails bluePrint = InventoryManager.Instance.bluPrintData.GetBluPrintDetails(furniture.itemID);
                        var buildItem = Instantiate(bluePrint.buildPrefab, furniture.position.ToVector3(), Quaternion.identity, furnitureParent);
                        if (buildItem.GetComponent<Box>())
                        {
                            buildItem.GetComponent<Box>().InitBox(furniture.boxIndex);
                        }
                    }
                }

            }

        }
        /// <summary>
        /// 重新加载当前建造场景的所有可点击物品，主要用于场景切换后恢复可点击物品状态
        /// </summary>
        private void RecreateBuildSceneItemClick()
        {
            if (buildItemClickDict.TryGetValue(currentBuildCode, out List<SceneItem> savedItems))
            {
                foreach (var item in savedItems)
                {
                    var produceItem = Instantiate(itemClickPrefab, item.position.ToVector3(), Quaternion.identity, itemParent);
                    produceItem.itemID = item.itemID;
                    produceItem.SetItemSprite();
                    string key = (item.position.x - 0.5f) + "X" + (item.position.y - 0.5f) + "Y" + SceneManager.GetActiveScene().name;
                    var tile = GridMapManager.Instance.GetTileDetails(key);
                    if (tile != null)
                        tile.havePlace = 1;
                }
            }
        }
        /// <summary>
        /// 重新加载当前建造场景的所有动物，主要用于场景切换后恢复动物状态
        /// </summary>
        private void RecreateBuildSceneAnimal()
        {
            if (buildAnimalCountDict.TryGetValue(currentBuildCode, out List<SceneAnimal> animals))
            {
                foreach (SceneAnimal animal in animals)
                {
                    //TODO:Ŀǰֻ�ܸ��ݼ���ģ�峡������ȡ���λ�ã�֮�������������콨���ĳ���
                    TileDetails pos = GridMapManager.Instance.GetRandomPlaceFurnitureTile(ChickenCoopMap);
                    Collider2D area = FindAnyObjectByType<AnimalArea>().GetComponent<Collider2D>();
                    if (pos != null)
                    {
                        GameObject obj = Instantiate(animal.animalDetails.animalPrefab,
                            new Vector3(pos.gridX + 0.5f, pos.gridY + 0.5f, 0),
                            Quaternion.identity, animalParent);
                        AnimalController controller = obj.GetComponent<AnimalController>();
                        controller.animalDetails = animal.animalDetails;
                        controller.currentGrowthDay = animal.growthDay;
                        controller.animCodeID = animal.animalCode;
                        controller.activityArae = area;
                        controller.isOutSide = animal.isOutSide;
                        Debug.Log(controller.currentGrowthDay);
                        controller.SetStartState(false);
                    }
                }
            }
        }
        /// <summary>
        /// 根据动物的大小类型判断当前场景是否有可以进行繁殖的建筑
        /// </summary>
        /// <param name="sortSize"></param>
        /// <returns></returns>
        public bool HaveBuildingCanBreeding(AnimalSizeType sortSize)
        {
            List<SceneBuilding> currentFarmBuilding = new List<SceneBuilding>();
            if (sceneBuildingDict.TryGetValue("Farm", out currentFarmBuilding))
            {
                if (currentFarmBuilding != null)
                {
                    foreach (var farmBuilding in currentFarmBuilding)
                    {
                        if (farmBuilding.acceptAnimalSize == sortSize && farmBuilding.isDone)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        /// <summary>
        /// 当前场景是否是建造建筑的场景
        /// </summary>
        /// <returns></returns>
        private bool IsCurrentSceneOfCoop()
        {
            foreach (var coopScene in coopSceneNames)
            {
                if (coopScene == SceneManager.GetActiveScene().name)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 根据动物的code获取建造建筑的活动范围
        /// </summary>
        /// <param name="animalCode"></param>
        private Collider2D GetBuildArea(int animalCode)
        {
            foreach (var collder in buildAreaList)
            {
                if (animalCode == collder.code)
                {
                    return collder.area;
                }
            }
            return null;
        }
        /// <summary>
        /// 根据建造建筑的buildCode获取建造建筑
        /// </summary>
        /// <param name="buildCode"></param>
        /// <returns></returns>
        private BuildingItem FindBuildingByCode(int buildCode)
        {
            foreach (var building in FindObjectsOfType<BuildingItem>())
            {
                if (building.buildCodeID == buildCode)
                    return building;
            }
            return null;
        }
        /// <summary>
        /// 生成室外动物，主要用于每天早上8点生成动物从建筑中走出
        /// </summary>
        private void SpawnOutdoorAnimals()
        {
            List<AnimalSpawnInfo> spawnList = new List<AnimalSpawnInfo>();
            // 收集所有待生成的动物
            List<int> buildCodesToProcess = new List<int>(buildAnimalCountDict.Keys);
            foreach (int buildCode in buildCodesToProcess)
            {
                if (!buildAnimalCountDict.ContainsKey(buildCode)) continue;
                List<SceneAnimal> animals = buildAnimalCountDict[buildCode];
                BuildingItem building = FindBuildingByCode(buildCode);
                if (building == null || !building.isDone) continue;
                Collider2D activityArea = GetBuildArea(buildCode);
                if (activityArea == null) continue;
                // 收集还在舍内的动物
                List<SceneAnimal> animalsToSpawn = new List<SceneAnimal>();
                foreach (SceneAnimal animal in animals)
                {
                    if (!animal.isOutSide)
                        animalsToSpawn.Add(animal);
                }
                foreach (SceneAnimal animal in animalsToSpawn)
                {
                    spawnList.Add(new AnimalSpawnInfo
                    {
                        buildCode = buildCode,
                        animal = animal,
                        building = building,
                        area = activityArea
                    });
                    animals.Remove(animal);
                }
                if (animals.Count == 0)
                    buildAnimalCountDict.Remove(buildCode);
            }
            if (spawnList.Count > 0)
                StartCoroutine(SpawnAnimalsCoroutine(spawnList));
        }
        /// <summary>
        /// 开始生成走出建筑的动物，生成间隔为2秒
        /// </summary>
        /// <param name="spawnList"></param>
        /// <returns></returns>
        private IEnumerator SpawnAnimalsCoroutine(List<AnimalSpawnInfo> spawnList)
        {
            for (int i = 0; i < spawnList.Count; i++)
            {
                AnimalSpawnInfo info = spawnList[i];
                GameObject obj = Instantiate(
                    info.animal.animalDetails.animalPrefab,
                    info.building.entrance.transform.position,
                    Quaternion.identity,
                    animalParent
                );
                AnimalController controller = obj.GetComponent<AnimalController>();
                controller.animalDetails = info.animal.animalDetails;
                controller.currentGrowthDay = info.animal.growthDay;
                controller.animCodeID = info.animal.animalCode;
                controller.activityArae = info.area;
                controller.isOutSide = true;
                controller.SetStartState(false);
                controller.transform.position = info.building.entrance.transform.position;
                if (i < spawnList.Count - 1)
                    yield return new WaitForSeconds(2f);
            }
        }
        public GameSaveData GenerateSaveData()
        {
            GetAllSceneFurniture();
            GetAllSceneBuilding();
            GetAllSceneAnimal();
            GetAllSceneItems();
            GetAllSceneKnockItem();
            GetAllSceneReapableItem();
            GameSaveData saveData = new GameSaveData();
            saveData.sceneItemDict = this.sceneItemDict;
            saveData.sceneFurnitureDict = this.sceneFurnitureDict;
            saveData.sceneBuildingDict = this.sceneBuildingDict;
            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            this.sceneFurnitureDict = saveData.sceneFurnitureDict;
            this.sceneItemDict = saveData.sceneItemDict;
            this.sceneBuildingDict = saveData.sceneBuildingDict;
            RecreateAllItem();
            RebuilFurniture();
            ReBuildBuilding();
            RecreateAnimal();
            RecreateKnockItem();
            RecreateReapableItem();
        }
    }


}
