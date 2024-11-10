using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using QFramework;
using UnityEngine.SceneManagement;
using YooAsset;
using Object = UnityEngine.Object;

public class YooassetUtility : IUtility
{
    private ResourcePackage package;

    private static string PackageName = "MiniGame1";
    
    public YooassetUtility()
    {
        YooAssets.Initialize();
        
        InitPackage().Forget();
    }
    
    async UniTask InitPackage()
    {
        var playMode = EPlayMode.EditorSimulateMode;

        // 创建资源包裹类
        package = YooAssets.TryGetPackage(PackageName);
        if (package == null)
            package = YooAssets.CreatePackage(PackageName);

        switch (playMode)
        {
            // 单机运行模式
            case EPlayMode.EditorSimulateMode:
            {
                var initParameters = new EditorSimulateModeParameters();
                var simulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(EDefaultBuildPipeline.BuiltinBuildPipeline, PackageName);
                initParameters.SimulateManifestFilePath  = simulateManifestFilePath;
                await package.InitializeAsync(initParameters);
                break;
            }
            case EPlayMode.OfflinePlayMode:
            {
                var initParameters = new OfflinePlayModeParameters();
                await package.InitializeAsync(initParameters);
                break;
            }
        }
    }
    
    public async UniTask<T> LoadAssetAsync<T>(string path) where T : Object
    {
        AssetHandle handle = package.LoadAssetAsync<T>(path);
        await handle;
        return handle.AssetObject as T;
    }
    
    public async UniTask<T> LoadSubAssetAsync<T>(string path,string subName) where T : Object
    {
        SubAssetsHandle  handle = package.LoadSubAssetsAsync<T>(path);
        await handle;
        return handle.GetSubAssetObject<T>(subName);
    }

    public async UniTask<SceneHandle> LoadSceneAsync(string scenePathName, Action<EErrorCode> onError,
        LoadSceneMode loadSceneMode = LoadSceneMode.Single, Action<string, long, long> onProgress = null)
    {
        if (package == null) 
            return null;
        SceneHandle handle = package.LoadSceneAsync(scenePathName, loadSceneMode, false, 100);
        await handle;
        onProgress?.Invoke(scenePathName, 100, 100);
        return handle;
    }
}
