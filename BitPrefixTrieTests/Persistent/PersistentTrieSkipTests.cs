using System.IO;
using System.Linq;
using BitPrefixTrie;
using BitPrefixTrie.Persistent;
using Xunit;
using Xunit.Abstractions;

namespace BitPrefixTrieTests.Persistent
{
    public class PersistentTrieSkipTests
    {
        private readonly MemoryStream _stream;
        private readonly PersistentTrie _underTest;
        private ITestOutputHelper Helper { get; }

        public PersistentTrieSkipTests(ITestOutputHelper helper)
        {
            Helper = helper;
            _stream = new MemoryStream();
            _ = new PersistentTrie(_stream)
            {
                {"Alpha", "Alpha value"},
                {"Alphabet", "Alphabet value"},
                {"Bat", "Bat value"},
                {"Badminton", "Badminton value"},
                {"Charlie", "Charlie value"},
                {"Delta", "Delta value"},
                {"Epsilon", "Epsilon value"},
                {"Epsilon Delta Gamma", "Epsilon Delta Gamma value"},
                {"Gamma", "Gamma value"},
                {"Aarg", "Aarg value"},
                {"Beast", "Beast value"},
            };

            _underTest = new PersistentTrie(_stream);

            Helper.WriteLine(_underTest.ToString());
        }

        [Fact]
        public void Given_a_Trie_with_11_values_When_skipping_10_then_the_last_is_returned()
        {
            var skipped = _underTest.Skip(10).ToArray();
            Assert.Single(skipped, kv => kv.Key == "Gamma");
        }

        [Fact]
        public void Given_a_Trie_with_11_values_When_skipping_5_then_the_last_half_list_is_returned()
        {
            var skipped = _underTest.Skip(5).ToArray();
            var fromlinq = Enumerable.Skip(_underTest, 5).ToArray();
            Assert.Equal(fromlinq, skipped);
            Assert.Equal(6, skipped.Length);
        }

        [Fact]
        public void Given_a_Trie_with_11_values_When_skipping_0_then_the_list_is_returned()
        {
            var fromLinq = _underTest;
            var skipped = _underTest.Skip(0).ToArray();
            //Assert.Equal(11, skipped.Length);
            //Assert.Equal(fromLinq.Length, skipped.Length);
            Assert.Equal(fromLinq, skipped);
        }

        [Fact]
        public void Given_a_Trie_then_the_list_is_returned()
        {
            var fromLinq = _underTest;
            var array = _underTest.ToArray();
            Assert.Equal(fromLinq, array);
        }
    }
}