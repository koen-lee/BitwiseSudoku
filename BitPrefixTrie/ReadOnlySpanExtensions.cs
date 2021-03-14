using System;

namespace BitPrefixTrie
{
    public static class ReadOnlySpanExtensions
    {
        public static Bits ToBits(this ReadOnlySpan<byte> bytes)
        {
            return new Bits(bytes.ToArray());
        }
    }
}
