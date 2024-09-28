using MDO2.Core.Model.Metadata;
using BulkFileIdentificationHandler.Core.Data.Model;

namespace BulkFileIdentificationHandler.Core.Data
{
    public interface IDataService
    {
        Task<List<IndexDocumentTypeEntity>> GetIndexDocumentTypeAsync(MetadataFile metadataFile);
        Task<List<SplitDocumentTypeEntity>> GetSplitDocumentTypeAsync(MetadataFile metadataFile);
    }
}
