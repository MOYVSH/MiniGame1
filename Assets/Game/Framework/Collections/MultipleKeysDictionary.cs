using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// [(a,b,c),1]
/// [(d,e,f),2]
/// same as Dictionary<List<T1>, T2>
/// </summary>
/// <typeparam name="T1"></typeparam>
/// <typeparam name="T2"></typeparam>
public class MultipleKeysDictionary<T1, T2>
{
    protected Dictionary<List<T1>, T2> _entry;
    protected Dictionary<T1, List<T1>> _keys;
    protected Dictionary<T2, List<T1>> _values;

    public int Count => _entry.Count;

    public Dictionary<List<T1>, T2> Entry => _entry;

    public MultipleKeysDictionary() : this(64)
    {
    }


    public MultipleKeysDictionary(int capacity)
    {
        if (typeof(T1) == typeof(T2))
        {
            throw new InvalidOperationException("MultipleKeysDictionary不支持泛型相同");
        }

        _entry = new Dictionary<List<T1>, T2>(capacity);
        _keys = new Dictionary<T1, List<T1>>(capacity);
        _values = new Dictionary<T2, List<T1>>(capacity);
    }

    public void Add(T1 k, T2 v)
    {
        if (_keys.TryGetValue(k, out var rk))
        {
            throw new InvalidOperationException($"Already exists a key : {k}");
        }

        this[k] = v;
    }

    public T2 this[T1 k]
    {
        get
        {
            if (TryGetValue(k, out var v))
            {
                return v;
            }

            return default(T2);
        }
        set
        {
            if (_values.TryGetValue(value, out var rk))
            {
                rk.Add(k);
                _keys[k] = rk;
            }
            else
            {
                if (_keys.TryGetValue(k, out rk))
                {
                    rk.Remove(k);
                    var v = _entry[rk];
                    if (rk.Count == 0)
                    {
                        _entry.Remove(rk);
                        _values.Remove(v);
                    }
                }

                rk = new List<T1>() {k};
                _entry[rk] = value;
                _keys[k] = rk;
                _values[value] = rk;
            }
        }
    }

    public virtual void Remove(T1 k)
    {
        if (_keys.TryGetValue(k, out var rk))
        {
            if (_entry.TryGetValue(rk, out var v))
            {
                _keys.Remove(k);
                _entry.Remove(rk);
                _values.Remove(v);
            }
        }
    }

    public void Clear()
    {
        _entry.Clear();
        _keys.Clear();
        _values.Clear();
    }

    public virtual bool TryGetValue(T1 key, out T2 value)
    {
        if (key == null)
        {
            value = default;
            return false;
        }

        if (_keys.TryGetValue(key, out var rk))
        {
            if (_entry.TryGetValue(rk, out value))
            {
                return true;
            }
        }

        value = default;
        return false;
    }
}