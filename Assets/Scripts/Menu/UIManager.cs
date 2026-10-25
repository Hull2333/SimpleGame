using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using DG.Tweening;
namespace MFarm.Inventory
{
    public class UIManager : Singleton<UIManager>  //调用在MainCanvas对象上
    {
        private GameObject menuCanvas;

        public GameObject menuPrefab;

        //获取右上角设置按钮
        public Button settingBtn;
        //暂停菜单
        public GameObject pausePanel;
        //音量控制Slider
        public Slider volumeSlider;
        public Canvas blackBoardCanvas;
        //获取重要道具UI
        public GameObject recerveUIPanel;
        public TextMeshProUGUI recerveUIText;
        private Coroutine bounceCoroutine;

        //睡觉提示UI
        public GameObject sleepPanel;
        public Button sleepYesButton;
        public Button sleepNoButton;
        [Header("结算界面UI")]
        //结算物品UI
        public GameObject SetleUI;
        public Transform SetleSlotHolder;
        //结算物品格
        public GameObject SetleSlotPrefab;
        public TextMeshProUGUI SetleAllValue;
        //结算物品详情
        public GameObject SetleDesUI;
        public TextMeshProUGUI setleItemName;
        public TextMeshProUGUI setleItemValue;
        private void Awake()
        {
            //点击settingBtn按钮执行TogglePausePanel方法
            settingBtn.onClick.AddListener(TogglePausePanel);
            //滑动volumeSlider执行AudioManager脚本下的SetMasterVolume方法
            volumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
            //点击睡觉面板的按钮时执行对应方法
            sleepYesButton.onClick.AddListener(SleepYesButtonClick);
            sleepNoButton.onClick.AddListener(SleepNoButtonClick);
            // 结算界面只在睡觉后显示，开局先隐藏
            // 结算物品详情开局先隐藏
            if (SetleDesUI != null)
            {
                SetleDesUI.SetActive(false);
            }
            SetleUI.SetActive(false);
        }

        private void OnEnable()
        {
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
            EventHandler.StartNPCEvent += OnStartNPCEvent;
            EventHandler.EndNPCEvent += OnEndNPCEvent;
            EventHandler.RecerveItemEvent += OnRecerveItemEvent;
            EventHandler.QuitRecerveItemUIEvent += OnQuitRecerveItemUIEvent;
            EventHandler.OpenSleepPanelEvent += OnOpenSleepPanelEvent;
            EventHandler.ShowSettleUIEvent += ShowSettleUI;
            EventHandler.HideSettleUIEvent += HideSettleUI;
        }

        private void OnDisable()
        {
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
            EventHandler.StartNPCEvent -= OnStartNPCEvent;
            EventHandler.EndNPCEvent -= OnEndNPCEvent;
            EventHandler.RecerveItemEvent -= OnRecerveItemEvent;
            EventHandler.QuitRecerveItemUIEvent -= OnQuitRecerveItemUIEvent;
            EventHandler.OpenSleepPanelEvent -= OnOpenSleepPanelEvent;
            EventHandler.ShowSettleUIEvent -= ShowSettleUI;
            EventHandler.HideSettleUIEvent -= HideSettleUI;
        }



