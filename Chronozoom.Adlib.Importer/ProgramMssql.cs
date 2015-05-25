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
    class ProgramMssql
    {
        private static string APIURL = "http://amdata.adlibsoft.com/wwwopac.ashx?database=AMcollect&search=all&limit=300&xmltype=grouped";
        private static string APIFACETS = "http://amdata.adlibsoft.com/wwwopac.ashx?command=facets&database=amcollect&search=all&facet=creator&xmltype=grouped&limit=300";
        private static MssqlDao dao = new MssqlDao();
        public ProgramMssql()
        {



        }

        public static void GenerateTimeline()
        {
            var timeline = new Mssql.Timeline() { Title = "Musea in the Netherlands", Description = "This timeline is all about the first 400 creators of the Amsterdam museum. Also do these creators show their work in chronological order." };
            int timelineOrid = dao.AddTimeline(timeline);
            var first = new Mssql.ContentItem();
            first.Title = "Amsterdam Museum collection sorted by date => creator";
            first.Source = String.Empty;
            first.ParentId = timelineOrid;
            int firstOrid = dao.AddContentItemToTimeline(first, timelineOrid);

            Console.WriteLine("Download facets");
            AdlibFacets facets = DownloadCreatorFacets();
            Console.WriteLine("End facets");

            Console.WriteLine("Download by creator");
            List<Task<Tuple<string, AdlibSearchRecords>>> downloadItemsByCreator = DownloadItemsByCreator(facets);
            Console.WriteLine("End download by creator");

            Console.WriteLine("Parse to cz domain");
            List<Mssql.ContentItem> items = ParseAdlibItemsToContentItems(downloadItemsByCreator, firstOrid);
            Console.WriteLine("End parse to cz domain");

            var begin = items.Min(r => r.BeginDate);
            var end = items.Max(r => r.EndDate);
            if (end < begin)
            {
                end = begin;
            }
            dao.UpdateParent(begin, end, firstOrid, true);
        }

        public static void GenerateAnneFrankTimeline()
        {
            var timeline = new Mssql.Timeline()
            {
                BeginDate = 1900,
                EndDate = 1960,
                Description =
                    "A timeline about the first and second worldwar. This timeline has special focus to Anne Frank",
                Title = "Worldwar I and II"
            };
            var timelineId = dao.AddTimeline(timeline);

            var wwi = new Mssql.ContentItem()
            {
                BeginDate = 1914,
                EndDate = 1918,
                HasChildren = true,
                TimelineId = timelineId,
                Priref = 0,
                Title = "World War I",
                Source = "http://upload.wikimedia.org/wikipedia/commons/2/20/WWImontage.jpg",
                ParentId = 0
            };
            var wwii = new Mssql.ContentItem()
            {
                BeginDate = 1940,
                EndDate = 1945,
                HasChildren = true,
                TimelineId = timelineId,
                Priref = 0,
                Title = "World War II",
                Source = "http://upload.wikimedia.org/wikipedia/commons/5/54/Infobox_collage_for_WWII.PNG",
                ParentId = 0
            };
            int wwiId = dao.AddContentItemToTimeline(wwi, timelineId);
            int wwiiId = dao.AddContentItemToTimeline(wwii, timelineId);
            var i1 = new Mssql.ContentItem()
            {
                BeginDate = 1914M,
                EndDate = 1914M,
                HasChildren = false,
                Title = "German soldiers in a railway goods wagon on the way to the front in 1914",
                Source = "http://upload.wikimedia.org/wikipedia/commons/c/c0/German_soldiers_in_a_railroad_car_on_the_way_to_the_front_during_early_World_War_I%2C_taken_in_1914._Taken_from_greatwar.nl_site.jpg",
                ParentId = wwiId,
                Priref = 0,

            };
            var i2 = new Mssql.ContentItem()
            {
                BeginDate = 1914M,
                EndDate = 1914M,
                HasChildren = false,
                Title = "Melbourne recruiting WWI",
                Source = "http://upload.wikimedia.org/wikipedia/commons/b/bd/Melbourne_recruiting_WWI.jpg",
                ParentId = wwiId,
                Priref = 0
            };
            var i3 = new Mssql.ContentItem()
            {
                BeginDate = 1916M,
                EndDate = 1916M,
                HasChildren = false,
                Title = "Royal Irish Rifles ration party",
                Source = "http://upload.wikimedia.org/wikipedia/commons/thumb/f/f5/Royal_Irish_Rifles_ration_party_Somme_July_1916.jpg/800px-Royal_Irish_Rifles_ration_party_Somme_July_1916.jpg",
                ParentId = wwiId,
                Priref = 0
            };
            var i4 = new Mssql.ContentItem()
            {
                BeginDate = 1916M,
                EndDate = 1916M,
                HasChildren = false,
                Title = "British 55th Division gas casualties",
                Source = "http://upload.wikimedia.org/wikipedia/commons/thumb/d/dc/British_55th_Division_gas_casualties_10_April_1918.jpg/800px-British_55th_Division_gas_casualties_10_April_1918.jpg",
                ParentId = wwiId,
                Priref = 0
            };
            dao.AddContentItem(i1, wwiId);
            dao.AddContentItem(i2, wwiId);
            dao.AddContentItem(i3, wwiId);
            dao.AddContentItem(i4, wwiId);
            var wii1 = new Mssql.ContentItem()
            {
                BeginDate = 1940M,
                EndDate = 1940M,
                HasChildren = false,
                Title = "View from St Paul's Cathedral after the Blitz",
                Source =
                    "http://upload.wikimedia.org/wikipedia/commons/5/5d/View_from_St_Paul%27s_Cathedral_after_the_Blitz.jpg",
                Priref = 0,
                ParentId = wwiiId
            };
            var wii2 = new Mssql.ContentItem()
            {
                BeginDate = 1941M,
                EndDate = 1941M,
                HasChildren = false,
                Title = "Australians at Tobruk",
                Source =
                    "http://upload.wikimedia.org/wikipedia/commons/thumb/8/8a/AustraliansAtTobruk.jpg/587px-AustraliansAtTobruk.jpg",
                Priref = 0,
                ParentId = wwiiId
            };
            var wii3 = new Mssql.ContentItem()
            {
                BeginDate = 1943M,
                EndDate = 1943M,
                HasChildren = false,
                Title = "SBD VB-16 over USS Washington",
                Source =
                    "http://upload.wikimedia.org/wikipedia/commons/thumb/0/07/SBD_VB-16_over_USS_Washington_1943.jpg/771px-SBD_VB-16_over_USS_Washington_1943.jpg",
                Priref = 0,
                ParentId = wwiiId
            };
            var wii4 = new Mssql.ContentItem()
            {
                BeginDate = 1940M,
                EndDate = 1945M,
                HasChildren = true,
                Title = "Anne Frank",
                Source = "http://upload.wikimedia.org/wikipedia/en/4/47/Anne_Frank.jpg",
                Priref = 0,
                ParentId = wwiiId
            };
            dao.AddContentItem(wii1, wwiiId);
            dao.AddContentItem(wii2, wwiiId);
            dao.AddContentItem(wii3, wwiiId);
            int annefrankId = dao.AddContentItem(wii4, wwiiId);
            var anne1 = new Mssql.ContentItem()
            {
                BeginDate = 1944M,
                EndDate = 1944M,
                HasChildren = false,
                Title = "Anne Frank House Bookcase",
                Source =
                    "http://upload.wikimedia.org/wikipedia/commons/thumb/b/bb/AnneFrankHouse_Bookcase.jpg/430px-AnneFrankHouse_Bookcase.jpg",
                ParentId = annefrankId
            };
            var anne2 = new Mssql.ContentItem()
            {
                BeginDate = 1941M,
                EndDate = 1941M,
                HasChildren = false,
                Title = "Hut AnneFrank Westerbork",
                Source =
                    "http://upload.wikimedia.org/wikipedia/commons/thumb/4/49/Hut-AnneFrank-Westerbork.jpg/800px-Hut-AnneFrank-Westerbork.jpg",
                ParentId = annefrankId
            };
            var anne3 = new Mssql.ContentItem()
            {
                BeginDate = 1945M,
                EndDate = 1945M,
                HasChildren = false,
                Title = "Diary Anne Frank",
                Source =
                    "http://upload.wikimedia.org/wikipedia/en/thumb/4/47/Het_Achterhuis_%28Diary_of_Anne_Frank%29_-_front_cover%2C_first_edition.jpg/220px-Het_Achterhuis_%28Diary_of_Anne_Frank%29_-_front_cover%2C_first_edition.jpg",
                ParentId = annefrankId
            };
            dao.AddContentItem(anne1, annefrankId);
            dao.AddContentItem(anne2, annefrankId);
            dao.AddContentItem(anne3, annefrankId);
        }

        private static List<Mssql.ContentItem> ParseAdlibItemsToContentItems(List<Task<Tuple<string, AdlibSearchRecords>>> downloadItemsByCreator, int fromOrid)
        {
            List<Mssql.ContentItem> parents = new List<Mssql.ContentItem>();
            foreach (Task<Tuple<string, AdlibSearchRecords>> task in downloadItemsByCreator)
            {
                var term = task.Result.Item1;
                var items = task.Result.Item2;

                if (items.Records.Count() > 2)
                {
                    Mssql.ContentItem parent = new Mssql.ContentItem();
                    parent.Title = term;

                    int parentOrid = dao.AddContentItem(parent, fromOrid);
                    var contentItems = new List<Mssql.ContentItem>();
                    foreach (var adlibSearchRecord in items.Records)
                    {
                        if (adlibSearchRecord.Title == null)
                        {
                            adlibSearchRecord.Title = "No title";
                        }
                        var item = new Mssql.ContentItem();
                        decimal beginDate = 0;
                        decimal endDate = 0;
                        var parsedBegin = Decimal.TryParse(adlibSearchRecord.Begin, out beginDate);
                        var parsedEnd = Decimal.TryParse(adlibSearchRecord.End, out endDate);
                        if (endDate < beginDate) endDate = beginDate;
                        item.BeginDate = adlibSearchRecord.Begin == null ? 0 : beginDate;
                        item.EndDate = adlibSearchRecord.End == null ? 0 : endDate;
                        item.Title = adlibSearchRecord.Title;
                        item.Priref = adlibSearchRecord.Priref;
                        item.ParentId = parentOrid;
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
                        else
                        {
                            item.Source = String.Empty; ;
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

    }
}
