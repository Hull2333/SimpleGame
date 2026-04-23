using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using MFarm.Inventory;
using UnityEngine.UI;
using MFarm.Cooked;
using static UnityEditor.Progress;

    public class CookedSlot : MonoBehaviour, IPointerClickHandler //调用在CookListButton按钮上
    {
        public int itemID;
        private ItemDetails itemDetails;
        //private CookedDetails cookedDetails;
        public Text text;
        public Image image;
    private CookedListSetUp cookedListSetUp;
        //public Image cookedSlotImage;
        ////烹饪按钮
        //public GameObject cookedMakeButton;
        ////关闭按钮
        //public Button exitButton;
        //public Canvas cookedCanves;
        ////食材image
        //[SerializeField] private Image[] ingredientsItem;
        //private GameObject player;
        //public GameObject cookedImage;
        //private bool cookedImageActive = false;
        //private void Awake()
        //{
        //    cookedMakeButton.GetComponent<Button>().onClick.AddListener(CookedMake);
        //    exitButton.onClick.AddListener(ExitCookedUI);
        //    player = FindObjectOfType<PlayerController>().gameObject;
        //}
        private void Start()
        {
            if (itemID != 0)
            {
                GetCookedDetails(itemID);
            }
        cookedListSetUp = GameObject.FindObjectOfType<CookedListSetUp>();
        }
        //private void Update()
        //{

        //    //控制烹饪按钮开关
        //    if (itemID != null && InventoryManager.Instance.CheckIngredients(itemID) && cookedSlotImage.enabled == true)
        //    {
        //        cookedMakeButton.gameObject.SetActive(true);
        //    }
        //    if (itemID == null || !InventoryManager.Instance.CheckIngredients(itemID))
        //    {
        //        cookedMakeButton.gameObject.SetActive(false);
        //    }
        //    if (cookedImageActive)
        //    {
        //        StartCoroutine(CloseCookedImage());
        //    }

        //}
        /// <summary>
        /// 获取食谱信息
        /// </summary>
        /// <param name="ID"></param>
        public void GetCookedDetails(int ID)
        {
            itemID = ID;
            itemDetails = InventoryManager.Instance.GetItemDetails(itemID);
            if (itemDetails != null)
            {
                text.text = itemDetails.itemName.ToString();
                image.sprite = itemDetails.itemOnWorldSprite;
            }
        }
        //}
        /// <summary>
        /// 显示食谱所需的食材信息
        /// </summary>
        /// <param name="ID"></param>
        //public void SetupIngredientsSlot(int ID)
        //{
        //    Debug.Log(ingredientsItem.Length);
        //    var cookedDetails = InventoryManager.Instance.cookedData.GetCookedDetails(ID);
        //    for (int i = 0; i < ingredientsItem.Length; i++)
        //    {
        //        if (i < cookedDetails.cookResource.Length)
        //        {
        //            var resource = cookedDetails.cookResource[i];
        //            ingredientsItem[i].gameObject.SetActive(true);
        //            ingredientsItem[i].sprite = InventoryManager.Instance.GetItemDetails(resource.itemID).itemIcon;
        //            ingredientsItem[i].transform.GetChild(0).GetComponent<Text>().text = resource.itemAmount.ToString();
        //        }
        //        else
        //        {
        //            ingredientsItem[i].gameObject.SetActive(false);
        //        }
                //用于点击关闭UI按钮后，食材所需的Panel也要关闭 
                //if (cookedSlotImage.enabled == false)
                //{
                //    ingredientsItem[i].gameObject.SetActive(false);
                //}

        //    }
        //}
        /// <summary>
        /// 点击烹饪按钮后开始烹饪
        /// </summary>
        //    public void CookedMake()
        //    {
        //        //点击烹饪后，关闭playerBoxCollider 0.2s,以防止拾取物品方法多次执行，导致拾取的菜肴数量大于1
        //        player.GetComponent<BoxCollider2D>().enabled = false;
        //        cookedImage.gameObject.SetActive(true);
        //        cookedImageActive = true;
        //        EventHandler.CallCookedMakeEvent(itemID);
        //        StartCoroutine(ClosePlayerBoxCollider());
        //    }

        //    private IEnumerator ClosePlayerBoxCollider()
        //    {
        //        yield return new WaitForSeconds(0.2f);
        //        player.GetComponent<BoxCollider2D>().enabled = true;
        //    }
        //    private IEnumerator CloseCookedImage()
        //    {
        //        yield return new WaitForSeconds(0.4f);
        //        cookedImage.gameObject.SetActive(false);
        //        cookedImageActive = false;
        //    }
        //    /// <summary>
        //    /// 关闭CookedUI,同时关闭所有已点选UI
        //    /// </summary>
        //    private void ExitCookedUI()
        //    {
        //        cookedMakeButton.gameObject.SetActive(false);
        //        cookedSlotImage.enabled = false;
        //        SetupIngredientsSlot(itemID);
        //        PlayerController playerController = player.GetComponent<PlayerController>();
        //        cookedCanves.enabled = false;
        //        playerController.isMoving = true;
        //        playerController.inputDisable = false;

        //    }


        /// <summary>
        /// 点击食谱执行的方法
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
        cookedListSetUp.GetCookedDetail(itemID);
        }
    }
    


