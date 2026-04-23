using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFarm.AStar
{
    /// <summary>
    /// 每个地图格子的Cost值
    /// </summary>
    //IComparable 比较Node值
    public class Node : IComparable<Node>
    {
        //网格坐标
        public Vector2Int gridPosition;
        //距离Start格子的距离
        public int gCost;
        //距离target格子的距离
        public int hCost;
        //当前格子的值
        public int FCost => gCost + hCost;
        //当前格子是否是障碍
        public bool isObstacle = false;
        //父节点
        public Node parentNode;
        /// <summary>
        /// 初始化网格
        /// </summary>
        /// <param name="pos"></param>
        public Node(Vector2Int pos)
        {
            gridPosition = pos;
            parentNode = null;
        }
        /// <summary>
        /// 比较当前格子和other的FCost大小，大于返回1，等于返回0，小于返回-1
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Node other)
        {
            int result = FCost.CompareTo(other.FCost);
            //当FCost相等时，在比较hCost的大小
            if (result == 0)
            {
                result = hCost.CompareTo(other.hCost);
            }
           return result;
        }
    }
}

