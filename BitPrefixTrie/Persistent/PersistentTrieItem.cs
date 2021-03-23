using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using static System.BitConverter;
using KVP = System.Collections.Generic.KeyValuePair<System.Collections.Generic.IEnumerable<bool>, byte[]>;

namespace BitPrefixTrie.Persistent
{

    /// <summary>
    /// Stream format:
    /// [byte] hasValue flag
    /// [long] offset of True
    /// [long] offset of False
    /// [ushort] prefix length in bits
    /// [byte[ceiling(prefix/8)]] prefix bits
    /// hasValue? [ushort] size of value in bytes
    /// hasValue? [byte[size]] value (utf-8)
    /// </summary>
    [DebuggerDisplay("{Prefix} {System.Text.Encoding.UTF8.GetString(Value)}")]
    public class PersistentTrieItem : IEnumerable<KVP>
    {
        private readonly Stream _storage;
        private readonly uint _offset;

        public readonly Bits Prefix;

        private Func<PersistentTrieItem> False;
        private uint FalseCount;
        private Func<PersistentTrieItem> True;
        private uint TrueCount;
        public readonly byte[] Value;
        public bool HasValue;
        private bool _isDirty;
        private static readonly IEnumerable<KVP> EmptyResult = Enumerable.Empty<KVP>();
        public long Count => FalseCount + TrueCount + (HasValue ? 1 : 0);

        public PersistentTrieItem(Stream storage, uint offset)
        {
            _offset = offset;
            _storage = storage;
            if (offset == 0 && storage.Length == 0)
            {
                Prefix = Bits.Empty;
                PersistNewItem();
            }
            else
            {
                // Stream format:
                // [1:byte] hasValue flag
                // [4:uint] offset of True
                // [4:uint] True value count
                // [4:uint] offset of False
                // [4:uint] False value count
                // [2:ushort] prefix length in bits
                // [ceiling(prefix/8):byte[]] prefix bits
                // hasValue? [2:ushort] size of value in bytes
                // hasValue? [size:byte[]] value (utf-8)
                _storage.Seek(_offset, SeekOrigin.Begin);
                var data = ReadArray(1 + 8 + 8 + 2);
                if (data[0] == 0)
                    HasValue = false;
                else if (data[0] == 0xff)
                    HasValue = true;
                else
                {
                    throw new InvalidDataException("this is no HasValue flag");
                }
                var trueOffset = ToUInt32(data, 1);
                TrueCount = ToUInt32(data, 5);
                var falseOffset = ToUInt32(data, 9);
                FalseCount = ToUInt32(data, 13);
                var prefixLength = ToUInt16(data, 17);
                var length = (prefixLength / 8) + (prefixLength % 8 == 0 ? 0 : 1);
                var prefixBytes = ReadArray(length);
                Prefix = new Bits(prefixBytes).Take(prefixLength);
                if (HasValue)
                {
                    var valueLength = ToUInt16(ReadArray(2), 0);
                    Value = ReadArray(valueLength);
                }
                True = GetChildFactory(trueOffset);
                False = GetChildFactory(falseOffset);
            }
        }

        private PersistentTrieItem(Stream storage)
        {
            _storage = storage;
            _offset = (uint)_storage.Length;
        }

        public PersistentTrieItem(Stream storage, Bits prefix)
            : this(storage)
        {
            HasValue = false;
            Prefix = prefix;
            PersistNewItem();
        }

        private PersistentTrieItem(Stream storage, Bits prefix, byte[] value)
            : this(storage)
        {
            Value = value;
            HasValue = true;
            Prefix = prefix;
            PersistNewItem();
        }

        private PersistentTrieItem(Stream storage, Bits prefix, bool hasValue, byte[] value,
            Func<PersistentTrieItem> @true, Func<PersistentTrieItem> @false, uint trueCount, uint falseCount)
            : this(storage)
        {
            Prefix = prefix;
            HasValue = hasValue;
            Value = value;
            True = @true;
            TrueCount = trueCount;
            False = @false;
            FalseCount = falseCount;
            PersistNewItem();
        }

        private Func<PersistentTrieItem> GetChildFactory(uint offset)
        {
            if (offset == 0)
                return null;
            Lazy<PersistentTrieItem> item = new Lazy<PersistentTrieItem>(
                () => new PersistentTrieItem(_storage, offset));
            return () => item.Value;
        }

        private byte[] ReadArray(int length)
        {
            var data = new byte[length];
            var read = 0;
            while (read < data.Length)
            {
                var readFromStream = _storage.Read(data, read, data.Length - read);
                if (readFromStream <= 0) throw new EndOfStreamException("but more bytes needed; data corruption?");
                read += readFromStream;
            }
            return data;
        }

        private void MarkDirty()
        {
            _isDirty = true;
        }

        private void PersistNewItem()
        {
            MarkDirty();
            Persist();
            PersistImmutable();
        }

