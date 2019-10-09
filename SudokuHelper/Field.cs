using System;
using System.Collections.Generic;
using System.Numerics;

namespace SudokuHelper
{
    class Field
    {
        private readonly SudokuSet[] _sets;

        private ulong _mask;
        public int Value
        {
            get => _mask == 0 ? 0 : BitOperations.TrailingZeroCount(_mask) + 1;
            set
            {
                if (value == 0)
                {
                    if (_mask == 0)
                        return;
                    throw new InvalidOperationException();
                }

                var mask = 1ul << (value - 1);
                foreach (var set in _sets)
                {
                    if (!set.TryRemovePossibility(mask))
                        throw new InvalidOperationException("Set already has a " + value);
                }
                _mask = mask;
            }
        }

        public bool HasValue { get => _mask != 0; }

        public Field(params SudokuSet[] sets)
        {
            _sets = sets;
            foreach( var set in sets)
            {
                set.Add(this);
            }
        }

        public ValueResult TrySetValue()
        {
            if (Value > 0) return ValueResult.Solved;
            var possibilities = _sets[0].PossibleFlags & _sets[1].PossibleFlags & _sets[2].PossibleFlags;
            if (possibilities == 0)
                return ValueResult.Impossible;
            if (BitOperations.PopCount(possibilities) == 1)
            {
                foreach (var set in _sets)
                {
                    set.TryRemovePossibility(possibilities);
                }
                _mask = possibilities;
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
