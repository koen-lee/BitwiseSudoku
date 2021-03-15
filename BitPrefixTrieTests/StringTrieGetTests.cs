using System.Collections.Generic;
using BitPrefixTrie;
using Xunit;
using Xunit.Abstractions;

namespace BitPrefixTrieTests
{
    public class StringTrieGetTests
    {
        private readonly StringTrie<string> _underTest;
        private ITestOutputHelper Helper { get; }

        public StringTrieGetTests(ITestOutputHelper helper)
        {
            Helper = helper;
            _underTest = new StringTrie<string>
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
        }

        [Fact]
        public void Given_a_filled_Trie_When_an_existing_item_is_fetched_Then_it_is_found()
        {
            //when
            var result = _underTest["Gamma"];
            Assert.Equal("Gamma value", result);
        }


        [Fact]
        public void Given_a_filled_Trie_When_an_non_existing_item_is_fetched_Then_KeyNotFoundException()
        {
            //when
            Assert.Throws<KeyNotFoundException>(() =>
                _underTest["does not exist"]
            );
        }

        [Fact]
        public void Given_a_filled_Trie_When_TryGetValue_an_non_existing_item_is_fetched_Then_false()
        {
            Assert.False(_underTest.TryGetValue("does not exist", out _));
        }

        [Fact]
        public void Given_a_filled_Trie_When_TryGetValue_an_existing_item_is_fetched_Then_true()
        {
            Assert.True(_underTest.TryGetValue("Epsilon", out var value));
            Assert.Equal("Epsilon value", value);
        }
    }
}