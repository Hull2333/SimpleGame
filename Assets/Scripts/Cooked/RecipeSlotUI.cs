using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using MFarm.Inventory;
using UnityEngine.UI;
using MFarm.Cooked;
using static UnityEditor.Progress;
using TMPro;

public class RecipeSlotUI : MonoBehaviour, IPointerClickHandler //调用在RecipeSlot按钮上
{
    //未解锁该食谱
    public bool isGray;
    public CookedUI cookedUI;
    public int itemID;
    public CookedData_SO cookedData;
    private ItemDetails itemDetails;
    private CookedDetails cookedDetails;
    public TextMeshProUGUI recipeName;
    public Image reciprImage;
    private void OnEnable()
    {
        itemDetails = InventoryManager.Instance.GetItemDetails(itemID);
        cookedDetails = cookedData.GetCookedDetails(itemID);
        //未解锁
        if (isGray)
        {
            reciprImage.sprite = cookedDetails.graySprite;
            recipeName.gameObject.SetActive(false);
            foreach (int ID in InventoryManager.Instance.learnedRecipeList)
            {
                
                if (itemID == ID)
                {
                    reciprImage.sprite = itemDetails.itemIcon;
                    recipeName.gameObject.SetActive(true);
                    recipeName.text = itemDetails.itemName;
                    isGray = false;
                }
            }
        }
        //已解锁
        else
        {
            reciprImage.sprite = itemDetails.itemIcon;
            recipeName.gameObject.SetActive(true);
            recipeName.text = itemDetails.itemName;
        }
    }
    /// <summary>
    /// 点击食谱执行的方法
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isGray)
        {
            cookedUI.currentRecipeDetails = cookedDetails;
            cookedUI.recipeSprite = itemDetails.itemIcon;
            cookedUI.tweenInitPos = transform.position;
            cookedUI.ShowIngredientSlotUI(cookedDetails.cookResource);

        }
        
    }
}



