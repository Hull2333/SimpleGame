using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySelf : MonoBehaviour //调用在需要动画帧结束后摧毁自身的物体上
{
    public void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}
