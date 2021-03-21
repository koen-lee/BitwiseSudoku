using System.Collections.Generic;
using System.Linq;

namespace DutchNameGenerator
{
    public class Generator
    {
        private List<string> _firstNames;
        private List<string> _familyNames;

        public Generator()
        {
            _firstNames = new FirstNames().TopNames().ToList();
            _familyNames = new FamilyNames().TopNames().ToList();
        }

        public IEnumerable<string> GenerateUniqueNames()
        {
            return GetCombinations().SelectMany(x => x);
        }

        private IEnumerable<string[]> GetCombinations()
        {
            foreach (var firstnameSet in _firstNames.ChopToPieces(100))
            {
                var firstNames = firstnameSet.ToArray();
                foreach (var lastnameSet in _familyNames.ChopToPieces(100))
                {
                    var combinations = (from first in firstNames
                        from last in lastnameSet
                        select string.Join(' ', first, last)).ToArray();
                    combinations.Shuffle();
                    yield return combinations;
                }
            }
        }
    }
}