using System.IO;
using System.Linq;
using BitPrefixTrie.Persistent;
using Xunit;
using Xunit.Abstractions;

namespace BitPrefixTrieTests.Persistent
{
    public class PersistentTrieFromTests
    {
        private readonly MemoryStream _stream;
        private readonly PersistentTrie _underTest;
        private ITestOutputHelper Helper { get; }

        public PersistentTrieFromTests(ITestOutputHelper helper)
        {
            Helper = helper;
            _stream = new MemoryStream();
            var temp = new PersistentTrie(_stream)
            {
                {"Alpha", "Alpha value"},
                {"Alphabet", "Alphabet value"},
                {"Bae", "Bae value"},
                {"Badminton", "Badminton value"},
                {"Charlie", "Charlie value"},
                {"Delta", "Delta value"},
                {"Epsilon", "Epsilon value"},
                {"Epsilon Delta Gamma", "Epsilon Delta Gamma value"},
                {"Gamma", "Gamma value"},
                {"Aarg", "Aarg value"},
                {"Beast", "Beast value"},
            };
            temp.Persist();

            _underTest = new PersistentTrie(_stream);

            Helper.WriteLine(_underTest.ToString());
        }

        [Fact]
        public void Given_a_Trie_with_11_values_When_From_G_then_the_last_is_returned()
        {
            var fromItems = _underTest.From("G").ToArray();
            Assert.Single(fromItems, kv => kv.Key == "Gamma");
        }

        [Fact]
        public void Given_a_Trie_with_11_values_When_From_Gamma_then_the_last_is_returned()
        {
            var fromItems = _underTest.From("Gamma").ToArray();
            Assert.Single(fromItems, kv => kv.Key == "Gamma");
        }

        [Fact]
        public void Given_a_Trie_with_11_values_When_From_A_then_all_is_returned()
        {
            var fromItems = _underTest.From("A").ToArray();
            Assert.Equal(_underTest.ToArray(), fromItems.ToArray());
        }

        [Fact]
        public void Given_a_Trie_with_11_values_When_From_Charles_then_4_are_skipped()
        {
            /*Charles is between Beast and Charlie*/
            var fromItems = _underTest.From("Charles").ToArray();
            Assert.Equal( _underTest.Skip(6).ToArray(), fromItems.ToArray());
        }
    }
}