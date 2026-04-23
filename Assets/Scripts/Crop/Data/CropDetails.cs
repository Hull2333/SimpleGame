using UnityEngine;
[System.Serializable]
public class CropDetails
{
    public int seedItemID;
    [Header("不同阶段需要的天数")]
    public int[] growthDays;
    //完整的生长时间
    public int totalGrowthDays
    {
        get
        {
            int amount = 0;
            foreach (var days in growthDays)
            {
                amount += days;
            }
            return amount;
        }
    }
    [Header("不同生长阶段物品Prefab")]
    public GameObject[] growthPrefab;
    public Sprite[] growthSprites;
    [Header("可种植的季节")]
    public Season[] seasons;

    [Space]
    [Header("收割工具")]
    public int[] harvestToolItemID;
    [Header("每次工具使用次数")]
    public int[] requireActionCount;
    [Header("转换新物品ID")]
    public int transferItemID;

    [Space]
    [Header("收割果实信息")]
    public int[] producedItemID;
    public int[] producedMinAmount;
    public int[] producedMaxAmount;
    //生成产品的范围
    public Vector2 spawnRadius;
    [Header("再次生长时间")]
    //倒退的天数
    public int daysToRegrow;
    //倒退的次数
    public int regrowTimes;
    [Header("Options")]
    //是否打开农作物碰撞体
    public bool openCollider;
    //产品是否生成在玩家身上
    public bool generateAtPlayerPosition;
    //与玩家互动时是否有动画，比如每砍一下树，树会摇晃
    public bool hasAnimation;
    //是否有粒子特效
    public bool hasParticalEffect;
    public ParticalEffectType effectType;
    public Vector3 effectPos;
    //音效
    public SoundName soundEffect;
    /// <summary>
    /// 检查选择的工具是否可以收割
    /// </summary>
    /// <param name="toolID">工具ID</param>
    /// <returns></returns>
    public bool CheckToolAvailable(int toolID)
    {
        foreach(var tool in harvestToolItemID)
        {
            if (tool == toolID)
            {
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// 使用工具的次数
    /// </summary>
    /// <param name="toolID"></param>
    /// <returns></returns>
    public int GetTotalRequireCount(int toolID)
    {
        //在收割工具列表中找到收割工具后，返回对应的工具使用次数
        for(int i = 0; i < harvestToolItemID.Length; i++)
        {
            if (harvestToolItemID[i]== toolID)
            {
                return requireActionCount[i];
            }
        }
        return -1;
    }
    
}
