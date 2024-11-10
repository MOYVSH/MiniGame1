using System;
using System.Collections.Generic;


internal class LinkedKeyValuePair<TKey, TValue>
{
    internal KeyValuePair<TKey, TValue> Kvp;
    internal LinkedKeyValuePair<TKey, TValue> Before, After;

    public TKey Key
    {
        get { return Kvp.Key; }
    }

    public TValue Value
    {
        get { return Kvp.Value; }
    }

    public LinkedKeyValuePair(TKey key, TValue value)
    {
        Kvp = new KeyValuePair<TKey, TValue>(key, value);
    }

    public LinkedKeyValuePair(KeyValuePair<TKey, TValue> kvp)
    {
        Kvp = kvp;
    }

    public static implicit operator KeyValuePair<TKey, TValue>(LinkedKeyValuePair<TKey, TValue> lkvp)
    {
        if (lkvp == null)
        {
            return (KeyValuePair<TKey, TValue>)default;
        }

        return lkvp.Kvp;
    }

    public static implicit operator LinkedKeyValuePair<TKey, TValue>(KeyValuePair<TKey, TValue> kvp)
    {
        return new LinkedKeyValuePair<TKey, TValue>(kvp);
    }
}