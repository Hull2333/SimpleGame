using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCFunction : MonoBehaviour    //调用在需要打开NPC背包的NPC对象上
{
    //NPC背包
    public InventoryBag_SO shopData;
    //当前NPC背包是否打开
    private bool isOpen;

    private void Update()
    {
        if(isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            //关闭商店
            CloseShop();
        }
    }
    /// <summary>
    /// 打开商店
    /// </summary>
    public void OpenShop()
    {
        isOpen = true;
        EventHandler.CallBaseBagOpenEvent(SlotType.Shop,shopData);
        EventHandler.CallUpdateGameStateEvent(GameState.Pause);
    }

    public void CloseShop()
    {
        isOpen = false;
        EventHandler.CallBaseBagCloseEvent(SlotType.Shop,shopData);
        EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
    }
}
