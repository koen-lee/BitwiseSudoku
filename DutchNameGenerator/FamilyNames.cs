using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace DutchNameGenerator
{
    public class FamilyNames
    {
        public IEnumerable<string> TopNames()
        {
            using var stream = File.OpenRead("achternamentop10000.xml");
            var serializer = new XmlSerializer(typeof(Root));
            var root = (Root)serializer.Deserialize(stream);
            return root.FamilyNames.Select(n =>
            {
                if (n.prefix == string.Empty)
                    return n.naam;
                return n.prefix + " " + n.naam;
            }).Distinct();
        }

        [XmlType("root")]
        public class Root
        {
            [XmlElement("record")]
            public FamilyName[] FamilyNames { get; set; }
            public class FamilyName
            {
                public string prefix;
                public string naam;
            }
        }
    }
}