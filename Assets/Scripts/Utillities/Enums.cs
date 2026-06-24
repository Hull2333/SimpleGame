public enum ItemType
{
    //种子，商品，家具
    Seed,Commodity,Furniture,Cooked,Laterfish,Seafish,
    //锄头，斧头，镐头，镰刀，水壶，收割，剑，服装
    HoeTool,AxeTool,BreakTool,ReapTool,WaterTool,Sword,FishingRod,Equipment_Head,Equipment_Body,
    //收割杂草
    ReapableScenery,
    //敌人
    Enemy,TreeSeed

}
public enum FishLevel
{
    Null,Commmon, Good, Epic, Legend
}
public enum FishlifeSeason
{
    Null,Sprite,Summer,Autumn,Winter, SpriteAndSummer, SpriteAndAutumn
}

public enum FishlifeTime
{
    Null,AllDay,Day,Night
}
/// <summary>
/// 背包的类型，比如玩家背包、NPC背包、箱子
/// </summary>
public enum SlotType 
{
    Bag,Box,Shop,Reward,Equipment_Head,Equipment_Body,SellBox,Bait,Ingredient
}
/// <summary>
/// 背包库存的位置
/// </summary>
public enum InventoryLocation
{
    Player, Box, SellBox,Shop
}

/// <summary>
/// 玩家行为状态
/// </summary>
public enum PartType
{
    None,Carry,Hoe,Break,Water,Collect,Reap,Attack1,Fishing,Axe
}
/// <summary>
/// 玩家装备类型
/// </summary>
public enum EquipType 
{
    None,Clothes01_Head,Clothes01_Body
}

/// <summary>
/// 玩家部位
/// </summary>
public enum PartName
{
    Body,Head,Arm,Shadow,Tool
}
/// <summary>
/// 季节类型
/// </summary>
public enum Season
{
    春天,夏天,秋天,冬天
}
/// <summary>
/// 瓦片的类型
/// </summary>
public enum GridType
{
    Diggable, DropItem, PlaceFurniture, NPCObstalacle, Finshing, WeedsGrow, Rocks, BigRocks, Trees, LaterFishing, SeaFishing, ChopItem,BigWeeds,
}
/// <summary>
/// 粒子特效类型
/// </summary>
public enum ParticalEffectType
{
    None,LeavesFall01,LeavesFall02,Rock,ReapableScenery,EnemyHit01,Eat01,HoeEffect,WeedFall,WaterEffect01,EarthenEffect,WoodEffect01,CropEffect01,FeatherEffect01, FeatherEffect02,
    HoeEffect02,
    
}
/// <summary>
/// 游戏状态
/// </summary>
public enum GameState
{
    Gameplay,Pause
}
/// <summary>
/// 灯光模式
/// </summary>
public enum LightShift
{
    Morning,Night
}
/// <summary>
/// 声音类型
/// </summary>
public enum SoundName
{
    none,FootStepSoft,FootStepHard,
    Axe,Pickaxe,Hoe,Reap,Water,Basket,Chop,
    Pickup,Plant,TreeFalling,Rustle,
    AmbientCountryside1,AmbientCountryside2,MusicCalm1,MusicCalm2,AmbientIndoor1
}
/// <summary>
/// 敌人状态
/// </summary>
public enum EnemyStateType
{
    Idle,Patrol,Chase,Attack,Hurted,Death,Fainting,
}
/// <summary>
/// 菜肴名称
/// </summary>
public enum CookedMenu
{
    ApplePie,BakedApple,
}
public enum TaskType
{
    collect,fight,dialogue,
}
/// <summary>
/// NPC名称
/// </summary>
public enum NPCname
{
    Freda,DaTuan,Hanies,Liam,Adolf,Rose
}
/// <summary>
/// 任务类型
/// </summary>
public enum QuestType 
{
    Plot,BullteinBoard,
}
/// <summary>
/// 鼠标类型
/// </summary>
public enum CursorType 
{
    Normal,Check,Fishing,Attack,Pat,Cook,
}
/// <summary>
/// 物品在背包中的大小比例
/// </summary>
public enum SpriteScale
{
    Normal,Middle,Lager
}
/// <summary>
/// 对话选项类型
/// </summary>
public enum DialogueOptionType
{
    None,Quest,ItemShop,BuildShop,AnimalShop
}
/// <summary>
/// 动物大小
/// </summary>
public enum AnimalSizeType
{
    Small,Middle,Large,
}


