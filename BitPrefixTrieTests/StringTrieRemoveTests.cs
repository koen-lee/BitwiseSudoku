using BitPrefixTrie;
using Xunit;
using Xunit.Abstractions;

namespace BitPrefixTrieTests
{
    public class StringTrieRemoveTests
    {
        private readonly StringTrie<string> _underTest;
        private ITestOutputHelper Helper { get; }

        public StringTrieRemoveTests(ITestOutputHelper helper)
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
        public void Given_a_filled_Trie_When_an_existing_item_is_removed_Then_true()
        {
            //when
            var result = _underTest.Remove("Gamma");
            Assert.True(result);
        }

        [Fact]
        public void Given_a_filled_Trie_When_an_existing_item_is_removed_Then_its_no_longer_there()
        {
            //when
            _underTest.Remove("Gamma");
            Assert.DoesNotContain("Gamma", _underTest.Keys);
        }

        [Fact]
        public void Given_a_filled_Trie_When_an_existing_item_is_removed_twice_Then_its_no_longer_there()
        {
            //when
            Assert.True(_underTest.Remove("Gamma"));
            Assert.False(_underTest.Remove("Gamma"));
            Assert.DoesNotContain("Gamma", _underTest.Keys);
        }

        [Fact]
        public void Given_a_filled_Trie_When_an_existing_item_is_removed_and_added_Then_its_there_again()
        {
            //when
            _underTest.Remove("Gamma");
            _underTest.Add("Gamma", "there and back again");
            Assert.Equal("there and back again", _underTest["Gamma"]);
        }

        [Fact]
        public void Given_a_filled_Trie_When_an_nonexisting_item_is_removed_Then_false()
        {
            //when
            var result = _underTest.Remove("nonexisting");
            Assert.False(result);
        }
    }
}