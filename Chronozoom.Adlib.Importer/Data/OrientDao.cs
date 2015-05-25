using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chronozoom.Adlib.Importer.Entities;
using Orient.Client;
using Orient.Client.API.Query.Interfaces;
using Orient.Client.API.Types;

namespace Chronozoom.Adlib.Importer.Data
{
    public class OrientDao
    {
        private ODatabase db;
        private OServer server;

        public void Connect(string host, int port, string username, string password, string database)
        {
            OClient.CreateDatabasePool(host, port, database, ODatabaseType.Graph, username, password, 10, database,false);
            db = new ODatabase(database);
        }

        public ORID AddTimeline(Timeline timeline)
        {
            var oDocument = db.Create.Vertex<Timeline>(timeline).Run();
            timeline.ORID = oDocument.ORID;
            return oDocument.ORID;
        }
        public void Disconnect()
        {

            db.Close();
        }

        public ODatabase GetContext()
        {
            return db;
        }

        public void Connect(string host, int port, string username, string password)
        {
            server = new OServer(host, port, username, password);
        }

        public bool CreateDatabase(String databasename)
        {
            if (server.DatabaseExist(databasename, OStorageType.PLocal))
            {
                if (db.Schema.Classes().Count() <= 11)
                {
                    CreateClassesInDatabase();
                }
                return false;
            }
            var created = server.CreateDatabase(databasename, ODatabaseType.Graph, OStorageType.PLocal);
            CreateClassesInDatabase();
            return created;
        }

        public void CreateClassesInDatabase()
        {
            db.Create.Class<Timeline>().Extends<OVertex>().CreateProperties().Run();
            db.Create.Class<ContentItem>().Extends<OVertex>().CreateProperties().Run();
            db.Create.Class<Contains>("Contains").Extends<OEdge>().CreateProperties().Run();
        }

        public Timeline getTimeline()
        {
            List<Timeline> docs = db.Select().From("Timeline").Limit(1).ToList<Timeline>();
            return docs[0];
        }

        public ORID AddContentItem(ContentItem parent, ORID fromOrid)
        {
            var oDocument = db.Create.Vertex<ContentItem>(parent).Run();
            db.Create.Edge("Contains").From(fromOrid).To(oDocument.ORID).Run();
            parent.ORID = oDocument.ORID;
            return oDocument.ORID;
        }

        public void DeleteContentItem(ORID orid)
        {
            int run = db.Delete.Vertex(orid).Run();
        }

        public bool SchemaExists()
        {
            if (db != null)
            {
                if (db.Schema != null)
                {
                    if (db.Schema.Classes() == null)
                    {
                        return false;
                    }
                    return db.Schema.Classes().Any();
                }
            }
            return db.Schema.Classes().Any();
        }

        public void UpdateParent(decimal begin, decimal end, ORID parentOrid, bool hasChildren)
        {
            db.Update(parentOrid).Set("BeginDate", begin).Set("EndDate", end).Set("HasChildren", hasChildren).Run();
        }

        public bool Exists(string databasename)
        {
            return server.DatabaseExist(databasename, OStorageType.PLocal);
        }
    }
}
