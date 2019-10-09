using System;
using System.Collections.Generic;

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

        internal void Add(Field field)
        {
            _fields.Add(field);
        }
    }
}

