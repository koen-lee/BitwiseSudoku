using BitPrefixTrie;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BitPrefixTrieTests
{
    public class TrieTests
    {
        [Fact]
        public void Given_a_new_Trie_When_one_item_is_added_Then_it_has_one_item()
        {
            //Given
            var trie = new Trie<string>();
            trie.AddItem(new byte[] { 0x1 }, "first");
            //Then
            var item = trie.Single();

            AssertEqual(item, 0x1, "first");
        }

        [Fact]
        public void Given_a_new_Trie_When_two_items_are_added_Then_it_has_two_item()
        {
            //Given
            var trie = new Trie<string>();
            trie.AddItem(new byte[] { 0x1 }, "first");
            trie.AddItem(new byte[] { 0x2 }, "second");
            //Then
            var items = trie.ToArray();
            AssertEqual(items[0], 0x1, "first");
            AssertEqual(items[1], 0x2, "second");
        }

        [Fact]
        public void Given_a_new_Trie_When_two_items_are_added_in_reverse_order_Then_it_has_two_item()
        {
            //Given
            var trie = new Trie<string>();
            trie.AddItem(new byte[] { 0x2 }, "second");
            trie.AddItem(new byte[] { 0x1 }, "first");
            //Then
            var items = trie.ToArray();
            AssertEqual(items[0], 0x1, "first");
            AssertEqual(items[1], 0x2, "second");
        }

        [Fact]
        public void Given_a_Trie_with_one_item_When_a_subitem_is_added_Then_it_has_two_item()
        {
            //Given
            var trie = new Trie<string>();
            trie.AddItem(new byte[] { 0x1 }, "first");
            trie.AddItem(new byte[] { 0x3 }, "third");
            //Then
            var items = trie.ToArray();
            AssertEqual(items[0], 0x1, "first");
            AssertEqual(items[1], 0x3, "third");
        }

        private static void AssertEqual(KeyValuePair<IEnumerable<byte>, string> item, byte key, string value)
        {
            Assert.Equal(new[] { key }, item.Key);
            Assert.Equal(value, item.Value);
        }
    }
}
