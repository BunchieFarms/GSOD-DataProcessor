using GSOD_DataProcessor.Models;
using HtmlAgilityPack;

namespace GSOD_DataProcessor.Business
{
    public class NoaaSiteParsing
    {
        public static bool CheckNoaaSiteIfUpdateAvailable()
        {
            bool retVal = true;
            using (var context = new weatheredContext())
            {
                var currentYear = DateTime.Now.Year.ToString();
                var html = AppSettings.noaaGsodUri;
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
                    return false;
                }
                if (currentYearUpdateDate.LastModified <= context.NoaaArchiveUpdates.First(x => x.Filename == currentYear).Lastsiteupdate)
                    return false;
                context.NoaaArchiveUpdates.First(x => x.Filename == currentYear).Lastsiteupdate = currentYearUpdateDate.LastModified;
                NoaaArchive.SetNewestArchiveFileName(currentYearUpdateDate.Name!);
            }
            return retVal;
        }
    }
}
