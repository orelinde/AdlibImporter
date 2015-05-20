using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Chronozoom.Adlib.Importer.AdlibXml.SearchRecord
{
    [XmlRoot("adlibXML")]
    public class AdlibSearchRecords
    {

        [XmlElement("diagnostic")]
        public Diagnostic Diagnostics { get; set; }

        [XmlArray("recordList")]
        [XmlArrayItem("record")]
        public AdlibSearchRecord[] Records { get; set; }
    }

    public class Diagnostic
    {
        [XmlElement("hits")]
        public string Hits { get; set; }
    }

    public class AdlibSearchRecord
    {
        [XmlAttribute("priref")]
        public int Priref { get; set; }

        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("reproduction")]
        public Reproduction[] Reproductions;

        [XmlElement("production.date.start")]
        public string Begin { get; set; }

        [XmlElement("production.date.end")]
        public string End { get; set; }
    }

    public class Reproduction
    {
        [XmlElement("reproduction.identifier_URL")]
        public string Identifier { get; set; }

        [XmlElement("reproduction.reference")]
        public string Reference { get; set; }
    }
}

