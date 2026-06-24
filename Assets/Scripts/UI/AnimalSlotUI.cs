using MFarm.Inventory;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static AnimalData_SO;

public class AnimalSlotUI : MonoBehaviour //调用在AnimalShopSlot预制体上
{
    public Image spriteImage;
    public TextMeshProUGUI name;
    public TextMeshProUGUI price;
    public int animalID;
    public AnimalDetails currentAnimalDetails;
    public AnimalData_SO animalData;
    public ItemManager itemManager;
    private Button button => GetComponent<Button>();
    public void Start()
    {
        button.onClick.AddListener(SelectAnimalBuilding);
    }
    public void UpdateAnimalSlotUI(int ID)
    {
        if(itemManager == null)
        {
            itemManager = FindAnyObjectByType<ItemManager>();
        }
        animalID = ID;
        currentAnimalDetails = animalData.GetAnimalDetails(ID);
        spriteImage.sprite = currentAnimalDetails.animalSprite;
        name.text = currentAnimalDetails.name;
        price.text = currentAnimalDetails.animalPrice.ToString();
        button.interactable = true;
        //检查是否符合购买动物的条件
        if (!itemManager.HaveBuildingCanBreeding(currentAnimalDetails.animSize))
        {
            button.interactable = false;
        }

    }
    /// <summary>
    /// 选择动物要放置的建筑
    /// </summary>
    private void SelectAnimalBuilding()
    {
        EventHandler.CallBuildindModeEvent(null,currentAnimalDetails, true);
    }

    public void BuyAnimal()
    {
        //EventHandler.CallBuyAnimalEvent()
    }
}
