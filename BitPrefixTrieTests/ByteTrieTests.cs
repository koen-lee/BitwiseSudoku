using BitPrefixTrie;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace BitPrefixTrieTests
{
    public class ByteTrieTests
    {
        public ITestOutputHelper Helper { get; }

        public ByteTrieTests(ITestOutputHelper helper)
        {
            Helper = helper;
        }

        [Fact]
        public void Given_a_new_Trie_When_one_item_is_added_Then_it_has_one_item()
        {
            //Given
            var trie = new ByteTrie<string>();
            trie.AddItem(new byte[] { 0x1 }, "first");
            //Then
            var item = trie.Single();

            AssertEqual(item, 0x1, "first");
        }

        [Fact]
        public void Given_a_new_Trie_When_two_items_are_added_Then_it_has_two_item()
        {
            //Given
            var trie = new ByteTrie<string>();
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
            var trie = new ByteTrie<string>();
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
            var trie = new ByteTrie<string>();
            trie.AddItem(new byte[] { 0x1 }, "first");
            trie.AddItem(new byte[] { 0x3 }, "third");
            //Then
            var items = trie.ToArray();
            AssertEqual(items[0], 0x1, "first");
            AssertEqual(items[1], 0x3, "third");
        }


        [Fact]
        public void Given_a_Trie_with_two_items_When_a_subitem_is_added_Then_it_has_three_items()
        {
            //Given
            var trie = new ByteTrie<string>();
            trie.AddItem(new byte[] { 0x1 }, "first");
            trie.AddItem(new byte[] { 0x3 }, "third");
            //When
            trie.AddItem(new byte[] { 0x2 }, "second");

            Helper.WriteLine(trie.ToString());
            //Then
            var items = trie.ToArray();
            AssertEqual(items[0], 0x1, "first");
            AssertEqual(items[1], 0x2, "second");
            AssertEqual(items[2], 0x3, "third");
        }

        [Fact]
        public void Given_a_Trie_with_two_items_When_a_subitem_is_added_to_an_existing_node_Then_it_has_three_items()
        {
            //Given
            var trie = new ByteTrie<string>();
            trie.AddItem(new byte[] { 0x1, 0xff }, "0x01ff");
            trie.AddItem(new byte[] { 0x1, 0x00 }, "0x0100");
            //When
            trie.AddItem(new byte[] { 0x1 }, "0x01");

            Helper.WriteLine(trie.ToString());
            //Then
            var items = trie.ToArray();
            Assert.Equal(new byte[] { 0x1 }, items[0].Key);
            Assert.Equal("0x01", items[0].Value);
            Assert.Equal(new byte[] { 0x1, 0x00 }, items[1].Key);
            Assert.Equal("0x0100", items[1].Value);
            Assert.Equal(new byte[] { 0x1, 0xff }, items[2].Key);
            Assert.Equal("0x01ff", items[2].Value);
        }

        private static void AssertEqual(KeyValuePair<IEnumerable<byte>, string> item, byte key, string value)
        {
            Assert.Equal(new[] { key }, item.Key);
            Assert.Equal(value, item.Value);
        }
    }
}
