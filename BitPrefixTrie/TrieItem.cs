using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BitPrefixTrie
{

    [DebuggerDisplay("{Prefix} {Value}")]
    public class TrieItem<T> : IEnumerable<KeyValuePair<Bits, T>>
    {
        public readonly Bits Prefix;

        private TrieItem<T> False;
        private TrieItem<T> True;
        public readonly T Value;
        public bool HasValue;

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

        internal void AddItem(Bits newPrefix, T value)
        {
            var common = newPrefix.Common(Prefix);
            if (common.Count == newPrefix.Count)
            {
                throw new InvalidOperationException("either a duplicate key or we need to mutate ourselves, both should be caught in AddToChild");
            }
            else if (newPrefix.Skip(Prefix.Count).First())
            {
                AddToChild(ref True, newPrefix.Skip(Prefix.Count + 1), value);
            }
            else
            {
                AddToChild(ref False, newPrefix.Skip(Prefix.Count + 1), value);
            }
        }

        private void AddToChild(ref TrieItem<T> child, IEnumerable<bool> enumerable, T value)
        {
            var bits = new Bits(enumerable);
            if (child == null)
                child = new TrieItem<T>(bits, value);
            else
            {
                var commonBits = child.Prefix.Common(bits);
                if (child.Prefix.Count == commonBits.Count)
                {
                    if (child.Prefix.Count == bits.Count)
                    {
                        // Set value, if not already set
                        if (child.HasValue)
                            throw new ArgumentException("Duplicate key");

                        var oldChild = child;
                        child = new TrieItem<T>(oldChild.Prefix, true, value, oldChild.True, oldChild.False);
                    }
                    else
                        child.AddItem(bits, value);
                }
                else
                {
                    //split subtrie along the common prefix
                    var oldChild = child;
                    if (commonBits.Count == bits.Count)
                    {
                        child = new TrieItem<T>(commonBits, value);
                    }
                    else
                    {
                        child = new TrieItem<T>(commonBits);
                        child.AddItem(bits, value);
                    }
                    child.MakeGrandchild(oldChild);
                }
            }
        }

        private void MakeGrandchild(TrieItem<T> oldChild)
        {
            var discriminator = oldChild.Prefix.Skip(Prefix.Count).First();
            var grandchild = new TrieItem<T>(oldChild.Prefix.Skip(Prefix.Count + 1), oldChild.HasValue, oldChild.Value, oldChild.True, oldChild.False);
            if (discriminator)
            {
                if (True != null) throw new InvalidOperationException("Child 1 already set");
                True = grandchild;
            }
            else
            {
                if (False != null) throw new InvalidOperationException("Child 0 already set");
                False = grandchild;
            }
        }

        public IEnumerator<KeyValuePair<Bits, T>> GetEnumerator()
        {
            if (HasValue)
                yield return new KeyValuePair<Bits, T>(Prefix, Value);
            if (False != null)
                foreach (var item in False)
                    yield return new KeyValuePair<Bits, T>(new Bits(Prefix.Append(false).Concat(item.Key)), item.Value);
            if (True != null)
                foreach (var item in True)
                    yield return new KeyValuePair<Bits, T>(new Bits(Prefix.Append(true).Concat(item.Key)), item.Value);
        }

        public IEnumerable<string> GetDescription()
        {
            yield return $"{Prefix} {Value}";
            var padding = new string(' ', (Math.Max(0, Prefix.Count - 3)));
            if (False != null)
            {
                yield return $"{padding} + 0";
                foreach (var item in False.GetDescription())
                    yield return $"{padding} |  " + item;
            }
            if (True != null)
            {
                yield return $"{padding} + 1";
                foreach (var item in True.GetDescription())
                    yield return $"{padding} |  " + item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public TrieItem<T> Find(Bits bits)
        {
            if (bits.Count <= Prefix.Count)
                return bits.Equals(Prefix) ? this : null;
            if (bits.Skip(Prefix.Count).First())
            {
                return True?.Find(bits.Skip(Prefix.Count + 1));
            }
            else
            {
                return False?.Find(bits.Skip(Prefix.Count + 1));
            }
        }
    }
}
