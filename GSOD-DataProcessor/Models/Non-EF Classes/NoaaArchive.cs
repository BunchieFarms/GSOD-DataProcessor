namespace GSOD_DataProcessor.Models
{
    public static class NoaaArchive
    {
        private static string newestArchiveFileName = "";
        private static string uncompressedFolderName = "";
        public static string NewestArchiveFileName { get => newestArchiveFileName; }
        public static string UncompressedFolderName { get => uncompressedFolderName; }
        public static void SetNewestArchiveFileName(string fileName)
        {
            newestArchiveFileName = fileName;
            uncompressedFolderName = fileName.Split(".")[0];
        }
    }
}
