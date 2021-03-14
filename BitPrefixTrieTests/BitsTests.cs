using BitPrefixTrie;
using Xunit;

namespace BitPrefixTrieTests
{
    public class BitsTests
    {
        [Fact]
        public void Given_a_byte_then_the_msb_is_first()
        {
            //Given
            var bits = new Bits(new byte[] { 0x80 });

            Assert.Equal(new[] { true, false, false, false, false, false, false, false }, bits);
        }

        [Fact]
        public void Given_a_bit_list_then_the_msb_is_first()
        {
            //Given
            var bits = new Bits(new[] { true, false });

            Assert.Equal(new[] { true, false }, bits);
        }


        [Fact]
        public void Given_a_byte_list_then_it_roundtrips()
        {
            //Given
            byte[] bytes = new byte[] { 0xF0, 0x01, 0x02, 0x03 };
            var bits = new Bits(bytes);

            Assert.Equal(bytes, bits.AsBytes());
        }

        [Fact]
        public void Given_a_bits_list_then_it_roundtrips()
        {
            //Given
            byte[] bytes = new byte[] { 0xF0, 0x01, 0x02, 0x03 };
            var bits1 = new Bits(bytes);
            var bits2 = new Bits(bits1);
            Assert.Equal(bytes, bits2.AsBytes());
        }

    }
}
