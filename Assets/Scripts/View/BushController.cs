using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFarm.Inventory;

public class BushController : MonoBehaviour //调用在BigBush对象上
{
    private CursorManager cursorManager;
    private SpriteRenderer spriteRenderer;
    //春冬季节灌木丛图片
    public Sprite springBushSprite;
    public Sprite winterBushSprite;
    //灌木丛的花
    public GameObject flowerSprite;
    //灌木丛结果位置
    public Transform[] fruitPos;
    //春冬季节灌木丛果实的ID
    public int itemID_Spring;
    public int itemID_Winter;
    //生成的果实Item
    public GameObject bounceItemBase;
    public Transform fruitParent;
    //已同步到的季节，用来判断季节是否发生切换
    private Season currentSeason;
    //是否已经同步过一次季节状态
    private bool hasSyncedSeason;
    //是否已结果
    private bool isFruiting;
    //采摘后是否正在等待重新结果
    private bool waitRegrow;
    //采摘后已经过去的天数
    private int daysAfterPicking;
    //记录已生成的果实，季节结束或切换场景时统一清理
    private List<GameObject> fruitList = new List<GameObject>();

    private void OnEnable()
    {
        EventHandler.GameDayEvent += OnGameDayEvent;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
    }

    private void OnDisable()
    {
        EventHandler.GameDayEvent -= OnGameDayEvent;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
    }

    //跨天时季节可能变化，按当天季节刷新灌木丛
    private void OnGameDayEvent(int day, Season season, Weekday weekday)
    {
        RefreshBushState(season);
        //采摘后满三天且不在春天时重新结果
        TryRegrow(season);
    }

    //进入场景时按当前季节同步一次，避免在别的场景跨季后回来状态不对
    private void OnAfterSceneLoadedEvent()
    {
        RefreshBushState(TimeManager.Instance.gameSeason);
        //切场景时按当前季节换灌木丛自身的图片
        UpdateBushSprite(TimeManager.Instance.gameSeason);
    }
    private void Start()
    {
        cursorManager = GameObject.FindWithTag("CursorManager").GetComponent<CursorManager>();
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (Input.GetKey(KeyCode.Mouse0) && cursorManager.canPat && isFruiting)
            {
                PickingFruit();
            }
        }
    }
    /// <summary>
    /// 采摘果实
    /// </summary>
    private void PickingFruit()
    {
        //采摘后灌木丛不再处于结果状态，避免按住右键时重复触发
        isFruiting = false;
        //采摘后开始计时，过三天重新结果
        waitRegrow = true;
        daysAfterPicking = 0;
        for (int i = 0; i < fruitList.Count; i++)
        {
            GameObject fruit = fruitList[i];
            //果实可能已被玩家捡走销毁，这里需要判空
            if (fruit == null)
            {
                continue;
            }
            //把果实重力调回1，让果实从灌木丛上掉落
            Rigidbody2D fruitRigidbody = fruit.GetComponent<Rigidbody2D>();
            if (fruitRigidbody != null)
            {
                fruitRigidbody.gravityScale = 1;
            }
            //生成时弹跳脚本被关闭，Start还没执行过；这里启用后Start只会施力一次，Update也能继续做二次弹跳
            ItemBounce itemBounce = fruit.GetComponent<ItemBounce>();
            if (itemBounce != null)
            {
                itemBounce.enabled = true;
            }
        }
        //果实已经采摘，清空记录避免后续重复处理
        fruitList.Clear();
    }
    /// <summary>
    /// 按季节刷新灌木丛：春天只开花不结果，其他季节关花并在没有果实时结果
    /// </summary>
    private void RefreshBushState(Season season)
    {
        //季节没有变化就不再处理，避免同一季内每天推进都重复生成果实
        if (hasSyncedSeason && season == currentSeason)
        {
            return;
        }
        hasSyncedSeason = true;
        currentSeason = season;
        //只有春天开花
        flowerSprite.SetActive(season == Season.春天);
        //春天不结果，明确标记为未结果状态
        if (season == Season.春天)
        {
            isFruiting = false;
        }
        //除春天外，进入季节时还没有果实就结果
        if (season != Season.春天 && !isFruiting)
        {
            GenerateFruits(season);
        }

    }

    /// <summary>
    /// 按当前季节换灌木丛自身的图片：春天用春季图，冬天用冬季图
    /// </summary>
    private void UpdateBushSprite(Season season)
    {
        //场景加载事件可能早于Start执行，这里补取一次组件避免空引用
        SpriteRenderer renderer = spriteRenderer != null ? spriteRenderer : GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            return;
        }
        if (season == Season.春天 && springBushSprite != null)
        {
            renderer.sprite = springBushSprite;
        }
        else if (season == Season.冬天 && winterBushSprite != null)
        {
            renderer.sprite = winterBushSprite;
        }
    }

    /// <summary>
    /// 采摘后过三天重新结果，春天不结果
    /// </summary>
    private void TryRegrow(Season season)
    {
        //没有在等待结果、已经结果或春天时不重新结果
        if (!waitRegrow || isFruiting || season == Season.春天)
        {
            return;
        }
        daysAfterPicking++;
        if (daysAfterPicking < 3)
        {
            return;
        }
        waitRegrow = false;
        daysAfterPicking = 0;
        GenerateFruits(season);
    }

    /// <summary>
    /// 结果：在随机的结果位置生成1~3个果实
    /// </summary>
    private void GenerateFruits(Season season)
    {
        if (bounceItemBase == null || fruitPos == null || fruitPos.Length == 0)
        {
            return;
        }
        //先打乱结果位置的下标，保证每个果实落在不同的位置上
        List<int> posIndexList = new List<int>();
        for (int i = 0; i < fruitPos.Length; i++)
        {
            posIndexList.Add(i);
        }
        for (int i = posIndexList.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = posIndexList[i];
            posIndexList[i] = posIndexList[randomIndex];
            posIndexList[randomIndex] = temp;
        }
        //随机1~3个果实，结果位置不够时按实际位置数量生成
        int fruitCount = Mathf.Min(Random.Range(1, 4), posIndexList.Count);
        for (int i = 0; i < fruitCount; i++)
        {
            Transform pos = fruitPos[posIndexList[i]];
            GameObject fruit = Instantiate(bounceItemBase, pos.position, Quaternion.identity,fruitParent);
            //果实要静止挂在灌木丛上，先把重力设为0避免往下掉落
            Rigidbody2D fruitRigidbody = fruit.GetComponent<Rigidbody2D>();
            if (fruitRigidbody != null)
            {
                fruitRigidbody.gravityScale = 0;
            }
            //冬天用冬季果实ID，其他季节用春季果实ID
            int fruitID = season == Season.冬天 ? itemID_Winter : itemID_Spring;
            //把果实的ItemID设置为灌木丛配置的果实ID，Item脚本会按ID初始化图片
            Item item = fruit.GetComponent<Item>();
            if (item != null)
            {
                item.itemID = fruitID;
                item.Init(fruitID);
            }
            //果实要静止挂在灌木丛上，关闭弹跳脚本避免被弹飞
            ItemBounce itemBounce = fruit.GetComponent<ItemBounce>();
            if (itemBounce != null)
            {
                itemBounce.enabled = false;
            }
            fruitList.Add(fruit);
        }
        isFruiting = true;
    }

}
