using MFarm.Inventory;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;
namespace MFarm.Cooked 
{
    public class CookedUI :Singleton<CookedUI> //调用在CookedCanvas上
    {
        public GameObject cookedPanel;
        //食材SlotUI预制体
        public SlotUI ingredientSlotUI;
        //当前以生成的食材SlotUI预制体
        public List<SlotUI> currentIngredientSlotUIs;
        public Transform ingredientHolder;
        public Animator makeButtonAnim;
        public Button makeButton;
        //当前选择的菜肴
        public CookedDetails currentRecipeDetails;
        public Button quitButton;
        public GameObject chooseRecipeTip;
        //滑动特效菜肴Image
        public GameObject tweenRecipeImage;
        public Transform tweenRecipeParent;
        [HideInInspector] public Sprite recipeSprite;
        public Vector3 tweenInitPos;
        [Header("食材显示位置队列")]
        public GameObject ingredientStartPos;
        public List<Vector2> currentingredientToGoPosList = new List<Vector2>();
        public List<Vector2> ingredientToGoPosList1 = new List<Vector2>();
        public List<Vector2> ingredientToGoPosList2 = new List<Vector2>();
        public List<Vector2> ingredientToGoPosList3 = new List<Vector2>();
        public List<Vector2> ingredientToGoPosList4 = new List<Vector2>();
        private void Awake()
        {
            makeButton.onClick.AddListener(MakeRecipe);
            quitButton.onClick.AddListener(QuitCookedUI);
            GetComponent<Canvas>().worldCamera = Camera.main;
            GetComponent<Canvas>().sortingLayerName = "ValueTile";
        }

