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
    private static string PackageName = "MiniGame1";
    private static string hostServerIP = "http://127.0.0.1";//服务器地址
    private static string appVersion = "v1.0"; //版本号
    private ResourcePackage _package = null; //资源包对象

    private MyYooAsset _yoosetAsset = null; //自定义的YooAsset类
    
    public YooassetUtility()
    {
        _yoosetAsset = new MyYooAsset(EPlayMode.HostPlayMode);
    }
    
    public async UniTask InitPackage()
    {
        await _yoosetAsset.Initialize();
        _package = _yoosetAsset.GetPackage();
    }

    public async UniTask<List<UnityEngine.TextAsset>> LoadConfigsAsync()
    {
        // 不知道是不是设计问题 这个地方得传一个确定文件的路径 不能是父级文件夹的路径
        AllAssetsHandle handle = _package.LoadAllAssetsAsync<UnityEngine.TextAsset>("Assets/Game/MiniGame_Res/Config/test_tbfirst");
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
}
