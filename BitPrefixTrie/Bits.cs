using System;
using System.Collections;
using System.Collections.Generic;

namespace BitPrefixTrie
{
    public struct Bits : IEnumerable<bool>
    {
        internal static readonly Bits Empty = new Bits(new Byte[0]);
        private readonly byte[] _fullBytes;
        private readonly byte _partialBitCount;
        private readonly byte _partialByte;
        public int Count => _fullBytes.Length * 8 + _partialBitCount;

        public Bits(byte[] bits)
        {
            _fullBytes = bits;
            _partialBitCount = _partialByte = 0;
        }

        public Bits(IEnumerable<bool> bits)
        {
            byte partialByte = 0;
            byte partialCount = 0;
            var fullBytes = new List<byte>();
            foreach (var bit in bits)
            {
                if (bit)
                    partialByte |= (byte)(1 << partialCount);
                partialCount++;
                if (partialCount == 8)
                {
                    fullBytes.Add(partialByte);
                    partialCount = 0;
                    partialByte = 0;
                }
            }
            _fullBytes = fullBytes.ToArray();
            _partialBitCount = partialCount;
            _partialByte = partialByte;
        }

        internal IEnumerable<byte> AsBytes()
        {
            if (_partialBitCount != 0) throw new InvalidOperationException();
            return _fullBytes;
        }

        public IEnumerator<bool> GetEnumerator()
        {
            foreach (var b in _fullBytes)
            {
                foreach (var bit in GetBits(b, 8))
                    yield return bit;
            }
            foreach (var bit in GetBits(_partialByte, _partialBitCount))
                yield return bit;
        }

        private IEnumerable<bool> GetBits(byte b, int count)
        {
            for (int i = 0; i < count; i++)
                yield return (b & (1 << i)) != 0;
        }
        public Bits Common(IEnumerable<bool> enumerable)
        {
            return new Bits(CommonInternal(enumerable));
        }

        private IEnumerable<bool> CommonInternal(IEnumerable<bool> other)
        {
            using var otherbit = other.GetEnumerator();
            foreach (var mybit in this)
            {
                if (otherbit.MoveNext() && mybit == otherbit.Current)
                {
                    yield return mybit;
                }
                else
                {
                    yield break;
                }
            }

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
