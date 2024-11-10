using System;
using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneController : MonoSingleton<GameSceneController>, IController
{
    public static bool isChangeLevel = false;
    public IArchitecture GetArchitecture()
    {
        return MiniGame.Interface;
    }

    public string logTag { get; }
    public void Log(object msg, GameObject context = null) { }
    public void LogWarning(object msg, GameObject context = null) { }
    public void LogError(object msg, GameObject context = null) { }
    
    public async UniTaskVoid FirstLoadMainScene()
    {
        var system = GameArchitecture.Interface.GetSystem<UISystem>();
        // system.OpenPanel<LoadingView>();
        this.SendCommand<InitModelDataCmd>(); // 初始化存档数据
        // system.GetOpenedPanel<LoadingView>().SetProgress(0.3f);
        GenerateObjectPool();
        await LoadCurrentSceneAsync();
    }
    
    private void GenerateObjectPool()
    {
        GameObject obj = new GameObject("ObjectPool");
        DontDestroyOnLoad(obj);
        ObjectPool pool = obj.AddComponent<ObjectPool>();
        pool.freeNode = obj.transform;
    }

    async UniTask LoadCurrentSceneAsync()
    {
        await Task.Delay(TimeSpan.FromSeconds(0.2f));
        //var system = GameArchitecture.Interface.GetSystem<UISystem>();
        // system.GetOpenedPanel<LoadingView>().SetProgress(0.3f);
        // 预加载的一些东西
        // 控制进度条
        await Task.Delay(TimeSpan.FromSeconds(0.2f));
        
        # region 加载场景
        
        var u = this.GetUtility<YooassetUtility>();
        var handle = await u.LoadSceneAsync("GameScene", e =>
        {
            if (e != EErrorCode.None)
                Debug.LogError(e);
        });
        handle.Completed += sceneHandle =>
        {
            isChangeLevel = false;
        };
        #endregion
        
        DoAfterLevelLoad();
        
        // 延迟关闭loading界面
        await DelayCloseLoading();
    }
    
    async UniTask DelayCloseLoading()
    {
        //await Task.Delay(TimeSpan.FromSeconds(1f));
        //var system = GameArchitecture.Interface.GetSystem<UISystem>();
        //system.ClosePanel<LoadingView>();
    }

    private void DoAfterLevelLoad(bool isCurrent = true)
    {
        // 打开Tip类型的UI 
        this.SendCommand<AfterSceneInitLogicCmd>();
        // 打开主界面层级比较低的UI
    }
}
