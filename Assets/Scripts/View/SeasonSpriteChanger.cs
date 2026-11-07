using UnityEngine;

/// <summary>
/// 按当前季节切换自身的图片，未放入对应季节图片时保持原图
/// 调用在需要随季节换图的对象上（该对象需有 SpriteRenderer）
/// </summary>
public class SeasonSpriteChanger : MonoBehaviour
{
    [Header("各季节图片")]
    public Sprite springSprite;
    public Sprite summerSprite;
    public Sprite autumnSprite;
    public Sprite winterSprite;

    [Header("各季节夜晚图片")]
    public Sprite springNightSprite;
    public Sprite summerNightSprite;
    public Sprite autumnNightSprite;
    public Sprite winterNightSprite;

    [Header("夜晚是否换图")]
    //打勾后夜晚换成该季节的夜晚图片，不打勾则夜晚不换图
    public bool changeSpriteAtNight;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        EventHandler.LightShiftChangeEvent += OnLightShiftChangeEvent;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        EventHandler.LightShiftChangeEvent -= OnLightShiftChangeEvent;
    }

    /// <summary>
    /// 切换场景后按当前季节换图
    /// </summary>
    private void OnAfterSceneLoadedEvent()
    {
        //没有 SpriteRenderer 或时间管理器还没准备好时不处理
        if (spriteRenderer == null || TimeManager.Instance == null)
        {
            return;
        }
        Sprite seasonSprite = GetSeasonSprite(TimeManager.Instance.gameSeason);
        //该季节没有配置图片就保持现在的图片
        if (seasonSprite != null)
        {
            spriteRenderer.sprite = seasonSprite;
        }
    }

    /// <summary>
    /// 时段变化时换图：夜晚用该季节的夜晚图片，白天时段切回该季节的白天图片
    /// </summary>
    private void OnLightShiftChangeEvent(Season season, LightShift lightShift, float timeDifference)
    {
        //没打勾就不在夜晚换图
        if (!changeSpriteAtNight)
        {
            return;
        }
        if (spriteRenderer == null)
        {
            return;
        }
        Sprite targetSprite = lightShift == LightShift.Night ? GetSeasonNightSprite(season) : GetSeasonSprite(season);
        //该时段没有配置图片就保持现在的图片
        if (targetSprite != null)
        {
            spriteRenderer.sprite = targetSprite;
        }
    }

    /// <summary>
    /// 取出当前季节对应的图片
    /// </summary>
    private Sprite GetSeasonSprite(Season season)
    {
        switch (season)
        {
            case Season.春天: return springSprite;
            case Season.夏天: return summerSprite;
            case Season.秋天: return autumnSprite;
            case Season.冬天: return winterSprite;
            default: return null;
        }
    }

    /// <summary>
    /// 取出当前季节对应的夜晚图片
    /// </summary>
    private Sprite GetSeasonNightSprite(Season season)
    {
        switch (season)
        {
            case Season.春天: return springNightSprite;
            case Season.夏天: return summerNightSprite;
            case Season.秋天: return autumnNightSprite;
            case Season.冬天: return winterNightSprite;
            default: return null;
        }
    }
}
