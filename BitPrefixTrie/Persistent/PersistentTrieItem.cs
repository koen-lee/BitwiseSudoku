﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

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
    [DebuggerDisplay("{Prefix} {Value}")]
    public class PersistentTrieItem : IEnumerable<KeyValuePair<Bits, string>>
    {
        private readonly Stream _storage;
        private readonly uint _offset;

        public readonly Bits Prefix;

        private Func<PersistentTrieItem> False;
        private uint FalseCount;
        private Func<PersistentTrieItem> True;
        private uint TrueCount;
        public readonly string Value;
        public bool HasValue;

        public PersistentTrieItem(Stream storage, uint offset)
        {
            _offset = offset;
            _storage = storage;
            if (offset == 0 && storage.Length == 0)
            {
                Prefix = Bits.Empty;
                Persist();
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
                var trueOffset = BitConverter.ToUInt32(data, 1);
                TrueCount = BitConverter.ToUInt32(data, 5);
                var falseOffset = BitConverter.ToUInt32(data, 9);
                FalseCount = BitConverter.ToUInt32(data, 13);
                var prefixLength = BitConverter.ToUInt16(data, 17);
                var length = (prefixLength / 8) + (prefixLength % 8 == 0 ? 0 : 1);
                var prefixBytes = ReadArray(length);
                Prefix = new Bits(new Bits(prefixBytes).Take(prefixLength));
                if (HasValue)
                {
                    var valueLength = BitConverter.ToUInt16(ReadArray(2), 0);
                    Value = Encoding.UTF8.GetString(ReadArray(valueLength));
                }
                True = GetChildFactory(trueOffset);
                False = GetChildFactory(falseOffset);
            }
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

        private void Persist()
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

            List<byte> buffer = new List<byte>(20);

            buffer.AddRange(new[] { (byte)(HasValue ? 0xff : 0x00) });
            buffer.AddRange(BitConverter.GetBytes(True?.Invoke()._offset ?? 0));
            buffer.AddRange(BitConverter.GetBytes(TrueCount));
            buffer.AddRange(BitConverter.GetBytes(False?.Invoke()._offset ?? 0));
            buffer.AddRange(BitConverter.GetBytes(FalseCount));
            buffer.AddRange(BitConverter.GetBytes((ushort)Prefix.Count));
            buffer.AddRange(Prefix.GetPartialBytes().ToArray());
            if (HasValue)
            {
                var valueBytes = Encoding.UTF8.GetBytes(Value);
                buffer.AddRange(BitConverter.GetBytes((ushort)(valueBytes.Length)));
                buffer.AddRange(valueBytes);
            }

            _storage.Seek(_offset, SeekOrigin.Begin);
            _storage.Write(buffer.ToArray());
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
            Persist();
        }

        private PersistentTrieItem(Stream storage, Bits prefix, string value)
            : this(storage)
        {
            Value = value;
            HasValue = true;
            Prefix = prefix;
            Persist();
        }

        private PersistentTrieItem(Stream storage, Bits prefix, bool hasValue, string value,
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
            Persist();
        }

        internal void AddItem(Bits newPrefix, string value)
        {
            var common = newPrefix.Common(Prefix);
            if (common.Count == newPrefix.Count)
            {
                throw new InvalidOperationException("either a duplicate key or we need to mutate ourselves, both should be caught in AddToChild");
            }

            if (newPrefix.Skip(Prefix.Count).First())
            {
                AddToChild(ref True, ref TrueCount, newPrefix.Skip(Prefix.Count + 1), value);
            }
            else
            {
                AddToChild(ref False, ref FalseCount, newPrefix.Skip(Prefix.Count + 1), value);
            }
        }

        private void AddToChild(ref Func<PersistentTrieItem> child, ref uint childCount, IEnumerable<bool> enumerable,
            string value)
        {
            var bits = new Bits(enumerable);
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
            Persist();
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
                new Bits(oldChild.Prefix.Skip(Prefix.Count + 1)),
                oldChild.HasValue,
                oldChild.Value,
                oldChild.True,
                oldChild.False, oldChild.TrueCount, oldChild.FalseCount));
            if (discriminator)
            {
                if (True != null) throw new InvalidOperationException("Child 1 already set");
                True = grandchild;
                TrueCount = (uint)(oldChild.TrueCount + oldChild.FalseCount + (oldChild.HasValue ? 1 : 0));
            }
            else
            {
                if (False != null) throw new InvalidOperationException("Child 0 already set");
                False = grandchild;
                FalseCount = (uint)(oldChild.TrueCount + oldChild.FalseCount + (oldChild.HasValue ? 1 : 0));
            }
            Persist();
        }

        public IEnumerator<KeyValuePair<Bits, string>> GetEnumerator()
        {
            if (HasValue)
                yield return new KeyValuePair<Bits, string>(Prefix, Value);
            if (False != null)
                foreach (var item in False())
                    yield return new KeyValuePair<Bits, string>(new Bits(Prefix.Append(false).Concat(item.Key)), item.Value);
            if (True != null)
                foreach (var item in True())
                    yield return new KeyValuePair<Bits, string>(new Bits(Prefix.Append(true).Concat(item.Key)), item.Value);
        }

        public IEnumerable<KeyValuePair<Bits, string>> Skip(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (count == 0 && HasValue)
                yield return new KeyValuePair<Bits, string>(Prefix, Value);
            var leftToSkip = count;
            if (FalseCount > count)
            {
                foreach (var item in False().Skip(count))
                    yield return new KeyValuePair<Bits, string>(new Bits(Prefix.Append(false).Concat(item.Key)), item.Value);
                leftToSkip = 0;
            }
            else
            {
                leftToSkip -= (int)FalseCount;
            }
            if (True != null)
                foreach (var item in True().Skip(leftToSkip))
                    yield return new KeyValuePair<Bits, string>(new Bits(Prefix.Append(true).Concat(item.Key)), item.Value);
        }

        public IEnumerable<string> GetDescription()
        {
            if (Prefix.Count > 0 || HasValue) yield return $"{Prefix} {Value}";
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
                return True?.Invoke().Find(new Bits(bits.Skip(Prefix.Count + 1)));
            }
            else
            {
                return False?.Invoke().Find(new Bits(bits.Skip(Prefix.Count + 1)));
            }
        }

        public bool Remove(Bits bits)
        {
            if (bits.Count <= Prefix.Count)
            {
                if (bits.Equals(Prefix) && HasValue)
                {
                    HasValue = false;
                    Persist();
                    return true;
                }
                else
                    return false;
            }
            if (bits.Skip(Prefix.Count).First())
            {
                if (True == null || !True().Remove(new Bits(bits.Skip(Prefix.Count + 1))))
                    return false;
                TrueCount--;
                return true;
            }
            else
            {
                if (False == null || !False().Remove(new Bits(bits.Skip(Prefix.Count + 1))))
                    return false;
                FalseCount--;
                return true;
            }
        }
    }
}
