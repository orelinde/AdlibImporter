using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Chronozoom.Adlib.Importer.AdlibXml.FacetRecord;
using Chronozoom.Adlib.Importer.AdlibXml.SearchRecord;
using Chronozoom.Adlib.Importer.Data;
using Chronozoom.Adlib.Importer.Entities;
using Newtonsoft.Json;
using Orient.Client;

namespace Chronozoom.Adlib.Importer
{
    class Program
    {
        private static string APIURL = "http://amdata.adlibsoft.com/wwwopac.ashx?database=AMcollect&search=all&limit=400&xmltype=grouped";
        private static string APIFACETS = "http://amdata.adlibsoft.com/wwwopac.ashx?command=facets&database=amcollect&search=all&facet=creator&xmltype=grouped&limit=400";
        private static string HOST = "84.25.163.6"; //84.25.163.6 david ip
        private static int PORT = 2424;
        private static string SERVERUSERNAME = "root";
        private static string SERVERPASSWORD = "password"; //8E644FE1AE29539AF2208479DE1796D79BE579627CB28874054E665B0562BBC7
        private static string USERNAME = "chronozoom";
        private static string PASSWORD = "password";
        private static string DATABASE = "ChronoZoom";
        private static OrientDao dao;
        static void Main(string[] args)
        {
            //dao = new OrientDao();
            //dao.Connect(HOST, PORT, SERVERUSERNAME, SERVERPASSWORD);
            //dao.Connect(HOST, PORT, USERNAME, PASSWORD, DATABASE);
            //CreateDatabaseIfNotExists(dao);

            //Timeline timeline = new Timeline() { Title = "Musea in the Netherlands" };
            //ORID timelineOrid = dao.AddTimeline(timeline);
            //ContentItem first = new ContentItem();
            //first.Title = "Amsterdam Museum collection sorted by date => creator";

            //ORID firstOrid = dao.AddContentItem(first, timelineOrid);

            //Console.WriteLine("Download facets");
            //AdlibFacets facets = DownloadCreatorFacets();
            //Console.WriteLine("End facets");

            //Console.WriteLine("Download by creator");
            //List<Task<Tuple<string, AdlibSearchRecords>>> downloadItemsByCreator = DownloadItemsByCreator(facets);
            //Console.WriteLine("End download by creator");

            //Console.WriteLine("Parse to cz domain");
            //List<ContentItem> items = ParseAdlibItemsToContentItems(downloadItemsByCreator, firstOrid);
            //Console.WriteLine("End parse to cz domain");

            //var begin = items.Min(r => r.BeginDate);
            //var end = items.Max(r => r.EndDate);
            //if (end < begin)
            //{
            //    end = begin;
            //}
            //dao.UpdateParent(begin, end, firstOrid, true);

            ////first.ContentItems = items;
            ////first.BeginDate = first.ContentItems.Min(r => r.BeginDate);
            ////first.EndDate = first.ContentItems.Max(r => r.EndDate);
            ////first.ContentItems = items;
            ////timeline.ContentItems = new List<ContentItem> { first };
            ////timeline.BeginDate = first.ContentItems.Min(r => r.BeginDate);
            ////timeline.EndDate = first.ContentItems.Max(r => r.EndDate);
            ////if (timeline.BeginDate > timeline.EndDate) timeline.EndDate = timeline.BeginDate;
            ////if (first.BeginDate > first.EndDate) first.EndDate = first.BeginDate;
            ////string json = JsonConvert.SerializeObject(timeline, Formatting.Indented);
            //////write string to file
            ////System.IO.File.WriteAllText(@"D:\path1.txt", json);
            ////string json = File.ReadAllText(@"D:\path1.txt");
            ////Timeline timeline = JsonConvert.DeserializeObject<Timeline>(json);
            ////dao.Disconnect();
            /// 
            ProgramMssql.GenerateAnneFrankTimeline();
            Console.WriteLine("Ended....... press a key to close");
            Console.ReadKey();
        }

