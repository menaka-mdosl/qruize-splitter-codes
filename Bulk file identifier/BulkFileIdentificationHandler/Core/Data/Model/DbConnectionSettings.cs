namespace BulkFileIdentificationHandler.Core.Data.Model
{
    public class DbConnectionSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string SplitterIndexingCollectionName { get; set; }
        public string SplitterSplittingCollectionName { get; set; }
        public string DocumentTypesCollectionName { get; set; }
        public string AutoIndexDocumentTypesCollectionName { get; set; }
    }
}
