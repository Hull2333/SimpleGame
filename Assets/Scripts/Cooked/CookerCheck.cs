using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CookerCheck : MonoBehaviour   //调用在烹饪家具对象上
{
    //进入到可烹饪范围
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            EventHandler.CallCheckCookedUIEvent(true);
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            EventHandler.CallCheckCookedUIEvent(false);
        }
    }
}
