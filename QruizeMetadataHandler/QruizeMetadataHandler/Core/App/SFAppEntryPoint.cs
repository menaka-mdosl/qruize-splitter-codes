using Amazon.S3;
using Amazon.SimpleNotificationService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QruizeMetadataHandler.Core.App.Model;

namespace QruizeMetadataHandler.Core.App
{
    public class SFAppEntryPoint : BaseAppEntryPoint<SFAppEntryPoint>
    {
        public SFAppEntryPoint(IConfigurationRoot configuration,
                                ILogger<SFAppEntryPoint> logger,
                                IAmazonS3 s3client,
                                IAmazonSimpleNotificationService snsClient)
            : base(configuration, logger, s3client, snsClient)
        { }
        private async Task<ProcessedFileEntry> ProcessRecordAsync(List<MessegeSettings> record)
        {
            Logger.LogInformation($"Processing message {record}");

            var convertResult = await ProcessMessage(record);//parsing to event notification object
            return convertResult;
        }

        /// <summary>
        /// Internal implementation of run. Implemented due to S4457 in SonarQube 
        /// </summary>
        /// <param name="input"></param>
        /// <seealso cref="https://rules.sonarsource.com/csharp/RSPEC-4457"/>
        /// <returns></returns>
        private async Task<object> RunInternal(object input)
        {

            List<MessegeSettings> evnt = (List<MessegeSettings>)input;

            var result = await ProcessRecordAsync(evnt);
            

            //evnt.newDocId = result.ConvertedDocument.NewDocId;
            //evnt.newS3Bucket = result.ConvertedDocument.ConvertedBucketName;
            //evnt.newS3Key = result.ConvertedDocument.NewS3Key;
            //evnt.processed = result.Processed;

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
            else if (input.GetType() != typeof(List<MessegeSettings>))
            {
                throw new ArgumentException("Invalid input",
                    nameof(input));
            }

            return RunInternal(input);
        }
    }
}
