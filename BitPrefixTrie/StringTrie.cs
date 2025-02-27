﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitPrefixTrie
{
    public class StringTrie<TValue> : IDictionary<string, TValue>
    {
        private TrieItem<TValue> _root = new TrieItem<TValue>(Bits.Empty);
        private static readonly Encoding _encoding = Encoding.UTF8;

        public void Add(string key, TValue value)
        {
            _root.AddItem(GetBits(key), value);
            Count++;
        }

        public bool ContainsKey(string key)
        {
            var foundNode = FindNodeForKey(key);
            return foundNode != null;
        }

        private TrieItem<TValue> FindNodeForKey(string key)
        {
            return _root.Find(GetBits(key));
        }

        private static Bits GetBits(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return new Bits(_encoding.GetBytes(key));
        }

        public bool Remove(string key)
        {
            var item = _root.Find(GetBits(key));
            if (item == null)
                return false;

            var result = item.HasValue;
            item.HasValue = false;
            return result;
        }

        public bool TryGetValue(string key, out TValue value)
        {
            var foundNode = _root.Find(GetBits(key));
            if (foundNode != null && foundNode.HasValue)
            {
                value = foundNode.Value;
                return true;
            }

            value = default;
            return false;
        }

        public TValue this[string key]
        {
            get
            {
                if (!TryGetValue(key, out TValue value))
                    throw new KeyNotFoundException($"not found: {key.Substring(10)}{(key.Length > 10 ? "..." : "")}");
                return value;
            }
            set => Add(key, value);
        }

        public ICollection<string> Keys => this.Select(kv => kv.Key).ToList().AsReadOnly();
        public ICollection<TValue> Values => this.Select(kv => kv.Value).ToList().AsReadOnly();

        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            foreach (var item in _root)
            {
                var key = _encoding.GetString(item.Key.AsBytes().ToArray());
                yield return new KeyValuePair<string, TValue>(key, item.Value);
            }
        }

        public override string ToString()
        {
            return string.Join("\n", _root.GetDescription());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _root = new TrieItem<TValue>(Bits.Empty);
        }

        public bool Contains(KeyValuePair<string, TValue> item)
        {
            return TryGetValue(item.Key, out var val) && Equals(val, item.Value);
        }

        /// <summary>Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="array" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex" /> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.</exception>
        public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length - arrayIndex < Count)
                throw new ArgumentException("will not fit");
            foreach (var kv in this)
            {
                array[arrayIndex++] = kv;
            }
        }


        /// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, <see langword="false" />. This method also returns <see langword="false" /> if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        public bool Remove(KeyValuePair<string, TValue> item)
        {
            return Contains(item) && Remove(item.Key);
        }

        public int Count { get; private set; }
        public bool IsReadOnly => false;
    }
}