using System;
using System.IO;
using System.Linq;
using System.Text;
using HeavyRainAnalysis;
using Xunit;

namespace HeavyRainAnalysisTests
{
    public class ParserTests
    {
        [Fact]
        public void Parser_parses_a_line()
        {
            var line =
                "2003-04-01 15:50:00  235_R_obs           De Kooy waarneemterrein                         52.926944           4.781111            1.2                 600                 600                 60                  2.4                 2.1                 ";
            var underTest = new Parser();
            var stream = new MemoryStream();
            stream.Write(Encoding.ASCII.GetBytes(line));
            stream.Position = 0;
            var result = underTest.Parse(stream).Single();

            var roundtrip = underTest.Serialize(result);
            Assert.Equal(line, roundtrip);
        }
    }
}
