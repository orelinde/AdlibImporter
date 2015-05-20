using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orient.Client;

namespace Chronozoom.Adlib.Importer.Entities
{
    public class ContentItem
    {
        public ORID ORID { get; set; }
        public string Title { get; set; }
        public decimal BeginDate { get; set; }
        public decimal EndDate { get; set; }
        public string Source { get; set; }
        public bool HasChildren { get; set; }
        public int Priref { get; set; }
    }
}
