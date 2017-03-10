
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace ME {

    public class SimpleDictionary<K, V> : IDictionary, IDictionary<K, V> {

        public struct Enumerator : IDictionaryEnumerator, IEnumerator<KeyValuePair<K, V>> {

            private readonly SimpleDictionary<K, V> dictionary;
            private int index;

            public Enumerator(SimpleDictionary<K, V> dictionary) {

                this.index = -1;
                this.dictionary = dictionary;

            }

            public bool MoveNext() {

				return ++this.index < this.dictionary.Count;

            }

            public void Reset() {

                this.index = 0;

            }

            public KeyValuePair<K, V> Current {
                get {
                    return this.dictionary.GetAt(this.index);
                }
            }

            object IEnumerator.Current { get { return this.Current; } }

            public DictionaryEntry Entry {

                get {

                    var current = this.Current;
                    return new DictionaryEntry(current.Key, current.Value);

                }
            }

            public object Key { get { return this.Current.Key; } }
            public object Value { get { return this.Current.Value; } }

            public void Dispose() {
            }

        }

        private readonly List<K> keys;
        private readonly List<V> values;
        private readonly IEqualityComparer<K> keyEqualityComparer;
        private object syncRoot;

        public SimpleDictionary(int capacity = 4, IEqualityComparer<K> keyEqualityComparer = null) {

            this.keys = new List<K>(capacity);
            this.values = new List<V>(capacity);
            this.keyEqualityComparer = keyEqualityComparer ?? EqualityComparer<K>.Default;

        }

        public void Add(object key, object value) {

            this.Add((K)key, (V)value);

        }

        public void Add(KeyValuePair<K, V> item) {

            this.Add(item.Key, item.Value);

        }

        void ICollection<KeyValuePair<K, V>>.Clear() {

            this.Clear();

        }

        public bool Contains(KeyValuePair<K, V> item) {

            return this.IndexOf(item) >= 0;

        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex) {

            if (array == null) {
                
                throw new System.ArgumentNullException();

            }

            if (arrayIndex < 0 || arrayIndex > array.Length)
                throw new System.ArgumentOutOfRangeException();

            if (array.Length - arrayIndex < this.Count)
                throw new System.ArgumentException("Array too small");

            for (var i = 0; i < this.Count; ++i) {

                array[i + arrayIndex] = new KeyValuePair<K, V>(this.keys[i], this.values[i]);

            }

        }

        public void RemoveAll(System.Func<KeyValuePair<K, V>, bool> predicate) {

            for (var i = this.Count - 1; i >= 0; --i) {
                
                if (predicate(this.GetAt(i)) == false) continue;

                this.RemoveAt(i);

            }

        }

        public K GetKeyAt(int index) {

            return this.keys[index];

        }

        public void GetKeyAt(int index, out K key) {

            key = this.keys[index];

        }

        public V GetValueAt(int index) {

            return this.values[index];

        }

        public void GetValueAt(int index, out V value) {

            value = this.values[index];

        }

        public void GetAt(int index, out KeyValuePair<K, V> keyValue) {

            keyValue = new KeyValuePair<K, V>(this.keys[index], this.values[index]);

        }

        public KeyValuePair<K, V> GetAt(int index) {

            return new KeyValuePair<K, V>(this.keys[index], this.values[index]);

        }

        public int IndexOf(KeyValuePair<K, V> item) {

            for (var i = 0; i < this.keys.Count; ++i) {

                if (this.keyEqualityComparer.Equals(this.keys[i], item.Key) == false) continue;
                if (EqualityComparer<V>.Default.Equals(this.values[i], item.Value) == false) continue;

                return i;

            }

            return -1;

        }

        public int IndexOf(K key) {

            for (var i = 0; i < this.keys.Count; ++i) {

                if (this.keyEqualityComparer.Equals(this.keys[i], key) == false) continue;

                return i;

            }

            return -1;

        }

        public bool Remove(KeyValuePair<K, V> item) {

            var index = this.IndexOf(item);
            if (index < 0) return false;

            this.keys.RemoveAt(index);
            this.values.RemoveAt(index);

            return true;

        }

        int ICollection<KeyValuePair<K, V>>.Count { get { return this.Count; } }
        bool ICollection<KeyValuePair<K, V>>.IsReadOnly { get { return false; } }

        void IDictionary.Clear() {

            this.Clear();

        }

        public bool Contains(object key) {

            return this.IndexOf((K)key) >= 0;

        }

        public Enumerator GetEnumerator() {

            return new Enumerator(this);

        }

        IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator() {

            return this.GetEnumerator();

        }

        IDictionaryEnumerator IDictionary.GetEnumerator() {

            return this.GetEnumerator();

        }

        public void Remove(object key) {

            var index = this.IndexOf((K)key);

            if (index < 0) return;

            this.keys.RemoveAt(index);
            this.values.RemoveAt(index);

        }

        public void RemoveAt(int index) {

            this.keys.RemoveAt(index);
            this.values.RemoveAt(index);

        }

        public bool IsFixedSize { get { return false; } }
        bool IDictionary.IsReadOnly { get { return false; } }

        object IDictionary.this[object key] {
            get { return this.GetByKey((K)key); }
            set { this.SetByKey((K)key, (V)value); }
        }

        public void Add(K key, V value) {

            var index = this.IndexOf(key);
            if (index >= 0) {
                
				throw new System.DuplicateWaitObjectException();

            }

            this.Insert(key, value);

        }

        private void Insert(K key, V value) {

            this.keys.Add(key);
            this.values.Add(value);

        }

        public bool ContainsKey(K key) {

            return this.IndexOf(key) >= 0;

        }

        public bool Remove(K key) {

            var index = this.IndexOf(key);

            if (index < 0) return false;

            this.keys.RemoveAt(index);
            this.values.RemoveAt(index);

            return true;

        }

        public bool TryGetValue(K key, out V value) {

            var index = this.IndexOf(key);
            if (index < 0) {

                value = default(V);
                return false;

            }

            value = this.values[index];

            return true;

        }

        private V GetByKey(K key) {

            var index = this.IndexOf(key);
            if (index < 0) {

                throw new System.ArgumentException();

            }

            return this.values[index];

        }

        private void SetByKey(K key, V value) {

            var index = this.IndexOf(key);
            if (index < 0) {

                this.Insert(key, value);

                return;

            }

            this.values[index] = value;

        }

        V IDictionary<K, V>.this[K key] {

            get { return this.GetByKey(key); }
            set { this.SetByKey(key, value); }

        }

        public V this[K key]
        {

            get { return this.GetByKey(key); }
            set { this.SetByKey(key, value); }

        }

        public List<K> Keys { get { return this.keys; } }
        public List<V> Values { get { return this.values; } }

        ICollection IDictionary.Keys { get { return this.Keys; } }
        ICollection IDictionary.Values { get { return this.Values; } }
        
        ICollection<K> IDictionary<K, V>.Keys { get { return this.Keys; } }
        ICollection<V> IDictionary<K, V>.Values { get { return this.Values; } }

        System.Collections.IEnumerator IEnumerable.GetEnumerator() {

            return this.GetEnumerator();

        }

        public void CopyTo(System.Array array, int index) {

            if (array == null) {

                throw new System.ArgumentNullException();

            }

            if (index < 0 || index > array.Length)
                throw new System.ArgumentOutOfRangeException();

            if (array.Length - index < this.Count)
                throw new System.ArgumentException("Array too small");

            for (var i = 0; i < this.Count; ++i) {

                array.SetValue(new KeyValuePair<K, V>(this.keys[i], this.values[i]), i + index);

            }

        }

        int ICollection.Count { get { return this.Count; } }
        public bool IsSynchronized { get { return false; } }

        public object SyncRoot {
            get {

                if (this.syncRoot == null)
                    Interlocked.CompareExchange<object>(ref this.syncRoot, new object(), (object)null);

                return this.syncRoot;

            }
        }

        public int Count { get { return this.keys.Count; } }

        public void Clear() {

            this.keys.Clear();
            this.values.Clear();

        }

    }

}