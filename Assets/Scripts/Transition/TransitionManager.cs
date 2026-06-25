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
        public Canvas transtionCanvas;
        public Material transtionMaterial;
        public int transtionValue;
        public int smoothValue;
        private Tween currentTween;
        public string GUID => GetComponent<DataGUID>().guid;
        [Header("建筑场景相关")]
        //新场景计数
        private int currentBuildingCount;
        public string currentSceneName;
        public int currentBuildCode;
        //建筑场景物品列表
        private List<SceneRootObect> buildSceneList = new List<SceneRootObect>();

        protected override void Awake()
        {
            base.Awake();
            //游戏一开始就加载UI创建,因为游戏打包后，一开始只可以生成一个场景
            SceneManager.LoadScene("UI", LoadSceneMode.Additive);
        }
        private void OnEnable()
        {
            EventHandler.TransitionEvent += OnTransitionEvent;
            EventHandler.TranstionBuildSceneEvent += OnTranstionBuildSceneEvent;
            //新游戏开始时需要重置的shuju
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
            //游戏结束时的事件
            EventHandler.EndGameEvent += OnEndGameEvent;
           
        }

        private void OnDisable() 
        {
            EventHandler.TransitionEvent -= OnTransitionEvent;
            EventHandler.TranstionBuildSceneEvent -= OnTranstionBuildSceneEvent;
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
        private void OnTranstionBuildSceneEvent(Vector3 vector,string tempSceneName,int code,bool isCome)
        {
            if (!isFade)
            {
                StartCoroutine(TranstionBuildingScene(vector, tempSceneName, code, isCome));
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
        private IEnumerator TranstionBuildingScene(Vector3 targetPosition,string sceneName,int code, bool isCome)
        {
            //加载场景之前所需要做的事情
            EventHandler.CallBeforeSceneUnloadEvent();
            yield return PlayFadeTranstion();
            //进场景
            if (isCome)
            {
                yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
                yield return StartCoroutine(CreateScene(sceneName, code));
            }
            //出场景
            else
            {
                yield return StartCoroutine(GetAndSaveBuildSceneObject());
                yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
                //加载新场景
                yield return LoadSceneSetActive(sceneName);
            }
            //加载新场景后人物的位置
            EventHandler.CallMoveToPosition(targetPosition);
            //加载新场景之后所需要做的事情
            EventHandler.CallAfterSceneLoadedEvent();
            yield return PlayOpenTranstion();
        }
        /// <summary>
        /// 创建建筑场景
        /// </summary>
        /// <param name="templateSceneName"></param>
        /// <returns></returns>
        private IEnumerator CreateScene(string templateSceneName,int code)
        {
            currentBuildingCount++;
            currentSceneName = $"{templateSceneName}{currentBuildingCount}";
            Debug.Log(templateSceneName);
            //加载模板场景
            AsyncOperation loadTemplateScene = SceneManager.LoadSceneAsync(templateSceneName, LoadSceneMode.Additive);
            while (!loadTemplateScene.isDone)
            {
                yield return null;
            }
            //获取模板场景
            Scene templateScene = SceneManager.GetSceneByName(templateSceneName);
            if (!templateScene.isLoaded)
            {
                Debug.LogError($"模板场景 {templateSceneName} 加载失败");
                yield break;
            }
            GameObject[] sceneRootObjects;
            //获取模板场景中的所有根(父)物体
            sceneRootObjects = templateScene.GetRootGameObjects();
            foreach (var scene in buildSceneList)
            {
                if (scene.buildCode == code)
                {
                    sceneRootObjects = scene.objects;
                }
            }
            currentBuildCode = code;
            //场景但不激活新场景
            Scene newScene = SceneManager.CreateScene(currentSceneName);
            if (sceneRootObjects == null || sceneRootObjects.Length == 0 || sceneRootObjects[0] == null)
            {
                //buildSceneList中保存的引用已失效，回退到模板场景物体
                sceneRootObjects = templateScene.GetRootGameObjects();
            }
            foreach (var obj in sceneRootObjects)
            {
                // 克隆物体（包括所有子物体）
                GameObject clonedObj = Instantiate(obj, obj.transform.position, obj.transform.rotation);
                // 移动到新场景
                SceneManager.MoveGameObjectToScene(clonedObj, newScene);
            }
            // 7. 卸载模板场景
            yield return SceneManager.UnloadSceneAsync(templateSceneName);
            yield return SceneManager.SetActiveScene(newScene);
        }
        /// <summary>
        /// 获取当前建筑场景并保存
        /// </summary>
        /// <returns></returns>
        private IEnumerator GetAndSaveBuildSceneObject()
        {
            //获取当前场景的根物体并添加到列表中
            GameObject[] currentSceneRootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            var currentSceneOjects = new SceneRootObect { objects = currentSceneRootObjects, buildCode = currentBuildCode };
            var exitSceneObjects = buildSceneList.Find(s => s.buildCode == currentSceneOjects.buildCode);
            if (exitSceneObjects != null)
            {
                exitSceneObjects.objects = currentSceneOjects.objects;
            }
            else
            {
                buildSceneList.Add(currentSceneOjects);
            }
            yield return null;
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
            fadeCanvasGroup.blocksRaycasts = true;
            float speed = Mathf.Abs(fadeCanvasGroup.alpha - targetAlpha) / Settings.fadeDuration;

            while (!Mathf.Approximately(fadeCanvasGroup.alpha, targetAlpha))
            {
                fadeCanvasGroup.alpha = Mathf.MoveTowards(fadeCanvasGroup.alpha, targetAlpha, speed * Time.deltaTime);
                yield return null;
            }
            fadeCanvasGroup.blocksRaycasts = false;
            isFade = false;
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
