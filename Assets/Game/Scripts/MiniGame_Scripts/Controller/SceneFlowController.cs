using System;
using System.Collections;
using System.Threading.Tasks;
using MonsterLove.StateMachine;
        using UnityEngine;
        using Cysharp.Threading.Tasks;
        using QFramework;

        public class SceneFlowController : MonoSingleton<SceneFlowController>, IController
        {
            public enum SceneStates
            {
                Idle,           // 空闲状态
                Loading,        // 场景加载中
                Loaded,         // 场景加载完毕
                Running,        // 场景运行时
                Paused,         // 场景暂停
                Transitioning,  // 场景切换中
                Error           // 错误状态
            }
        
            public class Driver
            {
                public StateEvent Update;
                public StateEvent OnGUI;
                public StateEvent<string> OnSceneChanged;
                public StateEvent<float> OnLoadProgress;
                public StateEvent OnPause;
                public StateEvent OnResume;
            }
        
            [SerializeField] private string targetSceneName = "GameScene";
            [SerializeField] private float loadingProgress = 0f;
            [SerializeField] private bool autoStartLoading = true;
        
            private StateMachine<SceneStates, Driver> fsm;
            private UISystem uiSystem;
        
            public IArchitecture GetArchitecture()
            {
                return GameArchitecture.Interface;
            }
        
            // 实现 QFramework 接口成员
            public string logTag => "SceneFlowController";
            public void Log(object msg, GameObject context = null) => Debug.Log($"[{logTag}] {msg}", context);
            public void LogWarning(object msg, GameObject context = null) => Debug.LogWarning($"[{logTag}] {msg}", context);
            public void LogError(object msg, GameObject context = null) => Debug.LogError($"[{logTag}] {msg}", context);
        
            private void Awake()
            {
                fsm = new StateMachine<SceneStates, Driver>(this);
                uiSystem = this.GetSystem<UISystem>();
                fsm.ChangeState(SceneStates.Idle);
            }
        
            private void Update()
            {
                fsm.Driver.Update.Invoke();
            }
        
            private void OnGUI()
            {
                fsm.Driver.OnGUI.Invoke();
            }
        
            #region 状态机方法
        
            void Idle_Enter()
            {
                Debug.Log("场景控制器进入空闲状态");
                loadingProgress = 0f;
            }
        
            void Idle_OnGUI()
            {
                GUI.Label(new Rect(20, 20, 200, 30), "状态: 空闲");
        
                if (GUI.Button(new Rect(20, 60, 120, 30), "开始加载场景"))
                {
                    StartLoading(targetSceneName);
                }
            }
        
            void Loading_Enter()
            {
                Debug.Log("开始加载场景...");
                uiSystem?.OpenPanel<LoadingView>();
                StartCoroutine(LoadSceneCoroutine());
            }
        
            void Loading_Update()
            {
                fsm.Driver.OnLoadProgress.Invoke(loadingProgress);
                uiSystem?.GetOpenedPanel<LoadingView>()?.SetProgress(loadingProgress);
            }
        
            void Loading_OnGUI()
            {
                GUI.Label(new Rect(20, 20, 200, 30), "状态: 加载中");
                GUI.Label(new Rect(20, 50, 200, 30), $"进度: {loadingProgress * 100:F1}%");
                GUI.Box(new Rect(20, 80, 200, 20), "");
                GUI.Box(new Rect(20, 80, 200 * loadingProgress, 20), "");
            }
        
            void Loading_OnLoadProgress(float progress)
            {
                loadingProgress = progress;
            }
        
            void Loaded_Enter()
            {
                Debug.Log("场景加载完成");
                uiSystem?.GetOpenedPanel<LoadingView>()?.Complete();
                StartCoroutine(DelayedStart());
            }
        
            void Loaded_OnGUI()
            {
                GUI.Label(new Rect(20, 20, 200, 30), "状态: 加载完毕");
        
                if (GUI.Button(new Rect(20, 60, 120, 30), "开始运行"))
                {
                    fsm.ChangeState(SceneStates.Running);
                }
            }
        
            void Running_Enter()
            {
                Debug.Log("场景开始运行");
                uiSystem?.ClosePanel<LoadingView>();
                this.SendCommand<AfterSceneInitLogicCmd>();
            }
        
            void Running_Update()
            {
                // 场景运行时的更新逻辑
            }
        
            void Running_OnGUI()
            {
                GUI.Label(new Rect(20, 20, 200, 30), "状态: 运行中");
        
                if (GUI.Button(new Rect(20, 60, 80, 30), "暂停"))
                {
                    fsm.ChangeState(SceneStates.Paused);
                }
        
                if (GUI.Button(new Rect(110, 60, 80, 30), "切换场景"))
                {
                    fsm.ChangeState(SceneStates.Transitioning);
                }
            }
        
            void Running_OnPause()
            {
                fsm.ChangeState(SceneStates.Paused);
            }
        
            void Paused_Enter()
            {
                Debug.Log("场景暂停");
                Time.timeScale = 0f;
            }
        
            void Paused_OnGUI()
            {
                GUI.Label(new Rect(20, 20, 200, 30), "状态: 暂停");
        
                if (GUI.Button(new Rect(20, 60, 80, 30), "继续"))
                {
                    fsm.ChangeState(SceneStates.Running);
                }
            }
        
            void Paused_Exit()
            {
                Time.timeScale = 1f;
            }
        
            void Paused_OnResume()
            {
                fsm.ChangeState(SceneStates.Running);
            }
        
            void Transitioning_Enter()
            {
                Debug.Log("开始场景切换");
                StartCoroutine(TransitionToNewScene());
            }
        
            void Transitioning_OnGUI()
            {
                GUI.Label(new Rect(20, 20, 200, 30), "状态: 切换中");
            }
        
            void Error_Enter()
            {
                Debug.LogError("场景流程出现错误");
            }
        
            void Error_OnGUI()
            {
                GUI.Label(new Rect(20, 20, 200, 30), "状态: 错误");
        
                if (GUI.Button(new Rect(20, 60, 80, 30), "重试"))
                {
                    fsm.ChangeState(SceneStates.Idle);
                }
            }
        
            #endregion
        
            #region 协程方法
        
            private IEnumerator LoadSceneCoroutine()
            {
                // 模拟加载过程，不在try/catch中使用yield return
                bool hasError = false;
                
                // 模拟加载进度
                for (float i = 0; i <= 1f; i += 0.1f)
                {
                    loadingProgress = i;
                    yield return new WaitForSeconds(0.2f);
                }
        
                // 使用UniTask加载场景（不在yield return中）
                LoadSceneAsync().Forget();
            }
        
            private async UniTaskVoid LoadSceneAsync()
            {
                try
                {
                    var utility = this.GetUtility<YooassetUtility>();
                    var handle = await utility.LoadSceneAsync(targetSceneName, e =>
                    {
                        if (e != EErrorCode.None)
                        {
                            Debug.LogError($"场景加载错误: {e}");
                            fsm.ChangeState(SceneStates.Error);
                        }
                    });
        
                    loadingProgress = 1f;
                    fsm.ChangeState(SceneStates.Loaded);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"加载场景时发生异常: {ex.Message}");
                    fsm.ChangeState(SceneStates.Error);
                }
            }
        
            private IEnumerator DelayedStart()
            {
                yield return new WaitForSeconds(1f);
                fsm.ChangeState(SceneStates.Running);
            }
        
            private IEnumerator TransitionToNewScene()
            {
                yield return new WaitForSeconds(0.5f);
                targetSceneName = "NewScene";
                fsm.ChangeState(SceneStates.Loading);
            }
        
            #endregion
        
            #region 公共方法

            public async UniTaskVoid FirstLoadMainScene()
            {
                await Task.Delay(TimeSpan.FromSeconds(0.2f));
                var system = GameArchitecture.Interface.GetSystem<UISystem>();
                system.OpenPanel<LoadingView>();
                this.SendCommand<InitModelDataCmd>(); // 初始化存档数据
                system.GetOpenedPanel<LoadingView>().SetProgress(0.3f);
                GenerateObjectPool();
                StartLoading("GameScene");
            }

            private void GenerateObjectPool()
            {
                GameObject obj = new GameObject("ObjectPool");
                DontDestroyOnLoad(obj);
                ObjectPool pool = obj.AddComponent<ObjectPool>();
                pool.freeNode = obj.transform;
            }
            
            public void StartLoading(string sceneName)
            {
                targetSceneName = sceneName;
                fsm.ChangeState(SceneStates.Loading);
            }
        
            public void PauseScene()
            {
                if (fsm.State == SceneStates.Running)
                {
                    fsm.Driver.OnPause.Invoke();
                }
            }
        
            public void ResumeScene()
            {
                if (fsm.State == SceneStates.Paused)
                {
                    fsm.Driver.OnResume.Invoke();
                }
            }
        
            public void TransitionToScene(string newSceneName)
            {
                targetSceneName = newSceneName;
                fsm.ChangeState(SceneStates.Transitioning);
            }
        
            public SceneStates CurrentState => fsm.State;
            public bool IsInTransition => fsm.IsInTransition;
        
            #endregion
        }