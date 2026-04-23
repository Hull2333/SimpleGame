using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using UnityEngine.WSA;

public class SwapTimemap : MonoBehaviour //调用在每个scenen中的Grid对象上
{
    public Tilemap[] tilemaps;
    public TileBase[] springTileBase;
    public TileBase[] summerTileBase;
    public TileBase[] autumnTileBase;
    public TileBase[] WinterTileBase;
    private Dictionary<TileBase,TileBase> springToSummerTileDic = new Dictionary<TileBase, TileBase>();
    private Dictionary<TileBase, TileBase> summerToAutumnTileDic = new Dictionary<TileBase, TileBase>();
    private Dictionary<TileBase, TileBase> autumnToWinterTileDic = new Dictionary<TileBase, TileBase>();
    private Dictionary<TileBase, TileBase> winterToSpringTileDic  = new Dictionary<TileBase, TileBase>();
    public int swapInt;
    public void Start()
    {
        //匹配春天和夏天之间的瓦片
        for (int i = 0; i < springTileBase.Length; i++)
        {
            if (!springToSummerTileDic.ContainsKey(springTileBase[i]))
            {
                springToSummerTileDic.Add(springTileBase[i], summerTileBase[i]);
            }
        }
        //匹配夏天和秋天之间的瓦片
        for (int i = 0; i < summerTileBase.Length; i++)
        {
            if (!summerToAutumnTileDic.ContainsKey(summerTileBase[i]))
            {
                summerToAutumnTileDic.Add(summerTileBase[i], autumnTileBase[i]);
            }
        }
        //匹配秋天和冬天之间的瓦片
        for (int i = 0; i < autumnTileBase.Length; i++)
        {
            if (!autumnToWinterTileDic.ContainsKey(autumnTileBase[i]))
            {
                autumnToWinterTileDic.Add(autumnTileBase[i], WinterTileBase[i]);
            }
        }
        //匹配冬天和春天之间的瓦片
        for (int i = 0; i < WinterTileBase.Length; i++)
        {
            if (!winterToSpringTileDic.ContainsKey(WinterTileBase[i]))
            {
                winterToSpringTileDic.Add(WinterTileBase[i], springTileBase[i]);
            }
        }
     
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SwapSeasonTile();
        }
    }
    /// <summary>
    /// 更换季节瓦片
    /// </summary>
    public void SwapSeasonTile()
    {
        for (int i = 0; i < tilemaps.Length; i++)
        {
            BoundsInt bounds = tilemaps[i].cellBounds;

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int position = new Vector3Int(x, y);
                    TileBase tile = tilemaps[i].GetTile(position);
                    if (tile != null)
                    {
                       
                        if (swapInt == 0)
                        {
                            swapInt++;
                            if (springToSummerTileDic.TryGetValue(tile, out TileBase newTile))
                            {
                                tilemaps[i].SetTile(position, newTile);
                            }
                        }
                        if (swapInt == 1)
                        {
                            if (summerToAutumnTileDic.TryGetValue(tile, out TileBase newTile))
                            {
                                tilemaps[i].SetTile(position, newTile);
                            }
                            swapInt++;
                        }
                        if (swapInt == 2)
                        {
                            if (autumnToWinterTileDic.TryGetValue(tile, out TileBase newTile))
                            {
                                tilemaps[i].SetTile(position, newTile);
                            }
                            swapInt++;
                        }
                        if (swapInt == 3)
                        {
                            if (winterToSpringTileDic.TryGetValue(tile, out TileBase newTile))
                            {
                                tilemaps[i].SetTile(position, newTile);
                            }
                            swapInt = 0;
                        }
                    }

                }
            }
        }


    }
}
