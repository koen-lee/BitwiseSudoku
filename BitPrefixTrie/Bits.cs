using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BitPrefixTrie
{
    [DebuggerDisplay("{ToString()}")]
    public readonly struct Bits : IEnumerable<bool>, IEquatable<Bits>
    {
        public static readonly Bits Empty = new Bits(new byte[0]);
        private readonly byte[] _bits;
        private readonly int _startBit;
        private readonly int _stopBit;
        public int Count => _stopBit - _startBit;

        public Bits(byte[] bits)
        {
            _bits = bits ?? throw new ArgumentNullException(nameof(bits));
            _startBit = 0;
            _stopBit = _bits.Length * 8;
        }

        public Bits(Bits bits) : this(bits._bits, bits._startBit, bits._stopBit)
        { }

        public Bits(byte[] bits, int startBit, int stopBit)
        {
            if (stopBit < startBit)
                throw new ArgumentException();
            _bits = bits ?? throw new ArgumentNullException(nameof(bits));
            _stopBit = stopBit;
            _startBit = startBit;
        }

        public Bits(IEnumerable<bool> bits)
        {
            byte partialByte = 0;
            var partialCount = 0;
            _startBit = 0;
            _stopBit = 0;
            var fullBytes = new List<byte>();

            foreach (var bit in bits)
            {
                if (bit)
                    partialByte |= (byte)(0x80 >> partialCount);
                partialCount++;
                _stopBit++;
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
        public IEnumerable<byte> AsBytes()
        {
            if (Count % 8 != 0) throw new InvalidOperationException();
            return GetPartialBytes();
        }

        /// <summary>
        /// returns a byte array where the last byte may be partial.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<byte> GetPartialBytes()
        {
            if (_startBit % 8 == 0)
                return _bits.Skip(_startBit / 8).Take(Count / 8 + (Count % 8 == 0 ? 0 : 1));
            var bitarray = new BitArray(_bits.AsSpan(_startBit / 8, Count / 8 + 1).ToArray());
            bitarray.LeftShift(_startBit % 8);
            var bitsAligned = new byte[Count / 8 + 1];
            bitarray.CopyTo(bitsAligned, 0);
            return bitsAligned;
        }

        public IEnumerator<bool> GetEnumerator()
        {
            var firstIndex = _startBit / 8;
            var firstBit = _startBit % 8;
            var count = Count;
            foreach (var b in _bits.Skip(firstIndex))
            {
                foreach (var bit in GetBits(b, firstBit))
                {
                    yield return bit;
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

        private IEnumerable<bool> GetBits(byte b, int start)
        {
            for (int i = start; i < 8; i++)
                yield return (b & (0x80 >> i)) != 0;
        }

        public Bits Common(IEnumerable<bool> enumerable)
        {
            return new Bits(_bits, _startBit, _startBit + CommonCount(enumerable));
        }

        private int CommonCount(IEnumerable<bool> other)
        {
            var commonCount = 0;
            using var otherbit = other.GetEnumerator();
            foreach (var mybit in this)
            {
                if (otherbit.MoveNext() && mybit == otherbit.Current)
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
            return new Bits(_bits, _startBit + count, _stopBit);
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
            return new Bits(_bits, _startBit, _startBit + count);
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
            return this.SequenceEqual(other);
        }

        public override bool Equals(object obj)
        {
            return obj is Bits other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_startBit, _stopBit, _bits);
        }
    }
}
