using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExclamationMark : MonoBehaviour    //调用在Exclamation Mark对象上
{
    public PlayerController playerController;
    public void CloseMark()
    {
        this.gameObject.SetActive(false);
        playerController.canCatchFish = false;
    }
    public void canCatchFish()
    {
        playerController.canCatchFish = true;
    }
    
}
