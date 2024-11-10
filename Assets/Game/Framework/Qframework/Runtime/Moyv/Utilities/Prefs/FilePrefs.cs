using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public interface IPrefsValue : IPrefs
{
    public Dictionary<string, int> intValues { get; set; }
    public Dictionary<string, float> floatValues { get; set; }
    public Dictionary<string, string> stringValues { get; set; }
}

public interface IPrefsRaw : IPrefsValue
{
    void ReadIntValues(byte[] bytes = null);
    void ReadStringValues(byte[] bytes = null);
    void ReadFloatValues(byte[] bytes = null);

    byte[] SaveIntValue(bool writeFile = false);
    byte[] SaveFloatValue(bool writeFile = false);
    byte[] SaveStringValue(bool writeFile = false);
}

/// <summary>
///
/// </summary>
public class FilePrefs : IPrefsRaw
{
    public static bool allowSave = true;

    public const string INT_FILENAME = "filePrefs_int";
    public const string STRING_FILENAME = "filePrefs_string";
    public const string FLOAT_FILENAME = "filePrefs_float";
    private const string FILE_SUFFIX = ".dat";
    public Dictionary<string, int> _intValues = new Dictionary<string, int>();
    public Dictionary<string, float> _floatValues = new Dictionary<string, float>();
    public Dictionary<string, string> _stringValues = new Dictionary<string, string>();

    public Dictionary<string, int> intValues
    {
        get => _intValues;
        set => _intValues = value;
    }

    public Dictionary<string, float> floatValues
    {
        get => _floatValues;
        set => _floatValues = value;
    }

    public Dictionary<string, string> stringValues
    {
        get => _stringValues;
        set => _stringValues = value;
    }

    private IPathProvider pathProvider;

    public FilePrefs()
    {
    }

    public FilePrefs(IPathProvider pathProvider)
    {
        this.pathProvider = pathProvider;
    }

    public void SetPathProvider(IPathProvider provider)
    {
        pathProvider = provider;
    }

    public void Reload()
    {
        ReadIntValues();
        ReadStringValues();
        ReadFloatValues();
    }

    public void SetInt(string key, int value)
    {
        intValues[key] = value;
    }

    public int GetInt(string key, int i = 0)
    {
        return intValues.GetValueOrDefault(key, i);
    }

    public void DeleteKey(string key)
    {
        if (string.IsNullOrEmpty(key))
            return;
        intValues.Remove(key);
        floatValues.Remove(key);
        stringValues.Remove(key);
    }

    public void SetBool(string key, bool value)
    {
        intValues[key] = value ? 1 : 0;
    }

    public bool GetBool(string key, bool i = true)
    {
        if (intValues.TryGetValue(key, out var v))
        {
            return v == 1;
        }

        return i;
    }

    public void SetFloat(string key, float value)
    {
        floatValues[key] = value;
    }

    public float GetFloat(string key, float i = 0)
    {
        return floatValues.GetValueOrDefault(key, i);
    }

    public void SetString(string key, string value)
    {
        stringValues[key] = value;
    }

    public string GetString(string key, string s)
    {
        return stringValues.GetValueOrDefault(key, s);
    }

    public void Save()
    {
        if (!allowSave) return;

        SaveIntValue(true);
        SaveFloatValue(true);
        SaveStringValue(true);
    }

    public void DeleteAll()
    {
        intValues.Clear();
        floatValues.Clear();
        stringValues.Clear();
    }

    public string[] GetAllKeys()
    {
        var keys = intValues.Keys.ToList();
        keys.AddRange(floatValues.Keys);
        keys.AddRange(stringValues.Keys);
        return keys.ToArray();
    }


    public bool HasKey(string key)
    {
        return intValues.ContainsKey(key) || floatValues.ContainsKey(key) || stringValues.ContainsKey(key);
    }

    public byte[] SaveIntValue(bool writeFile = false)
    {
        return SaveDictionary(INT_FILENAME, bw =>
        {
            foreach (KeyValuePair<string, int> pair in intValues)
            {
                bw.Write(pair.Key);
                bw.Write(pair.Value);
            }
        }, writeFile);
    }

