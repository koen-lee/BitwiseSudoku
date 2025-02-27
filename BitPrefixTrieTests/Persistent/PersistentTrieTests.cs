using System.Collections.Generic;
using System.IO;
using System.Linq;
using BitPrefixTrie;
using BitPrefixTrie.Persistent;
using Xunit;
using Xunit.Abstractions;

namespace BitPrefixTrieTests.Persistent
{
    public class PersistentTrieTests
    {
        private readonly MemoryStream _stream;
        public ITestOutputHelper Helper { get; }

        public PersistentTrieTests(ITestOutputHelper helper)
        {
            Helper = helper;
            _stream = new MemoryStream();
        }

        [Fact]
        public void Given_a_new_Trie_When_one_item_is_added_Then_it_has_one_item()
        {
            //Given
            var trie = new PersistentTrie(_stream);
            trie.Add("Alpha", "first");
            //Then
            Helper.WriteLine(trie.ToString());
            var item = trie.Single();

            AssertEqual(item, "Alpha", "first");

            trie.Persist();

            var copy = new PersistentTrie(_stream);
            Helper.WriteLine(copy.ToString());

            Assert.Equal(trie, copy);
        }

        [Fact]
        public void Given_a_new_Trie_When_two_items_are_added_Then_it_has_two_item()
        {
            //Given
            var trie = new PersistentTrie(_stream);
            trie.Add("Alpha", "first");
            Helper.WriteLine(trie.ToString());
            trie.Add("Beta", "second");
            Helper.WriteLine(trie.ToString());
            //Then
            var items = trie.ToArray();
            AssertEqual(items[0], "Alpha", "first");
            AssertEqual(items[1], "Beta", "second");
            trie.Persist();
            Assert.Equal(trie, new PersistentTrie(_stream));
        }

        [Fact]
        public void Given_a_new_Trie_When_two_items_are_added_with_indexer_Then_it_has_two_item()
        {
            //Given
            var trie = new PersistentTrie(_stream);
            trie["Alpha"] = "first";
            trie["Beta"] = "second";
            //Then
            var items = trie.ToArray();
            AssertEqual(items[0], "Alpha", "first");
            AssertEqual(items[1], "Beta", "second");
            trie.Persist();
            Assert.Equal(trie, new PersistentTrie(_stream));
        }

        [Fact]
        public void Given_a_new_Trie_When_two_items_are_added_in_reverse_order_Then_it_has_two_item()
        {
            //Given
            var trie = new PersistentTrie(_stream);
            trie.Add("Beta", "second");
            trie.Add("Alpha", "first");
            //Then
            var items = trie.ToArray();
            AssertEqual(items[0], "Alpha", "first");
            AssertEqual(items[1], "Beta", "second");
            trie.Persist();
            var copy = new PersistentTrie(_stream);
            Assert.Equal(trie, copy);
            Assert.Equal(trie.Count, copy.Count);
        }

        [Fact]
        public void Given_a_Trie_with_one_item_When_a_subitem_is_added_Then_it_has_two_item()
        {
            //Given
            var trie = new PersistentTrie(_stream);
            trie.Add("Alpha", "first");
            trie.Add("Aztec", "second");
            //Then
            var items = trie.ToArray();
            AssertEqual(items[0], "Alpha", "first");
            AssertEqual(items[1], "Aztec", "second");
            trie.Persist();
            Assert.Equal(trie, new PersistentTrie(_stream));
        }

        [Fact]
        public void Given_a_Trie_with_two_items_When_a_subitem_is_added_Then_it_has_three_items()
        {
            //Given
            var trie = new PersistentTrie(_stream);
            trie.Add("Alpha", "first");
            trie.Add("B", "second");
            //When
            trie.Add("Beta", "third");

            Helper.WriteLine(trie.ToString());
            //Then
            var items = trie.ToArray();
            AssertEqual(items[0], "Alpha", "first");
            AssertEqual(items[1], "B", "second");
            AssertEqual(items[2], "Beta", "third");
            trie.Persist();
            Assert.Equal(trie, new PersistentTrie(_stream));
        }

        [Fact]
        public void Given_a_Trie_with_two_items_When_a_subitem_is_added_at_a_node_Then_it_has_three_items()
        {
            //Given
            var trie = new PersistentTrie(_stream);
            trie.Add("Bat", "second");
            trie.Add("Bison", "third");
            //When
            trie.Add("B", "first");

            Helper.WriteLine(trie.ToString());
            //Then
            var items = trie.ToArray();
            AssertEqual(items[0], "B", "first");
            AssertEqual(items[1], "Bat", "second");
            AssertEqual(items[2], "Bison", "third");

            trie.Persist();
            Assert.Equal(trie, new PersistentTrie(_stream));
        }


        [Fact]
        public void Given_a_new_Trie_with_two_items_are_added_When_toArray_Then_the_array_is_filled()
        {
            //Given
            var trie = new PersistentTrie(_stream);
            trie.Add("Beta", "second");
            trie.Add("Alpha", "first");
            //Then
            var items = trie.ToArray();
            AssertEqual(items[0], "Alpha", "first");
            AssertEqual(items[1], "Beta", "second");

            trie.Persist();
            Assert.Equal(trie.AsEnumerable(), new PersistentTrie(_stream).AsEnumerable());
        }

        private static void AssertEqual(KeyValuePair<string, string> item, string key, string value)
        {
            Assert.Equal(key, item.Key);
            Assert.Equal(value, item.Value);
        }
    }
}
