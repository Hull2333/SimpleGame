using MFarm.AStar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFarm.AStar
{
    public class GridNodes //用于绘制所有游戏地图的Node值
    {
        //整个场景的宽度和高度
        private int width;
        private int height;
        //每个Node的x，y轴，起名叫gridNode
        private Node[,] gridNode;
        /// <summary>
        /// 构造游戏中每个格子的Cost值，构造函数初始化节点范围数组
        /// </summary>
        /// <param name="width">地图宽度</param>
        /// <param name="height">地图高度</param>
        public GridNodes(int width, int height)
        {
            this.width = width;
            this.height = height;

            gridNode = new Node[width, height];
            //获取每个网格的x,y坐标
            for(int x = 0; x < width; x++)
            {
                for(int y = 0; y < height; y++)
                {
                    gridNode[x,y]=new Node(new Vector2Int(x,y));
                }
            }
        }

        public Node GetGridNode(int xPos, int yPos)
        {
            if (xPos <= width && yPos <= height)
            {
                return gridNode[xPos, yPos];
            }
            Debug.Log("xPos:  " + xPos + "yPos:  " + yPos +  "超出网格范围: " + width +"  "+ height);
            return null;
        }
    }
}

