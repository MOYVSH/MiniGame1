using System.Collections;
using System.Collections.Generic;

struct LinkedKeyValuePairEnumerator<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>>
{
    private readonly LinkedKeyValuePair<TKey, TValue> _start;
    private LinkedKeyValuePair<TKey, TValue> _current;

    public LinkedKeyValuePairEnumerator(LinkedKeyValuePair<TKey, TValue> start)
    {
        _start = start;
        _current = null;
    }

    public bool MoveNext()
    {
        _current = (_current == null) ? _start : _current.After;

        return _current != null;
    }

    public void Reset()
    {
        _current = _start;
    }

    void System.IDisposable.Dispose()
    {
    }

    public KeyValuePair<TKey, TValue> Current
    {
        get { return _current; }
    }

    object IEnumerator.Current
    {
        get { return Current; }
    }
}