using BitPrefixTrie;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace BitPrefixTrieTests
{
    public class StringTrieTests
    {
        public ITestOutputHelper Helper { get; }

        public StringTrieTests(ITestOutputHelper helper)
        {
            Helper = helper;
        }

        [Fact]
        public void Given_a_new_Trie_When_one_item_is_added_Then_it_has_one_item()
        {
            //Given
            var trie = new StringTrie<string>();
            trie.AddItem("Alpha", "first");
            //Then
            var item = trie.Single();

            AssertEqual(item, "Alpha", "first");
        }

        [Fact]
        public void Given_a_new_Trie_When_two_items_are_added_Then_it_has_two_item()
        {
            //Given
            var trie = new StringTrie<string>();
            trie.AddItem("Alpha", "first");
            trie.AddItem("Beta", "second");
            //Then
            var items = trie.ToArray();
            AssertEqual(items[0], "Alpha", "first");
            AssertEqual(items[1], "Beta", "second");
        }

        [Fact]
        public void Given_a_new_Trie_When_two_items_are_added_in_reverse_order_Then_it_has_two_item()
        {
            //Given
            var trie = new StringTrie<string>();
            trie.AddItem("Beta", "second");
            trie.AddItem("Alpha", "first");
            //Then
            var items = trie.ToArray();
            AssertEqual(items[0], "Alpha", "first");
            AssertEqual(items[1], "Beta", "second");
        }

        [Fact]
        public void Given_a_Trie_with_one_item_When_a_subitem_is_added_Then_it_has_two_item()
        {
            //Given
            var trie = new StringTrie<string>();
            trie.AddItem("Alpha", "first");
            trie.AddItem("Aztec", "second");
            //Then
            var items = trie.ToArray();
            AssertEqual(items[0], "Alpha", "first");
            AssertEqual(items[1], "Aztec", "second");
        }


        [Fact]
        public void Given_a_Trie_with_two_items_When_a_subitem_is_added_Then_it_has_three_items()
        {
            //Given
            var trie = new StringTrie<string>();
            trie.AddItem("Alpha", "first");
            trie.AddItem("B", "second");
            //When
            trie.AddItem("Beta", "third");

            Helper.WriteLine(trie.ToString());
            //Then
            var items = trie.ToArray();
            AssertEqual(items[0], "Alpha", "first");
            AssertEqual(items[1], "B", "second");
            AssertEqual(items[2], "Beta", "third");
        }


        [Fact]
        public void Given_a_Trie_with_two_items_When_a_subitem_is_added_at_a_node_Then_it_has_three_items()
        {
            //Given
            var trie = new StringTrie<string>();
            trie.AddItem("Bat", "second");
            trie.AddItem("Bison", "third");
            //When
            trie.AddItem("B", "first");

            Helper.WriteLine(trie.ToString());
            //Then
            var items = trie.ToArray();
            AssertEqual(items[0], "B", "first");
            AssertEqual(items[1], "Bat", "second");
            AssertEqual(items[2], "Bison", "third");
        }

        private static void AssertEqual(KeyValuePair<string, string> item, string key, string value)
        {
            Assert.Equal(key, item.Key);
            Assert.Equal(value, item.Value);
        }
    }
}
