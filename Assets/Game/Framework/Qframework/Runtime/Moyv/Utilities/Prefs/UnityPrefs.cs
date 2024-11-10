using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;

public interface IPathProvider
{
    string GetSavePath();
    string GetLoadPath();
}

public interface IPrefs : IUtility
{
   
    void SetInt(string key, int value = 0);
    int GetInt(string key, int i = 0);

    void SetBool(string key, bool value);

    bool GetBool(string key, bool i = false);

    void SetString(string key, string value = "");
    string GetString(string key, string s = "");

    void SetFloat(string key, float value);
    float GetFloat(string key, float i = 0);

    void Save();
    void DeleteAll();

    string[] GetAllKeys();

    void DeleteKey(string key);
    bool HasKey(string key);

    void Reload();

    void SetPathProvider(IPathProvider provider);
}

public class UnityPrefs : IPrefs
{
    public static bool allowSave = true;
    
    public int GetInt(string key, int i = 0)
    {
        return PlayerPrefs.GetInt(key, i);
    }

    public void SetBool(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
    }

    public bool GetBool(string key, bool i)
    {
        return GetInt(key, i ? 1 : 0) == 1;
    }

    public string GetString(string key, string s = "")
    {
        return PlayerPrefs.GetString(key, s);
    }

    public void SetFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
    }

    public float GetFloat(string key, float i)
    {
        return PlayerPrefs.GetFloat(key, i);
    }

    public void Save()
    {
        if (!allowSave) return;
        
        PlayerPrefs.Save();
    }

    public void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
    }

    public string[] GetAllKeys()
    {
        throw new System.NotImplementedException();
    }

    public void DeleteKey(string key)
    {
        PlayerPrefs.DeleteKey(key);
    }

    public bool HasKey(string key)
    {
        return PlayerPrefs.HasKey(key);
    }

    public void Reload()
    {
        
    }

    public void SetPathProvider(IPathProvider provider)
    {
         
    }

    public void SetString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
    }

    public void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
    }
}