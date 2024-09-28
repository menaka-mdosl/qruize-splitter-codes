namespace QruizeMetadataHandler.Core.App.Model
{
    public class FileProcessingResult
    {
        public FileProcessingResult()
        {
            Failed = new List<ProcessedFileEntry>();
        }

        public List<ProcessedFileEntry> Failed { get; internal set; }
    }
}