        /// <summary>
        /// Persists the immutable and variable-size members: prefix and value.
        /// </summary>
        private void PersistImmutable()
        {
            List<byte> buffer = new List<byte>(20);

            buffer.AddRange(GetBytes((ushort)Prefix.Count));
            buffer.AddRange(Prefix.GetPartialBytes().ToArray());
            if (HasValue)
            {
                buffer.AddRange(GetBytes((ushort)Value.Length));
                buffer.AddRange(Value);
            }
            _storage.Seek(_offset + 1 + 8 + 8, SeekOrigin.Begin);
            _storage.Write(buffer.ToArray());

        }

        /// <summary>
        /// Persists the mutable parts of the item, which is everything except the prefix and value.
        /// </summary>
        public void Persist()
        {
            var itemsToPersist = new List<KeyValuePair<uint, Action>>(3);
            itemsToPersist.Add(new KeyValuePair<uint, Action>(_offset, PersistSelf));
            if (False != null)
            {
                var falseItem = False();
                itemsToPersist.Add(new KeyValuePair<uint, Action>(falseItem._offset, falseItem.Persist));
            }
            if (True != null)
            {
                var trueItem = True();
                itemsToPersist.Add(new KeyValuePair<uint, Action>(trueItem._offset, trueItem.Persist));
            }

            // unrolled sort
            if (itemsToPersist.Count == 1)
            {
                //this must be me
                PersistSelf();
            }
            else if (itemsToPersist.Count == 2)
            {
                if (itemsToPersist[0].Key < itemsToPersist[1].Key)
                {
                    itemsToPersist[0].Value();
                    itemsToPersist[1].Value();
                }
                else
                {
                    itemsToPersist[1].Value();
                    itemsToPersist[0].Value();
                }
            }
            else // 3 items
            {
                void SwapIfGreater(int index1, int index2)
                {
                    if (itemsToPersist[index1].Key > itemsToPersist[index2].Key)
                    {
                        var temp = itemsToPersist[index1];
                        itemsToPersist[index1] = itemsToPersist[index2];
                        itemsToPersist[index2] = temp;
                    }
                }
                SwapIfGreater(0, 1);
                SwapIfGreater(0, 2);
                SwapIfGreater(1, 2);

                itemsToPersist[0].Value();
                itemsToPersist[1].Value();
                itemsToPersist[2].Value();
            }
        }
        private void PersistSelf()
        {
            if (!_isDirty) return;
            // Stream format:
            // [1:byte] hasValue flag
            // [4:uint] offset of True
            // [4:uint] True value count
            // [4:uint] offset of False
            // [4:uint] False value count
            // [2:ushort] prefix length in bits
            // [ceiling(prefix/8):byte[]] prefix bits
            // hasValue? [2:ushort] size of value in bytes
            // hasValue? [size:byte[]] value (utf-8)

            byte[] buffer = new byte[1 + 8 + 8];

            buffer[0] = (byte)(HasValue ? 0xff : 0x00);
            if (!
               (TryWriteBytes(buffer.AsSpan(1), True?.Invoke()._offset ?? 0) &&
                TryWriteBytes(buffer.AsSpan(5), TrueCount) &&
                TryWriteBytes(buffer.AsSpan(9), False?.Invoke()._offset ?? 0) &&
                TryWriteBytes(buffer.AsSpan(13), FalseCount))
            ) throw new InvalidOperationException("Writing buffer failed");
            _storage.Seek(_offset, SeekOrigin.Begin);
            _storage.Write(buffer);
            _isDirty = false;
        }

        public void AddItem(Bits newPrefix, byte[] value)
        {
            Debug.Assert(newPrefix.Common(Prefix).Count != newPrefix.Count);
            if (newPrefix.GetBit(Prefix.Count))
            {
                AddToChild(ref True, ref TrueCount, newPrefix.Skip(Prefix.Count + 1), value);
            }
            else
            {
                AddToChild(ref False, ref FalseCount, newPrefix.Skip(Prefix.Count + 1), value);
            }
        }

        private void AddToChild(ref Func<PersistentTrieItem> child, ref uint childCount, Bits bits,
            byte[] value)
        {
            var theChild = child?.Invoke();
            if (theChild == null)
            {
                child = NewChild(new PersistentTrieItem(_storage, bits, value));
            }
            else
            {
                var commonCount = theChild.Prefix.CommonCount(bits);
                if (theChild.Prefix.Count == commonCount)
                {
                    if (theChild.Prefix.Count == bits.Count)
                    {
                        // Set value, if not already set
                        if (theChild.HasValue)
                            throw new ArgumentException("Duplicate key");

                        var oldChild = theChild;
                        child = NewChild(new PersistentTrieItem(_storage, oldChild.Prefix, true, value,
                            oldChild.True,
                            oldChild.False, oldChild.TrueCount, oldChild.FalseCount));
                    }
                    else
                        theChild.AddItem(bits, value);
                }
                else
                {
                    //split sub trie along the common prefix
                    if (commonCount == bits.Count)
                    {
                        child = NewChild(new PersistentTrieItem(_storage, bits, value));
                    }
                    else
                    {
                        child = NewChild(new PersistentTrieItem(_storage, bits.Take(commonCount)));
                        child().AddItem(bits, value);
                    }
                    child().MakeGrandchild(theChild);
                }
            }
            childCount++;
            MarkDirty();
        }

