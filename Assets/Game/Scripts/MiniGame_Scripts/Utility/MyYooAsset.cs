using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

/// <summary>
/// YooAsset资源系统测试脚本
/// </summary>
public class MyYooAsset
{
    string hostServerIP = "http://127.0.0.1";//服务器地址
    string appVersion = "v1.0"; //版本号

    public EPlayMode PlayMode = EPlayMode.HostPlayMode;//资源系统运行模式
    public string packageName = "MiniGame1"; //默认包名
    private ResourcePackage _package = null; //资源包对象

    public int downloadingMaxNum = 10;//最大下载数量
    public int filedTryAgain = 3;//失败重试次数
    private ResourceDownloaderOperation _downloader;//下载器
    private UpdatePackageManifestOperation _operationManifest;//更新清单

    public MyYooAsset(EPlayMode playMode)
    {
        // 设置资源系统运行模式
        PlayMode = playMode;
        Debug.LogError($"Set YooAsset PlayMode: {PlayMode}");
        
        // 这里可以添加一些初始化代码
        Debug.Log("MyYooAsset 初始化");
    }
    
    public async UniTask Initialize()
    {
        Debug.LogError($"资源系统运行模式：{PlayMode}");

        //1.初始化YooAsset资源系统
        YooAssets.Initialize();

        //2.初始化资源包
        await InitPackage();

        //3.获取资源版本
        await UpdatePackageVersion();

        //4.获取文件清单
        await UpdateManifest();

        //5.创建资源下载器
        CreateDownloader();

        //5.开始下载资源文件
        await BeginDownload();

        //6.清理未使用的缓存文件
        ClearFiles();
    }


    #region 初始化资源包
    private async UniTask InitPackage()
    {
        // 获取或创建资源包对象
        _package = YooAssets.TryGetPackage(packageName);
        if (_package == null)
            _package = YooAssets.CreatePackage(packageName);
        
        YooAssets.SetDefaultPackage(_package);

        // 编辑器下的模拟模式
        InitializationOperation initializationOperation = null;
        if (PlayMode == EPlayMode.EditorSimulateMode)
        {
            var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
            var packageRoot = buildResult.PackageRootDirectory;
            var createParameters = new EditorSimulateModeParameters();
            createParameters.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            initializationOperation = _package.InitializeAsync(createParameters);
        }

        // 单机运行模式
        if (PlayMode == EPlayMode.OfflinePlayMode)
        {
            var createParameters = new OfflinePlayModeParameters();
            createParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            initializationOperation = _package.InitializeAsync(createParameters);
        }

        // 联机运行模式
        if (PlayMode == EPlayMode.HostPlayMode)
        {
            //创建远端服务实例，用于资源请求
            string defaultHostServer = GetHostServerURL();
            string fallbackHostServer = GetHostServerURL();
            IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            
            // 创建联机模式参数，并设置内置及缓存文件系统参数
            HostPlayModeParameters createParameters = new HostPlayModeParameters
            {
                //创建内置文件系统参数
                BuildinFileSystemParameters = null,
                //创建缓存系统参数
                CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices)
            };
            //执行异步初始化
            initializationOperation = _package.InitializeAsync(createParameters);
        }

