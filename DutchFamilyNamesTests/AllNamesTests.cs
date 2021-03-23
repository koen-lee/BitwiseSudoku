using System.Linq;
using DutchNameGenerator;
using Xunit;

namespace DutchFamilyNamesTests
{
    public class AllNamesTests
    {
        [Fact]
        public void There_are_lots_of_names()
        {
            var undertest = new Generator();
            var count = undertest.GenerateUniqueNames().Count();
            Assert.True(count > 100000);
        }
    }
}