        private static List<ContentItem> ParseAdlibItemsToContentItems(List<Task<Tuple<string, AdlibSearchRecords>>> downloadItemsByCreator, ORID fromOrid)
        {
            List<ContentItem> parents = new List<ContentItem>();
            foreach (Task<Tuple<string, AdlibSearchRecords>> task in downloadItemsByCreator)
            {
                var term = task.Result.Item1;
                var items = task.Result.Item2;

                if (items.Records.Count() > 2)
                {
                    ContentItem parent = new ContentItem();
                    parent.Title = term;

                    ORID parentOrid = dao.AddContentItem(parent, fromOrid);
                    var contentItems = new List<ContentItem>();
                    foreach (var adlibSearchRecord in items.Records)
                    {
                        ContentItem item = new ContentItem();
                        decimal beginDate = 0;
                        decimal endDate = 0;
                        var parsedBegin = Decimal.TryParse(adlibSearchRecord.Begin, out beginDate);
                        var parsedEnd = Decimal.TryParse(adlibSearchRecord.End, out endDate);
                        if (endDate < beginDate) endDate = beginDate;
                        item.BeginDate = adlibSearchRecord.Begin == null ? 0 : beginDate;
                        item.EndDate = adlibSearchRecord.End == null ? 0 : endDate;
                        item.Title = adlibSearchRecord.Title;
                        item.Priref = adlibSearchRecord.Priref;

                        if (adlibSearchRecord.Reproductions != null)
                        {
                            Reproduction reproduction = adlibSearchRecord.Reproductions[0];
                            if (!reproduction.Reference.Contains("."))
                            {
                                item.Source = "http://ahm.adlibsoft.com/ahmimages/" + reproduction.Reference + ".jpg";
                            }
                            else
                            {
                                item.Source = "http://ahm.adlibsoft.com/ahmimages/" + reproduction.Reference;
                            }
                        }

                        contentItems.Add(item);
                        dao.AddContentItem(item, parentOrid);

                    }
                    if (contentItems.Any())
                    {
                        var begin = contentItems.Min(r => r.BeginDate);
                        var end = contentItems.Max(r => r.EndDate);
                        if (end < begin)
                        {
                            end = begin;
                        }
                        dao.UpdateParent(begin, end, parentOrid, true);
                        parent.BeginDate = begin;
                        parent.EndDate = end;
                    }
                    parents.Add(parent);
                }
            }
            return parents;
        }

        private static List<Task<Tuple<String, AdlibSearchRecords>>> DownloadItemsByCreator(AdlibFacets facets)
        {
            List<Task<Tuple<String, AdlibSearchRecords>>> tasks = new List<Task<Tuple<String, AdlibSearchRecords>>>();
            foreach (AdlibFacetRecord record in facets.Records)
            {
                var record1 = record;
                tasks.Add(Task.Run(() =>
                {
                    var url = "http://amdata.adlibsoft.com/wwwopac.ashx?database=AMcollect&xmltype=grouped&search=creator='" + record1.Term + "'";
                    AdlibSearchRecords adlibSearchRecords;
                    try
                    {
                        string xml = DownloadXml(url);
                        adlibSearchRecords = ParseToAdlibRecords(xml);
                    }
                    catch (AggregateException ex)
                    {
                        Console.WriteLine("Exception " + url);
                        adlibSearchRecords = new AdlibSearchRecords();
                        adlibSearchRecords.Records = new AdlibSearchRecord[0];
                        adlibSearchRecords.Diagnostics = new Chronozoom.Adlib.Importer.AdlibXml.SearchRecord.Diagnostic();
                        return new Tuple<string, AdlibSearchRecords>(record1.Term, adlibSearchRecords);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception " + url);
                        adlibSearchRecords = new AdlibSearchRecords();
                        adlibSearchRecords.Records = new AdlibSearchRecord[0];
                        adlibSearchRecords.Diagnostics = new Chronozoom.Adlib.Importer.AdlibXml.SearchRecord.Diagnostic();
                        return new Tuple<string, AdlibSearchRecords>(record1.Term, adlibSearchRecords);
                    }
                    return new Tuple<string, AdlibSearchRecords>(record1.Term, adlibSearchRecords);
                }));
            }
            Task.WaitAll(tasks.ToArray());
            return tasks;
        }
        private static AdlibFacets DownloadCreatorFacets()
        {
            string xmlFacets = DownloadXml(APIFACETS);
            return ParseToAdlibFacets(xmlFacets);
        }
        private static AdlibFacets ParseToAdlibFacets(string xmlFacets)
        {
            var xmlSerializer = new XmlSerializer(typeof(AdlibFacets));
            StringReader reader = new StringReader(xmlFacets);
            var adlibData = (AdlibFacets)xmlSerializer.Deserialize(reader);
            return adlibData;
        }
        private static AdlibSearchRecords ParseToAdlibRecords(string xmlFacets)
        {

            var xmlSerializer = new XmlSerializer(typeof(AdlibSearchRecords));
            StringReader reader = new StringReader(xmlFacets);
            var adlibData = (AdlibSearchRecords)xmlSerializer.Deserialize(reader);
            return adlibData;
        }
        private static string DownloadXml(string url)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = Timeout.InfiniteTimeSpan;
                return client.GetStringAsync(new Uri(url)).Result;
            }
        }

        private static void CreateDatabaseIfNotExists(OrientDao orientDao)
        {
            var created = orientDao.CreateDatabase(DATABASE);
        }
    }
}
