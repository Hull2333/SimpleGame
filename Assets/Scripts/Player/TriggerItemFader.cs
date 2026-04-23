using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TriggerItemFader : MonoBehaviour   //调用在Player对象上
{

    public void OnTriggerEnter2D(Collider2D other)
    {
        //运用数组的方式来调用物体遮挡透视效果
        ItemFader[] faders = other.GetComponentsInChildren<ItemFader>();
        if(faders.Length > 0)
        {
            foreach(var item in faders)
            {
                item.FadeOut();
            }
        }

       
    }
    public void OnTriggerExit2D(Collider2D other)
    {
        ItemFader[] faders = other.GetComponentsInChildren<ItemFader>();
        if (faders.Length > 0)
        {
            foreach (var item in faders)
            {
                item.FadeIn();
            }
        }
    }
}
