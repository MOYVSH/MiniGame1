using UnityEngine;

public class PreloadResource
{
    private static bool _isInit = false;

    public static void InitData()
    {

        if (_isInit) return;
        LoadPrefab();
        LoadConfig();
        _isInit = true;
        Debug.Log("Preload Finished！！！！");
    }

    private static void LoadConfig()
    {
        
    }

    private static void LoadPrefab()
    {
        
    }
}
