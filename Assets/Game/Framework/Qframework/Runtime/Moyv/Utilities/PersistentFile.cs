using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using QFramework;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public interface IPersistentFile : IUtility
{
    T Read<T>(string fileName, Action<Exception> onError = null, SerializationBinder binder = null);

    bool Save<T>(string fileName, T ss);

    bool SaveTemp<T>(string fileName, T ss);

    bool MoveTempAsRegular(string fileName);

    bool RecoverFromBackup(string fileName);

    bool DeleteBackup(string fileName);

    string GetFileFullPath(string fileName);

    string GetWorkingDir();

    void Delete(string fileName);

    void SetPathProvider(IPathProvider provider);
}

public class PersistentFile : IPersistentFile
{
    private IPathProvider _pathProvider;
    public const string FILE_SUFFIX = ".dat";

    public PersistentFile(IPathProvider provider)
    {
        this._pathProvider = provider;
    }

    public virtual T Read<T>(string fileName, Action<Exception> onError = null, SerializationBinder binder = null)
    {
        T mp = default(T);
        Stream stream = null;
        try
        {
            stream = File.Open(GetFileFullPath(fileName),
                FileMode.OpenOrCreate);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Binder = binder;
            if (stream.Length <= 0)
            {
                stream.Close();
                return mp;
            }

            stream.Seek(0, SeekOrigin.Begin);
            mp = (T)bf.Deserialize(stream);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            if (onError != null)
                onError(e);
        }

        if (stream != null)
            stream.Close();
        return mp;
    }

    public virtual bool Save<T>(string fileName, T ss)
    {
        Stream steam = null;
        try
        {
            var realPath = GetFileFullPath(fileName);
            var tempPath = realPath + ".temp";
            steam = File.Open(tempPath, FileMode.OpenOrCreate);

            var bf = new BinaryFormatter();
            bf.Serialize(steam, ss);
            steam.Close();
            if (File.Exists(realPath))
                File.Delete(realPath);
            File.Move(tempPath, realPath);
            return true;
        }
        catch (Exception e)
        {
            if (steam != null)
                steam.Close();
            Debug.LogError(e);
            return false;
        }
    }

    public virtual bool SaveTemp<T>(string fileName, T ss)
    {
        Stream steam = null;
        try
        {
            var realPath = GetFileFullPath(fileName);
            var tempPath = realPath + ".temp";
            steam = File.Open(tempPath, FileMode.OpenOrCreate);

            var bf = new BinaryFormatter();
            bf.Serialize(steam, ss);
            steam.Close();

            return true;
        }
        catch (Exception e)
        {
            if (steam != null)
                steam.Close();
            Debug.LogError(e);
            return false;
        }
    }

    public virtual bool MoveTempAsRegular(string fileName)
    {
        try
        {
            var realPath = GetFileFullPath(fileName);
            var bakPath = realPath + ".bak";
            var tempPath = realPath + ".temp";

            if (File.Exists(realPath))
            {
                if (File.Exists(bakPath))
                {
                    File.Delete(bakPath);
                }

                File.Move(realPath, bakPath);
            }

            File.Move(tempPath, realPath);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return false;
        }
    }

    public virtual bool DeleteBackup(string fileName)
    {
        try
        {
            var realPath = GetFileFullPath(fileName);
            var bakPath = realPath + ".bak";
            if (File.Exists(bakPath))
            {
                File.Delete(bakPath);
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return false;
        }
    }

    public virtual bool RecoverFromBackup(string fileName)
    {
        try
        {
            var realPath = GetFileFullPath(fileName);
            var bakPath = realPath + ".bak";
            var tempPath = realPath + ".temp";
            if (!File.Exists(bakPath))
            {
                return true;
            }

            if (File.Exists(realPath))
            {
                File.Delete(realPath);
            }

            File.Move(bakPath, realPath);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return false;
        }
    }

    public string GetFileFullPath(string fileName)
    {
        return Path.Combine(_pathProvider.GetLoadPath(), fileName + FILE_SUFFIX);
    }

    public string GetWorkingDir()
    {
        return _pathProvider.GetLoadPath();
    }

    public void Delete(string fileName)
    {
        File.Delete(Path.Combine(_pathProvider.GetSavePath(), fileName + FILE_SUFFIX));
    }

    public void SetPathProvider(IPathProvider provider)
    {
        this._pathProvider = provider;
    }


#if UNITY_EDITOR
    [Serializable]
    public class MyPersistentModel
    {
        public int a = 3;

        public Dictionary<int, string> b = new Dictionary<int, string>()
        {
            { 1, "ccccc" },
            { 3, "dddddd" }
        };

        public Dictionary<int, HashItem> c = new Dictionary<int, HashItem>()
        {
            { 1, new HashItem() },
        };
    }

    [Serializable]
    public class HashItem
    {
        public List<int> c = new List<int>()
        {
            0, 1, 2
        };
    }


    [MenuItem("Tools/Test/IPersistentFile/Test")]
    public static void Test()
    {
        var my = new MyPersistentModel();

        my.a = 4;
        my.b.Add(5, "ddddd");
        my.c.Add(3, new HashItem()
        {
            c = new List<int>() { 3, 4, 3, 1, 3 }
        });

        MemoryStream ms = new MemoryStream();
        var bf = new BinaryFormatter();
        bf.Serialize(ms, my);
        ms.Close();

        ms = new MemoryStream(ms.ToArray());
        var nwmy = (MyPersistentModel)bf.Deserialize(ms);
        ms.Close();
        Debug.Log(nwmy.a);
    }


#endif
}