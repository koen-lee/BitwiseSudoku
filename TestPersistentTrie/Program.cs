using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BitPrefixTrie;
using BitPrefixTrie.Persistent;

namespace PhoneBook
{
    class Program
    {
        private static long _number = 12345678901;
        private static int _defragment_threshold = 1_000_000;

        static void Main(string[] args)
        {
            args ??= new string[0];
            if (args.Length == 0)
                args = new[] { "%HOMEPATH%/phonebook", "list" };
            var stopwatch = Stopwatch.StartNew();

            var fileInfo = new FileInfo(Environment.ExpandEnvironmentVariables(args[0]));
            PersistentTrie trie = null;
            FileStream stream = null;
            if (args[1] == "generate")
            {
                if (fileInfo.Exists)
                    fileInfo.Delete();
                stream = CreateTrie(fileInfo, out trie);
                var generateTimer = Stopwatch.StartNew();
               // var count = 100_000;
                var count = 10_000_000;
                var tick = count / 50;
                for (int i = 0; i < count; i++)
                {
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
                stream = CreateTrie(fileInfo, out trie);
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
                CreateTrie(fileInfo, out trie);
                ListPhonebook(trie.Skip(skip).Take(limit));
            }
            else if (args[1] == "rebuild")
            {
                stream = CreateTrie(fileInfo, out trie);
                Rebuild(ref trie, ref stream);
            }

            if (trie != null) trie.Persist();
            if (stream != null)
                using (stream) { /*dispose*/ }

            stopwatch.Stop();
            Console.WriteLine($"Elapsed: {stopwatch.Elapsed} ({stopwatch.Elapsed.TotalSeconds:0.0} seconds)");
        }

        private static FileStream CreateTrie(FileInfo fileInfo, out PersistentTrie trie)
        {
            var stream = fileInfo
                .Open(FileMode.OpenOrCreate);
            trie = new PersistentTrie(stream);
            return stream;
        }

        private static void Rebuild(ref PersistentTrie trie, ref FileStream stream)
        {
            var timer = Stopwatch.StartNew();
            DoRebuildTree(ref trie, ref stream);
            Console.WriteLine($"Defragmenting took {timer.ElapsedMilliseconds} ms");
        }

        private static void DoRebuildTree(ref PersistentTrie trie, ref FileStream stream)
        {
            _defragment_threshold = (int)(_defragment_threshold * 1.41);
            var newFile = new FileInfo(stream.Name + ".new");
            if (newFile.Exists) newFile.Delete();

            var tick = trie.Count / 50;
            var i = 0;
            var generateTimer = Stopwatch.StartNew();
            using (var newStream = newFile.Open(FileMode.CreateNew))
            {
                var newTrie = new PersistentTrie(newStream);
                foreach (var item in trie._root)
                {
                    newTrie._root.AddItem(new Bits(item.Key), item.Value);
                    if (i++ % tick == 0)
                    {
                        Console.WriteLine($"[{i,10}] {tick / generateTimer.Elapsed.TotalSeconds:0.0} inserts/sec");
                        generateTimer.Restart();
                    }
                }
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
            return (_number++).ToString();
        }
    }
}
