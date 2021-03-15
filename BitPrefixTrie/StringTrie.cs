using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitPrefixTrie
{
    public class StringTrie<TValue> : IEnumerable<KeyValuePair<string, TValue>>
    {
        private readonly TrieItem<TValue> root = new TrieItem<TValue>(Bits.Empty);
        private static readonly Encoding _encoding = Encoding.UTF8;
        public void AddItem(string key, TValue value)
        {
            var bytes = _encoding.GetBytes(key);
            root.AddItem(new Bits(bytes), value);
        }

        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            foreach (var item in root)
            {
                var key = _encoding.GetString(item.Key.AsBytes().ToArray());
                yield return new KeyValuePair<string, TValue>(key, item.Value);
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