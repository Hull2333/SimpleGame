using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFarm.Map;
using UnityEngine.Windows.Speech;
namespace MFarm.AStar
{
    public class AStar : Singleton<AStar>  //调用在NPCManager对象上,Singleton<AStar>单例模式，使其他脚本可以直接调用里面的方法，加Instance,using AStar
    {
        //每个地图的每个节点
        private GridNodes gridNodes;
        private Node startNode;
        private Node targetNode;
        private int gridWidth;
        private int gridHeight;
        private int originX;
        private int originY;
        //当前选中Node周围的8个Node
        private List<Node> openNodeList;
        //将openNodeList上面8个Node中选中的点加入到此列表中，HashSet快速查找被选中的点
        private HashSet<Node> closedNodeList;
        //是否找到路径
        private bool pathFound;
        /// <summary>
        /// 构建路径更新Stack每一步
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <param name="npcMocementStep"></param>
        public void buildPath(string sceneName,Vector2Int startPos,Vector2Int endPos,Stack<MovementStep> npcMocementStep)
        {
            pathFound = false;
            if(GenerateGridNodes(sceneName,startPos,endPos))
            {
                //查找最短路径
                if (FindShortestPath())
                {
                    //构建NPC移动路径
                    UpdatePathOnMovementStepStack(sceneName,npcMocementStep);
                }
            }
        }
        /// <summary>
        /// 构建网格节点信息，初始化两个列表
        /// </summary>
        /// <param name="sceneName">场景名字</param>
        /// <param name="startPos">起点</param>
        /// <param name="endPos">终点</param>
        /// <returns></returns>
        private bool GenerateGridNodes(string sceneName,Vector2Int startPos,Vector2Int endPos)
        {
            if(GridMapManager.Instance.GetGridDimensions(sceneName,out Vector2Int gridDimensions,out Vector2Int gridOrigin))
            {

                //根据瓦片地图范围构建网格移动节点范围数组
                gridNodes = new GridNodes(gridDimensions.x, gridDimensions.y);
                gridWidth = gridDimensions.x;
                gridHeight = gridDimensions.y;
                originX = gridOrigin.x;
                originY = gridOrigin.y;
                //执行到这里表示已经确认拿到了当前地图的信息，这是开始初始化两个列表
                openNodeList = new List<Node>();
                closedNodeList = new HashSet<Node>();
            }
            //没拿到当前地图信息
            else
            {
                return false;
            }
            //获取NPC移动的初始节点和终点，gridNodes的范围是从0,0开始，所以需要减去原点坐标得到实际位置
            startNode = gridNodes.GetGridNode(startPos.x - originX, startPos.y - originY);
            targetNode = gridNodes.GetGridNode(endPos.x - originX, endPos.y - originY);
            for(int x = 0; x < gridWidth; x++)
            {
                for(int y = 0; y < gridHeight; y++)
                {
                    //使获取到的瓦片坐标为正数
                    Vector3Int tilePos = new Vector3Int(x + originX, y + originY, 0);
                    var key = tilePos.x + "X" + tilePos.y + "Y" + sceneName;

                    TileDetails tile = GridMapManager.Instance.GetTileDetails(key);
                    if(tile != null)
                    {
                        Node node = gridNodes.GetGridNode(x,y);
                        //判断格子是否为障碍物
                        if (tile.isNPCObstacle)
                        {
                            node.isObstacle = true;
                        }
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// 找到最短路径所有Node并添加到closedNodelist
        /// </summary>
        /// <returns></returns>
        public bool FindShortestPath()
        {
            //添加起点
            openNodeList.Add(startNode);
            while(openNodeList.Count > 0)
            {
                //自动执行Node脚本中的CompareTo方法
                openNodeList.Sort();
                //比较出最近的Node后，为openNodeList的一个Node，将其在openNodeList中删除再加到closedNodeList中
                Node closeNode = openNodeList[0];
                openNodeList.RemoveAt(0);
                closedNodeList.Add(closeNode);
                //但选中的closeNode等于目标Node时,找到路径并跳出循环
                if(closeNode == targetNode)
                {
                    pathFound = true;
                    break;
                }
                //计算周围8个Node补充到openNodeList
                EvaluateNeighbourNodes(closeNode);
            }
            return pathFound;
        }
        /// <summary>
        /// 评估周围8个点，并生成对应消耗值
        /// </summary>
        /// <param name="currentNode">当前网格</param>
        private void EvaluateNeighbourNodes(Node currentNode)
        {
            Vector2Int currentNodePos = currentNode.gridPosition;
            //周围8个点中的可行的点
            Node validNeighbourNote;
            //遍历中心点周围的8个点，忽略自身的点
            for(int x= -1; x <= 1; x++)
            {
                for(int y= -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }
                    validNeighbourNote = GetValidNeighbourNode(currentNodePos.x + x, currentNodePos.y + y);
                    if (validNeighbourNote != null)
                    {
                        if (!openNodeList.Contains(validNeighbourNote))
                        {
                            //有效点到起点的距离
                            validNeighbourNote.gCost = currentNode.gCost + GetDistance(currentNode, validNeighbourNote);
                            //有效点到终点的距离
                            validNeighbourNote.hCost = GetDistance(validNeighbourNote, targetNode);
                            //链接父节点
                            validNeighbourNote.parentNode = currentNode;
                            openNodeList.Add(validNeighbourNote);   
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 找到有效的Node，非障碍，非已选择
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Node GetValidNeighbourNode(int x, int y)
        {
            if(x>=gridWidth || y>=gridHeight || x < 0 || y < 0)
            {
                return null;
            }
            //返回的Node既不是障碍物，也不是上一个选中的Node
            Node neighbourNode = gridNodes.GetGridNode(x, y);
            if(neighbourNode.isObstacle || closedNodeList.Contains(neighbourNode))
            {
                return null;
            }
            else
            {
                return neighbourNode;
            }    
        }
        /// <summary>
        /// 返回两点的距离值
        /// </summary>
        /// <param name="nodeA"></param>
        /// <param name="nodeB"></param>
        /// <returns>14的倍数，10的倍数</returns>
        private int GetDistance(Node nodeA,Node nodeB)
        {
            int xDistance = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
            int yDistance = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

            if(xDistance > yDistance)
            {
                return 14 * yDistance + 10 * (xDistance - yDistance);
            }
            return 14 * xDistance + 10 * (yDistance - xDistance);
        }
        /// <summary>
        /// 更新路径每一步的坐标的场景名字
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="npcMovementStep"></param>
        private void UpdatePathOnMovementStepStack(string sceneName,Stack<MovementStep> npcMovementStep)
        {
            //从路径终点开始倒着往回推算
            Node nextNode = targetNode;
            while(nextNode != null)
            {
                MovementStep newStep = new MovementStep();
                newStep.sceneName = sceneName;  
                newStep.gridCoordinate = new Vector2Int(nextNode.gridPosition.x + originX,nextNode.gridPosition.y + originY);
                //压入堆栈,Stack先进后出，List先进先出
                npcMovementStep.Push(newStep);
                nextNode = nextNode.parentNode;
            }
        }
    }
}
