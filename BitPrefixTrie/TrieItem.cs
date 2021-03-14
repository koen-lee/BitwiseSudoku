using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BitPrefixTrie
{

    public class TrieItem<T> : IEnumerable<KeyValuePair<Bits, T>>
    {
        public readonly Bits Prefix;

        private TrieItem<T> False;
        private TrieItem<T> True;
        public T Value { get; private set; }
        private bool HasValue { get; set; }

        public TrieItem(Bits prefix)
        {
            HasValue = false;
            Prefix = prefix;
        }
        public TrieItem(Bits prefix, T value)
        {
            Value = value;
            HasValue = true;
            Prefix = prefix;
        }

        public TrieItem(Bits prefix, bool hasValue, T value, TrieItem<T> @true, TrieItem<T> @false)
        {
            Prefix = prefix;
            HasValue = hasValue;
            Value = value;
            True = @true;
            False = @false;
        }

        internal void AddItem(Bits prefix, T value)
        {
            if (prefix.Any())
            {
                if (prefix.First())
                {
                    AddToChild(ref True, prefix.Skip(1), value);
                }
                else
                {
                    AddToChild(ref False, prefix.Skip(1), value);
                }
            }
            else
            {
                if (HasValue)
                    throw new ArgumentException("Duplicate key");
                else
                {
                    HasValue = true;
                    Value = value;
                }
            }
        }

        private void AddToChild(ref TrieItem<T> child, IEnumerable<bool> enumerable, T value)
        {
            if (child == null)
                child = new TrieItem<T>(new Bits(enumerable), value);
            else
            {

                var commonBits = child.Prefix.Common(enumerable);
                if (child.Prefix.Count == commonBits.Count)
                {
                    // Set value, if not already set
                    child.AddItem(new Bits(enumerable), value);
                }
                else
                {
                    //split subtrie along the common prefix
                    var oldChild = child;
                    child = new TrieItem<T>(commonBits);
                    child.AddItem(new Bits(enumerable.Skip(commonBits.Count)), value);
                    var discriminator = oldChild.Prefix.Skip(commonBits.Count).First();
                    var splitChild = new TrieItem<T>(new Bits(oldChild.Prefix.Skip(commonBits.Count + 1)), oldChild.HasValue, oldChild.Value, oldChild.True, oldChild.False);
                    child.AddChild(discriminator, splitChild);
                }
            }

        }

        private void AddChild(bool discriminator, TrieItem<T> splitChild)
        {
            if(discriminator)
            {
                if (True != null) throw new InvalidOperationException("Child 1 already set");
                True = splitChild;
            } else
            {
                if( False != null) throw new InvalidOperationException("Child 0 already set");
                False = splitChild;
            }
        }

        public IEnumerator<KeyValuePair<Bits, T>> GetEnumerator()
        {
            if (HasValue)
                yield return new KeyValuePair<Bits, T>(Prefix, Value);
            if (True != null)
                foreach (var item in True)
                    yield return new KeyValuePair<Bits, T>(new Bits(Prefix.Append(true).Concat(item.Key)), item.Value);
            if (False != null)
                foreach (var item in False)
                    yield return new KeyValuePair<Bits, T>(new Bits(Prefix.Append(false).Concat(item.Key)), item.Value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
