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

            var rows = GetSets(9);
            var cols = GetSets(9);
            var sqrs = GetSets(9);

            var sudoku = new Field[known.Length];
            for (int i = 0; i < known.Length; i++)
            {
                int row = i / 9;
                int col = i % 9;
                sudoku[i] = new Field(rows[row], cols[col], sqrs[(row / 3) + 3 * (col / 3)])
                { Value = known[i] };
            }
            var s = new Sudoku(sudoku, rows.Concat(cols).Concat(sqrs).ToArray());
            var sw = Stopwatch.StartNew();
            var result = s.Solve(out s);

            Console.WriteLine(sw.Elapsed);
            Console.WriteLine(result);
            s.Write();
        }

        static void Main4()
        {
            var known = new[]{  0,3,4,0,
                                4,0,0,2,
                                1,0,0,3,
                                0,2,1,0};

            var rows = GetSets(4);
            var cols = GetSets(4);
            var sqrs = GetSets(4);

            var sudoku = new Field[known.Length];
            for (int i = 0; i < known.Length; i++)
            {
                int row = i / 4;
                int col = i % 4;
                sudoku[i] = new Field(rows[row], cols[col], sqrs[(row / 2) + 2 * (col / 2)])
                { Value = known[i] };
            }

            var s = new Sudoku(sudoku, rows.Concat(cols).Concat(sqrs).ToArray());
            s.SolveSimple();
            s.Write();
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
