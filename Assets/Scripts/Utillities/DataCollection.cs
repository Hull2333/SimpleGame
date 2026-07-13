using JetBrains.Annotations;
using MFarm.Map;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static AnimalData_SO;
//实例化下列的数据，才可以在ItemDataList_OS对象上出现list物品数据
[System.Serializable]
public class ItemDetails
{
    public int itemID;
    public string itemName;
    //物品类型，根据枚举ItemType
    public ItemType itemType;
    [Header("装备数值")]
    public int minValue;
    public int maxValue;
    [Header("武器击退力")]
    public float forceValue;
    [Header("鱼类相关")]
    public FishLevel fishLevel;
    public FishlifeSeason fishlifeSeason;
    public FishlifeTime fishlifeTime;
    public int[] likeBaits;
    public Sprite itemIcon;
    public Sprite itemOnWorldSprite;
    [TextArea]
    public string itemDescription;
    public float itemUseRadius;
    public bool canPickedup;
    public bool canDropped;
    public bool canCarried;
    public bool canEat;
    [Header("鱼饵相关")]
    public bool isBait;
    public float decreaseFishingTime;
    public int itemPrice;
    public int recoverHealth;
    public int recoverStmina;
    public EquipType equipType;
    [Header("是否缩放，不缩放就选normal")]
    public SpriteScale scale;
    [Range(0, 1)]
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
//场景中的建造建筑
[System.Serializable]
public class SceneBuilding
{
    public int buildID;
    //建造时间
    public int buildDay;
    //建筑识别码
    public int buildCodeID;
    //是否完工
    public bool isDone;
    //可接受的动物尺寸
    public AnimalSizeType acceptAnimalSize;
    public SerializableVector3 position;
}
//场景中的动物
[System.Serializable]
public class SceneAnimal
{
    //成长的天数
    public int growthDay;
    public float friendliness;
    public bool isTouch;
    public AnimalDetails animalDetails;
    public int animalCode;
    //是否在室外
    public bool isOutSide;
}
//场景中的动物的活动范围
[System.Serializable]
public class BuildColliderArea
{
    public int code;
    public Collider2D area;
}
//待生成的动物
[System.Serializable]
public class AnimalSpawnInfo
{
    public int buildCode;
    public SceneAnimal animal;
    public BuildingItem building;
    public Collider2D area;
}

[System.Serializable]
public class SceneKnockItem
{
    public SerializableVector3 position;
    public int itemIndex;
}
[System.Serializable]
public class SceneReapableItem
{
    public SerializableVector3 position;
    public Sprite sprite;
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
    public bool canChopItem;
    public bool canBigWeeds;
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
    public int haveBigWeeds = -1;
    //是否有石矿
    public int haveRock = -1;
    //是否有大石矿
    public int haveBigRock = -1;
    //是否有树
    public int haveTree = -1;
    //是否有可劈砍物体
    public int haveChop = -1;
    //是否有放置东西
    public int havePlace = -1;
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
//NPC事件中其他出现的NPC
[System.Serializable]
public class EventNPC
{
    //对应的NPC事件步骤
    public int step;
    public GameObject NPC;
    //一开始的位置
    public Vector2 NPCStartPos;
    public stepPos[] NPCMovePos;
}
//NPC事件每一步的位置
[System.Serializable]
public class stepPos 
{
    public int step;
    public Vector2 pos;
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
[System.Serializable]
public class NPCEvent
{
    public string npcName;
    //该事件是否已经发生
    public bool isHappened;
    //好感度
    public float friendliness;
    //NPC事件开始和结束的时间
    public int eventStartHour;
    public int eventEndHour;
    //开始场景和位置
    public string startScene;
    //速度
    public float normalSpeed;
    public float maxSpeed;
    [Header("NPC的位置,NPC的位置、对话数据、动画数量要相等")]
    //下一个位置
    public Vector2[] nextPos;
    [Header("玩家的位置")]
    //玩家的位置
    public Vector2[] playerPos;
    //玩家最开始移动后面朝的方向
    public Vector2 playerFaceDir;
    //对话内容
    public DialogueData_OS[] dialogueData;
    public AnimationClip[] animClip;
    [Header("其他NPC的出现")]
    //其他NPC的出现
    public EventNPC[] eventNPC;
}
[System.Serializable]
public class NPCFriendLiness
{
    //NPC名字和对应的好感度
    public string NPCname;
    public float friendlinessValue;
}
[System.Serializable]
public class MineScene
{
    public string sceneName;
    public Vector2 GoToPos;
}
[System.Serializable]
public class AnimalItem
{
    public AnimalDetails animal;
    public int count;
}
//建造建筑中动物的生产物品
[System.Serializable]
public class buildProduceItem
{
    public int buildCode;
    public int itemID;
}






