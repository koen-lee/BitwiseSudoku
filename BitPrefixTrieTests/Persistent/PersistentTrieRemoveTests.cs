using System.IO;
using BitPrefixTrie;
using Xunit;
using Xunit.Abstractions;

namespace BitPrefixTrieTests.Persistent
{
    public class PersistentTrieRemoveTests
    {
        private MemoryStream _stream;
        private readonly PersistentTrie _underTest;
        private ITestOutputHelper Helper { get; }

        public PersistentTrieRemoveTests(ITestOutputHelper helper)
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
        }

        [Fact]
        public void Given_a_filled_Trie_When_a_new_trie_is_constructed_with_the_same_storage_Then_they_are_the_same()
        {
            var actual = new PersistentTrie(_stream);

            Helper.WriteLine(_underTest.ToString());
            Helper.WriteLine(actual.ToString());
            Assert.Equal(_underTest, actual);
        }

        [Fact]
        public void Given_a_filled_Trie_When_an_existing_item_is_removed_Then_true()
        {
            //when
            var result = _underTest.Remove("Gamma");
            Assert.True(result);
            Assert.Equal(_underTest, new PersistentTrie(_stream));
        }

        [Fact]
        public void Given_a_filled_Trie_When_an_existing_item_is_removed_Then_its_no_longer_there()
        {
            //when
            _underTest.Remove("Gamma");
            Assert.DoesNotContain("Gamma", _underTest.Keys);
            Assert.Equal(_underTest, new PersistentTrie(_stream));
        }

        [Fact]
        public void Given_a_filled_Trie_When_an_existing_item_is_removed_Then_a_recreate_is_smaller()
        {
            //when
            _underTest.Remove("Gamma");
            var newStream = new MemoryStream();
            var recreate = new PersistentTrie(newStream);
            foreach(var item in _underTest)
                recreate.Add(item);
            Assert.True(newStream.Length < _stream.Length);
        }

        [Fact]
        public void Given_a_filled_Trie_When_an_existing_item_is_removed_twice_Then_its_no_longer_there()
        {
            //when
            Assert.True(_underTest.Remove("Gamma"));
            Assert.False(_underTest.Remove("Gamma"));
            Assert.DoesNotContain("Gamma", _underTest.Keys);
            Assert.Equal(_underTest, new PersistentTrie(_stream));
        }

        [Fact]
        public void Given_a_filled_Trie_When_an_existing_item_is_removed_and_added_Then_its_there_again()
        {
            //when
            _underTest.Remove("Gamma");
            _underTest.Add("Gamma", "there and back again");
            Assert.Equal("there and back again", _underTest["Gamma"]);

            Assert.Equal(_underTest, new PersistentTrie(_stream));
        }

        [Fact]
        public void Given_a_filled_Trie_When_an_nonexisting_item_is_removed_Then_false()
        {
            //when
            var result = _underTest.Remove("nonexisting");
            Assert.False(result);
            Assert.Equal(_underTest, new PersistentTrie(_stream));
        }
    }
}