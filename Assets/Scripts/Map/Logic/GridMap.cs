using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
//该脚本在编辑模式下运行，而不是在游戏中运行
[ExecuteInEditMode]
public class GridMap : MonoBehaviour
{
    public MapData_SO mapData;
    public GridType gridType;
    private Tilemap currentTilemap;

    private void OnEnable()
    {
        if (!Application.IsPlaying(this))
        {
            currentTilemap = GetComponent<Tilemap>();
            if(mapData != null)
            {
                mapData.tilePropertyes.Clear();
            }
        }
    }
    private void OnDisable()
    {
        if (!Application.IsPlaying(this))
        {
            currentTilemap = GetComponent<Tilemap>();
            UpdateTileProperties();
            //将mapData标记为Dirty，在可以实时保存
//确保是在编辑模式下运行
#if UNITY_EDITOR
            if (mapData != null)
            {
                EditorUtility.SetDirty(mapData);
            }
#endif
        }
    }
    /// <summary>
    /// 获取当前地图中的每一个Tile格子
    /// </summary>
    private void UpdateTileProperties() 
    {
        //检测实际有绘制的地块,压缩地图块
        currentTilemap.CompressBounds();
        if (!Application.IsPlaying(this))
        {
            if (mapData != null)
            {
                //已绘制范围的左下角坐标
                Vector3Int startPos = currentTilemap.cellBounds.min;
                //已绘制范围的右上角坐标
                Vector3Int endPos = currentTilemap.cellBounds.max;
                //遍历实际绘制范围Tile的x坐标
                for(int x = startPos.x; x < endPos.x; x++)
                {
                    //遍历实际绘制范围Tile的y坐标
                    for (int y = startPos.y; y < endPos.y; y++)
                    {
                        //获取当前地图中的每一个Tile,TileBase表示每一个Tile
                        TileBase tile = currentTilemap.GetTile(new Vector3Int(x,y,0));
                        if(tile != null)
                        {
                            TileProperty newTile = new TileProperty
                            {
                                tileCoordinate = new Vector2Int(x, y),
                                gridType = this.gridType,
                                boolTypeValue = true
                            };
                            //最后将收集到的全部Tile加到tilePropertyes列表中
                            mapData.tilePropertyes.Add(newTile);
                        }
                    }
                }
            }
        }
    }
}
