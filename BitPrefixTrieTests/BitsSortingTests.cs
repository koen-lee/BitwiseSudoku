using BitPrefixTrie;
using Xunit;

namespace BitPrefixTrieTests
{
    public class BitsSortingTests
    {
        public static object[][] LeftIsSmallerThanRight = new[]
        {
            new object[] {new Bits(false), new Bits(true)},
            new object[] {new Bits(false, true), new Bits(true)},
            new object[] {new Bits(false, true), new Bits(false, true, true)},
            new object[] {new Bits(false, true), new Bits(false, true, false)},
        };

        [Theory]
        [MemberData(nameof(LeftIsSmallerThanRight))]
        public void Left_is_smaller_than_right(Bits left, Bits right)
        {
            Assert.True(left < right);
            Assert.False(right < left);
        }

        [Theory]
        [MemberData(nameof(LeftIsSmallerThanRight))]
        public void Right_is_greater_than_left(Bits left, Bits right)
        {
            Assert.False(left > right);
            Assert.True(right > left);
        }
    }
}