        private void OnEnable()
        {
           
            EventHandler.CookedMenuSetupEvent += OnCookedMenuSetupEvent;
        }

      
        private void OnDisable()
        {
            EventHandler.CookedMenuSetupEvent -= OnCookedMenuSetupEvent;
        }
        private void OnCookedMenuSetupEvent()
        {
            cookedPanel.SetActive(true);
            chooseRecipeTip.SetActive(true);
            makeButtonAnim.gameObject.SetActive(false);
            InventoryManager.Instance.anyBagOpened = true;
            currentIngredientSlotUIs = new List<SlotUI>();
            //清空食材栏下的子物体
            for (int i = 0; i < ingredientHolder.childCount; i++)
            {
                Destroy(ingredientHolder.GetChild(i).gameObject);
            }
            EventHandler.CallUpdateGameStateEvent(GameState.Pause);

        }
        /// <summary>
        /// 显示所需食材
        /// </summary>
        public void ShowIngredientSlotUI(InventoryItem[] Ingredients)
        {
            currentIngredientSlotUIs.Clear();
            chooseRecipeTip.SetActive(false);
            //清空食材栏下的子物体
            for (int i = 0; i < ingredientHolder.childCount; i++)
            {
                Destroy(ingredientHolder.GetChild(i).gameObject);
            }
            GenerateTweenIngredientImage(Ingredients);
            MakeTweenIngredientImage();
            //根据食材是否足够且背包还有空位来启动制作按钮
            if (InventoryManager.Instance.CheckIngredients(currentRecipeDetails.ID) && InventoryManager.Instance.CheckBagCapacity())
            {
                makeButtonAnim.gameObject.SetActive(true);
            }
            else
            {
                makeButtonAnim.gameObject.SetActive(false);
            }
        }
        /// <summary>
        /// 制作菜肴
        /// </summary>
        public void MakeRecipe()
        {
            GenerateTweenRecipeImage();
            makeButtonAnim.SetTrigger("active");
            InventoryManager.Instance.AddRewardItem(currentRecipeDetails.ID, 1);
            //减去玩家背包中的食材
            foreach (var resource in currentRecipeDetails.cookResource)
            {
                InventoryManager.Instance.RemoveItem(resource.itemID, resource.itemAmount);
            }
            //更新总数量
            foreach (SlotUI currentIngredientSlotUI in currentIngredientSlotUIs)
            {
                currentIngredientSlotUI.GetComponent<Animator>().SetTrigger("shake");
                int ingredientAllAmount = InventoryManager.Instance.playerBag.GetItemAllAmount(currentIngredientSlotUI.itemDetails.itemID);
                currentIngredientSlotUI.UpdateSlot(currentIngredientSlotUI.itemDetails, currentIngredientSlotUI.itemAmount, true, ingredientAllAmount);
                //数量字体变红
                if (currentIngredientSlotUI.itemAmount > ingredientAllAmount)
                {
                    currentIngredientSlotUI.SetRedAmountText();
                }
                else
                {
                    currentIngredientSlotUI.RecoverOriginTextColor();
                }
            }
            //根据食材是否足够且背包还有空位来启动制作按钮
            if (InventoryManager.Instance.CheckIngredients(currentRecipeDetails.ID) && InventoryManager.Instance.CheckBagCapacity())
            {
                makeButtonAnim.gameObject.SetActive(true);
            }
            else
            {
                makeButtonAnim.gameObject.SetActive(false);
            }

        }
        /// <summary>
        /// 生成并滑动菜肴到玩家背包UI
        /// </summary>
        public void GenerateTweenRecipeImage()
        {
            GameObject recipeImage = Instantiate(tweenRecipeImage, Vector3.zero, Quaternion.identity, tweenRecipeParent);
            recipeImage.GetComponent<Image>().sprite = recipeSprite;
            //拿到相对于tweenRecipeParent Panel的局部坐标
            var recipeImageRectTranform = recipeImage.GetComponent<RectTransform>();
            recipeImageRectTranform.anchoredPosition = tweenRecipeParent.InverseTransformPoint(tweenInitPos);
            Vector3 OriginPos = new Vector3(Random.Range(recipeImageRectTranform.anchoredPosition.x - 20, recipeImageRectTranform.anchoredPosition.x + 20), (Random.Range(recipeImageRectTranform.anchoredPosition.y - 20, recipeImageRectTranform.anchoredPosition.y + 20)));
            recipeImage.GetComponent<CoinTween>().PlayTween(0f, OriginPos);
        }
        /// <summary>
        /// 显示食材Image
        /// </summary>
        /// <param name="Ingredients"></param>
        public void GenerateTweenIngredientImage(InventoryItem[] Ingredients)
        {
            switch (Ingredients.Length)
            {
                case 1:
                    currentingredientToGoPosList = ingredientToGoPosList1;
                    break;
                case 2:
                    currentingredientToGoPosList = ingredientToGoPosList2;
                    break;
                case 3:
                    currentingredientToGoPosList = ingredientToGoPosList3;
                    break;
                case 4:
                    currentingredientToGoPosList = ingredientToGoPosList4;
                    break;
            }

            for (int i = 0; i < currentingredientToGoPosList.Count; i++)
            {
                ItemDetails ingredientsItem = InventoryManager.Instance.GetItemDetails(Ingredients[i].itemID);
                var ingredientSlot = Instantiate(ingredientSlotUI, ingredientStartPos.transform.position, Quaternion.identity, ingredientHolder);
                int ingredientAllAmount = InventoryManager.Instance.playerBag.GetItemAllAmount(ingredientsItem.itemID);
                ingredientSlot.UpdateSlot(ingredientsItem, Ingredients[i].itemAmount, true, ingredientAllAmount);
                //数量字体变红
                if (ingredientSlot.itemAmount > ingredientAllAmount)
                {
                    ingredientSlot.SetRedAmountText();
                }
                else
                {
                    ingredientSlot.RecoverOriginTextColor();
                }
                currentIngredientSlotUIs.Add(ingredientSlot);
            }
        }
        /// <summary>
        /// 食材Image移动向食材对应位置
        /// </summary>
        private void MakeTweenIngredientImage()
        {
            float delay = 0;
            for (int i = 0; i < currentIngredientSlotUIs.Count; i++)
            {
                currentIngredientSlotUIs[i].GetComponent<CoinTween>().targetPos = currentingredientToGoPosList[i];
                currentIngredientSlotUIs[i].GetComponent<CoinTween>().PlayTweenOfAlpha(delay);
                delay += 0.1f;
            }
        }
        /// <summary>
        /// 关闭烹饪UI
        /// </summary>
        public void QuitCookedUI()
        {
            cookedPanel.SetActive(false);
            InventoryManager.Instance.anyBagOpened = false;
            EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
        }
    }
}


