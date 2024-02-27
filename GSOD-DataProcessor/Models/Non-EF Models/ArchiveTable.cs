using HtmlAgilityPack;

namespace GSOD_DataProcessor.Models;

public class ArchiveTable
{
    public string? Name { get; set; }
    public DateTime LastModified { get; set; }
    public ArchiveTable(HtmlNode x)
    {
        if (x.ChildNodes[0].InnerText.StartsWith(NoaaArchive.UncompressedFolderName))
        {
            Name = x.ChildNodes[0].InnerText;
            LastModified = DateTime.Parse(x.ChildNodes[1].InnerText);
        }
    }
}
