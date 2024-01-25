using GSOD_DataProcessor.Models;
using HtmlAgilityPack;

namespace GSOD_DataProcessor.Business
{
    public class NoaaSiteParsing
    {
        public static bool CheckNoaaSiteIfUpdateAvailable()
        {
            Logging.Log("CheckNoaaSiteIfUpdateAvailable", "Start");
            bool retVal = true;
            using (var context = new weatheredContext())
            {
                var currentYear = DateTime.Now.Year.ToString();
                var html = AppSettings.NoaaGsodUri;
                HtmlWeb web = new HtmlWeb();
                var htmlDoc = web.Load(html);
                var node = htmlDoc.DocumentNode.SelectSingleNode("//table");
                HtmlNodeCollection childNodes = node.ChildNodes;
                ArchiveTable currentYearUpdateDate = childNodes.Where(x => x.Name == "tr" && x.ChildNodes[0].InnerText.StartsWith(currentYear)).Select(x => new ArchiveTable(x)).First();
                if (context.NoaaArchiveUpdates.FirstOrDefault(x => x.Filename == currentYear) == null)
                {
                    NoaaArchiveUpdate noaaArchiveUpdate = new NoaaArchiveUpdate { Filename = currentYear, Lastsiteupdate = currentYearUpdateDate.LastModified };
                    context.NoaaArchiveUpdates.Add(noaaArchiveUpdate);
                    context.SaveChanges();
                    Logging.Log("CheckNoaaSiteIfUpdateAvailable", "Added New Archive Entry to DB");
                    return false;
                }
                if (currentYearUpdateDate.LastModified <= context.NoaaArchiveUpdates.First(x => x.Filename == currentYear).Lastsiteupdate)
                {
                    Logging.Log("CheckNoaaSiteIfUpdateAvailable", "No Update Available");
                    return false;
                }
                
                context.NoaaArchiveUpdates.First(x => x.Filename == currentYear).Lastsiteupdate = currentYearUpdateDate.LastModified;
                context.SaveChanges();
                NoaaArchive.SetNewestArchiveFileName(currentYearUpdateDate.Name!);
                Logging.Log("CheckNoaaSiteIfUpdateAvailable", "Update Is Available");
            }
            return retVal;
        }
    }
}