        private Func<PersistentTrieItem> NewChild(PersistentTrieItem child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            return () => child;
        }

        private void MakeGrandchild(PersistentTrieItem oldChild)
        {
            var discriminator = oldChild.Prefix.Skip(Prefix.Count).First();
            var grandchild = NewChild(new PersistentTrieItem(_storage,
                oldChild.Prefix.Skip(Prefix.Count + 1),
                oldChild.HasValue,
                oldChild.Value,
                oldChild.True,
                oldChild.False, oldChild.TrueCount, oldChild.FalseCount));
            if (discriminator)
            {
                if (True != null) throw new InvalidOperationException("Child 1 already set");
                True = grandchild;
                TrueCount = (uint)oldChild.Count;
            }
            else
            {
                if (False != null) throw new InvalidOperationException("Child 0 already set");
                False = grandchild;
                FalseCount = (uint)oldChild.Count;
            }
            MarkDirty();
        }

        public IEnumerator<KVP> GetEnumerator()
        {
            if (HasValue)
                yield return new KVP(Prefix, Value);
            if (False != null)
                foreach (var item in False())
                    yield return new KVP(Prefix.Append(false).Concat(item.Key), item.Value);
            if (True != null)
                foreach (var item in True())
                    yield return new KVP(Prefix.Append(true).Concat(item.Key), item.Value);
        }

        public IEnumerable<KVP> Skip(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (count == 0 && HasValue)
                yield return new KVP(Prefix, Value);
            var leftToSkip = count;
            if (FalseCount > count)
            {
                foreach (KVP item in EnumerateFalse(x => x.Skip(count)))
                    yield return item;
                leftToSkip = 0;
            }
            else
            {
                leftToSkip -= (int)FalseCount;
            }

            foreach (KVP item in EnumerateTrue(x => x.Skip(leftToSkip)))
                yield return item;
        }

        public IEnumerable<string> GetDescription()
        {
            if (Prefix.Count > 0 || HasValue) yield return $"{Prefix} {Encoding.UTF8.GetString(Value ?? new byte[0])}";
            var padding = new string(' ', Prefix.Count);
            if (False != null)
            {
                yield return $"{padding}+0";
                foreach (var item in False().GetDescription())
                    yield return $"{padding}{(True != null ? '|' : ' ')} " + item;
            }
            if (True != null)
            {
                yield return $"{padding}+1";
                foreach (var item in True().GetDescription())
                    yield return $"{padding}  " + item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public PersistentTrieItem Find(Bits bits)
        {
            if (bits.Count <= Prefix.Count)
                return bits.Equals(Prefix) ? this : null;
            if (bits.Skip(Prefix.Count).First())
            {
                return True?.Invoke().Find(bits.Skip(Prefix.Count + 1));
            }
            else
            {
                return False?.Invoke().Find(bits.Skip(Prefix.Count + 1));
            }
        }

        public bool Remove(Bits bits)
        {
            if (bits.Count <= Prefix.Count)
            {
                if (bits.Equals(Prefix) && HasValue)
                {
                    HasValue = false;
                    MarkDirty();
                    return true;
                }
                else
                    return false;
            }
            if (bits.Skip(Prefix.Count).First())
            {
                if (True == null || !True().Remove(bits.Skip(Prefix.Count + 1)))
                    return false;
                TrueCount--;
                MarkDirty();
                return true;
            }
            else
            {
                if (False == null || !False().Remove(bits.Skip(Prefix.Count + 1)))
                    return false;
                FalseCount--;
                MarkDirty();
                return true;
            }
        }

        public IEnumerable<KVP> From(Bits key)
        {
            if (key.Equals(Prefix) || key < Prefix)
            {
                foreach (var item in this)
                    yield return item;
                yield break;
            }

            var commonCount = Prefix.CommonCount(key);
            if (commonCount != Prefix.Count)
                yield break;

            var discriminator = key.GetBit(Prefix.Count);
            if (!discriminator)
            {
                foreach (KVP item in EnumerateFalse(x => x.From(key.Skip(Prefix.Count + 1))))
                    yield return item;
                foreach (KVP item in EnumerateTrue(x => x))
                    yield return item;
            }
            else
            {
                foreach (KVP item in EnumerateTrue(x => x.From(key.Skip(Prefix.Count + 1))))
                    yield return item;
            }
        }

        private IEnumerable<KVP> EnumerateTrue(Func<PersistentTrieItem, IEnumerable<KVP>> selector)
        {
            if (True == null) yield break;
            foreach (var item in selector(True()))
            {
                yield return new KVP(Prefix.Append(true).Concat(item.Key), item.Value);
            }
        }
        private IEnumerable<KVP> EnumerateFalse(Func<PersistentTrieItem, IEnumerable<KVP>> selector)
        {
            if (False == null) yield break;
            foreach (var item in selector(False()))
            {
                yield return new KVP(Prefix.Append(false).Concat(item.Key), item.Value);
            }
        }
    }
}
