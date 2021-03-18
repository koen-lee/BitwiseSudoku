using System;
using System.Linq;
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

        [Fact]
        public void Given_a_bits_list_then_msb_is_first()
        {
            //Given
            var bits = new Bits(new[] { true, true, true, true, false, false, false, false });
            Assert.Equal(new byte[] { 0xF0 }, bits.AsBytes());
        }

        [Fact]
        public void Given_complete_bytes_When_GetPartialBytes_then_the_original_is_returned()
        {
            //Given
            var bits = new Bits(new byte[] { 0x1, 0x2, 0xff });
            Assert.Equal(new byte[] { 0x1, 0x2, 0xff }, bits.GetPartialBytes());
        }

        [Fact]
        public void Given_partial_bytes_When_GetPartialBytes_then_the_partial_byte_is_returned_too()
        {
            //Given
            var bits = new Bits(new[] { true, true, true, true, false, false, false, false, true });
            Assert.Equal(new byte[] { 0xF0, 0x80 }, bits.GetPartialBytes());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(10)]
        public void Given_bits_When_Skip_then_the_remaining_bits_are_returned(int count)
        {
            //Given
            var bits = new Bits(new[] { true, true, true, true, false, false, false, false, true });
            Assert.Equal(Enumerable.Skip(bits, count), bits.Skip(count));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(10)]
        public void Given_bits_When_Take_then_the_remaining_bits_are_returned(int count)
        {
            //Given
            var bits = new Bits(new[] { true, true, true, true, false, false, false, false, true });
            Assert.Equal(Enumerable.Take(bits, count), bits.Take(count));
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(3, true)]
        [InlineData(4, false)]
        [InlineData(5, false)]
        [InlineData(6, false)]
        [InlineData(7, false)]
        [InlineData(8, true)]
        public void Given_bits_When_First_then_the_first_bit_is_returned(int skip, bool expected)
        {
            //Given
            var bits = new Bits(new[] { true, true, true, true, false, false, false, false, true });
            Assert.Equal(expected, bits.Skip(skip).First());
        }

        [Fact]
        public void Given_empty_Bits_When_First_then_InvalidOperationException()
        {
            //Given
            Assert.Throws<InvalidOperationException>(() => Bits.Empty.First());
        }

    }
}
