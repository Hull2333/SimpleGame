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

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                EventHandler.CallTransitionEvent(sceneToGo, positionToGo);
            }
        }
    }

}
