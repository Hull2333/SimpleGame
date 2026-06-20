using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFarm.Save
{
    [Serializable]
    public class GameSaveData
    {
        /// <summary>
        /// 场景信息
        /// </summary>
        public string dataSceneName;
        /// <summary>
        /// 存储人物坐标，string人物名字
        /// </summary>
        public Dictionary<string, SerializableVector3> characterPosDict;
        /// <summary>
        /// 存储场景中的所有物品
        /// </summary>
        public Dictionary<string, List<SceneItem>> sceneItemDict;
        /// <summary>
        /// 存储所有家具信息
        /// </summary>
        public Dictionary<string, List<SceneFurniture>> sceneFurnitureDict;
        /// <summary>
        /// 存储所有的建造建筑信息
        /// </summary>
        public Dictionary<string, List<SceneBuilding>> sceneBuildingDict;
        /// <summary>
        /// 存储瓦片信息
        /// </summary>
        public Dictionary<string, TileDetails> tileDetailsDict;
        /// <summary>
        /// 存储第一次加载的bool值
        /// </summary>
        public Dictionary<string, bool> firstLoadDict;
        /// <summary>
        /// 存储各种背包信息
        /// </summary>
        public Dictionary<string, List<InventoryItem>> inventoryDict;
        /// <summary>
        /// 存储游戏时间信息
        /// </summary>
        public Dictionary<string, int> timeDict;
        /// <summary>
        /// 存储玩家金钱
        /// </summary>
        public int playerMoney;

        //NPC
        /// <summary>
        /// NPC schelude的目标场景
        /// </summary>
        public string targetScene;
        /// <summary>
        /// NPC是否可互动
        /// </summary>
        public bool interactable;
        /// <summary>
        /// NPC动作
        /// </summary>
        public int animationInstanceID;
    }
}


