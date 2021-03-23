using System.Linq;
using DutchNameGenerator;
using Xunit;

namespace DutchFamilyNamesTests
{
    public class ListExtensionsTests
    {
        [Fact]
        public void ChopToPiecesTests()
        {
            var undertest = Enumerable.Range(0, 100);
            var pieces = undertest.ChopToPieces(10);
            Assert.Equal(10, pieces.Count());
            int i = 0;
            foreach (var enumerable in pieces)
            {
                Assert.Equal(10, enumerable.Count());
                foreach (var item in enumerable)
                {
                    Assert.Equal(i, item);
                    i++;
                }
            }
        }

        [Fact]
        public void ChopToPieces_partial_piece()
        {
            var underTest = Enumerable.Range(0, 15);
            var pieces = underTest.ChopToPieces(10);
            Assert.Equal(2, pieces.Count());
            Assert.Equal(10, pieces.First().Count());
            Assert.Equal(5, pieces.Last().Count());
        }
    }
}