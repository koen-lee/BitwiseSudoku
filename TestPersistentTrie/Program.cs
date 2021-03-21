using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BitPrefixTrie;
using BitPrefixTrie.Persistent;
using DutchNameGenerator;

namespace PhoneBook
{
    class Program
    {
        private static long _number = 12345678901;
        private static int _defragment_threshold = 1_000_000;
        private static Random _random;
        static void Main(string[] args)
        {
            args ??= new string[0];
            if (args.Length == 0)
                args = new[] { "%HOMEPATH%/phonebook", "list" };

            var fileInfo = new FileInfo(Environment.ExpandEnvironmentVariables(args[0]));
            var stream = fileInfo
               .Open(FileMode.OpenOrCreate);
            var stopwatch = Stopwatch.StartNew();
            var trie = new PersistentTrie(stream);

            if (args[1] == "generate")
            {
                var names = new Generator().GenerateUniqueNames();
                var generateTimer = Stopwatch.StartNew();
                var count = 100_000;
                var tick = count / 50;
                int i = 0;
                foreach (var name in names.Take(count))
                {
                    i++;
                    trie.Add(Guid.NewGuid().ToString(), GeneratePhoneNumber());
                    if ((i + 1) % tick == 0)
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

                Defragment(ref trie, ref stream);
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

            trie.Persist();
            using (stream) {/*dispose*/ }

            stopwatch.Stop();
            Console.WriteLine($"Elapsed: {stopwatch.Elapsed} ({stopwatch.Elapsed.TotalSeconds:0.0} seconds)");
        }

        private static void Defragment(ref PersistentTrie trie, ref FileStream stream)
        {
            if (trie.Count % _defragment_threshold != 0) return;
            var timer = Stopwatch.StartNew();
            DoRebuildTree(ref trie, ref stream);
            Console.WriteLine($"Defragmenting took {timer.ElapsedMilliseconds} ms");
        }

        private static void DoRebuildTree(ref PersistentTrie trie, ref FileStream stream)
        {
            _defragment_threshold = (int)(_defragment_threshold * 1.41);
            var newFile = new FileInfo(stream.Name + ".new");
            if (newFile.Exists) newFile.Delete();
            using (var newStream = newFile.Open(FileMode.CreateNew))
            {
                var newTrie = new PersistentTrie(newStream);
                foreach (var item in trie._root)
                    newTrie._root.AddItem(new Bits(item.Key), item.Value);
                using (stream)
                {
                    /*dispose*/
                }
                newTrie.Persist();
            }

            newFile.Replace(stream.Name, null);
            stream = new FileStream(stream.Name, FileMode.Open);
            trie = new PersistentTrie(stream);
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
            return _random.Next((int)1e10, (int)1e12).ToString();
        }
    }
}
