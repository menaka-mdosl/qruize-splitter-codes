using MDO2.Core.Model.Metadata;
using QruizeBatchReportHandler.Core.Data.Model;

namespace QruizeBatchReportHandler.Core.Data
{
    public interface IDataService
    {
        Task<List<IndexDocumentTypeEntity>> GetIndexDocumentTypeAsync(MetadataFile metadataFile);
        Task<List<SplitDocumentTypeEntity>> GetSplitDocumentTypeAsync(MetadataFile metadataFile);
    }
}
