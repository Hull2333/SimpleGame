using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using UnityEngine.WSA;

public class SwapTimemap : MonoBehaviour //挂在每个scene的Grid上，随季节切换地图图块
{
    public Tilemap[] tilemaps;
    public TileBase[] springTileBase;
    public TileBase[] summerTileBase;
    public TileBase[] autumnTileBase;
    public TileBase[] WinterTileBase;
    //记录每张图块在同组里的下标，四季数组按同一下标对应同一块地图
    private Dictionary<TileBase, int> tileIndexDic = new Dictionary<TileBase, int>();
    private TileBase[][] seasonTileBases;
    //当前地图所处的季节，场景中的图块默认按春季摆放
    private Season currentSeason = Season.春天;

    private void Update()
    {
        //测试用：按 Q 让地图切到下一个季节
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SwapSeasonTile((Season)(((int)currentSeason + 1) % seasonTileBases.Length));
        }
    }

    private void Awake()
    {
        seasonTileBases = new TileBase[][] { springTileBase, summerTileBase, autumnTileBase, WinterTileBase };
        for (int season = 0; season < seasonTileBases.Length; season++)
        {
            for (int i = 0; i < seasonTileBases[season].Length; i++)
            {
                if (!tileIndexDic.ContainsKey(seasonTileBases[season][i]))
                {
                    tileIndexDic.Add(seasonTileBases[season][i], i);
                }
            }
        }
    }

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

    //日期推进时季节可能变化，按当前季节同步地图
    private void OnGameDayEvent(int day, Season season, Weekday weekday)
    {
        SwapSeasonTile(season);
    }

    //进入场景时同步一次，室内场景没有该脚本，回到室外时地图可能还停留在旧季节
    private void OnAfterSceneLoadedEvent()
    {
        SwapSeasonTile(TimeManager.Instance.gameSeason);
    }

    /// <summary>
    /// 把地图图块切换到指定季节
    /// </summary>
    public void SwapSeasonTile(Season season)
    {
        if (season == currentSeason)
        {
            return;
        }
        TileBase[] targetTiles = seasonTileBases[(int)season];
        for (int i = 0; i < tilemaps.Length; i++)
        {
            BoundsInt bounds = tilemaps[i].cellBounds;

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int position = new Vector3Int(x, y);
                    TileBase tile = tilemaps[i].GetTile(position);
                    if (tile != null && tileIndexDic.TryGetValue(tile, out int index) && index < targetTiles.Length)
                    {
                        tilemaps[i].SetTile(position, targetTiles[index]);
                    }
                }
            }
        }
        currentSeason = season;
    }
}
