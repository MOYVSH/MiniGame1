using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ObjectPool : MonoSingleton<ObjectPool>
{


    //[Serializable]
    private class PrefabConfig
    {
        public GameObject prefab;
        public int preloadNumber;
        public bool willGrow;
        public int poolCapacity;

        internal Transform parentNode;
        internal Queue<GameObject> freeQueue;
        internal ObjectPool owner;

        private bool hasCleared;

        public PrefabConfig()
        {
            preloadNumber = 20;
            willGrow = true;
            freeQueue = new Queue<GameObject>();
        }

        public void Preload()
        {
            for (int i = 0; i < preloadNumber; i++)
            {
                AddInstance();
            }
        }

        public void ReleaseAllInstance()
        {
            foreach (var item in freeQueue)
            {
                owner.releaseQueue.Enqueue(item);
            }
            freeQueue.Clear();
        }

        void AddInstance()
        {
            var instance = Instantiate<GameObject>(prefab);

            // add to dictionary
            owner.instanceDictionary.Add(instance, this);

            SetPooledObject(instance);
        }

        public GameObject GetPooledObject()
        {
            GameObject instance = null;

                if (freeQueue.Count == 0)
                {
                    if (willGrow)
                    {
                        AddInstance();
                    }
                    else
                    {
                        return null;
                    }
                }

                instance = freeQueue.Dequeue();

                // detach
                instance.transform.SetParent(null, false);
            


            return instance;
        }

        public void SetPooledObject(GameObject instance)
        {
            if (freeQueue.Contains(instance))
            {
#if UNITY_EDITOR
                Debug.LogWarningFormat("Object {0} is already in queue", instance);
#endif
                return;
            }

            if (poolCapacity != 0 && freeQueue.Count >= poolCapacity)
            {
                // just destroy it
                GameObject.Destroy(instance);
                return;
            }

            // hide and set as child
            instance.SetActive(false);
            instance.transform.SetParent(parentNode, false);
            instance.transform.localPosition = Vector3.zero;

            IRecyclableInstance[] recyclables = instance.GetComponentsInChildren<IRecyclableInstance>(true);
            // Check if need recycle
            if (recyclables.Length > 0)
            {
                for (int index = 0; index < recyclables.Length; index++)
                {
                    recyclables[index].DeactivateInstance();
                }
            }

            freeQueue.Enqueue(instance);
        }
    }

    private const int defaultCapacity = 10;
    //public List<PrefabConfig> prefabConfigList = new List<PrefabConfig>();
    public Transform freeNode;

    internal Queue<GameObject> releaseQueue = new Queue<GameObject>();

    private Dictionary<GameObject, PrefabConfig> instanceDictionary = new Dictionary<GameObject, PrefabConfig>();
    private Dictionary<GameObject, PrefabConfig> prefabDictionary = new Dictionary<GameObject, PrefabConfig>();

    protected override void Awake()
    {
        base.Awake();
        /*foreach (var config in prefabConfigList)
        {
            config.owner = this;

            // add to dictionary
            prefabDictionary.Add(config.prefab, config);

            // create parent node
            var parentNode = new GameObject(config.prefab.name);
            parentNode.transform.SetParent(freeNode, false);
            parentNode.transform.localPosition = Vector3.zero;

            // set parent node
            config.parentNode = parentNode.transform;

            // preload objects
            config.Preload();
        }*/
    }

    void Update()
    {
        UpdateReleaseQueue();
    }

    void UpdateReleaseQueue()
    {
        if (releaseQueue.Count == 0)
        {
            return;
        }

        var item = releaseQueue.Dequeue();
        Destroy(item);
        item = null;
    }

    public void RegisterPrefab(GameObject prefab, int preloadNumber, bool willGrow, int capacity)
    {
        var item = new PrefabConfig
        {
            prefab = prefab,
            preloadNumber = preloadNumber,
            willGrow = willGrow,
            owner = this,
            poolCapacity = capacity
        };

        // add to dictionary
        prefabDictionary.Add(item.prefab, item);

        // create parent node
        var parentNode = new GameObject(item.prefab.name);
        parentNode.transform.SetParent(freeNode, false);
        parentNode.transform.localPosition = Vector3.zero;

        // set parent node
        item.parentNode = parentNode.transform;

        // preload objects
        item.Preload();
    }

    public bool CheckPrefabInPool(GameObject prefab)
    {
        return prefabDictionary.ContainsKey(prefab);
    }

    public void SetPoorCapacity(GameObject prefab, int capacity)
    {
        PrefabConfig item;
        if (prefabDictionary.TryGetValue(prefab, out item))
        {
            item.poolCapacity = capacity;
        }
    }

    public GameObject GetFreeObject(GameObject prefab)
    {
        PrefabConfig item;
        if (!prefabDictionary.TryGetValue(prefab, out item))
        {
            //Debug.Log(string.Format("No pool for prefab named: {0}, Creating one with default capacity", prefab.name));
            RegisterPrefab(prefab, 0, true, defaultCapacity);
            prefabDictionary.TryGetValue(prefab, out item);
        }
        GameObject freeInstance = null;

        freeInstance = item.GetPooledObject();
        return freeInstance;
    }

    public void SetFreeObject(GameObject instance)
    {
        PrefabConfig config;
        if (!instanceDictionary.TryGetValue(instance, out config))
        {
            IRecyclableInstance[] recyclables = instance.GetComponentsInChildren<IRecyclableInstance>(true);
            // Check if need recycle
            if (recyclables.Length > 0)
            {
                for (int index = 0; index < recyclables.Length; index++)
                {
                    recyclables[index].DeactivateInstance();
                }
            }
            else
            {
                Destroy(instance);
            }
            return;
        }
        config.SetPooledObject(instance);
    }

    public void ReleaseUnusedInstance(GameObject[] holdPrefabs)
    {
        foreach (var item in instanceDictionary)
        {
            // Ignore can't grow items
            if (!item.Value.willGrow)
            {
                continue;
            }

            bool markDelete = true;
            foreach (var prefab in holdPrefabs)
            {
                if (prefab == item.Value.prefab)
                {
                    markDelete = false;
                    break;
                }
            }

            if (markDelete)
            {
                item.Value.ReleaseAllInstance();
            }
        }
    }
}
