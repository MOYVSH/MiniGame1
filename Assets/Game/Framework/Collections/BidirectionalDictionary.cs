using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class BidirectionalDictionary<T1, T2>
{
    private Dictionary<T1, T2> _forward;
    private Dictionary<T2, T1> _reverse;

    public int Count => _forward.Count;

    public BidirectionalDictionary() : this(64)
    {
    }


    public BidirectionalDictionary(int capacity)
    {
        if (typeof(T1) == typeof(T2))
        {
            throw new InvalidOperationException("BidirectionalDictionary不支持泛型相同");
        }

        _forward = new Dictionary<T1, T2>(capacity);
        _reverse = new Dictionary<T2, T1>(capacity);
    }

    public virtual T2 this[T1 path]
    {
        get { return _forward[path]; }
        set
        {
            _forward[path] = value;
            _reverse[value] = path;
        }
    }

    public virtual T1 this[T2 asset]
    {
        get { return _reverse[asset]; }
        set
        {
            _reverse[asset] = value;
            _forward[value] = asset;
        }
    }

    public void Add(T1 t1, T2 t2)
    {
        if (t1 == null || t2 == null) return;
        _forward.Add(t1, t2);
        _reverse.Add(t2, t1);
    }

    public void Clear()
    {
        _forward.Clear();
        _reverse.Clear();
    }

    public void Remove(T1 i)
    {
        if (i == null) return;
        if (!_forward.TryGetValue(i, out T2 value))
            return;
        _forward.Remove(i);
        _reverse.Remove(value);
    }

    public void Remove(T2 i)
    {
        if (i == null) return;
        if (!_reverse.TryGetValue(i,out T1 value))
            return;

        _reverse.Remove(i);
        _forward.Remove(value);
    }

    public virtual bool TryGetValue(T1 key, out T2 value)
    {
        if (key == null)
        {
            value = default;
            return false;
        }

        return _forward.TryGetValue(key, out value);
    }

    public virtual bool TryGetValue(T2 key, out T1 value)
    {
        if (key == null)
        {
            value = default;
            return false;
        }

        return _reverse.TryGetValue(key, out value);
    }
}