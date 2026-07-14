using DG.Tweening;
using MFarm.Save;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.SceneManagement;
namespace MFarm.Transition
{
    public class TransitionManager : Singleton<TransitionManager>,ISaveable  //调用在Transition对象上
    {
        //将场景属性以滚动列表的方式查看和选择
        //[sceneName]
        //设置新游戏开始时玩家所在场景
        public string startSceneName = string.Empty;
        //加载场景的Loading界面
        public CanvasGroup fadeCanvasGroup;
        private bool isFade;
        [Header("转场效果")]
        private Canvas transtionCanvas;
        public Material transtionMaterial;
        private int transtionValue;
        private int smoothValue;
        private Tween currentTween;
        public string GUID => GetComponent<DataGUID>().guid;

        protected override void Awake()
        {
            base.Awake();
            //游戏一开始就加载UI创建,因为游戏打包后，一开始只可以生成一个场景
            SceneManager.LoadScene("UI", LoadSceneMode.Additive);
        }
        private void OnEnable()
        {
            EventHandler.TransitionEvent += OnTransitionEvent;
            //新游戏开始时需要重置的shuju
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
            //游戏结束时的事件
            EventHandler.EndGameEvent += OnEndGameEvent;
           
        }

        private void OnDisable() 
        {
            EventHandler.TransitionEvent -= OnTransitionEvent;
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
            EventHandler.EndGameEvent -= OnEndGameEvent;
           
        }

        

        private void Start()
        {
            ISaveable saveable = this;
            saveable.RegisterSaveable();
            fadeCanvasGroup = FindObjectOfType<CanvasGroup>();
            transtionCanvas = FindObjectOfType<TranstionCanve>().GetComponent<Canvas>();
            transtionCanvas.worldCamera = Camera.main;
            transtionCanvas.sortingLayerName = "ValueTile";
            transtionValue = Shader.PropertyToID("_Value");
            smoothValue = Shader.PropertyToID("_SmoothValue");
            transtionMaterial.SetFloat(transtionValue, 1 + transtionMaterial.GetFloat(smoothValue));
        }
        private void OnTransitionEvent(string sceneToGo, Vector3 positionToGo)
        {
            //避免人物快速的切换场景
            if (!isFade)
            {
                StartCoroutine(Transition(sceneToGo, positionToGo));
            }

        }
        private void OnStartNewGameEvent(int obj)
        {
            StartCoroutine(LoadSaveDataScene(startSceneName));
        }

        private void OnEndGameEvent()
        {
            StartCoroutine(UnloadScene());
        }


        /// <summary>
        /// 场景切换
        /// </summary>
        /// <param name="sceneName">切换的新场景</param>
        /// <param name="targetPosition">传送点</param>
        /// <returns></returns>
        private IEnumerator Transition(string sceneName, Vector3 targetPosition)
        {
            //加载场景之前所需要做的事情
            EventHandler.CallBeforeSceneUnloadEvent();
            //Loading界面的出现
            //yield return Fade(1);
            yield return PlayFadeTranstion();
            //yield return currentTween;
            //获取当前场景并卸载
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            //加载新场景
            yield return LoadSceneSetActive(sceneName);
            //加载新场景后人物的位置
            EventHandler.CallMoveToPosition(targetPosition);
            //加载新场景之后所需要做的事情
            EventHandler.CallAfterSceneLoadedEvent();
            //Loading界面的消失
            //yield return Fade(0);
            yield return PlayOpenTranstion();
            //yield return currentTween;
        }
        /// <summary>
        /// 加载场景并激活，仅仅只有激活功能没有切换场景的功能
        /// </summary>
        /// <param name="sceneName">场景名</param>
        /// <returns></returns>
        private IEnumerator LoadSceneSetActive(string sceneName)
        {
            //LoadSceneAsync，异步加载，LoadSceneMode.Additive在原场景上叠加新场景sceneName
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            //在unity的场景列表中获取需要加载的新场景
            Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            SceneManager.SetActiveScene(newScene);
        }
        
        /// <summary>
        /// Loading界面的浮现和消失
        /// </summary>
        /// <param name="targetAlpha">1是黑，0是透明</param>
        /// <returns></returns>
        private IEnumerator Fade(float targetAlpha)
        {
            //开始出现加载界面使，鼠标无法点击
            isFade = true;
            foreach (Transform child in fadeCanvasGroup.transform)
                child.gameObject.SetActive(true);
            fadeCanvasGroup.blocksRaycasts = true;
            float speed = Mathf.Abs(fadeCanvasGroup.alpha - targetAlpha) / Settings.fadeDuration;

            while (!Mathf.Approximately(fadeCanvasGroup.alpha, targetAlpha))
            {
                fadeCanvasGroup.alpha = Mathf.MoveTowards(fadeCanvasGroup.alpha, targetAlpha, speed * Time.deltaTime);
                yield return null;
            }
            fadeCanvasGroup.blocksRaycasts = false;
            isFade = false;
            foreach (Transform child in fadeCanvasGroup.transform)
                child.gameObject.SetActive(false);
        }
        /// <summary>
        /// 焦点收缩转场
        /// </summary>
        private IEnumerator PlayFadeTranstion()
        {
            isFade = true;
            currentTween = transtionMaterial.DOFloat(0f - transtionMaterial.GetFloat(smoothValue), transtionValue, 0.5f)
         .SetEase(Ease.InQuad).OnComplete(() =>
         {
             transtionMaterial.SetFloat(transtionValue, -0.1f);
         });
            yield return currentTween.WaitForCompletion();
        }
        /// <summary>
        /// 焦点展开转场
        /// </summary>
        private IEnumerator PlayOpenTranstion()
        {
            
            currentTween = transtionMaterial.DOFloat(1f + transtionMaterial.GetFloat(smoothValue), transtionValue, 0.5f)
         .SetEase(Ease.OutQuad).OnComplete(() =>
         {
             transtionMaterial.SetFloat(transtionValue, 1f);
             isFade = false;
         });
           
            yield return currentTween.WaitForCompletion();
        }
        /// <summary>
        /// 加载游戏进度场景
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        private IEnumerator LoadSaveDataScene(string sceneName)
        {
            //给加载存档一个渐入渐出的效果
            yield return Fade(1f);
            //在游戏过程中，加载另外游戏进度
            if(SceneManager.GetActiveScene().name != "OriginalGame")
            {
                EventHandler.CallBeforeSceneUnloadEvent();
                Debug.Log(SceneManager.GetActiveScene().buildIndex);
                //卸载当前场景
                yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            }
            //加载存档里的场景
            yield return LoadSceneSetActive(sceneName);
            EventHandler.CallAfterSceneLoadedEvent();
            yield return Fade(0f);
        }
        /// <summary>
        /// 游戏结束的协程
        /// </summary>
        /// <returns></returns>
        private IEnumerator UnloadScene()
        {
            //呼叫地图卸载前的事件，再把当前的场景卸载掉
            EventHandler.CallBeforeSceneUnloadEvent();
            yield return Fade(1f);
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            yield return Fade(0);
        }

        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.dataSceneName = SceneManager.GetActiveScene().name;

            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            //加载游戏进度场景
            StartCoroutine(LoadSaveDataScene(saveData.dataSceneName));
        }
    }

}
