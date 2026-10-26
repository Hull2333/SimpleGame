using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using MFarm.CropPlant;
using static UnityEditor.Progress;

namespace MFarm.Inventory
{
    public class Item : MonoBehaviour   //调用在ItemBase对象上
    {
        public int itemID;

        private SpriteRenderer spriteRenderer;
        public ItemDetails itemDetails;
        private Transform player;
        private Item item;
        //是否已经预定了拾取名额（正在飞向玩家但还没放进背包）
        private bool reserved;
        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            item = GetComponent<Item>();
        }

        private void Start()
        {
            if (itemID != 0)
            {
                Init(itemID);
            }
        }
        public void Update()
        {
            ItemMoveToPlayer();
        }
        /// <summary>
        /// 生成物品在地图上
        /// </summary>
        /// <param name="ID"></param>

        public void Init(int ID)
        {
            //使当前itemID等于传进来的ID
            itemID = ID;
            itemDetails = InventoryManager.Instance.GetItemDetails(itemID);
            //避免搜索结果为空
            if (itemDetails != null)
            {
                spriteRenderer.sprite = itemDetails.itemOnWorldSprite != null ? itemDetails.itemOnWorldSprite : itemDetails.itemIcon;
            }
                //if(itemDetails != null)
                //{
                //    spriteRenderer.sprite = itemDetails.itemOnWorldSprite != null ? itemDetails.itemOnWorldSprite : itemDetails.itemIcon;
                //    //修改所有物品碰撞体尺寸
                //    Vector2 newSize = new Vector2(spriteRenderer.sprite.bounds.size.x / 2, spriteRenderer.sprite.bounds.size.y / 2);
                //    coll.size = newSize;
                //    //修改物品碰撞体y轴偏移至sprite中心
                //    coll.offset = new Vector2(0, spriteRenderer.sprite.bounds.center.y);
                //}
             //当Item类型为ReapableScenery，直接添加ReapItem脚本，初始化Item信息以及可以生成收割后产生的物品
            //if (itemDetails.itemType == ItemType.ReapableScenery)
            //{
                //gameObject.AddComponent<ReapableItem>();
                //gameObject.GetComponent<ReapItem>().InitCropData(itemDetails.getItemID);
                //人物经过杂草时晃动
                //gameObject.AddComponent<ItemInterActive>();  
            //}
            //if(itemDetails.itemType == ItemType.Enemy)
            //{
            //    gameObject.AddComponent<ReapItem>();
            //    gameObject.GetComponent<ReapItem>().InitCropData(itemDetails.getItemID);
            //}
        }
        /// <summary>
        /// PLayer进入物体吸附范围
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.tag == "Player")
            {
                //已经预定过拾取名额就不再重复预定
                if (reserved)
                {
                    return;
                }
                //按背包剩余容量预定拾取名额，容量不够时不吸附，只提示背包已满
                if (InventoryManager.Instance.TryReservePickup(itemID))
                {
                    reserved = true;
                    player = other.transform;
                }
                else
                {
                    EventHandler.CallBagFullTipEvent();
                }
            }
        }
        /// <summary>
        /// Item找到PLayer开始往Player移动
        /// </summary>
        private void ItemMoveToPlayer()
        {
            if (player != null && itemDetails.canPickedup)
            {
                transform.position = Vector2.MoveTowards(transform.position, player.position, 8f * Time.deltaTime);
                if (Vector3.Distance(gameObject.transform.position, player.transform.position) <= 0.1f)
                {
                    //飞行途中容量可能被别的东西占用，放不下就取消吸附并释放名额，避免一直跟着玩家
                    if (!InventoryManager.Instance.AddItem(item, true))
                    {
                        player = null;
                        ReleasePickupReservation();
                        return;
                    }
                    //更新玩家接受的任务
                    EventHandler.CallUpdateQuestProgressEvent(itemID,1);
                }

            }
        }

        /// <summary>
        /// 释放预定但没用上的拾取名额
        /// </summary>
        private void ReleasePickupReservation()
        {
            if (reserved)
            {
                InventoryManager.Instance.ReleasePickup(itemID);
                reserved = false;
            }
        }

        private void OnDestroy()
        {
            //物品被捡走销毁或随场景卸载时释放名额，避免名额泄漏
            ReleasePickupReservation();
        }
    }
   

}

