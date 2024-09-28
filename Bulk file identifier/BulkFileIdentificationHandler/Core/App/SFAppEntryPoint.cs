using Amazon.S3;
using Amazon.SimpleNotificationService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BulkFileIdentificationHandler.Core.App.Model;
using BulkFileIdentificationHandler.Core.Data;

namespace BulkFileIdentificationHandler.Core.App
{
    internal class SFAppEntryPoint : BaseAppEntryPoint<SFAppEntryPoint>
    {
        public SFAppEntryPoint(IConfigurationRoot configuration,
                                ILogger<SFAppEntryPoint> logger,
                                IAmazonS3 amazonS3Client,
                                IAmazonSimpleNotificationService snsClient,
                                IDataService dataService)
            : base(configuration, logger, amazonS3Client, snsClient, dataService)
        { }
        private async Task<ProcessedFileEntry> ProcessRecordAsync(MessegeSettings record)
        {
            Logger.LogInformation($"Processing message {record}");

            var result = await ProcessMessage(record);//parsing to event notification object
            return result;
        }

        /// <summary>
        /// Internal implementation of run. Implemented due to S4457 in SonarQube 
        /// </summary>
        /// <param name="input"></param>
        /// <seealso cref="https://rules.sonarsource.com/csharp/RSPEC-4457"/>
        /// <returns></returns>
        private async Task<object> RunInternal(object input)
        {

            MessegeSettings evnt = (MessegeSettings)input;

            var result = await ProcessRecordAsync(evnt);
            evnt.bulkType = result.ConvertedDocument.bulkType;
            evnt.processed = result.Processed;

            if (!result.Processed)
            {
                throw new AppProcessingException("Error occurred while processing ");
            }

            return evnt;
        }

        public override Task<object> Run(object input)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            else if (input.GetType() != typeof(MessegeSettings))
            {
                throw new ArgumentException("Invalid input",
                    nameof(input));
            }

            return RunInternal(input);
        }
    }
}
