using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFarm.Inventory;
using Unity.VisualScripting;
public class AnimatorOverride : MonoBehaviour   //调用在Player对象上
{

    public SpriteRenderer holdItem;
    [Header("角色各部位动画")]
    public List<AnimatorType> animatorTypes;
    //字典,通过输入关键字string来返回对应的Animator,储存Player对象下的所有Animator
    private Dictionary<string,Animator> animatorNameDict = new Dictionary<string,Animator>();
    public SpriteRenderer[] playerSprite;
    public GameObject eatAnim;
    private PlayerController playerController;
    //当前装备的装备信息
    public ItemDetails equipHead,equipBody;
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        //遍历玩家全身的动画
        foreach (Animator anim in playerController.animators)
        {
            animatorNameDict.Add(anim.name, anim);
        }
    }

    private void OnEnable()
    {
        EventHandler.ItemSelectEvent += OnItemSelectEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.EquipSlotEvent += OnEquipSlotEvent;
        EventHandler.DisplayCollectItemSprite += OnDisplayCollectItemSprite;
    }
    private void OnDisable()
    {
        EventHandler.ItemSelectEvent -= OnItemSelectEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.EquipSlotEvent -= OnEquipSlotEvent;
        EventHandler.DisplayCollectItemSprite -= OnDisplayCollectItemSprite;
    }

    /// <summary>
    /// 设置切换场景后，手部动作恢复为默认，且拿着的东西回到背包
    /// </summary>
    private void OnBeforeSceneUnloadEvent()
    {
        holdItem.enabled = false;
        SwitchAnimator(PartType.None);
    }

    /// <summary>
    /// 不同的item类型在玩家Slot选中时返回不同的手臂动画
    /// </summary>
    /// <param name="itemDetails"></param>
    /// <param name="isSelected"></param>
    private void OnItemSelectEvent(ItemDetails itemDetails, bool isSelected)
    {
        if(itemDetails == null)
        {
            SwitchAnimator(PartType.None);
            holdItem.enabled = false;
        }
        else
        {
            //不同的ItemType返回不同的PartType
            PartType currentType = itemDetails.itemType switch
            {

                ItemType.Seed => PartType.Carry,
                ItemType.Commodity => PartType.Carry,
                ItemType.cooked => PartType.Carry,
                ItemType.HoeTool => PartType.Hoe,
                ItemType.AxeTool => PartType.Axe,
                ItemType.BreakTool => PartType.Break,
                ItemType.WaterTool => PartType.Water,
                ItemType.ReapTool => PartType.Reap,
                ItemType.Furniture => PartType.Carry,
                ItemType.Sword => PartType.Attack1,
                ItemType.FishingRod => PartType.Fishing,
                ItemType.Laterfish => PartType.Carry,
                ItemType.Seafish => PartType.Carry,
                ItemType.Equipment_Body => PartType.Carry,
                ItemType.Equipment_Head => PartType.Carry,
                _ => PartType.None

            };
            //取消选中物品时，结束举手动画,同时关闭头顶的物品图片
            if (isSelected == false)
            {
                currentType = PartType.None;
                holdItem.enabled = false;
            }
            else
            {
                if (currentType == PartType.Carry)
                {
                    holdItem.sprite = itemDetails.itemOnWorldSprite;
                    holdItem.enabled = true;
                }
                else
                {
                    holdItem.enabled = false;
                }

            }
            SwitchAnimator(currentType);
        }
        
    }
    private void OnEquipSlotEvent(SlotUI headSlot, SlotUI bodySlot)
    {
        equipHead = headSlot.itemDetails;
        equipBody = bodySlot.itemDetails;
    }
    private void OnDisplayCollectItemSprite(int ID)
    {
        holdItem.enabled = true;
        holdItem.sprite = InventoryManager.Instance.GetItemDetails(ID).itemOnWorldSprite;
        holdItem.gameObject.GetComponent<Animator>().SetTrigger("isCollect");
    }
    /// <summary>
    /// 根据当前的玩家部位类型来切换对应的玩家动画   
    /// </summary>
    /// <param name="partType"></param>
    private void SwitchAnimator(PartType partType)
    {
        foreach (var anim in animatorTypes)
        {
            if (anim.partType == partType)
            {
                //当没有装备头部装备时
                if(equipHead == null || equipHead.itemID == 0)
                {
                    //更换头部动作
                    if(anim.partName == PartName.Head && anim.equipType == EquipType.None)
                    {
                        animatorNameDict[anim.partName.ToString()].runtimeAnimatorController = anim.overrideController;
                    }
                }
                //当没有装备身体装备时
                if(equipBody == null || equipBody.itemID == 0)
                {
                    if(anim.equipType == EquipType.None)
                    {
                        //更换手部、身体、工具、影子动画
                        if (anim.partName == PartName.Body)
                        {
                            animatorNameDict[anim.partName.ToString()].runtimeAnimatorController = anim.overrideController;
                        }
                        if (anim.partName == PartName.Arm)
                        {
                            animatorNameDict[anim.partName.ToString()].runtimeAnimatorController = anim.overrideController;
                        }
                        if (anim.partName == PartName.Tool)
                        {
                            animatorNameDict[anim.partName.ToString()].runtimeAnimatorController = anim.overrideController;
                        }
                        if(anim.partName == PartName.Shadow)
                        {
                            animatorNameDict[anim.partName.ToString()].runtimeAnimatorController = anim.overrideController;
                        }
                    }
                   
                }
                //当装了头部装备时
                if(equipHead != null)
                {
                    //更换头部动作
                    if (anim.partName == PartName.Head && anim.equipType == equipHead.equipType)
                    {
                        animatorNameDict[anim.partName.ToString()].runtimeAnimatorController = anim.overrideController;
                    }
                }
                //当装备了身体装备时
                if(equipBody != null)
                {
                    //更换手部、身体、工具、影子动画
                    if (anim.equipType == equipBody.equipType)
                    {
                        if (anim.partName == PartName.Body)
                        {
                            animatorNameDict[anim.partName.ToString()].runtimeAnimatorController = anim.overrideController;
                        }
                        if (anim.partName == PartName.Arm)
                        {
                            animatorNameDict[anim.partName.ToString()].runtimeAnimatorController = anim.overrideController;
                        }
                        if (anim.partName == PartName.Tool)
                        {
                            animatorNameDict[anim.partName.ToString()].runtimeAnimatorController = anim.overrideController;
                        }
                        if (anim.partName == PartName.Shadow)
                        {
                            animatorNameDict[anim.partName.ToString()].runtimeAnimatorController = anim.overrideController;
                        }
                    }
                   
                }
                
             
            }
        }

    }
    /// <summary>
    /// 播放吃东西动画
    /// </summary>
    public void PlayEatAnim()
    {
        playerController.inputDisable = true;
        EventHandler.CallParticleEffectEvent(ParticalEffectType.Eat01,new Vector3(transform.position.x  , transform.position.y + 2f), Vector2.up);
        for (int i = 0; i < playerSprite.Length; i++)
        {
            playerSprite[i].enabled = false;
        }
        eatAnim.SetActive(true);
        StartCoroutine(WaitForEatOver());
    }
    /// <summary>
    /// 等待吃东西动画结束后恢复玩家图片
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitForEatOver()
    {
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < playerSprite.Length; i++)
        {
            playerSprite[i].enabled = true;
        }
        eatAnim.SetActive(false);
        playerController.inputDisable = false;
    }
}
