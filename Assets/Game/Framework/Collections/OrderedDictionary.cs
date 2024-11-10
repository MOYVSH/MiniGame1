using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// used by Lru cache.
/// </summary>
/// http://d.hatena.ne.jp/Kazuhira/20151226/1451134718
/// http://qiita.com/matarillo/items/c09e0f3e5a61f84a51e2
/// https://github.com/Unity-Technologies/mono/blob/unity-staging/mcs/class/corlib/System.Collections.Generic/Dictionary.cs
/// https://android.googlesource.com/platform/libcore/+/master/ojluni/src/main/java/java/util/HashMap.java
/// https://android.googlesource.com/platform/libcore/+/master/ojluni/src/main/java/java/util/LinkedHashMap.java
/// https://android.googlesource.com/platform/frameworks/support.git/+/795b97d901e1793dac5c3e67d43c96a758fec388/v4/java/android/support/v4/util/LruCache.java
public class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
    #region Field

    private LinkedKeyValuePair<TKey, TValue> _head;
    private LinkedKeyValuePair<TKey, TValue> _tail;
    private readonly Dictionary<TKey, LinkedKeyValuePair<TKey, TValue>> _dict;

    #endregion

    #region Property

    public int Count
    {
        get { return _dict.Count; }
    }

    public bool IsReadOnly
    {
        get { return false; }
    }

    public TValue this[TKey key]
    {
        get
        {
            if (key == null) 
            {
                throw new System.ArgumentNullException(nameof(key));
            }

            TValue value;
            if (TryGetValue(key, out value))
            {
                return value;
            }

            throw new KeyNotFoundException();
        }
        set
        {
            if (key == null)
            {
                throw new System.ArgumentNullException("key");
            }

            if (_dict.ContainsKey(key))
            {
                Remove(key);
            }

            if (value != null)
            {
                Add(key, value);
            }
        }
    }

    public ICollection<TKey> Keys
    {
        get { return _dict.Keys; }
    }

    public ICollection<TValue> Values
    {
        get { return _dict.Values.Select(kvp => kvp.Value).ToArray(); }
    }

    public bool AccessOrder { get; private set; }

    public KeyValuePair<TKey, TValue> Eldest
    {
        get { return _head; }
    }

    #endregion

    public OrderedDictionary(bool accessOrder = false)
    {
        AccessOrder = accessOrder;
        _dict = new Dictionary<TKey, LinkedKeyValuePair<TKey, TValue>>();
    }

    #region Interface

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> value)
    {
        var last = NewKVP(value);
        _dict[value.Key] = last;
    }

    public void Add(TKey key, TValue value)
    {
        if (key == null)
        {
            throw new System.ArgumentNullException("key");
        }
        var last = NewKVP(key, value);
        _dict[key] = last;
    }

    public void Clear()
    {
        _dict.Clear();
        _head = _tail = null;
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> value)
    {
        return ContainsKey(value.Key);
    }

    public bool ContainsKey(TKey key)
    {
        for (var e = _head; e != null; e = e.After)
        {
            if (e.Key.Equals(key))
            {
                return true;
            }
        }

        return false;
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        var tmp = new LinkedKeyValuePair<TKey, TValue>[array.Length];
        _dict.Values.CopyTo(tmp, arrayIndex);
        // array = System.Array.ConvertAll(tmp, element => element.Kvp);
        if (arrayIndex < 0 || arrayIndex >= array.Length)
        {
            throw new ArgumentOutOfRangeException("arrayIndex");
        }

        if (array.Length - arrayIndex < _dict.Count)
        {
            throw new ArgumentException("目标数组长度小于数据个数");
        }
        var index = arrayIndex;
        foreach (var kvp in _dict.Values)
        {
            array[index++] = kvp.Kvp;
        }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return new LinkedKeyValuePairEnumerator<TKey, TValue>(_head);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> value)
    {
        RemoveKVP(_dict[value.Key]);
        return _dict.Remove(value.Key);
    }

    public bool Remove(TKey key)
    {
        RemoveKVP(_dict[key]);
        return _dict.Remove(key);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        LinkedKeyValuePair<TKey, TValue> kvp;
        if (_dict.TryGetValue(key, out kvp))
        {
            AfterNodeAccess(kvp);
            value = kvp.Value;
            return true;
        }

        value = default(TValue);
        return false;
    }

    #endregion

    #region Linked List

    // ReSharper disable once InconsistentNaming
    private LinkedKeyValuePair<TKey, TValue> NewKVP(TKey key, TValue value)
    {
        var lkvp = new LinkedKeyValuePair<TKey, TValue>(key, value);
        InsertLast(lkvp);

        return lkvp;
    }

    // ReSharper disable once InconsistentNaming
    private LinkedKeyValuePair<TKey, TValue> NewKVP(KeyValuePair<TKey, TValue> kvp)
    {
        var lkvp = new LinkedKeyValuePair<TKey, TValue>(kvp);
        InsertLast(lkvp);

        return lkvp;
    }

    private void AfterNodeAccess(LinkedKeyValuePair<TKey, TValue> kvp)
    {
        var last = _tail;
        if (!AccessOrder || last == kvp)
        {
            return;
        }

        LinkedKeyValuePair<TKey, TValue> p = kvp, b = p.Before, a = p.After;
        p.After = null;

        if (b == null)
        {
            _head = a;
        }
        else
        {
            b.After = a;
        }

        if (a != null)
        {
            a.Before = b;
        }
        else
        {
            last = b;
        }

        if (last == null)
        {
            _head = p;
        }
        else
        {
            p.Before = last;
            last.After = p;
        }

        _tail = p;
    }

    private void InsertLast(LinkedKeyValuePair<TKey, TValue> kvp)
    {
        var last = _tail;
        _tail = kvp;

        if (last == null)
        {
            _head = kvp;
        }
        else
        {
            kvp.Before = last;
            last.After = _tail;
        }
    }

    // ReSharper disable once InconsistentNaming
    private void RemoveKVP(LinkedKeyValuePair<TKey, TValue> kvp)
    {
        LinkedKeyValuePair<TKey, TValue> p = kvp, b = p.Before, a = p.After;

        if (b == null)
        {
            _head = a;
        }
        else
        {
            b.After = a;
        }

        if (a == null)
        {
            _tail = b;
        }
        else
        {
            a.Before = b;
        }
    }

    #endregion
}