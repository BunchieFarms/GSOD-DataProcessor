using GSOD_DataProcessor.Models;
using GSOD_DataProcessor.Shared;
using HtmlAgilityPack;
using MongoDB.Driver;

namespace GSOD_DataProcessor.Business;

public class NoaaSiteParsing
{
    public static async Task<bool> CheckNoaaSiteIfUpdateAvailable()
    {
        var _noaaArchiveUpdateCollection = MongoBase.WeatheredDB.GetCollection<NoaaArchiveUpdate>(Constants.NoaaArchiveUpdate);

        Logging.Log("CheckNoaaSiteIfUpdateAvailable", "Start");
        var currentYear = DateTime.Now.Year.ToString();
        var html = AppSettings.NoaaGsodUri;
        HtmlWeb web = new HtmlWeb();
        var htmlDoc = web.Load(html);
        var node = htmlDoc.DocumentNode.SelectSingleNode("//table");
        HtmlNodeCollection childNodes = node.ChildNodes;
        ArchiveTable currentYearUpdateDate = childNodes.Where(x => x.Name == "tr" && x.ChildNodes[0].InnerText.StartsWith(currentYear)).Select(x => new ArchiveTable(x)).First();

        var currYearDocument = _noaaArchiveUpdateCollection.AsQueryable().FirstOrDefault(x => x.FileName == currentYear);
        if (currYearDocument == null)
        {
            NoaaArchiveUpdate noaaArchiveUpdate = new NoaaArchiveUpdate { FileName = currentYear, LastSiteUpdate = currentYearUpdateDate.LastModified };
            await _noaaArchiveUpdateCollection.InsertOneAsync(noaaArchiveUpdate);
            Logging.Log("CheckNoaaSiteIfUpdateAvailable", "Added New Archive Entry to DB");
            return true;
        }
        if (currentYearUpdateDate.LastModified <= currYearDocument.LastSiteUpdate)
        {
            Logging.Log("CheckNoaaSiteIfUpdateAvailable", "No Update Available");
            return false;
        }

        var updatedDocument = Builders<NoaaArchiveUpdate>.Update.Set(x => x.LastSiteUpdate, currentYearUpdateDate.LastModified);
        await _noaaArchiveUpdateCollection.UpdateOneAsync(x => x.LastSiteUpdate == currYearDocument.LastSiteUpdate, updatedDocument);
        NoaaArchive.SetNewestArchiveFileName(currentYearUpdateDate.Name!);
        Logging.Log("CheckNoaaSiteIfUpdateAvailable", "Update Is Available");
        
        return true;
    }
}
