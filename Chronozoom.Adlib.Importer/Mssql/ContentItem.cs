using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chronozoom.Adlib.Importer.Mssql
{
    public class ContentItem
    {
        public int Id { get; set; }
        public decimal BeginDate { get; set; }
        public decimal EndDate { get; set; }
        public string Title { get; set; }
        public bool HasChildren { get; set; }
        public string Source { get; set; }
        public int Priref { get; set; }
        public int ParentId { get; set; }
        public int TimelineId { get; set; }
    }
}