    public byte[] SaveFloatValue(bool writeFile = false)
    {
        return SaveDictionary(FLOAT_FILENAME, bw =>
        {
            foreach (KeyValuePair<string, float> pair in floatValues)
            {
                bw.Write(pair.Key);
                bw.Write(pair.Value);
            }
        }, writeFile);
    }

    public byte[] SaveStringValue(bool writeFile = false)
    {
        return SaveDictionary(STRING_FILENAME, bw =>
        {
            foreach (KeyValuePair<string, string> pair in stringValues)
            {
                bw.Write(pair.Key);
                bw.Write(pair.Value);
            }
        }, writeFile);
    }

    internal byte[] SaveDictionary(string fileName, Action<BinaryWriter> writer, bool writeFile = false)
    {
        byte[] bytes = null;

        try
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8, false))
                {
                    writer(bw);
                }

                bytes = ms.ToArray();
            }

            if (writeFile)
                File.WriteAllBytes(Path.Combine(pathProvider.GetSavePath(), fileName + FILE_SUFFIX), bytes);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }

        return bytes;
    }


    public void ReadIntValues(byte[] bytes = null)
    {
        intValues.Clear();
        if (bytes == null)
            ReadDictionary(INT_FILENAME, br => { intValues[br.ReadString()] = br.ReadInt32(); });
        else
        {
            ReadDictionary(bytes, br => { intValues[br.ReadString()] = br.ReadInt32(); });
        }
    }

    public void ReadFloatValues(byte[] bytes = null)
    {
        floatValues.Clear();
        if (bytes == null)
            ReadDictionary(FLOAT_FILENAME, br => { floatValues[br.ReadString()] = br.ReadSingle(); });
        else
        {
            ReadDictionary(bytes, br => { floatValues[br.ReadString()] = br.ReadSingle(); });
        }
    }

    public void ReadStringValues(byte[] bytes = null)
    {
        stringValues.Clear();
        if (bytes == null)
            ReadDictionary(STRING_FILENAME, br => { stringValues[br.ReadString()] = br.ReadString(); });
        else
        {
            ReadDictionary(bytes, br => { stringValues[br.ReadString()] = br.ReadString(); });
        }
    }


    private void ReadDictionary(byte[] bytes, Action<BinaryReader> fillData)
    {
        try
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (BinaryReader br = new BinaryReader(ms, Encoding.UTF8, false))
                {
                    while (br.BaseStream.Position < br.BaseStream.Length)
                    {
                        fillData(br);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
    }

    private void ReadDictionary(string fileName, Action<BinaryReader> fillData)
    {
        if (pathProvider == null)
        {
            Debug.LogError("PathProvider is NULL");
            return;
        }

        var file = Path.Combine(pathProvider.GetLoadPath(), fileName + FILE_SUFFIX);

        if (!File.Exists(file))
        {
            return;
        }

        try
        {
            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                using (BinaryReader br = new BinaryReader(fs, Encoding.UTF8, false))
                {
                    while (br.BaseStream.Position < br.BaseStream.Length)
                    {
                        fillData(br);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
    }

#if UNITY_EDITOR
    // [MenuItem("Tools/Test/FilePrefs/Save")]
    // public static void Test()
    // {
    //     var fp = new FilePrefs();
    //
    //     fp.SetInt("a", 1);
    //     fp.SetInt("a", 3);
    //     fp.SetInt("b", 4);
    //
    //     fp.SetString("c", "ahahhaha");
    //     fp.SetString("d", "dddddd");
    //     fp.Save();
    // }
    //
    // [MenuItem("Tools/Test/FilePrefs/Read")]
    // public static void Test1()
    // {
    //     var fp = new FilePrefs();
    //     foreach (KeyValuePair<string, int> keyValuePair in fp.intValues)
    //     {
    //         Debug.Log(keyValuePair.Key + " -> " + keyValuePair.Value);
    //     }
    //
    //     foreach (KeyValuePair<string, string> keyValuePair in fp.stringValues)
    //     {
    //         Debug.Log(keyValuePair.Key + " -> " + keyValuePair.Value);
    //     }
    // }

#endif
}