using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chronozoom.Adlib.Importer.Mssql;
using Dapper;

namespace Chronozoom.Adlib.Importer.Data
{
    public class MssqlDao
    {
        public int AddContentItem(Mssql.ContentItem parent, int fromid)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DBstring"].ConnectionString);
            con.Open();
            var query = @"insert 
                            into
                            contentitem(Title,BeginDate,EndDate,Source,HasChildren,Priref,ParentId) 
                            Values(@title,@begindate,@enddate,@source,@haschildren,@priref,@parentId);
                            SELECT CAST(SCOPE_IDENTITY() as int)";
            var results = con.Query<int>(query, new
            {
                title = parent.Title,
                begindate = parent.BeginDate,
                enddate = parent.EndDate,
                source = parent.Source,
                haschildren = parent.HasChildren,
                priref = parent.Priref,
                parentId = parent.ParentId

            }).FirstOrDefault();
            con.Close();
            return results;
        }

        public int AddContentItemToTimeline(Mssql.ContentItem ci, int fromid)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DBstring"].ConnectionString);
            con.Open();
            var query = @"insert 
                            into
                            contentitem(Title,BeginDate,EndDate,Source,HasChildren,Priref,TimelineId,ParentId) 
                            Values(@title,@begindate,@enddate,@source,@haschildren,@priref,@timelineId,@parentId);
                            SELECT CAST(SCOPE_IDENTITY() as int)";
            var results = con.Query<int>(query, new
            {
                title = ci.Title,
                begindate = ci.BeginDate,
                enddate = ci.EndDate,
                source = ci.Source,
                haschildren = ci.HasChildren,
                priref = ci.Priref,
                timelineId = ci.TimelineId,
                parentId = 0

            }).FirstOrDefault();
            con.Close();
            return results;
        }

        public int AddTimeline(Timeline timeline)
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DBstring"].ConnectionString);
            con.Open();
            var query = @"insert 
                            into
                            timeline(Title,Description,BeginDate,EndDate) 
                            Values(@title,@description,@begindate,@enddate);
                            SELECT CAST(SCOPE_IDENTITY() as int)";
            var results = con.Query<int>(query, new
            {
                title = timeline.Title,
                description = timeline.Description,
                begindate = timeline.BeginDate,
                enddate = timeline.EndDate
            }).FirstOrDefault();
            con.Close();
            return results;
        }

        public void UpdateParent(decimal begin, decimal end, int parentOrid, bool hasChildren)
        {
            var hasChildreninnumber = 0;
            if (hasChildren) hasChildreninnumber = 1;
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DBstring"].ConnectionString);
            con.Open();
            var query =
                 @"update contentItem set BeginDate=@begindate, EndDate=@enddate, HasChildren=@haschildren where Id=@id";
            con.Execute(query, new
            {
                begindate = begin,
                enddate = end,
                haschildren = hasChildreninnumber,
                Id = parentOrid
            });
            con.Close();
        }


    }
}
