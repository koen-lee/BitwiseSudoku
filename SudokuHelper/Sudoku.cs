using System;
using System.Linq;

namespace SudokuHelper
{

    class Sudoku
    {
        private readonly Field[] sudoku;
        private readonly SudokuSet[] sets;

        public Sudoku(Field[] sudoku, SudokuSet[] sets)
        {
            this.sudoku = sudoku;
            this.sets = sets;
        }

        public void Write()
        {
            var side = (int)Math.Sqrt(sudoku.Length);
            for (int i = 0; i < sudoku.Length; i++)
            {
                int col = i % side;
                if (col == 0)
                    Console.WriteLine();
                if (sudoku[i].Value != 0)
                {
                    Console.Write(" " + sudoku[i].Value);
                }
                else
                {
                    Console.Write("  ");
                }
            }
        }

        public ValueResult SolveSimple()
        {
            ValueResult result = ValueResult.Set;

            while (result == ValueResult.Set)
            {
                result = TrySetAField();
            }
            return result;
        }

        private ValueResult SolveUsingSets()
        {
            ValueResult result = ValueResult.Set;

            while (result == ValueResult.Set)
            {
                var simpleResult = SolveSimple();
                if (simpleResult == ValueResult.Impossible || simpleResult == ValueResult.Solved)
                    return simpleResult;
                result = TrySetAFieldForSet();
            }
            return result;
        }

        private ValueResult TrySetAFieldForSet()
        {
            foreach (var set in sets)
            {
                var result = set.TrySetValue();
                if (result == ValueResult.Set)
                    return result;
            }
            return ValueResult.NotSet;
        }

        public ValueResult Solve(out Sudoku solution)
        {
            solution = this;

            var result = SolveUsingSets();
            if (result == ValueResult.Solved)
                return result;
            if (result == ValueResult.Impossible)
                return result;
            var deciser = sudoku.Where(f => !f.HasValue)
                .Select(f => new { Field = f, Possibilities = f.GetPossibleValues().ToList() })
                .OrderBy(x => x.Possibilities.Count)
                .First();
            foreach (var value in deciser.Possibilities)
            {
                var candidate = CloneAndSet(deciser.Field, value);
                if (candidate.Solve(out solution) == ValueResult.Solved)
                    return ValueResult.Solved;
            }
            return ValueResult.Impossible;
        }

        private Sudoku CloneAndSet(Field field, int value)
        {
            int length = sudoku.Length;
            int side = (int)Math.Sqrt(length);
            int block = (int)Math.Sqrt(side);
            var rows = GetSets(side);
            var cols = GetSets(side);
            var sqrs = GetSets(side);

            var newFields = new Field[length];
            for (int i = 0; i < length; i++)
            {
                int row = i / side;
                int col = i % side;
                newFields[i] = new Field(rows[row], cols[col], sqrs[(row / block) + block * (col / block)]);
                if (sudoku[i].HasValue)
                    newFields[i].Value = sudoku[i].Value;
                if (sudoku[i] == field)
                    newFields[i].Value = value;
            }
            return new Sudoku(newFields, rows.Concat(cols).Concat(sqrs).ToArray());
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

        private ValueResult TrySetAField()
        {
            var result = ValueResult.Solved;
            foreach (var field in sudoku)
            {
                var fieldresult = field.TrySetValue();
                switch (fieldresult)
                {
                    case ValueResult.Solved:
                    case ValueResult.NotSet:
                        if (result == ValueResult.Solved)
                            result = fieldresult;
                        break;
                    case ValueResult.Set:
                    case ValueResult.Impossible:
                        return fieldresult;
                }
            }
            return result;
        }

        public bool IsSolved()
        {
            return sudoku.All(f => f.HasValue);
        }
    }
}
