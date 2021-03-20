using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using static System.BitConverter;

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
    public class PersistentTrieItem : IEnumerable<KeyValuePair<IEnumerable<bool>, byte[]>>
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
            False?.Invoke().Persist();
            True?.Invoke().Persist();
            _isDirty = false;
        }

        public void AddItem(Bits newPrefix, byte[] value)
        {
            Debug.Assert(newPrefix.Common(Prefix).Count != newPrefix.Count);
            if (newPrefix.Skip(Prefix.Count).First())
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
                var commonBits = theChild.Prefix.Common(bits);
                if (theChild.Prefix.Count == commonBits.Count)
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
                    if (commonBits.Count == bits.Count)
                    {
                        child = NewChild(new PersistentTrieItem(_storage, commonBits, value));
                    }
                    else
                    {
                        child = NewChild(new PersistentTrieItem(_storage, commonBits));
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

        public IEnumerator<KeyValuePair<IEnumerable<bool>, byte[]>> GetEnumerator()
        {
            if (HasValue)
                yield return new KeyValuePair<IEnumerable<bool>, byte[]>(Prefix, Value);
            if (False != null)
                foreach (var item in False())
                    yield return new KeyValuePair<IEnumerable<bool>, byte[]>(Prefix.Append(false).Concat(item.Key), item.Value);
            if (True != null)
                foreach (var item in True())
                    yield return new KeyValuePair<IEnumerable<bool>, byte[]>(Prefix.Append(true).Concat(item.Key), item.Value);
        }

        public IEnumerable<KeyValuePair<IEnumerable<bool>, byte[]>> Skip(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (count == 0 && HasValue)
                yield return new KeyValuePair<IEnumerable<bool>, byte[]>(Prefix, Value);
            var leftToSkip = count;
            if (FalseCount > count)
            {
                foreach (var item in False().Skip(count))
                    yield return new KeyValuePair<IEnumerable<bool>, byte[]>(Prefix.Append(false).Concat(item.Key), item.Value);
                leftToSkip = 0;
            }
            else
            {
                leftToSkip -= (int)FalseCount;
            }
            if (True != null)
                foreach (var item in True().Skip(leftToSkip))
                    yield return new KeyValuePair<IEnumerable<bool>, byte[]>(Prefix.Append(true).Concat(item.Key), item.Value);
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
    }
}