        private void Start()
        {
            menuCanvas = GameObject.FindWithTag("MenuCanvas");
            GetComponent<Canvas>().worldCamera = FindAnyObjectByType<Camera>();
            GetComponent<Canvas>().sortingLayerName = "Collision";
            Instantiate(menuPrefab, menuCanvas.transform);
        }
        private void OnAfterSceneLoadedEvent()
        {
            //如果menuCanvas下面有子物体就删除
            if (menuCanvas.transform.childCount > 0)
            {
                Destroy(menuCanvas.transform.GetChild(0).gameObject);
            }
        }
        private void OnStartNPCEvent()
        {
            EventHandler.CallUpdateGameStateEvent(GameState.Pause);
            GetComponent<Canvas>().enabled = false;
            blackBoardCanvas.gameObject.SetActive(true);
        }
        private void OnEndNPCEvent()
        {
            EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
            GetComponent<Canvas>().enabled = true;
            blackBoardCanvas.gameObject.SetActive(false);
        }
        private void OnRecerveItemEvent(ItemDetails item)
        {
            recerveUIPanel.gameObject.SetActive(true);
            StartCoroutine(DelayedSetPanelPosition());
            StartCoroutine(BounceLoopDoTween(item));
            bounceCoroutine = StartCoroutine(BounceLoopDoTween(item));
        }
        private void OnQuitRecerveItemUIEvent()
        {
            recerveUIPanel.gameObject.SetActive(false);
            StopCoroutine(bounceCoroutine);
        }
        /// <summary>
        /// 定位Player
        /// </summary>
        private void SetPanelAtPlayerPosition()
        {
            var player = FindObjectOfType<PlayerController>();
            if (player != null && Camera.main != null)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(player.transform.position);
                screenPos.y += 50f;
                recerveUIPanel.GetComponent<RectTransform>().position = screenPos;
            }
        }
        /// <summary>
        /// 文字跳动协程
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private IEnumerator BounceLoopDoTween(ItemDetails item)
        {
            string prefix = "获得";
            string suffix = "!";
            recerveUIText.text = $"{prefix}<color=red>{item.itemName}</color>{suffix}";
            recerveUIText.ForceMeshUpdate();

            TMP_TextInfo textInfo = recerveUIText.textInfo;
            int charCount = textInfo.characterCount;

            // 缓存原始顶点位置
            Vector3[][] originalVerts = new Vector3[textInfo.meshInfo.Length][];
            for (int m = 0; m < textInfo.meshInfo.Length; m++)
                originalVerts[m] = (Vector3[])textInfo.meshInfo[m].vertices.Clone();

            // 恢复所有顶点到原始位置
            void RestoreAll()
            {
                for (int m = 0; m < textInfo.meshInfo.Length; m++)
                    System.Array.Copy(originalVerts[m], textInfo.meshInfo[m].vertices, originalVerts[m].Length);
            }

            while (true)
            {
                for (int i = 0; i < charCount; i++)
                {
                    if (!textInfo.characterInfo[i].isVisible) continue;

                    int matIdx = textInfo.characterInfo[i].materialReferenceIndex;
                    int vertIdx = textInfo.characterInfo[i].vertexIndex;
                    Vector3[] verts = textInfo.meshInfo[matIdx].vertices;

                    Tween tween = DOTween.To(
                        () => 0f,
                        (y) =>
                        {
                            RestoreAll();
                            for (int v = 0; v < 4; v++)
                                verts[vertIdx + v] = originalVerts[matIdx][vertIdx + v] + Vector3.up * y;
                            recerveUIText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
                        },
                        10f, 0.1f
                    ).SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo);

                    yield return tween.WaitForCompletion();
                }
                yield return new WaitForSeconds(0.3f);
            }
        }
        /// <summary>
        /// 等相机更新完这一帧
        /// </summary>
        /// <returns></returns>
        private IEnumerator DelayedSetPanelPosition()
        {
            yield return new WaitForEndOfFrame(); 
            SetPanelAtPlayerPosition();
        }
        /// <summary>
        /// 切出暂停菜单
        /// </summary>
        private void TogglePausePanel()
        {
            //判断当前是否打开了暂停菜单
            bool isOpen = pausePanel.activeInHierarchy;
            //当暂停菜单打开时，点击关闭暂停菜单
            if (isOpen)
            {
                pausePanel.SetActive(false);
                Time.timeScale = 1.0f;
            }
            else
            {
                //垃圾回收，清理内存
                System.GC.Collect();
                pausePanel.SetActive(true);
                //游戏时间暂停
                Time.timeScale = 0f;
            }
        }
        /// <summary>
        /// 从暂停菜单回到主菜单
        /// </summary>
        public void ReturnMenuCanvas()
        {
            //在返回主菜单前要把时间暂停取消，否则会使后面进入游戏无法运行
            Time.timeScale = 1.0f;
            StartCoroutine(BackToMenu());
        }
        /// <summary>
        /// 从暂停菜单返回到游戏中
        /// </summary>
        public void ReturnGame()
        {
            Time.timeScale = 1.0f;
            pausePanel.gameObject.SetActive(false);

        }
        /// <summary>
        /// 退出到主菜单
        /// </summary>
        /// <returns></returns>
        private IEnumerator BackToMenu()
        {
            pausePanel.SetActive(false);
            //结束游戏的事件
            EventHandler.CallEndGameEvent();
            yield return new WaitForSeconds(1);
            Instantiate(menuPrefab, menuCanvas.transform);
        }
        /// <summary>
        /// 玩家触碰睡觉区域时打开睡觉面板，并暂停游戏
        /// </summary>
        private void OnOpenSleepPanelEvent()
        {
            sleepPanel.SetActive(true);
            //打开面板期间暂停游戏时间和玩家操作
            Time.timeScale = 0f;
            EventHandler.CallUpdateGameStateEvent(GameState.Pause);
        }
        /// <summary>
        /// 点击睡觉面板的“是”：关闭面板并恢复游戏状态，然后通知PlayerController开始睡觉流程
        /// </summary>
        private void SleepYesButtonClick()
        {
            //关闭面板，恢复timeScale与游戏状态，睡觉流程交给PlayerController执行
            CloseSleepPanel();
            EventHandler.CallSleepEvent();
        }
        /// <summary>
        /// 点击睡觉面板的“否”：不进入第二天
        /// </summary>
        private void SleepNoButtonClick()
        {
            CloseSleepPanel();
        }
        /// <summary>
        /// 关闭睡觉面板并恢复游戏
        /// </summary>
        private void CloseSleepPanel()
        {
            sleepPanel.SetActive(false);
            Time.timeScale = 1f;
            EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
        }

        /// <summary>
        /// 显示卖车结算界面：按卖车格子里的物品种类生成结算格
        /// </summary>
        private void ShowSettleUI()
        {
            // 按物品ID汇总数量与总价值，同种物品只生成一个结算格
            Dictionary<int, int[]> settleData = new Dictionary<int, int[]>();
            foreach (InventoryItem item in InventoryManager.Instance.sellCarItemList)
            {
                if (item.itemID == 0 || item.itemAmount <= 0) continue;
                ItemDetails details = InventoryManager.Instance.GetItemDetails(item.itemID);
                int value = (int)(item.itemAmount * details.itemPrice * details.sellPercentage);
                if (settleData.ContainsKey(item.itemID))
                {
                    settleData[item.itemID][0] += item.itemAmount;
                    settleData[item.itemID][1] += value;
                }
                else
                {
                    settleData[item.itemID] = new int[] { item.itemAmount, value };
                }
            }
            // 汇总卖车里所有物品的总价值
            int allValue = 0;
            foreach (int[] data in settleData.Values)
            {
                allValue += data[1];
            }
            // 总价值从0滚动到实际金额（结算界面处于暂停状态，需用不受时间缩放影响的更新）
            int shownValue = 0;
            DOTween.To(
                () => shownValue,
                x =>
                {
                    shownValue = x;
                    SetleAllValue.text = x.ToString();
                },
                allValue,
                2.5f
            ).SetUpdate(true);
            // 结算界面要盖在眼睛幕布(ValueTile层)之上，所以给它单独挂一个Canvas
            if (SetleUI.GetComponent<Canvas>() == null)
            {
                Canvas settleCanvas = SetleUI.AddComponent<Canvas>();
                settleCanvas.overrideSorting = true;
                settleCanvas.sortingLayerName = "ValueTile";
                settleCanvas.sortingOrder = 2;
                // 嵌套Canvas需要自己的射线检测组件，否则结算格收不到鼠标事件
                if (SetleUI.GetComponent<GraphicRaycaster>() == null)
                {
                    SetleUI.AddComponent<GraphicRaycaster>();
                }
            }
            SetleUI.SetActive(true);
            // 复位动画状态，避免上一次的关闭动画残留到本次显示
            Animator settleAnim = SetleUI.GetComponent<Animator>();
            if (settleAnim != null)
            {
                settleAnim.Rebind();
                settleAnim.Update(0f);
            }
            // 界面激活后逐个生成结算格
            StartCoroutine(GenerateSettleSlots(settleData));
        }

        /// <summary>
        /// 每隔 0.1s 生成一个结算格，全部生成完后通知睡觉流程继续
        /// </summary>
        private IEnumerator GenerateSettleSlots(Dictionary<int, int[]> settleData)
        {
            foreach (KeyValuePair<int, int[]> data in settleData)
            {
                GameObject slot = Instantiate(SetleSlotPrefab, SetleSlotHolder);
                SetleSlotUI slotUI = slot.GetComponent<SetleSlotUI>();
                ItemDetails details = InventoryManager.Instance.GetItemDetails(data.Key);
                slotUI.setleItemImage.sprite = details.itemIcon;
                slotUI.itemDetails = details;
                slotUI.SetleAmount.text = data.Value[0].ToString();
                slotUI.SetleAllValue.text = data.Value[1].ToString();
                // 每生成一个等待 0.1s，让结算格逐个出现
                yield return new WaitForSeconds(0.1f);
            }
            // 通知睡觉流程：结算信息已全部显示
            EventHandler.CallSettleUIGenerateDoneEvent();
        }


        /// <summary>
        /// 关闭卖车结算界面
        /// </summary>
        private void HideSettleUI()
        {
            StartCoroutine(HideSettleUIRoutine());
        }

        /// <summary>
        /// 播放关闭动画，动画结束后隐藏界面并清空结算格
        /// </summary>
        private IEnumerator HideSettleUIRoutine()
        {
            Animator settleAnim = SetleUI.GetComponent<Animator>();
            if (settleAnim != null)
            {
                settleAnim.SetTrigger("SetlePanelClose");
                yield return null;   // 等一帧，让动画状态切换生效
                yield return new WaitForSeconds(settleAnim.GetCurrentAnimatorStateInfo(0).length);
            }
            SetleUI.SetActive(false);
            // 关闭结算界面时一并隐藏物品详情
            if (SetleDesUI != null)
            {
                SetleDesUI.SetActive(false);
            }
            // 清掉本次显示的结算格，下次显示时重新生成
            for (int i = SetleSlotHolder.childCount - 1; i >= 0; i--)
            {
                Destroy(SetleSlotHolder.GetChild(i).gameObject);
            }
            // 通知睡觉流程：界面已关闭
            EventHandler.CallSettleUIHideDoneEvent();
        }
    }
}
