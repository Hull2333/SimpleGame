using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
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
        }

        private void OnDisable()
        {
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        }

        private void Start()
        {
            menuCanvas = GameObject.FindWithTag("MenuCanvas");
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
 


