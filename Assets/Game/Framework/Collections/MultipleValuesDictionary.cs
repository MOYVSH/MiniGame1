using System;
using System.Collections.Generic;

public class MultipleValuesDictionary<T1, T2>
{
    protected Dictionary<T1, List<T2>> _entry;
    protected Dictionary<T2, List<T1>> _values;

    public int Count => _entry.Count;
    
    public MultipleValuesDictionary() : this(64)
    {
    }


    public MultipleValuesDictionary(int capacity)
    {
        if (typeof(T1) == typeof(T2))
        {
            throw new InvalidOperationException("MultipleValuesDictionary不支持泛型相同");
        }

        _entry = new Dictionary<T1, List<T2>>(capacity);
        _values = new Dictionary<T2, List<T1>>(capacity);
    }

    public void Add(T1 k, T2 v)
    {
        if (_entry.TryGetValue(k, out var vs))
        {
            if (_values.TryGetValue(v, out var ks))
            {
                throw new InvalidOperationException("Already Exists");
            }
            else
            {
                if (vs == null)
                {
                    vs = new List<T2>();
                }

                vs.Add(v);

                _values[v] = new List<T1>() {k};
            }
        }
        else
        {
            _entry[k] = new List<T2> {v};


            if (!_values.TryGetValue(v, out var ks))
            {
                _values[v] = ks = new List<T1>();
            }

            ks.Add(k);
        }
    }

    public List<T2> this[T1 k]
    {
        get
        {
            if (TryGetValue(k, out var v))
            {
                return v;
            }

            return default;
        }
        set
        {
            Remove(k);
            //_entry[k] = value;
            var count = value.Count;
            for (var i = 0; i < count; i++)
            {
                Add(k, value[i]);
            }
        }
    }

    public List<T1> this[T2 k]
    {
        get
        {
            if (TryGetValue(k, out var v))
            {
                return v;
            }

            return default;
        }
        set
        {
            Remove(k);
            _values[k] = value;
            foreach (T1 t1 in value)
            {
                Add(t1, k);
            }
        }
    }

    public virtual bool TryGetValue(T1 key, out List<T2> value)
    {
        if (key == null)
        {
            value = default;
            return false;
        }

        return _entry.TryGetValue(key, out value);
    }

    public virtual bool TryGetValue(T2 key, out List<T1> value)
    {
        if (key == null)
        {
            value = default;
            return false;
        }

        return _values.TryGetValue(key, out value);
    }

    public void Clear()
    {
        _entry.Clear();
        _values.Clear();
    }

    public virtual void Remove(T1 k)
    {
        if (_entry.TryGetValue(k, out var rk))
        {
            foreach (T2 t2 in rk)
            {
                if (_values.TryGetValue(t2, out var v))
                {
                    if (v != null)
                    {
                        v.Remove(k);

                        if (v.Count == 0)
                        {
                            _values.Remove(t2);
                        }
                    }
                    else
                    {
                        _values.Remove(t2);
                    }
                }
            }

            _entry.Remove(k);
        }
    }


    public virtual void Remove(T2 k)
    {
        if (_values.TryGetValue(k, out var rk))
        {
            foreach (T1 t1 in rk)
            {
                if (_entry.TryGetValue(t1, out var v))
                {
                    if (v != null)
                    {
                        v.Remove(k);

                        if (v.Count == 0)
                        {
                            _entry.Remove(t1);
                        }
                    }
                    else
                    {
                        _entry.Remove(t1);
                    }
                }
            }

            _values.Remove(k);
        }
    }

    public virtual Dictionary<T1, List<T2>>.KeyCollection Keys
    {
        get
        {
            return _entry.Keys;
        }
    }
}