using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T mInstance = null;
    private static GameObject parent;

    public static T Instance
    {
        get
        {
            if (mInstance == null && !Initializer.GetSingletonStatus<T>())
            {
                if (!parent)
                {
                    parent = GameObject.Find("Boot");
                    if (!parent)
                    {
                        parent = new GameObject("Boot");
                    }
                }

                mInstance = FindObjectOfType<T>();
                if (mInstance == null)
                {
                    GameObject go = new GameObject(typeof(T).Name);
                    mInstance = go.AddComponent<T>();
                    if (parent)
                    {
                        go.transform.parent = parent.transform;
                    }
                }

                if (Application.isPlaying) DontDestroyOnLoad(parent);
            }

            return mInstance;
        }
    }


    /// <summary>
    /// MonoSingleton起始点
    /// </summary>
    public void Startup()
    {
    }

    protected virtual void Awake()
    {
        if (mInstance == null)
        {
            mInstance = this as T;
        }

        var instances = FindObjectsOfType<T>();
        foreach (var instance in instances)
        {
            if (mInstance != instance)
            {
                Debug.LogWarning("Destroy unused instances of " + typeof(T).Name + "   Boot has/have " +
                                 this.transform.parent.childCount + " child(ren)");
                if (instance.transform.parent.childCount == 1)
                {
                    Destroy(instance.transform.parent.gameObject);
                }
                else
                    Destroy(instance.gameObject);
            }
        }


        if (Application.isPlaying)
            if (gameObject.transform.parent)
                DontDestroyOnLoad(gameObject.transform.parent.gameObject);

        Init();
    }

    protected virtual void Init()
    {
    }


    public virtual void Dispose()
    {
    }

    public virtual void OnDestroy()
    {
        if (this == mInstance)
        {
            Initializer.SetToIsDestroying<T>();

            Dispose();
            mInstance = null;
        }
    }

    public virtual void OnApplicationQuit()
    {
        if (this == mInstance)
        {
            Initializer.SetToIsDestroying<T>();
        }
    }
}

public class Initializer
{
    public static void SetToIsDestroying<T>()
    {
        var type = typeof(T);
        singletonStatus[type] = true;
    }

    public static bool GetSingletonStatus<T>()
    {
        var type = typeof(T);
        if (singletonStatus.TryGetValue(type, out bool isDestroying))
        {
            return isDestroying && Application.isPlaying;
        }

        singletonStatus[type] = false;
        return false;
    }

    private static Dictionary<Type, bool> singletonStatus = new Dictionary<Type, bool>();

    [RuntimeInitializeOnLoadMethod]
    static void InitLoader()
    {
#if UNITY_EDITOR
        Debug.LogWarning("Clear ALL singletonStatus");
#endif
        singletonStatus.Clear();
    }
}