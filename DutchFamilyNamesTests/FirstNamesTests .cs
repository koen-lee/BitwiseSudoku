using System.IO;
using System.Xml.Serialization;
using DutchNameGenerator;
using Xunit;
using Xunit.Abstractions;

namespace DutchFamilyNamesTests
{
    public class FirstNamesTests
    {
        private readonly ITestOutputHelper _helper;
        private FamilyNames _underTest;

        public FirstNamesTests(ITestOutputHelper helper)
        {
            _helper = helper;
            _underTest = new FamilyNames();
        }

        [Fact]
        public void AllNames()
        {
            var topNames = _underTest.TopNames();
            foreach (var name in topNames)
            {
                _helper.WriteLine(name);
            }
        }

        [Fact]
        public void SerializeNames()
        {
            var root = new FamilyNames.Root() {FamilyNames = new[]
            {
                new FamilyNames.Root.FamilyName{naam = "Leeuwen", prefix = "van"},
                new FamilyNames.Root.FamilyName{naam = "Bogaert", prefix = "van den"}
            }};

            var serializer = new XmlSerializer(typeof(FamilyNames.Root));
            StringWriter writer = new StringWriter();
            serializer.Serialize(writer, root);
            var result = writer.ToString();
            Assert.NotEmpty(result);
        }
    }
}
