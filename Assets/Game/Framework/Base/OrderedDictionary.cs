using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MOYV
{
    /// <summary>
    /// Lru cache.
    /// </summary>
    /// http://d.hatena.ne.jp/Kazuhira/20151226/1451134718
    /// http://qiita.com/matarillo/items/c09e0f3e5a61f84a51e2
    /// https://github.com/Unity-Technologies/mono/blob/unity-staging/mcs/class/corlib/System.Collections.Generic/Dictionary.cs
    /// https://android.googlesource.com/platform/libcore/+/master/ojluni/src/main/java/java/util/HashMap.java
    /// https://android.googlesource.com/platform/libcore/+/master/ojluni/src/main/java/java/util/LinkedHashMap.java
    /// https://android.googlesource.com/platform/frameworks/support.git/+/795b97d901e1793dac5c3e67d43c96a758fec388/v4/java/android/support/v4/util/LruCache.java
    public class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        #region Internal Class

        private class LinkedKeyValuePair
        {
            internal KeyValuePair<TKey, TValue> Kvp;
            internal LinkedKeyValuePair Before, After;

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

            public static implicit operator KeyValuePair<TKey, TValue>(LinkedKeyValuePair lkvp)
            {
                return lkvp.Kvp;
            }

            public static implicit operator LinkedKeyValuePair(KeyValuePair<TKey, TValue> kvp)
            {
                return new LinkedKeyValuePair(kvp);
            }
        }

        // ReSharper disable once InconsistentNaming
        private struct LKVPEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly LinkedKeyValuePair _start;
            private LinkedKeyValuePair _current;

            public LKVPEnumerator(LinkedKeyValuePair start)
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

        #endregion

        #region Field

        private LinkedKeyValuePair _head;
        private LinkedKeyValuePair _tail;
        private readonly Dictionary<TKey, LinkedKeyValuePair> _dict;

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
                    throw new System.ArgumentNullException("key");
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
            _dict = new Dictionary<TKey, LinkedKeyValuePair>();
        }

        #region Interface

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> value)
        {
            var last = NewKVP(value);
            _dict[value.Key] = last;
        }

        public void Add(TKey key, TValue value)
        {
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
            var tmp = new LinkedKeyValuePair[array.Length];
            _dict.Values.CopyTo(tmp, arrayIndex);
            array = System.Array.ConvertAll(tmp, element => element.Kvp);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new LKVPEnumerator(_head);
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
            LinkedKeyValuePair kvp;
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
        private LinkedKeyValuePair NewKVP(TKey key, TValue value)
        {
            var lkvp = new LinkedKeyValuePair(key, value);
            InsertLast(lkvp);

            return lkvp;
        }

        // ReSharper disable once InconsistentNaming
        private LinkedKeyValuePair NewKVP(KeyValuePair<TKey, TValue> kvp)
        {
            var lkvp = new LinkedKeyValuePair(kvp);
            InsertLast(lkvp);

            return lkvp;
        }

        private void AfterNodeAccess(LinkedKeyValuePair kvp)
        {
            var last = _tail;
            if (!AccessOrder || last == kvp)
            {
                return;
            }

            LinkedKeyValuePair p = kvp, b = p.Before, a = p.After;
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

        private void InsertLast(LinkedKeyValuePair kvp)
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
        private void RemoveKVP(LinkedKeyValuePair kvp)
        {
            LinkedKeyValuePair p = kvp, b = p.Before, a = p.After;

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

            p = kvp = null;
        }

        #endregion
    }
}