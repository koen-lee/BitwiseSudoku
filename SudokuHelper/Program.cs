using System;
using System.Diagnostics;
using System.Linq;

namespace SudokuHelper
{
    partial class Program
    {
        static void Main(string[] args)
        {
            Main4();
            Main9();
            Main9();
        }

        static void Main9()
        {
            var known = new[]{
                5,0,0,0,7,0,2,0,0,
                0,4,0,0,0,2,0,0,3,
                0,3,0,0,0,0,0,6,0,
                9,7,0,6,4,0,0,0,0,
                8,0,0,0,0,0,0,0,5,
                0,0,0,0,1,8,0,7,6,
                0,9,0,0,0,0,0,4,0,
                6,0,0,9,0,0,0,5,0,
                0,0,4,0,6,0,0,0,1
            };

            var s = Sudoku.Create(known);
            var sw = Stopwatch.StartNew();
            var result = s.Solve(out s);

            Console.WriteLine(sw.Elapsed);
            Console.WriteLine(result);
            Console.WriteLine(s);
        }

        static void Main4()
        {
            var known = new[]{  0,3,4,0,
                                4,0,0,2,
                                1,0,0,3,
                                0,2,1,0};

            var s = Sudoku.Create(known);
            s.SolveSimple();
            Console.WriteLine(s);
        }

        private static SudokuSet[] GetSets(int count)
        {
            ulong initialFlags = (1ul << count) - 1;
            var result = new SudokuSet[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = new SudokuSet(initialFlags);
            }
            return result;
        }
    }
}
