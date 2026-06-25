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
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (isRandomTeleport)
                {
                    GetNextMineScene();
                }
                EventHandler.CallTransitionEvent(sceneToGo, positionToGo);

            }
        }
        /// <summary>
        /// 获取下一个矿洞场景
        /// </summary>
        private void GetNextMineScene()
        {
            int sceneIndex = Random.Range(0, mineData.mineSceneList.Count);
            sceneToGo = mineData.mineSceneList[sceneIndex].sceneName;
            positionToGo = mineData.mineSceneList[sceneIndex].GoToPos;
        }
    }

}
