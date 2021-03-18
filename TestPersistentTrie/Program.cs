using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BitPrefixTrie.Persistent;

namespace PhoneBook
{
    class Program
    {
        private static long _number = 12345678901;

        static void Main(string[] args)
        {
            if (args?.Length == 0)
                args = new[] { "~/phonebook", "list" };

            using var stream = new FileStream(args[0], FileMode.OpenOrCreate);
            var trie = new PersistentTrie(stream);

            if (args[1] == "generate")
            {
                var count = 10_000_000;

                for (int i = 0; i < count; i++)
                {
                    trie.Add(Guid.NewGuid().ToString(), GeneratePhoneNumber());
                }
            }
            else if (args[1] == "add")
            {
                trie.Add(args[2], args[3]);
            }
            else if (args[2] == "list")
            {
                var skip = 0;
                var limit = 100;
                if (args.Length > 3)
                    skip = int.Parse(args[3]);
                if (args.Length > 4)
                    limit = int.Parse(args[4]);
                ListPhonebook(trie.Skip(skip).Take(limit));
            }
        }

        private static void ListPhonebook(IEnumerable<KeyValuePair<string, string>> items)
        {
            foreach (var kv in items)
            {
                Console.WriteLine($"{kv.Key,30} {kv.Value}");
            }
        }

        public static string GeneratePhoneNumber()
        {
            return (_number++).ToString();
        }
    }
}
