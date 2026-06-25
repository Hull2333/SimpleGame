using MFarm.Save;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static AnimalData_SO;
using static UnityEditor.Progress;
namespace MFarm.Inventory
{
    public class ItemManager : MonoBehaviour,ISaveable    //调用在ItemManager对象上
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
        public Transform buildingParent;
        public Transform animalParent;
        private Transform furnitureParent;
        private Transform playerTransform => FindObjectOfType<PlayerController>().transform;

        public string GUID => GetComponent<DataGUID>().guid;
        public MineSceneDataList_SO mineData;
        //用字典来存储每个场景中的所有Item
        private Dictionary<string,List<SceneItem>> sceneItemDict = new Dictionary<string,List<SceneItem>>();
        //用字典来存储每个场景中的所有Furniture
        private Dictionary<string,List<SceneFurniture>> sceneFurnitureDict = new Dictionary<string, List<SceneFurniture>>();
        //用字典来存储每个场景的所有的Building
        private Dictionary<string, List<SceneBuilding>> sceneBuildingDict = new Dictionary<string, List<SceneBuilding>>();
        //用字典来存储每个场景的knockItem
        public Dictionary<string, List<SceneKnockItem>> sceneKnockItemDict = new Dictionary<string, List<SceneKnockItem>>();
        //用字典来存储每个场景的杂草和大杂草
        public Dictionary<string, List<SceneReapableItem>> sceneWeedItemDict = new Dictionary<string, List<SceneReapableItem>>();
        public Dictionary<string, List<SceneReapableItem>> sceneBigWeedItemDict = new Dictionary<string, List<SceneReapableItem>>();
        //用字典来存储每个场景的动物
        public Dictionary<string, List<SceneAnimal>> sceneAnimalDict = new Dictionary<string, List<SceneAnimal>>();
        [Header("动物相关")]
        public List<AnimalItem> currentBuyAnimalList = new List<AnimalItem>();
        private HashSet<int> buildCodeIDs = new HashSet<int>();
        //场景中动物的活动范围
        public List<BuildColliderArea> buildAreaList = new List<BuildColliderArea>();
        private void OnEnable()
        {
            EventHandler.InstantiateItemInScene += OnInstantiateItemInScene;
            EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
            EventHandler.DropItemEvent += OnDropItemEvent;
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadeEvent;
            //建造物品生成事件
            EventHandler.BuildFurnitureEvent += OnBuildFurnitureEvent;
            //新游戏开始需要重置的数据
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
            EventHandler.InstantiateBuildingOnMapEvent += OnInstantiateBuildingOnMapEvent;
            EventHandler.BuyAnimalEvent += OnBuyAnimalEvent;
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
            EventHandler.BuyAnimalEvent -= OnBuyAnimalEvent;
        }

      

