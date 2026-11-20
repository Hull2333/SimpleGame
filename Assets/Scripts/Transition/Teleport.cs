using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
namespace MFarm.Transition
{
    public class Teleport : MonoBehaviour   //调用在所有传送点Teleport对象上
    {
        public string sceneToGo;
        //到达新场景时所在的位置
        public Vector3 positionToGo;
        [Header("随机传送相关，开启后可以不填上面的场景和位置")]
        public MineSceneDataList_SO mineData;
        public bool isRandomTeleport;
        public int buildCode;
        [Header("手动传送，开启后玩家在范围内右键点击传送点")]
        public bool needRightClick = false;
        //最近两次传送进入的矿洞场景（静态，跨场景保留），接下来的两次随机不会再抽到它们
        private static readonly List<string> recentMineScenes = new List<string>();
        private void OnTriggerEnter2D(Collider2D other)
        {
            //关闭手动传送时保持原来的进入触发器自动传送
            if (!needRightClick && other.CompareTag("Player"))
            {
                TriggerTeleport();
            }
        }
        private void OnTriggerStay2D(Collider2D other)
        {
            //手动模式下，玩家处于传送点碰撞范围内且右键点到传送点才触发
            if (needRightClick && other.CompareTag("Player") && Input.GetMouseButtonDown(1) && IsMouseOnTeleport())
            {
                TriggerTeleport();
            }
        }
        private bool IsMouseOnTeleport()
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return GetComponent<Collider2D>().OverlapPoint(mouseWorldPos);
        }
        private void TriggerTeleport()
        {
            if (isRandomTeleport)
            {
                GetNextMineScene();
            }
            if(buildCode != 0)
            {
                EventHandler.CallGetCurrentBuildCode(buildCode,transform.position);
            }
            EventHandler.CallTransitionEvent(sceneToGo, positionToGo);
        }
        /// <summary>
        /// 获取下一个矿洞场景
        /// </summary>
        private void GetNextMineScene()
        {
            //先筛掉最近两次刚进入过的场景，避免短时间内重复传送到同一个矿洞
            List<MineScene> candidates = new List<MineScene>();
            foreach (MineScene scene in mineData.mineSceneList)
            {
                if (!recentMineScenes.Contains(scene.sceneName))
                {
                    candidates.Add(scene);
                }
            }
            //可选场景不足时退回全部场景，避免没有任何可传送目标
            if (candidates.Count == 0)
            {
                candidates = mineData.mineSceneList;
            }
            int sceneIndex = Random.Range(0, candidates.Count);
            sceneToGo = candidates[sceneIndex].sceneName;
            positionToGo = candidates[sceneIndex].GoToPos;
            //记录本次进入的场景，只保留最近两条记录
            recentMineScenes.Add(sceneToGo);
            while (recentMineScenes.Count > 2)
            {
                recentMineScenes.RemoveAt(0);
            }
        }
    }

}
