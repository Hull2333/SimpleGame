using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CookerController : MonoBehaviour   //调用在烹饪家具对象上
{
    //烹饪UI
    private Canvas cookedCanve;
    private PlayerController playerController;

    public void Start()
    {
        cookedCanve = GameObject.FindWithTag("CookedUI").GetComponent<Canvas>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if(other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            playerController = other.GetComponentInParent<PlayerController>();
            playerController.isMoving = false;
            playerController.inputDisable = true;
            //暂停游戏时间
            EventHandler.CallUpdateGameStateEvent(GameState.Pause);
            cookedCanve.enabled = true;
            EventHandler.CallCookedMenuSetupEvent(2500);
        }
    }
}
