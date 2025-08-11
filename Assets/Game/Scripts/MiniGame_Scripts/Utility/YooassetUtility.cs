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
    public readonly static string PackageName = "MiniGame1";
    public readonly static string hostServerIP = "http://127.0.0.1";//服务器地址
    public readonly static string appVersion = "v1.1"; //版本号
    private ResourcePackage _package = null; //资源包对象

    private MyYooAsset _yoosetAsset = null; //自定义的YooAsset类
    
    public YooassetUtility()
    {
         _yoosetAsset = new MyYooAsset(EPlayMode.HostPlayMode);
        //_yoosetAsset = new MyYooAsset(EPlayMode.EditorSimulateMode);
    }
    
    public async UniTask InitPackage()
    {
        await _yoosetAsset.Initialize();
        _package = _yoosetAsset.GetPackage();
    }

    #region Load

    public async UniTask<List<TextAsset>> LoadConfigsAsync()
    {
        // 不知道是不是设计问题 这个地方得传一个确定文件的路径 不能是父级文件夹的路径
        AllAssetsHandle handle = _package.LoadAllAssetsAsync<TextAsset>("Assets/Game/MiniGame_Res/Config/test_tbfirst");
        await handle;
        List<TextAsset> list = new List<TextAsset>();
        foreach(var assetObj in handle.AllAssetObjects)
        {    
            list.Add(assetObj as TextAsset);
        }    
        return list;
    }

    public T LoadAssetSync<T>(string path) where T : Object
    {
        AssetHandle handle = _package.LoadAssetSync(path);
        return handle.AssetObject as T;
    }
    
    public async UniTask<T> LoadSubAssetAsync<T>(string path,string subName) where T : Object
    {
        SubAssetsHandle  handle = _package.LoadSubAssetsAsync<T>(path);
        await handle;
        return handle.GetSubAssetObject<T>(subName);
    }

    public async UniTask<SceneHandle> LoadSceneAsync(string scenePathName, Action<EErrorCode> onError,
        LoadSceneMode loadSceneMode = LoadSceneMode.Single, Action<string, long, long> onProgress = null)
    {
        if (_package == null) 
            return null;
        
        try
        {
            SceneHandle handle = _package.LoadSceneAsync(scenePathName, loadSceneMode, LocalPhysicsMode.None, false, 100);
            await handle;
            onProgress?.Invoke(scenePathName, 100, 100);
            return handle;
        }
        catch (Exception e)
        {
            throw;
        }


    }
    
    #endregion


    #region Release
    
    // 卸载所有引用计数为零的资源包。
    // 可以在切换场景之后调用资源释放方法或者写定时器间隔时间去释放。
    public async UniTask UnloadUnusedAssets()
    {
        var package = YooAssets.GetPackage(PackageName);
        var operation = package.UnloadUnusedAssetsAsync();
        await operation;
    }

    // 强制卸载所有资源包，该方法请在合适的时机调用。
    // 注意：Package在销毁的时候也会自动调用该方法。
    public async UniTask ForceUnloadAllAssets()
    {
        var package = YooAssets.GetPackage(PackageName);
        var operation = package.UnloadAllAssetsAsync();
        await operation;
    }

    // 尝试卸载指定的资源对象
    // 注意：如果该资源还在被使用，该方法会无效。
    public void TryUnloadUnusedAsset(string assetPath)
    {
        var package = YooAssets.GetPackage(PackageName);
        package.TryUnloadUnusedAsset(assetPath);
    }
    
    #endregion
}
