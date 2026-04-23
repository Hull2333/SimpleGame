using MFarm.Map;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
//实例化下列的数据，才可以在ItemDataList_OS对象上出现list物品数据
[System.Serializable]
public class ItemDetails
{
    public int itemID;
    public string itemName;
    //物品类型，根据枚举ItemType
    public ItemType itemType;
    //装备数值
    public int minValue;
    public int maxValue;
    //武器力度
    public float forceValue;
    public FishLevel fishLevel;
    public FishlifeSeason fishlifeSeason;
    public FishlifeTime fishlifeTime;
    public Sprite itemIcon;
    public Sprite itemOnWorldSprite;
    [TextArea]
    public string itemDescription;
    public float itemUseRadius;
    public bool canPickedup;
    public bool canDropped;
    public bool canCarried;
    public bool canEat;
    public int itemPrice;
    public int recoverHealth;
    public int recoverStmina;
    public EquipType equipType;
    [Range(0,1)]
    public float sellPercentage;
}
[System.Serializable]
public struct InventoryItem
{
    public int itemID;
    public int itemAmount;
}
[System.Serializable]
public class GetItemTip
{
    public int itemID;
    public Sprite itemIcon;
    public int tipAmount;
    public bool showing;
}

[System.Serializable]
public class AnimatorType
{
    public EquipType equipType;
    public PartType partType;
    public PartName partName;
    public AnimatorOverrideController overrideController;
}
[System.Serializable]
public class CursorAnimator
{
    public CursorType cursorType;
    public AnimatorOverrideController cursorController;
}
/// <summary>
/// 存储Item在场景中的坐标
/// </summary>
[System.Serializable] 
public class SerializableVector3
{
    public float x, y, z;

    public SerializableVector3(Vector3 pos)
    {
        this.x = pos.x;
        this.y = pos.y;
        this.z = pos.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

    public Vector2Int ToVector2Int()
    {
        return new Vector2Int((int)x, (int)y);
    }
}
//场景中的所有物品
[System.Serializable]
public class SceneItem
{
    public int itemID;
    public SerializableVector3 position;
}
//场景中的建造家具
[System.Serializable]
public class SceneFurniture
{
    //TODO:更多属性信息
    public int itemID;
    public SerializableVector3 position;
    //箱子编号
    public int boxIndex;
}


[System.Serializable]
public class TileProperty
{
    public Vector2Int tileCoordinate;
    public GridType gridType;
    public bool boolTypeValue;
    
}

[System.Serializable]
public class TileDetails
{
    public int gridX, gridY;
    public bool canDig;
    public bool canDropItem;
    public bool canPlaceFurniture;
    public bool canWeeds;
    public bool canRock;
    public bool canBigRock;
    public bool isNPCObstacle;
    public bool canLaterFishing;
    public bool canSeaFishing;
    public bool canTree;
    //是否被挖坑，没有挖则是-1，有则是1，过了一天为2，以此类推
    public int daysSinceDug = -1;
    //是否已浇水
    public int daysSinceWatered = -1;
    //当前种下种子的信息
    public int seedItemID = -1;
    //种子种下的天数
    public int growthDays = -1;
    //种子距离上一次收获的次数
    public int daysSinceLastHarvest = -1;
    //预更新是否有杂草
    public int predictHaveWeeds = -1;
    //是否有杂草
    public int haveWeeds = -1;
    //是否有石矿
    public int haveRock = -1;
    //是否有大石矿
    public int haveBigRock = -1;
    //是否有树
    public int haveTree = -1;
}
[System.Serializable]
public class NPCPosition
{
    //npc对象
    public Transform npc;
    //开始场景
    public string startScene;
    //当前位置
    public Vector3 position;
}
[System.Serializable]
public class EquipImage 
{
    //显示玩家身上的装备图片
    public int itemID;
    public PartName equipPartName;
    public Sprite equipSprite;
}

#region:npc路径库
[System.Serializable]
/// <summary>
/// npc多个场景的移动，记录每个场景的路径
/// </summary>
public class SceneRoute
{
    public string fromSceneName;
    public string gotoSceneName;
    public List<ScenePath> scenePathList;
}
[System.Serializable]
/// <summary>
/// npc跨场景移动的路径
/// </summary>
public class ScenePath
{
    public string sceneName;
    //在sceneName中的起点
    public Vector2Int fromGridCell;
    //在sceneName中的终点
    public Vector2Int gotoGridCell;
}
#endregion