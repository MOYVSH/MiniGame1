using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using QFramework;
using UnityEngine;
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
    }
    
    public async UniTask InitPackage()
    {
        var playMode = EPlayMode.EditorSimulateMode;

        // 创建资源包裹类
        package = YooAssets.TryGetPackage(PackageName);
        if (package == null)
            package = YooAssets.CreatePackage(PackageName);
        YooAssets.SetDefaultPackage(package);


        switch (playMode)
        {
            // 单机运行模式
            case EPlayMode.EditorSimulateMode:
            {
                var buildResult = EditorSimulateModeHelper.SimulateBuild(PackageName);    
                var packageRoot = buildResult.PackageRootDirectory;
                var editorFileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                var initParameters = new EditorSimulateModeParameters();
                initParameters.EditorFileSystemParameters = editorFileSystemParams;
                var initOperation = package.InitializeAsync(initParameters);
                await initOperation;
                
                var op = package.RequestPackageVersionAsync();
                await op;
                await package.UpdatePackageManifestAsync(op.PackageVersion);
                
                if(initOperation.Status == EOperationStatus.Succeed)
                    Debug.Log("资源包初始化成功！");
                else 
                    Debug.LogError($"资源包初始化失败：{initOperation.Error}");
                break;
            }
            case EPlayMode.OfflinePlayMode:
            {
                var buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                var initParameters = new OfflinePlayModeParameters();
                initParameters.BuildinFileSystemParameters = buildinFileSystemParams;
                var initOperation = package.InitializeAsync(initParameters);
                await initOperation;
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

    public async UniTask<List<UnityEngine.TextAsset>> LoadConfigsAsync()
    {
        // 不知道是不是设计问题 这个地方得传一个确定文件的路径 不能是父级文件夹的路径
        AllAssetsHandle handle = package.LoadAllAssetsAsync<UnityEngine.TextAsset>("Assets/Game/MiniGame_Res/Config/test_tbfirst");
        await handle;
        List<UnityEngine.TextAsset> list = new List<UnityEngine.TextAsset>();
        foreach(var assetObj in handle.AllAssetObjects)
        {    
            list.Add(assetObj as UnityEngine.TextAsset);
        }    
        return list;
    }

    public T LoadAssetSync<T>(string path) where T : Object
    {
        AssetHandle handle = package.LoadAssetSync(path);
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
        
        try
        {
            SceneHandle handle = package.LoadSceneAsync(scenePathName, loadSceneMode, LocalPhysicsMode.None, false, 100);
            await handle;
            onProgress?.Invoke(scenePathName, 100, 100);
            return handle;
        }
        catch (Exception e)
        {
            throw;
        }


    }
}
