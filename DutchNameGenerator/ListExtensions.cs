using System;
using System.Collections.Generic;

namespace DutchNameGenerator
{
    public static class ListExtensions
    {
        static Random _random = new Random();

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

        public static IEnumerable<IEnumerable<T>> ChopToPieces<T>(this IEnumerable<T> items, int pieceSize)
        {
            var enumerator = items.GetEnumerator();
            bool done = false;

            IEnumerable<T> GetPiece()
            {
                for (int i = 0; i < pieceSize; i++)
                {
                    if (!enumerator.MoveNext())
                    {
                        done = true;
                        yield break;
                    }

                    yield return enumerator.Current;
                }
            }

            while (!done)
            {
                yield return GetPiece();
            }
        }
    }
}