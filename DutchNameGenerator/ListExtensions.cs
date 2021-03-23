using System;
using System.Collections.Generic;
using System.Linq;

namespace DutchNameGenerator
{
    public static class ListExtensions
    {
        static readonly Random _random = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var swapWithIndex = _random.Next(i, list.Count);
                var temp = list[i];
                list[i] = list[swapWithIndex];
                list[swapWithIndex] = temp;
            }
        }

        public static IEnumerable<T[]> ChopToPieces<T>(this IEnumerable<T> items, int pieceSize)
        {
            using var enumerator = items.GetEnumerator();
            bool done = false;
            if (!enumerator.MoveNext()) yield break;

            IEnumerable<T> GetPiece()
            {
                for (int i = 0; i < pieceSize; i++)
                {
                    yield return enumerator.Current;
                    if (!enumerator.MoveNext())
                    {
                        done = true;
                        yield break;
                    }
                }
            }

            while (!done)
            {
                yield return GetPiece().ToArray();
            }
        }
    }
}