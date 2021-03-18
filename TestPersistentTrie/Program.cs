using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            args ??= new string[0];
            if (args.Length == 0)
                args = new[] { "%HOMEPATH%/phonebook", "list" };

            using var stream = new FileInfo(Environment.ExpandEnvironmentVariables(args[0]))
                .Open(FileMode.OpenOrCreate);
            var stopwatch = Stopwatch.StartNew();
            var trie = new PersistentTrie(stream);

            if (args[1] == "generate")
            {
                var generateTimer = Stopwatch.StartNew();
                var count = 10_000_000;
                var tick = count / 50;
                for (int i = 0; i < count; i++)
                {
                    trie.Add(Guid.NewGuid().ToString(), GeneratePhoneNumber());
                    if (i % tick == 0)
                    {
                        Console.WriteLine($"[{i,10}] {tick / generateTimer.Elapsed.TotalSeconds:0.0} inserts/sec");
                        generateTimer.Restart();
                    }
                }
                Console.WriteLine();
            }
            else if (args[1] == "add")
            {
                trie.Add(args[2], args[3]);
            }
            else if (args[1] == "list")
            {
                var skip = 0;
                var limit = 100;
                if (args.Length > 2)
                    skip = int.Parse(args[2]);
                if (args.Length > 3)
                    limit = int.Parse(args[3]);
                ListPhonebook(trie.Skip(skip).Take(limit));
            }
            stopwatch.Stop();
            Console.WriteLine($"Elapsed: {stopwatch.Elapsed} ({stopwatch.Elapsed.TotalSeconds:0.0} seconds)");
        }

        private static void ListPhonebook(IEnumerable<KeyValuePair<string, string>> items)
        {
            foreach (var kv in items)
            {
                Console.WriteLine($"{kv.Key,-30} {kv.Value}");
            }
        }

        public static string GeneratePhoneNumber()
        {
            return (_number++).ToString();
        }
    }
}
