using MFarm.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldItem : MonoBehaviour //调用在Player的HoldItem对象上
{
    public PlayerController playerController;
    public ItemDetails itemDetails;

    /// <summary>
    /// 在HoldItem的收获动画帧中运行
    /// </summary>
    public void StartEvent()
    {
        //取消玩家举起动画
        EventHandler.CallItemSelectEvent(InventoryManager.Instance.currentSelectedItem, false);
    }
    /// <summary>
    /// 执行玩家吃完的动画，调用在HoldItem动画帧上 
    /// </summary>
    public void PlayEatDoneAnim()
    {
        itemDetails = InventoryManager.Instance.currentSelectedItem;
        foreach (var anim in playerController.animators)
        {
            anim.SetTrigger("isEatDone");
        }
        
        EventHandler.CallPlayerDecreaseHealthEvent(-itemDetails.recoverHealth);
        EventHandler.CallPlayerDecreaseStminaEvent(-itemDetails.recoverStmina);
        EventHandler.CallParticleEffectEvent(ParticalEffectType.Eat01,transform.position,Vector2.zero);
        
    }
    /// <summary>
    /// 恢复游戏，调用在HoldItem动画帧上 
    /// </summary>
    public void RecoverGameplay()
    {
        EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
        EventHandler.CallControlPlayerBagOpen(true);
        InventoryManager.Instance.RemoveItemOfIndex(1);
    }
}
