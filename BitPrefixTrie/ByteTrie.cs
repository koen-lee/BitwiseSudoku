using System;
using System.Collections;
using System.Collections.Generic;

namespace BitPrefixTrie
{
    public class ByteTrie<TValue> : IEnumerable<KeyValuePair<IEnumerable<byte>, TValue>>
    {
        private readonly TrieItem<TValue> root = new TrieItem<TValue>(Bits.Empty);
        public void AddItem(ReadOnlySpan<byte> key, TValue value)
        {
            root.AddItem(new Bits(key.ToArray()), value);
        }

        public IEnumerator<KeyValuePair<IEnumerable<byte>, TValue>> GetEnumerator()
        {
            foreach (var item in root)
            {
                var key = item.Key.AsBytes();
                yield return new KeyValuePair<IEnumerable<byte>, TValue>(key, item.Value);
            }
        }

        public override string ToString()
        {
            return string.Join("\n", root.GetDescription());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