        private void Start()
        {
            ISaveable saveable = this;
            saveable.RegisterSaveable();
        }
        private void OnBuildFurnitureEvent(BluPrintDetails bluPrintDetails, Vector3 bluPrintPos ,Transform parent)
        {
            var bluPrintGameObject = Instantiate(bluPrintDetails.buildPrefab, bluPrintPos, Quaternion.identity, parent);
            bluPrintGameObject.GetComponent<Furniture>().SetCollider(true);
            bluPrintGameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1f);
            //存储游戏中箱子数据
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
                GetAllSceneItems();
                GetAllSceneFurniture();
                GetAllSceneBuilding();
                GetAllSceneAnimal();
                GetAllSceneKnockItem();
                GetAllSceneReapableItem();
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
                RecreateAllItem();
                RebuilFurniture();
                ReBuildBuilding();
                RecreateAnimal();
                RecreateKnockItem();
                RecreateReapableItem();
            }
        }
        /// <summary>
        /// 在鼠标拖拽结束的地面位置生成Item
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="pos"></param>
        private void OnInstantiateItemInScene(int ID, Vector3 pos)
        {
            EventHandler.CallItemFirstPos(pos);
            var item = Instantiate(bounceItemPerfab, pos, Quaternion.identity, itemParent);
            item.itemID = ID;
            //使物品生成有个下落的动画
            //building.GetComponent<ItemBounce>().InitBounceItem(pos, Vector3.up);
        }

        private void OnDropItemEvent(int ID, Vector3 mousePos,ItemType itemType)
        {
            //如果丢弃的物品时种子，则不执行后面的方法
            if (itemType == ItemType.Seed)
            {
                return;
            }
            //TODO:扔东西的效果
            var item = Instantiate(bounceItemPerfab, playerTransform.position, Quaternion.identity, itemParent);
            item.itemID = ID;
            //normalized向量化
            var dir = (mousePos - playerTransform.position).normalized;
            item.GetComponent<ItemBounce>().InitBounceItem(mousePos,dir);
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
            //生成不重复的建筑物识别码
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
        private void OnBuyAnimalEvent(AnimalDetails animalDetails, int amount)
        {
            AnimalItem currentanimal = new AnimalItem { animal = animalDetails, count = amount };
            currentBuyAnimalList.Add(currentanimal);
        }
        /// <summary>
        /// 排除矿洞场景
        /// </summary>
        /// <param name="sceneName">当前场景名称</param>
        /// <returns>true为非矿洞场景，false为矿洞场景</returns>
        private bool ExcludeMineScene(string sceneName)
        {
            foreach(var scene in mineData.mineSceneList)
            {
                if(scene.sceneName == sceneName)
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 获取场景的物品信息
        /// </summary>
        private void GetAllSceneItems()
        {
            //存储当前场景的物品
            List<SceneItem> currentSceneItems = new List<SceneItem>();  
            foreach(var item in FindObjectsOfType<Item>())
            {
                SceneItem sceneItem = new SceneItem()
                {
                    itemID = item.itemID,
                    position = new SerializableVector3(item.transform.position)
                };
                currentSceneItems.Add(sceneItem);
            }
            //当前场景在字典中，则把当前场景的物品数据更新到字典中
            if (sceneItemDict.ContainsKey(SceneManager.GetActiveScene().name))
            {
                sceneItemDict[SceneManager.GetActiveScene().name] = currentSceneItems;
            }
            //如果时新场景，就把新场景的物品数据添加到字典中
            else
            {
                sceneItemDict.Add(SceneManager.GetActiveScene().name, currentSceneItems);
            }
            
        }
        /// <summary>
        /// 获取场景中所有家具信息
        /// </summary>
        private void GetAllSceneFurniture()
        {
            //存储当前场景的物品
            List<SceneFurniture> currentSceneFurniture = new List<SceneFurniture>();
            foreach (var item in FindObjectsOfType<Furniture>())
            {
                SceneFurniture sceneFurniture = new SceneFurniture()
                {
                    itemID = item.itemID,
                    position = new SerializableVector3(item.transform.position)
                };
                //如果场景中有箱子，地图刷新前保存箱子的编号数据
                if (item.GetComponent<Box>())
                {
                    sceneFurniture.boxIndex = item.GetComponent<Box>().index;
                }
                currentSceneFurniture.Add(sceneFurniture);
            }
            //当前场景在字典中，则把当前场景的物品数据更新到字典中
            if (sceneFurnitureDict.ContainsKey(SceneManager.GetActiveScene().name))
            {
                sceneFurnitureDict[SceneManager.GetActiveScene().name] = currentSceneFurniture;
            }
            //如果时新场景，就把新场景的物品数据添加到字典中
            else
            {
                sceneFurnitureDict.Add(SceneManager.GetActiveScene().name, currentSceneFurniture);
            }
        }
        /// <summary>
        /// 获取场景中的所有的建造建筑
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
            //当前场景在字典中，则把当前场景的物品数据更新到字典中
            if (sceneBuildingDict.ContainsKey(SceneManager.GetActiveScene().name))
            {
                sceneBuildingDict[SceneManager.GetActiveScene().name] = currentSceneBuinding;
            }
            //如果时新场景，就把新场景的物品数据添加到字典中
            else
            {
                sceneBuildingDict.Add(SceneManager.GetActiveScene().name, currentSceneBuinding);
            }
        }
        /// <summary>
        /// 获取所有场景中的KnockItem
        /// </summary>
        private void GetAllSceneKnockItem()
        {
            //存储当前场景的物品
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
            //当前场景在字典中，则把当前场景的物品数据更新到字典中
            if (sceneItemDict.ContainsKey(SceneManager.GetActiveScene().name))
            {
                sceneKnockItemDict[SceneManager.GetActiveScene().name] = currentSceneKnockItems;
            }
            //如果时新场景，就把新场景的物品数据添加到字典中
            else
            {
                sceneKnockItemDict.Add(SceneManager.GetActiveScene().name, currentSceneKnockItems);
            }
        }
        /// <summary>
        /// 获取所有场景中的ReapableItem
        /// </summary>
        private void GetAllSceneReapableItem()
        {
            //存储当前场景杂草
            List<SceneReapableItem> currentSceneWeedItems = new List<SceneReapableItem>();
            //存储当前场景大杂草
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
            //当前场景在字典中，则把当前场景的物品数据更新到字典中
            if (sceneItemDict.ContainsKey(SceneManager.GetActiveScene().name))
            {
                sceneWeedItemDict[SceneManager.GetActiveScene().name] = currentSceneWeedItems;
                sceneBigWeedItemDict[SceneManager.GetActiveScene().name] = currentSceneBigWeedItems;
            }
            //如果时新场景，就把新场景的物品数据添加到字典中
            else
            {
                sceneWeedItemDict.Add(SceneManager.GetActiveScene().name, currentSceneWeedItems);
                sceneBigWeedItemDict.Add(SceneManager.GetActiveScene().name, currentSceneBigWeedItems);
            }
        }
        /// <summary>
        /// 获取所有场景中的动物
        /// </summary>
        private void GetAllSceneAnimal()
        {
            List<SceneAnimal> currentSceneAnimalList = new List<SceneAnimal>();
            foreach (var animal in FindObjectsOfType<AnimalController>())
            {
                SceneAnimal sceneAnimal = new SceneAnimal { animalDetails = animal.animalDetails, 
                    animalCode = animal.animCodeID, 
                    growthDay = animal.currentGrowthDay 
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
        /// 刷新创建当前场景的物品
        /// </summary>
        private void RecreateAllItem()
        {
            List<SceneItem> currentSceneItems = new List<SceneItem>();
            if(sceneItemDict.TryGetValue(SceneManager.GetActiveScene().name,out currentSceneItems))
            {
                if (currentSceneItems != null)
                {
                    //删除当前场景的所有物品数据
                    foreach(var item in FindObjectsOfType<Item>())
                    {
                        Destroy(item.gameObject);
                    }
                    //重新生成当前场景的所有物品数据
                    foreach(var item in currentSceneItems)
                    {
                        Item newItem = Instantiate(itemPerfab, item.position.ToVector3(), Quaternion.identity, itemParent);
                        newItem.Init(item.itemID);
                    }
                }
            }
        }
        /// <summary>
        /// 重建当前场景家具
        /// </summary>
        private void RebuilFurniture()
        {
            List<SceneFurniture> currentSceneFurniture = new List<SceneFurniture>();
            if (sceneFurnitureDict.TryGetValue(SceneManager.GetActiveScene().name,out currentSceneFurniture))
            {
                if(currentSceneFurniture != null)
                {
                    foreach(SceneFurniture sceneFurniture in currentSceneFurniture)
                    {
                        BluPrintDetails bluePrint = InventoryManager.Instance.bluPrintData.GetBluPrintDetails(sceneFurniture.itemID); 
                        var buildItem = Instantiate(bluePrint.buildPrefab,sceneFurniture.position.ToVector3(),Quaternion.identity, furnitureParent);
                        //重新赋值箱子编号
                        if (buildItem.GetComponent<Box>())
                        {
                            buildItem.GetComponent<Box>().InitBox(sceneFurniture.boxIndex);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 重建当前场景建造建筑
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
                        //获取各个建筑的动物活动范围
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
        /// 重新加载生成KnockItem
        /// </summary>
        private void RecreateKnockItem()
        {
            
            List<SceneKnockItem> currentSceneItems = new List<SceneKnockItem>();
            if (sceneKnockItemDict.TryGetValue(SceneManager.GetActiveScene().name, out currentSceneItems))
            {
                if (currentSceneItems != null)
                {
                    //删除当前场景的所有物品数据
                    foreach (var item in FindObjectsOfType<KnockableItem>())
                    {
                        Destroy(item.gameObject);
                    }
                    //重新生成当前场景的所有物品数据
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
        /// 重新加载生成ReapableItem
        /// </summary>
        private void RecreateReapableItem()
        {

            List<SceneReapableItem> currentWeedsItems = new List<SceneReapableItem>();
            List<SceneReapableItem> currentBigWeedsItems = new List<SceneReapableItem>();
            if (sceneWeedItemDict.TryGetValue(SceneManager.GetActiveScene().name, out currentWeedsItems))
            {
                if (currentWeedsItems != null)
                {
                    //删除当前场景的所有物品数据
                    foreach (var item in FindObjectsOfType<ReapableItem>())
                    {
                        Destroy(item.gameObject);
                    }
                    //重新生成当前场景的所有物品数据
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
                    //重新生成当前场景的所有物品数据
                    foreach (var item in currentBigWeedsItems)
                    {
                        var bigWeedItem = Instantiate(bigWeedItemPerfab, item.position.ToVector3(), Quaternion.identity);
                        bigWeedItem.GetComponentInChildren<SpriteRenderer>().sprite = item.sprite;
                    }
                }
            }
        }
        /// <summary>
        /// 重新加载动物到场景中
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
                    Debug.Log(GetBuildArea(animal.animalCode).name);
                    animalInScene.GetComponent<AnimalController>().SetStartState(false);
                }
            }
        }
        /// <summary>
        /// 检查玩家是否有合适养殖的建筑
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
                        //有该尺寸类型的养殖建筑且已经完工
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
        /// 获取建筑的活动范围
        /// </summary>
        /// <param name="animalCode"></param>
        private Collider2D GetBuildArea(int animalCode)
        {
            foreach (var collder in buildAreaList)
            {
                if(animalCode == collder.code)
                {
                    return collder.area;
                }
            }
            return null;
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
