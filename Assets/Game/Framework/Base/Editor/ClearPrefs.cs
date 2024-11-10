using UnityEngine;
using UnityEditor;
using System.IO;

public class ClearPrefs : MonoBehaviour
{
    [MenuItem("Tools/清理数据")]
    public static void Clear()
    {
        PlayerPrefs.DeleteAll();
        if (Directory.Exists(Application.persistentDataPath))
            Directory.Delete(Application.persistentDataPath, true);
        if (Directory.Exists(Application.temporaryCachePath))
        {
            Directory.Delete(Application.temporaryCachePath, true);
        }

        if (Directory.Exists(AssetBundleBrowser.Platform.PERSISTENT_DATA_PATH))
        {
            Directory.Delete(AssetBundleBrowser.Platform.PERSISTENT_DATA_PATH, true);
        }

        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/应用数据目录/PersistentDataPath")]
    static void OpenPersistentDataPath()
    {
        System.Diagnostics.Process p = System.Diagnostics.Process.Start(Application.persistentDataPath);
        p.Close();
    }

    [MenuItem("Tools/应用数据目录/TemporaryCachePath")]
    static void OpenTemporaryCachePath()
    {
        System.Diagnostics.Process p = System.Diagnostics.Process.Start(Application.temporaryCachePath);
        p.Close();
    }
}