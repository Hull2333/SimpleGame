using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFarm.Inventory
{
    public class ItemPickUp : MonoBehaviour //调用在Player对象上
    {
        private Item item;       
        private void Start()
        {
            item = null;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {

            item = other.GetComponent<Item>();
            if (item != null)
            {
                if (item.itemDetails.canPickedup)
                {
                    //给拾取提示物品赋值ID，图片，数量
                    var itemTip = new GetItemTip { itemID = item.itemDetails.itemID, itemIcon = item.itemDetails.itemIcon, tipAmount = 1 };
                    //拾取物品提示
                    EventHandler.CallGetItemTipEvent(item.itemDetails);
                    //播放拾取音效
                    EventHandler.CallPlaySoundEvent(SoundName.Pickup);
                }
            }

        }

    }
        
}