        // WebGL运行模式
        if (PlayMode == EPlayMode.WebPlayMode)
        {
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
            var createParameters = new WebPlayModeParameters();
			string defaultHostServer = GetHostServerURL();
            string fallbackHostServer = GetHostServerURL();
            string packageRoot = $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE"; //注意：如果有子目录，请修改此处！
            IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            createParameters.WebServerFileSystemParameters = WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices);
            initializationOperation = _package.InitializeAsync(createParameters);
#else
            var createParameters = new WebPlayModeParameters();
            createParameters.WebServerFileSystemParameters = FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
            initializationOperation = _package.InitializeAsync(createParameters);
#endif
        }

        await initializationOperation;

        // 如果初始化失败弹出提示界面
        if (initializationOperation.Status != EOperationStatus.Succeed)
        {
            Debug.LogWarning($"{initializationOperation.Error}");
        }
        else
        {
            Debug.Log("初始化成功-------------------------");
        }
    }

    /// <summary>
    /// 获取资源服务器地址
    /// </summary>
    private string GetHostServerURL()
    {

#if UNITY_EDITOR
        if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
            return $"{hostServerIP}/CDN/Android/{appVersion}";
        else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
            return $"{hostServerIP}/CDN/IPhone/{appVersion}";
        else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
            return $"{hostServerIP}/CDN/WebGL/{appVersion}";
        else
            return $"{hostServerIP}/CDN/PC/{appVersion}";
#else
        if (Application.platform == RuntimePlatform.Android)
            return $"{hostServerIP}/CDN/Android/{appVersion}";
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
            return $"{hostServerIP}/CDN/IPhone/{appVersion}";
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
            return $"{hostServerIP}/CDN/WebGL/{appVersion}";
        else
            return $"{hostServerIP}/CDN/PC/{appVersion}";
#endif
    }

    /// <summary>
    /// 远端资源地址查询服务类
    /// </summary>
    private class RemoteServices : IRemoteServices
    {
        private readonly string _defaultHostServer;
        private readonly string _fallbackHostServer;

        public RemoteServices(string defaultHostServer, string fallbackHostServer)
        {
            _defaultHostServer = defaultHostServer;
            _fallbackHostServer = fallbackHostServer;
        }
        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return $"{_defaultHostServer}/{fileName}";
        }
        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return $"{_fallbackHostServer}/{fileName}";
        }
    }
    #endregion

    # region 获取package
    public ResourcePackage GetPackage()
    {
        if (_package == null)
        {
            Debug.LogError("资源包未初始化，请先调用Initialize方法");
            return null;
        }
        return _package;
    }
    
    # endregion
    
    #region 获取资源版本
    private async UniTask UpdatePackageVersion()
    {
        // 发起异步版本请求
        RequestPackageVersionOperation operation = _package.RequestPackageVersionAsync();
        await operation;

        // 处理版本请求结果
        if (operation.Status != EOperationStatus.Succeed)
        {
            Debug.LogWarning(operation.Error);
        }
        else
        {
            Debug.Log($"请求的版本: {operation.PackageVersion}");
            appVersion = operation.PackageVersion;
        }
    }
    #endregion

    #region 获取文件清单
    private async UniTask UpdateManifest()
    {
        _operationManifest = _package.UpdatePackageManifestAsync(appVersion);
        await _operationManifest;

        // 处理文件清单结果
        if (_operationManifest.Status != EOperationStatus.Succeed)
        {
            Debug.LogWarning(_operationManifest.Error);
        }
        else
        {
            Debug.Log("更新资源清单成功-------------------");
        }
    }
    #endregion

    #region 创建资源下载器
    void CreateDownloader()
    {
        _downloader = _package.CreateResourceDownloader(downloadingMaxNum, filedTryAgain);
        if (_downloader.TotalDownloadCount == 0)
        {
            Debug.Log("没有需要更新的文件");
            UpdateDone();
        }
        else
        {
            // 发现新更新文件后，挂起流程系统
            // 注意：开发者需要在下载前检测磁盘空间不足
            int count = _downloader.TotalDownloadCount;
            long bytes = _downloader.TotalDownloadBytes;
            Debug.Log($"需要更新{count}个文件, 大小是{bytes / 1024 / 1024}MB");
        }
    }
    #endregion

    #region 开始下载资源文件
    private IEnumerator BeginDownload()
    {
        _downloader.DownloadErrorCallback = DownloadErrorCallback;// 单个文件下载失败
        _downloader.DownloadUpdateCallback = DownloadUpdateCallback;// 下载进度更新
        _downloader.BeginDownload();//开始下载
        yield return _downloader;

        // 检测下载结果
        if (_downloader.Status != EOperationStatus.Succeed)
        {
            Debug.LogWarning(_operationManifest.Error);
            yield break;
        }
        else
        {
            Debug.Log("下载成功-------------------");
        }
    }

    // 单个文件下载失败
    public static void DownloadErrorCallback(DownloadErrorData errorData)
    {
        string fileName = errorData.FileName;
        string errorInfo = errorData.ErrorInfo;
        Debug.Log($"下载失败, 文件名: {fileName}, 错误信息: {errorInfo}");
    }

    // 下载进度更新
    public static void DownloadUpdateCallback(DownloadUpdateData updateData)
    {
        int totalDownloadCount = updateData.TotalDownloadCount;
        int currentDownloadCount = updateData.CurrentDownloadCount;
        long totalDownloadSizeBytes = updateData.TotalDownloadBytes;
        long currentDownloadSizeBytes = updateData.CurrentDownloadBytes;
        Debug.Log($"下载进度: {currentDownloadCount}/{totalDownloadCount}, " +
                  $"{currentDownloadSizeBytes / 1024}KB/{totalDownloadSizeBytes / 1024}KB");
    }
    #endregion

    #region 清理未使用的缓存文件
    void ClearFiles()
    {
        var operationClear = _package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);// 清理未使用的文件
        operationClear.Completed += Operation_Completed;// 添加清理完成回调
    }

    //文件清理完成
    private void Operation_Completed(AsyncOperationBase obj)
    {
        UpdateDone();
    }
    #endregion
    
    #region 热更新结束回调
    private void UpdateDone()
    {
        Debug.Log("热更新结束");
    }
    #endregion
}