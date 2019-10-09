using System;
using System.Collections.Generic;
using System.Numerics;

namespace SudokuHelper
{
    class SudokuSet
    {
        List<Field> _fields = new List<Field>(9);
        public SudokuSet(ulong possibleFlags)
        {
            PossibleFlags = possibleFlags;
        }

        public ulong PossibleFlags { get; private set; }

        internal bool TryRemovePossibility(ulong flag)
        {
            if ((PossibleFlags & flag) == 0)
                return false;
            PossibleFlags &= ~flag;
            return true;
        }

        public bool IsDone => PossibleFlags == 0;

        public ValueResult TrySetValue()
        {
            foreach (var field in _fields)
            {
                if (field.HasValue) continue;
                if (field.TrySetValue() == ValueResult.Set) return ValueResult.Set;
                var possible = field.PossibleMask;
                foreach (var otherfield in _fields)
                {
                    if (field == otherfield) continue;
                    if (otherfield.HasValue) continue;
                    possible = possible & ~otherfield.PossibleMask;
                }
                if (BitOperations.PopCount(possible) == 1)
                    field.SetMask(possible);
            }
            return ValueResult.NotSet;
        }

        internal void Add(Field field)
        {
            _fields.Add(field);
        }
    }
}

