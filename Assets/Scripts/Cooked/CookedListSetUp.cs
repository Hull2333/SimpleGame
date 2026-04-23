using MFarm.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;
namespace MFarm.Cooked 
{
    public class CookedListSetUp :Singleton<CookedListSetUp>
    {
        private int itemID;
        //菜肴大头照
        public Image cookedSlotIcon;
        //所需食材
        [SerializeField] private Image[] ingredientsItem;
        //烹饪按钮
        public Button cookedMakeButton;
        //关闭按钮
        public Button exitButton;
        public Canvas cookedCanves;
        private GameObject player;
        //cooked移动Image
        public GameObject cookedImage;
        //cookedImage当前是否在移动
        private bool cookedImageActive = false;
        private ItemDetails itemDetails;
        //烹饪菜单按钮
        public List<GameObject> cookedList;
        private void Awake()
        {
            cookedMakeButton.GetComponent<Button>().onClick.AddListener(CookedMake);
            exitButton.onClick.AddListener(ExitCookedUI);
            player = FindObjectOfType<PlayerController>().gameObject;
        }
        private void Update()
        {
            //控制烹饪按钮开关
            if (itemID != 0)
            {
                if(InventoryManager.Instance.CheckIngredients(itemID) && cookedSlotIcon.enabled == true)
                {
                    cookedMakeButton.interactable = true;
                    cookedMakeButton.GetComponentInChildren<Text>().enabled = true;
                }
                
            }
            if (itemID == 0 || !InventoryManager.Instance.CheckIngredients(itemID))
            {
                cookedMakeButton.interactable = false;
                cookedMakeButton.GetComponentInChildren<Text>().enabled = false;
            }
            if (cookedImageActive)
            {
                StartCoroutine(CloseCookedImage());
            }

        }

        private void OnEnable()
        {
            EventHandler.CookedMenuSetupEvent += OnCookedMenuSetupEvent;
        }

      
        private void OnDisable()
        {
            EventHandler.CookedMenuSetupEvent -= OnCookedMenuSetupEvent;
        }
        private void OnCookedMenuSetupEvent(int cookedID)
        {
            Debug.Log("1101");
            GameObject cooked = cookedID switch
            {
                2500 => cookedList[0],
                2501 => cookedList[1],
                _ => null,
            };
            cooked.SetActive(true);
            
        }


        /// <summary>
        /// 根据ItemID获取菜肴的食材信息并生成对应的食材Image
        /// </summary>
        /// <param name="ID"></param>
        public void GetCookedDetail(int ID)
        {
            if(ID != 0)
            {
                itemID = ID;
                itemDetails = InventoryManager.Instance.GetItemDetails(itemID);
                cookedListSetUp();
                var cookedDetails = InventoryManager.Instance.cookedData.GetCookedDetails(itemID);
                for (int i = 0; i < ingredientsItem.Length; i++)
                {
                    if (i < cookedDetails.cookResource.Length)
                    {
                        var resource = cookedDetails.cookResource[i];
                        ingredientsItem[i].gameObject.SetActive(true);
                        ingredientsItem[i].sprite = InventoryManager.Instance.GetItemDetails(resource.itemID).itemIcon;
                        ingredientsItem[i].transform.GetChild(0).GetComponent<Text>().text = resource.itemAmount.ToString();
                    }
                    else
                    {
                        ingredientsItem[i].gameObject.SetActive(false);
                    }
                    //用于点击关闭UI按钮后，食材所需的Panel也要关闭 
                    if (cookedSlotIcon.enabled == false)
                    {
                        ingredientsItem[i].gameObject.SetActive(false);
                    }
                }
            }

        }
        /// <summary>
        /// 启动Cooked大头照
        /// </summary>
        public void cookedListSetUp()
        {
            cookedSlotIcon.enabled = true;
            cookedSlotIcon.sprite = itemDetails.itemIcon;
        }
        /// <summary>
        /// 点击烹饪按钮
        /// </summary>
        public void CookedMake()
        {
            
            //点击烹饪后，关闭playerBoxCollider 0.2s,以防止拾取物品方法多次执行，导致拾取的菜肴数量大于1
            player.GetComponent<BoxCollider2D>().enabled = false;
            cookedImage.gameObject.SetActive(true);
            cookedImage.GetComponent<Image>().sprite = cookedSlotIcon.sprite;
            cookedImageActive = true;
            EventHandler.CallCookedMakeEvent(itemID);
            StartCoroutine(ClosePlayerBoxCollider());
        }
        /// <summary>
        /// 关闭CookedUI,同时关闭所有已点选UI
        /// </summary>
        private void ExitCookedUI()
        {
            //恢复游戏时间
            EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
            cookedSlotIcon.enabled = false;
            GetCookedDetail(itemID);
            cookedMakeButton.interactable = false;
            cookedMakeButton.GetComponentInChildren<Text>().enabled = false;
            PlayerController playerController = player.GetComponent<PlayerController>();
            cookedCanves.enabled = false;
            playerController.isMoving = true;
            playerController.inputDisable = false;
        }
        /// <summary>
        /// Image移动后关闭
        /// </summary>
        /// <returns></returns>
        private IEnumerator CloseCookedImage()
        {
            yield return new WaitForSeconds(0.4f);
            cookedImage.gameObject.SetActive(false);
            cookedImageActive = false;
        }
        /// <summary>
        /// 短暂关闭Player BoxCollider后打开
        /// </summary>
        /// <returns></returns>
        private IEnumerator ClosePlayerBoxCollider()
        {
            yield return new WaitForSeconds(0.2f);
            player.GetComponent<BoxCollider2D>().enabled = true;
        }
    }
}


