using MFarm.Save;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace MFarm.Inventory
{
    public class ItemManager : MonoBehaviour,ISaveable    //调用在ItemManager对象上
    {

        public Item itemPerfab;
        public Item bounceItemPerfab;
        private Transform itemParent;
        private Transform playerTransform => FindObjectOfType<PlayerController>().transform;

        public string GUID => GetComponent<DataGUID>().guid;

        //用字典来存储每个场景中的所有Item
        private Dictionary<string,List<SceneItem>> sceneItemDict = new Dictionary<string,List<SceneItem>>();
        //用字典来存储每个场景中的所有Furniture
        private Dictionary<string,List<SceneFurniture>> sceneFurnitureDict = new Dictionary<string, List<SceneFurniture>>();

        private void OnEnable()
        {
            EventHandler.InstantiateItemInScene += OnInstantiateItemInScene;
            EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
            EventHandler.DropItemEvent += OnDropItemEvent;
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadeEvent;
            //建造物品生成事件
            EventHandler.BuildFurnitureEvent += OnBuildFurnitureEvent;
            //菜肴制作后将菜肴生成在人物脚下拾取
            EventHandler.CookedMakeEvent += OnCookedMakeEvent;
            //新游戏开始需要重置的数据
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        }

        private void OnDisable()
        {
            EventHandler.InstantiateItemInScene -= OnInstantiateItemInScene;
            EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
            EventHandler.DropItemEvent -= OnDropItemEvent;
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadeEvent;
            EventHandler.BuildFurnitureEvent -= OnBuildFurnitureEvent;
            EventHandler.CookedMakeEvent -= OnCookedMakeEvent;
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        }
        private void Start()
        {
            ISaveable saveable = this;
            saveable.RegisterSaveable();
        }
        private void OnBuildFurnitureEvent(int ID, Vector3 mousePos)
        {
            BluPrintDetails bluePrint = InventoryManager.Instance.bluPrintData.GetBluPrintDetails(ID);
            //生成建造物品在鼠标位置上
            var buildItem = Instantiate(bluePrint.buildPrefab, mousePos, Quaternion.identity, itemParent);
            //存储游戏中箱子数据
            if(buildItem.GetComponent<Box>())
            {
                buildItem.GetComponent<Box>().index = InventoryManager.Instance.boxDataAmount;
                buildItem.GetComponent<Box>().InitBox(buildItem.GetComponent<Box>().index);
            }
        }
        private void OnCookedMakeEvent(int ID)
        {
            CookedDetails cookedDetails = InventoryManager.Instance.cookedData.GetCookedDetails(ID);
            var cookedItem = Instantiate(cookedDetails.cookPrefab, new Vector3(playerTransform.position.x,playerTransform.position.y + 1f), Quaternion.identity, itemParent);
        }

        private void OnBeforeSceneUnloadEvent()
        {
           GetAllSceneItems();
           GetAllSceneFurniture();
        }
        private void OnAfterSceneLoadeEvent()
        {
            itemParent = GameObject.FindWithTag("ItemParent").transform;
            RecreateAllItem();
            RebuilFurniture();
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
            //item.GetComponent<ItemBounce>().InitBounceItem(pos, Vector3.up);
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
                        var buildItem = Instantiate(bluePrint.buildPrefab,sceneFurniture.position.ToVector3(),Quaternion.identity,itemParent);
                        //重新赋值箱子编号
                        if (buildItem.GetComponent<Box>())
                        {
                            buildItem.GetComponent<Box>().InitBox(sceneFurniture.boxIndex);
                        }
                    }
                }
            }
        }

        public GameSaveData GenerateSaveData()
        {
            GetAllSceneFurniture();
            GetAllSceneItems();
            GameSaveData saveData = new GameSaveData();
            saveData.sceneItemDict = this.sceneItemDict;
            saveData.sceneFurnitureDict = this.sceneFurnitureDict;

            return saveData;


        }

        public void RestoreData(GameSaveData saveData)
        {
            this.sceneFurnitureDict = saveData.sceneFurnitureDict;
            this.sceneItemDict = saveData.sceneItemDict;

            RecreateAllItem();
            RebuilFurniture();
        }
    }

    
}
