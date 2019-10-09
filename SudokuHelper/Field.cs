using System;
using System.Collections.Generic;
using System.Numerics;

namespace SudokuHelper
{
    class Field
    {
        private readonly SudokuSet[] _sets;

        public ulong PossibleMask { get; private set; }

        public int Value
        {
            get => PossibleMask == 0 ? 0 : BitOperations.TrailingZeroCount(PossibleMask) + 1;
            set
            {
                if (value == 0)
                {
                    if (PossibleMask == 0)
                        return;
                    throw new InvalidOperationException();
                }

                var mask = 1ul << (value - 1);
                SetMask(mask);
            }
        }

        public void SetMask(ulong mask)
        {
            foreach (var set in _sets)
            {
                if (!set.TryRemovePossibility(mask))
                    throw new InvalidOperationException("Set already has mask " + mask);
            }
            PossibleMask = mask;
        }

        public bool HasValue { get => BitOperations.PopCount(PossibleMask) == 1; }

        public Field(params SudokuSet[] sets)
        {
            _sets = sets;
            foreach (var set in sets)
            {
                set.Add(this);
            }
        }

        public ValueResult TrySetValue()
        {
            if (HasValue) return ValueResult.Solved;
            var possibilities = _sets[0].PossibleFlags & _sets[1].PossibleFlags & _sets[2].PossibleFlags;
            if (possibilities == 0)
                return ValueResult.Impossible;
            PossibleMask = possibilities;
            if (BitOperations.PopCount(possibilities) == 1)
            {
                foreach (var set in _sets)
                {
                    if (!set.TryRemovePossibility(possibilities)) throw new InvalidOperationException();
                }
                return ValueResult.Set;
            }
            return ValueResult.NotSet;
        }

        public IEnumerable<int> GetPossibleValues()
        {
            var possibilities = _sets[0].PossibleFlags & _sets[1].PossibleFlags & _sets[2].PossibleFlags;
            var mask = 1ul;
            for (int i = 1; i <= 64; i++)
            {
                if ((possibilities & mask) != 0)
                    yield return i;
                mask <<= 1;
            }
        }
    }

    public enum ValueResult
    {
        Impossible,
        Set,
        NotSet,
        Solved,
    }
}
