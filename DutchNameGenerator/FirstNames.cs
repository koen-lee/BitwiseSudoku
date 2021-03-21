using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace DutchNameGenerator
{
    public class FirstNames
    {
        public IEnumerable<string> TopNamesWithTitle()
        {
            var root = DeserializeData();
            return root.FirstNames.Select(n => Title(n.geslacht) + " " + n.voornaam).Distinct();
        }
        public IEnumerable<string> TopNames()
        {
            var root = DeserializeData();
            return root.FirstNames.Select(n => n.voornaam).Distinct();
        }

        private static Root DeserializeData()
        {
            using var stream = File.OpenRead("voornamentop10000.xml");
            var serializer = new XmlSerializer(typeof(Root));
            var root = (Root)serializer.Deserialize(stream);
            return root;
        }

        private string Title(string geslacht)
        {
            return geslacht switch
            {
                "M" => "Dhr.",
                "V" => "Mevr.",
                _ => "? "
            };
        }

        [XmlType("database")]
        public class Root
        {
            [XmlElement("record")] public FirstName[] FirstNames { get; set; }

            public class FirstName
            {
                public string voornaam;
                public string geslacht;
            }
        }
    }
}
