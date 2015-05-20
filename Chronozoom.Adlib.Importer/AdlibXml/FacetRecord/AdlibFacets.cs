using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Chronozoom.Adlib.Importer.AdlibXml.FacetRecord
{
    [XmlRoot("adlibXML")]
    public class AdlibFacets
    {
        [XmlElement("diagnostic")]
        public Diagnostic Diagnostic { get; set; }

        [XmlArray("recordList")]
        [XmlArrayItem("record")]
        public AdlibFacetRecord[] Records { get; set; }
    }

    public class Diagnostic
    {
        [XmlElement("hits")]
        public string Hits { get; set; }
    }

    public class AdlibFacetRecord
    {
        [XmlElement("term")] 
        public string Term { get; set; }

        [XmlElement("count")] 
        public string Count { get; set; }
    }
}
