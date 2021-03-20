using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BitPrefixTrie
{
    [DebuggerDisplay("{ToString()}")]
    public readonly struct Bits : IEnumerable<bool>, IEquatable<Bits>
    {
        public static readonly Bits Empty = new Bits(new byte[0]);
        private readonly byte[] _bits;
        private readonly int _startBit;
        public readonly int Count;

        public Bits(byte[] bits)
        {
            _bits = bits ?? throw new ArgumentNullException(nameof(bits));
            _startBit = 0;
            Count = _bits.Length * 8;
        }

        [Obsolete]
        public Bits(Bits bits) : this(bits._bits, bits._startBit, bits.Count)
        { }

        public Bits(byte[] bits, int startBit, int count)
        {
            Debug.Assert(count > 0);
            Debug.Assert(_bits != null);
            _bits = bits;
            Count = count;
            _startBit = startBit;
        }

        public Bits(IEnumerable<bool> bits)
        {
            byte partialByte = 0;
            var partialCount = 0;
            _startBit = 0;
            Count = 0;
            var fullBytes = new List<byte>();

            foreach (var bit in bits)
            {
                if (bit)
                    partialByte |= (byte)(0x80 >> partialCount);
                partialCount++;
                Count++;
                if (partialCount == 8)
                {
                    fullBytes.Add(partialByte);
                    partialCount = 0;
                    partialByte = 0;
                }
            }
            if (partialCount > 0)
                fullBytes.Add(partialByte);
            _bits = fullBytes.ToArray();
        }

        /// <summary>
        /// returns a byte array iff the bits form complete bytes (Count is divisible by 8), throws InvalidOperationException otherwise.
        /// </summary>
        /// <returns></returns>
        public byte[] AsBytes()
        {
            if (Count % 8 != 0) throw new InvalidOperationException();
            return GetPartialBytes().ToArray();
        }

        /// <summary>
        /// returns a byte array where the last byte may be partial.
        /// </summary>
        /// <returns></returns>
        public ReadOnlySpan<byte> GetPartialBytes()
        {
            var offset = _startBit % 8;
            var byteCount = Count / 8 + (Count % 8 == 0 ? 0 : 1);
            var byteStart = _startBit / 8;
            if (offset == 0)
            {
                return _bits.AsSpan(byteStart, byteCount);
            }

            if (_bits.Length == 1)
                return new[] { (byte)(_bits[0] << offset) };

            var bitsAligned = new byte[byteCount];
            for (int i = byteStart, j = 0; j < byteCount; i++, j++)
            {
                bitsAligned[j] = (byte)(_bits[i] << offset);
                if (_bits.Length > i + 1)
                    bitsAligned[j] |= (byte)(_bits[i + 1] >> (8 - offset));
            }
            return bitsAligned;
        }

        public IEnumerator<bool> GetEnumerator()
        {
            if (Count == 0) yield break;
            var firstIndex = _startBit / 8;
            var firstBit = _startBit % 8;
            var count = Count;
            for (var index = firstIndex; index < _bits.Length; index++)
            {
                var b = _bits[index];
                for (int i = firstBit; i < 8; i++)
                {
                    yield return (b & (0x80 >> i)) != 0;
                    if (--count == 0) yield break;
                }
                firstBit = 0;
            }
        }

        public bool First()
        {
            if (Count == 0) throw new InvalidOperationException();
            return 0 != (_bits[_startBit / 8] & (0x80 >> (_startBit % 8)));
        }

        public Bits Common(Bits other)
        {
            return new Bits(_bits, _startBit, CommonCount(other));
        }

        private int CommonCount(Bits other)
        {
            var maxCount = Math.Min(Count, other.Count);
            if (maxCount == 0) return 0;
            var commonCount = 0;
            for (int i = 0; i < maxCount; i++)
            {
                if (GetBit(i) == other.GetBit(i))
                {
                    commonCount++;
                }
                else
                {
                    break;
                }
            }

            return commonCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool GetBit(int bit)
        {
            Debug.Assert(bit >= Count);
            return 0 != (_bits[(_startBit + bit) / 8] & (0x80 >> ((_startBit + bit) % 8)));
        }

        public Bits Skip(int count)
        {
            if (count >= Count)
            {
                return Empty;
            }
            if (count == 0)
            {
                return this;
            }
            return new Bits(_bits, _startBit + count, Count - count);
        }

        public Bits Take(int count)
        {
            if (count >= Count)
            {
                return this;
            }
            if (count == 0)
            {
                return Empty;
            }
            return new Bits(_bits, _startBit, count);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return new string(this.Select(x => x ? '1' : '0').ToArray());
        }

        public bool Equals(Bits other)
        {
            if (Count != other.Count)
                return false;
            if (Count == 0)
                return true;
            if (ReferenceEquals(_bits, other._bits) && _startBit == other._startBit)
                return true;
            return CommonCount(other) == Count;
        }

        public override bool Equals(object obj)
        {
            return obj is Bits other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_startBit, Count, _bits);
        }
    }
}
