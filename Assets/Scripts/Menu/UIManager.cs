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
        private void Awake()
        {
            //点击settingBtn按钮执行TogglePausePanel方法
            settingBtn.onClick.AddListener(TogglePausePanel);
            //滑动volumeSlider执行AudioManager脚本下的SetMasterVolume方法
            volumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
        }

        private void OnEnable()
        {
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
            EventHandler.StartNPCEvent += OnStartNPCEvent;
            EventHandler.EndNPCEvent += OnEndNPCEvent;
            EventHandler.RecerveItemEvent += OnRecerveItemEvent;
            EventHandler.QuitRecerveItemUIEvent += OnQuitRecerveItemUIEvent;
        }

        private void OnDisable()
        {
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
            EventHandler.StartNPCEvent -= OnStartNPCEvent;
            EventHandler.EndNPCEvent -= OnEndNPCEvent;
            EventHandler.RecerveItemEvent -= OnRecerveItemEvent;
            EventHandler.QuitRecerveItemUIEvent -= OnQuitRecerveItemUIEvent;
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


    }
}
