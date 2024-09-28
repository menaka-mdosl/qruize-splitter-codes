using BulkFileIdentificationHandler.Core.App;
using BulkFileIdentificationHandler.Core.Data.Model;
using BulkFileIdentificationHandler.Core.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MDO2.Core.Model.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.Pdf;

namespace BulkFileIdentificationHandler.Core.Data
{
    public class MongoDataService : IDataService
    {
        private readonly ILogger<MongoDataService> logger;


        private readonly IMongoCollection<IndexDocumentTypeEntity> indexingCollection;

        private readonly IMongoCollection<SplitDocumentTypeEntity> splittingCollection;
        public MongoDataService(IConfigurationRoot configurationRoot, ILogger<MongoDataService> logger)
        {
            this.logger = logger;

            this.logger?.LogDebug("Setting up mongodb client");
            var url = MongoUrl.Create(configurationRoot.GetDbConnectionString());
            var settings = MongoClientSettings.FromUrl(url);
            var client = new MongoClient(settings);
            var database = client.GetDatabase(configurationRoot.GetDbDatabaseName());

            this.logger?.LogDebug("Setting up mongodb collections");
            indexingCollection = database.GetCollection<IndexDocumentTypeEntity>(configurationRoot.GetDbSplitDocumentsIndexingCollectionName());
            splittingCollection = database.GetCollection<SplitDocumentTypeEntity>(configurationRoot.GetDbSplitDocumentsSplittingCollectionName());

            this.logger?.LogDebug("Db setup completed");
        }


        #region Split Document Types
        public async Task<List<SplitDocumentTypeEntity>> GetSplitDocumentTypeAsync(MetadataFile metadataFile)
        {
            string reportSepratorType = "BARCODE";
            string mgmtGrpSearchValue = metadataFile?.GetIndex(MetadataIndexName.ManagementGroup).IndexValue.ToString();
            string pmsSearchValue = metadataFile?.GetIndex(MetadataIndexName.PMS).IndexValue.ToString();
            string hotelsearchValue = metadataFile?.GetIndex(MetadataIndexName.Hotels).IndexValue.ToString();

            //string mgmtGrpSearchValue = "McNeill Hotel Company";
            //string pmsSearchValue = "Fosse";
            //string hotelsearchValue = "JwwAXMC";

            logger?.LogTrace($"Get DB Document. Pms= {mgmtGrpSearchValue},{pmsSearchValue},{hotelsearchValue}");

            

            var filteredDocuments = splittingCollection.AsQueryable().Where(d => d.ReportSeperatorType == reportSepratorType).ToList();

            var exactMatch = filteredDocuments.AsQueryable().Where(d =>
            d.Hotel == hotelsearchValue && d.PMS == pmsSearchValue && d.ManagementGroup == mgmtGrpSearchValue).ToList();

            if (exactMatch != null && exactMatch.Any())
                return await Task.FromResult(exactMatch);

            //var hotelMatch = filteredDocuments.AsQueryable().Where(d =>
            //    d.Hotel == hotelsearchValue && d.PMS == "*" && d.ManagementGroup == "*").ToList();

            //if (hotelMatch != null && hotelMatch.Any())
            //    return await Task.FromResult(hotelMatch);

            var pmsMatch = filteredDocuments.AsQueryable().Where(d =>
                d.Hotel == "*" && d.PMS == pmsSearchValue && d.ManagementGroup == mgmtGrpSearchValue).ToList();

            if (pmsMatch != null && pmsMatch.Any())
                return await Task.FromResult(pmsMatch);

            var managementGroupMatch = filteredDocuments.AsQueryable().Where(d =>
                d.Hotel == "*" && d.PMS == "*" && d.ManagementGroup == mgmtGrpSearchValue).ToList();

            if (managementGroupMatch != null && managementGroupMatch.Any())
                return await Task.FromResult(managementGroupMatch);

            var finalMatch = filteredDocuments.AsQueryable().Where(d =>
                d.Hotel == "*" && d.PMS == "*" && d.ManagementGroup == "*").ToList();

            return await Task.FromResult(finalMatch);
            
           
        }
        public async Task<List<IndexDocumentTypeEntity>> GetIndexDocumentTypeAsync(MetadataFile metadataFile)
        {
            string reportSepratorType = "BARCODE";
            string mgmtGrpSearchValue = metadataFile?.GetIndex(MetadataIndexName.ManagementGroup).IndexValue.ToString();
            string pmsSearchValue = metadataFile?.GetIndex(MetadataIndexName.PMS).IndexValue.ToString();
            string hotelsearchValue = metadataFile?.GetIndex(MetadataIndexName.Hotels).IndexValue.ToString();

            //string mgmtGrpSearchValue = "McNeill Hotel Company";
            //string pmsSearchValue = "Fosse";
            //string hotelsearchValue = "JwwAXMC";

            logger?.LogTrace($"Get DB Document. Pms= {mgmtGrpSearchValue},{pmsSearchValue},{hotelsearchValue}");


            var filteredDocuments = indexingCollection.AsQueryable().Where(d => d.ReportSeperatorType == reportSepratorType).ToList();

            var exactMatch = filteredDocuments.AsQueryable().Where(d =>
            d.Hotel == hotelsearchValue && d.PMS == pmsSearchValue && d.ManagementGroup == mgmtGrpSearchValue).ToList();

            if (exactMatch != null && exactMatch.Any())
                return await Task.FromResult(exactMatch);

            var pmsMatch = filteredDocuments.AsQueryable().Where(d =>
                d.Hotel == "*" && d.PMS == pmsSearchValue && d.ManagementGroup == mgmtGrpSearchValue).ToList();

            if (pmsMatch != null && pmsMatch.Any())
                return await Task.FromResult(pmsMatch);

            var managementGroupMatch = filteredDocuments.AsQueryable().Where(d =>
                d.Hotel == "*" && d.PMS == "*" && d.ManagementGroup == mgmtGrpSearchValue).ToList();

            if (managementGroupMatch != null && managementGroupMatch.Any())
                return await Task.FromResult(managementGroupMatch);

            var finalMatch = filteredDocuments.AsQueryable().Where(d =>
                d.Hotel == "*" && d.PMS == "*" && d.ManagementGroup == "*").ToList();

            return await Task.FromResult(finalMatch);
           
        }
        #endregion
    }
}
