using Amazon.S3;
using Amazon.SimpleNotificationService;
using QruizeMetadataHandler.Core.App.Model;
using MDO2.Core.Model.Metadata;
using MDO2.Core.QMS.Model.Message;
using MDO2.Core.QMS.Model.Message.EventData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MDO2.Core.LMD.S3.Model;
using QruizeMetadataHandler.Core.Splitting.Model;
using Path = System.IO.Path;
using System.Text;
using File = System.IO.File;
using Newtonsoft.Json;
using Amazon.S3.Model;
using MDO2.Core.Util;

namespace QruizeMetadataHandler.Core.App
{
    public abstract class BaseAppEntryPoint<T> : IAppEntryPoint
    {
        private static readonly HttpClient httpClient = new HttpClient();
        protected BaseAppEntryPoint(IConfigurationRoot configuration,
                                    ILogger<T> logger,
                                    IAmazonS3 amazonS3Client,
                                    IAmazonSimpleNotificationService snsClient)
        {
            Configuration = configuration;
            Logger = logger;
            AmazonS3Client = amazonS3Client;
            SnsClient = snsClient;
        }
        

        #region Static objects
        //-- application processing stages
        public const string STAGE_STARTING = "STARTING";
        public const string STAGE_S3_DOWNLOAD = "S3_DOWNLOAD";
        public const string STAGE_FILE_CONVERT = "FILE_CONVERT";
        public const string STAGE_S3_UPLOAD = "S3_UPLOAD";
        //--
        #endregion

        #region QMS event helpers
        protected virtual EventBody CreateFileInEvent(MetadataFile metadataFile)
        {
            var eventSrc = Configuration.GetValue<string>(AppConstants.CFG_KEY_QMS_EVENT_SOURCE_NAME);
            var evb = EventBodyFactory.Create<ConverterFileInEventData>(EventLevel.INFO, eventSrc);
            var evd = (ConverterFileInEventData)evb.Data;
            evd.DocId = metadataFile?.DocID;
            evd.ChainId = metadataFile?.ChainID;
            evd.ParentDocId = metadataFile?.ParentDocID;
            return evb;
        }
        protected virtual EventBody CreateFileUploadedEvent(List<MessegeSettings> indexedDocs,MetadataFile metadataFile)
        {
            var eventSrc = Configuration.GetQmsEventSourceName();
            var evb = EventBodyFactory.Create<ConverterFileUploadEventData>(EventLevel.INFO, eventSrc);
            var evd = (ConverterFileUploadEventData)evb.Data;
            evd.DocId = metadataFile?.DocID;
            evd.ChainId = metadataFile?.ChainID;
            evd.ParentDocId = metadataFile?.ParentDocID;
            foreach (var indexedDoc in indexedDocs)
            {
                evd.ReportList.Add(new IndexedReportDataElements
                {
                    DocId = indexedDoc.newDocId,
                    ReportName = indexedDoc.reportName,
                    BusinessDate = metadataFile?.GetIndex(MetadataIndexName.BusinessDate)?.IndexValue ?? "",
                    uploadStatus= "Success"
                });
            }
            return evb;
        }
        protected virtual EventBody CreateFileProcessingErrorEvent(MetadataFile metadataFile, QmsErrorEventData errorData)
        {
            var eventSrc = Configuration.GetValue<string>(AppConstants.CFG_KEY_QMS_EVENT_SOURCE_NAME);
            var evb = EventBodyFactory.Create<ConverterFileProcessingErrorEventData>(EventLevel.ERROR, eventSrc);
            var evd = (ConverterFileProcessingErrorEventData)evb.Data;
            evd.DocId = metadataFile?.DocID;
            evd.ChainId = metadataFile?.ChainID;
            evd.ParentDocId = metadataFile?.ParentDocID;
            evd.FailedAt = errorData.FailedAt;
            evd.ErrorMessage = errorData.ErrorMessage;
            evd.ExceptionData = errorData.Exception?.ToString() ?? "";
            evd.ErrorCode = errorData.ErrorCode;
            return evb;
        }
        internal virtual EventBody CreateSystemErrorAocEvent(MetadataFile metadataFile, string s3Bucket, string s3MetadataKey, string message)
        {
            var eventSrc = Configuration.GetQmsEventSourceName();
            var evb = EventBodyFactory.Create<QruizeXpsToPdfSystemErrorEventData>(EventLevel.ERROR, eventSrc);
            var evd = (QruizeXpsToPdfSystemErrorEventData)evb.Data;
            evd.DocId = metadataFile?.DocID;
            evd.ChainId = metadataFile?.ChainID;
            evd.ParentDocId = metadataFile?.ParentDocID;
            evd.S3BucketName = s3Bucket;
            evd.S3Key = metadataFile?.S3Key;
            evd.S3KeyMedatada = s3MetadataKey;
            evd.MaxAttempts = 0; //hard coded for now
            evd.ErrorMessage = message;
            return evb;
        }
        internal virtual async Task SendSystemErrorAoc(MetadataFile metadataFile, string s3Bucket, string metadataS3Key, string message)
        {
            var evb = CreateSystemErrorAocEvent(metadataFile, s3Bucket, metadataS3Key, message);
            await SendQMSMessage(evb);
        }
        protected virtual async Task SendQMSMessage(EventBody eventBody)
        {
            try
            {
                Console.WriteLine(eventBody.ToJson().ToString());
            }
            catch (Exception ex)
            {
            }
            try
            {
                var arn = Configuration.GetValue<string>(AppConstants.CFG_KEY_QMS_SNS_ARN);
                if (!string.IsNullOrWhiteSpace(arn))
                {
                    var client = new MDO2.Core.QMS.SnsQmsClient(SnsClient);
                    var response = await client.SendEventAsync(arn, eventBody);
                    if (!response.Success)
                    {
                        Logger.LogError($"Failed to send qms update. " +
                            $"EventLevel={eventBody.EventLevel}," +
                            $"EventType={eventBody.EventType}," +
                            $"Error={response.Exception?.ToString()}");
                    }
                    else
                    {
                        Logger.LogDebug($"QMS update sent. " +
                            $"EventLevel={eventBody.EventLevel}," +
                            $"EventType={eventBody.EventType}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error occurred while sending qms update. " +
                    $"EventLevel={eventBody.EventLevel}," +
                    $"EventType={eventBody.EventType}");
            }
        }
        #endregion

        #region Cleanup
        protected virtual void CleanUp(params string[] files)
        {
            foreach (var item in files)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(item) && File.Exists(item))
                    {
                        Logger.LogDebug($"Cleanup - Deleting file. {item}");
                        File.Delete(item);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, $"Failed to delete file. {item}");
                }
            }
        }
        #endregion

        #region Mapping
        protected async Task<ProcessedFileEntry> ProcessMessage(List<MessegeSettings> EventData)
        {
            FileProcessingResult s3FileProcessingResult = new FileProcessingResult();

                var fileProcessingResult = await ProcessFile(EventData);
                if (fileProcessingResult != null && !fileProcessingResult.Processed)
                {
                    fileProcessingResult.ProcessEvent = EventData;
                    s3FileProcessingResult.Failed.Add(fileProcessingResult);
                }

            return fileProcessingResult;

        }
        protected virtual async Task<string> ReadBinaryFile(MessegeSettings EventData)
        {

            var getObjectResponse = await AmazonS3Client.GetObjectAsync(EventData.newS3Bucket, EventData.newS3Key);

            if (getObjectResponse?.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                Logger.LogDebug($"Reading file. " +
                    $"Bucket={EventData.newS3Bucket}," +
                    $"Key={EventData.newS3Key}");

                var tempPath = Configuration.GetTempPath();
                Directory.CreateDirectory(tempPath);//create temp directory

                var finalFullPath = Path.Combine(tempPath, Path.GetFileName(EventData.newS3Key));//create final full path

                using (var outStream = File.OpenWrite(finalFullPath))
                {
                    getObjectResponse.ResponseStream.CopyTo(outStream);
                }

                Logger.LogInformation($"file reading successful. " +
                    $"Bucket={EventData.newS3Bucket}," +
                    $"Key={EventData.newS3Key}," +
                    $"LocalPath={finalFullPath}");

                return finalFullPath;
            }
            else
            {
                throw new S3FileDownloadException($"Failed to download file. " +
                    $"Bucket={EventData.newS3Bucket}," +
                    $"Key={EventData.newS3Key}," +
                    $"Error=HTTP status {getObjectResponse?.HttpStatusCode}");
            }

        }

        protected virtual async Task<string> SendMetadataDetails(MessegeSettings EventData, ConvertedDocument ConvertedDocument)
        {
            // API endpoint URL
            string apiUrl = Configuration.GetMetaGeneratorApiUrl();

            //set the s3 url
            string S3Url = $"https://{Configuration.GetConvertedS3Bucket()}.s3.{Configuration.GetS3Region()}.amazonaws.com/{EventData.newS3Key}";
            Meta meta = new Meta
            {
                ReportName = EventData.reportName,
                ReportCategory = "",
                Category = ""
            };
            var apiData = new ApiData 
            {
                newDocId = EventData.newDocId,
                metaBucketName = EventData.s3BucketName,
                metaKey = EventData.s3KeyMedatada,
                fileBucketName = EventData.newS3Bucket,
                fileKey = EventData.newS3Key,
                s3Key = EventData.newS3Key,
                s3Url= S3Url,
                meta= meta,
            };

            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);

            string jsonData = JsonConvert.SerializeObject(apiData,Formatting.Indented);
            // Set the request content to the JSON data

            request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            // Add headers to the request
            request.Headers.Add("Authorization", Configuration.GetMetaGeneratorApiHeader());


            // Send a POST request to the API endpoint
            var response = await httpClient.SendAsync(request); 

            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                // Read the response content as a string
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            else
            {
                // If the request was not successful, throw an exception
                throw new Exception($"Failed to call API. Status code: {response.StatusCode}");
            }
            
        }
        protected async Task<ProcessedFileEntry> ProcessFile(List<MessegeSettings> EventDatas)
        {

            
            Logger.LogInformation($"Starting Convert File. ");

            ConvertedDocument ConvertedDocument = new ConvertedDocument();
            ProcessedFileEntry result = new ProcessedFileEntry();
            List<MessegeSettings> uploadedList=new List<MessegeSettings>();

            string newMetadataFile = String.Empty;
            string ConvertedFileTempPath = String.Empty;
            bool errorOccurred = false;
            MetadataFile metadataFile = null;

            string currentProcessingStage = STAGE_STARTING;//track processing stage
            currentProcessingStage = STAGE_S3_DOWNLOAD; //set stage - S3 downloading


            try
            {
                
                foreach (var EventData in EventDatas)
                {
                    metadataFile = await ReadMetadataFile(EventData.s3BucketName, EventData.s3KeyMedatada);
                    ConvertedDocument.ConvertedBucketName = Configuration.GetConvertedS3Bucket();
                    ConvertedDocument.NewFileName = Path.GetFileName(EventData.newS3Key);
                    ConvertedDocument.NewMetadataName = $"{EventData.newDocId}.pdf{Configuration.GetUploadS3MetadataExtension()}";
                    ConvertedDocument.NewS3Key = EventData.newS3Key;
                    ConvertedDocument.NewDocId = EventData.newDocId;

                    newMetadataFile = await SendMetadataDetails(EventData, ConvertedDocument);//generate new mwtadata file
                    ConvertedFileTempPath = await ReadBinaryFile(EventData);

                    var failedItems = new List<UploadFailure>();

                    try
                    {

                        //Upload Into Converter Bucket
                        var converterFileUploadRequest = new LmdFileUploadRequest()
                        {
                            BucketName = ConvertedDocument.ConvertedBucketName,
                            MetadataExtention = Configuration.GetUploadS3MetadataExtension(),
                            S3Path = Configuration.GetConvertedS3Path(),
                            FilePath = ConvertedFileTempPath,
                            MetadataInfo = newMetadataFile,
                            S3KeyForFile = ConvertedDocument.NewFileName
                        };
                        string newS3KeyForFile = CreateS3Key(Configuration.GetConvertedS3Path(), ConvertedDocument.NewFileName);
                        var converterUplaodResult = await UploadToS3(converterFileUploadRequest);

                        uploadedList.Add(new MessegeSettings
                        {
                            s3BucketName = EventData.s3BucketName,
                            s3KeyMedatada = EventData.s3KeyMedatada,
                            docId = EventData.docId,
                            chainId = EventData.chainId,
                            fileExtension = EventData.fileExtension,
                            s3Key = EventData.s3Key,
                            s3Url = EventData.s3Url,
                            hotel = EventData.hotel,
                            hmg = EventData.hmg,
                            reportName = EventData.reportName,
                            connector = EventData.connector,
                            entityId = EventData.entityId,
                            projectId = EventData.projectId,
                            newDocId = EventData.newDocId,
                            newS3Key = newS3KeyForFile,
                            newS3Bucket = ConvertedDocument.ConvertedBucketName,
                            processed = true,
                        });
                    }
                    catch (Exception ex)
                    {
                        failedItems.Add(new UploadFailure()
                        {
                            Document = ConvertedDocument,
                            Error = ex
                        });
                    }

                    if (failedItems.Count != 0)
                    {
                        Logger.LogWarning($"Following reports are not uploaded to the S3 bucket FailedCount={failedItems.Count}");

                        foreach (var item in failedItems)
                        {
                            Logger.LogError($"Document={item.Document?.NewFileName},Error={item.Error}");
                        }
                    }
                }
            }

            catch (AppProcessingException ex)
            {
                errorOccurred = true;

                Logger?.LogError($"App Processing Error occurred. " +
                   $"S3Key={ConvertedDocument.NewS3Key}," +
                   $"S3Bucket={ConvertedDocument.ConvertedBucketName}," +
                   $"Error={ex}");

                //send qms aoc
                var message = $"App Processing Error occurred. Error={ex}";
                //await SendSystemErrorAoc(metadataFile, ConvertedDocument.ConvertedBucketName, ConvertedDocument.NewS3Key, message);
            }

            catch (Exception ex)
            {
                errorOccurred = true;

                Logger?.LogError($"Unidentified Exception Occured. " +
                   $"S3Key={ConvertedDocument.NewS3Key}," +
                   $"S3Bucket={ConvertedDocument.ConvertedBucketName}," +
                   $"Error={ex}");

                //send qms aoc
                var message = $"Unidentified Exception Occured. Error={ex}";
                //await SendSystemErrorAoc(metadataFile, ConvertedDocument.ConvertedBucketName, EventData.newS3Key, message);
            }
            finally
            {
                if (errorOccurred)
                {
                    result.Processed = false;
                    result.FailedAt = currentProcessingStage;

                    Logger?.LogInformation($"Currant Process is failed. " +
                       $"S3Key={ConvertedDocument.NewS3Key}," +
                       $"S3Bucket={ConvertedDocument.ConvertedBucketName},");
                }
                else
                {
                    result.Processed = true;
                    result.ProcessEvent = uploadedList;
                    result.ConvertedDocument = ConvertedDocument;

                    var eventData = CreateFileUploadedEvent(uploadedList, metadataFile);
                    await SendQMSMessage(eventData);
                }

                //clean up tempory files
                CleanUp(ConvertedFileTempPath);
                Logger?.LogInformation($"cleaned all temporary files. " +
                $"binaryFilePath={ConvertedFileTempPath}," +
                $"S3Bucket={ConvertedDocument.ConvertedBucketName},");

            }

            return result;
        }


        protected virtual async Task<MetadataFile> ReadMetadataFile(string bucket, string s3Key)
        {

            Logger.LogInformation($"Downloading metadata file. " +
                $"Bucket={bucket}," +
                $"Key={s3Key}");

            var getObjectResponse = await AmazonS3Client.GetObjectAsync(bucket, s3Key);
            if (getObjectResponse?.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                Logger.LogDebug($"Reading metadata file. " +
                    $"Bucket={bucket}," +
                    $"Key={s3Key}");

                var metadataFile = MetadataFile.Read(getObjectResponse.ResponseStream);

                Logger.LogInformation($"Metadata file reading successful. " +
                    $"Bucket={bucket}," +
                    $"Key={s3Key}");

                return metadataFile;
            }
            else
            {
                throw new S3FileDownloadException($"Failed to download Metadata file. " +
                    $"Bucket={bucket}," +
                    $"Key={s3Key}," +
                    $"Error=HTTP status {getObjectResponse?.HttpStatusCode}");
            }

        }
        #endregion

        #region S3 Upload
        private string CreateS3Key(string path, string key)
        {
            if (path.IsNullOrWhiteSpace())
                return key;
            else
                return $"{path.Trim('/', '\\')}/{key}";

        }
        protected async virtual Task<LmdFileUploadResponse> UploadToS3(LmdFileUploadRequest lmdFileUploadRequest)
        {
            try
            {
                var s3Client = new MDO2.Core.LMD.S3.S3Client(AmazonS3Client);
                var response = await s3Client.UploadLMDFileAsync(lmdFileUploadRequest);
                if (response.ResponseCode == System.Net.HttpStatusCode.OK)
                {
                    Logger.LogDebug($"File uploaded. " +
                        $"S3Key={response.BinaryFileKey}," +
                        //$"MetadataS3Key={response.MetadataFileKey}," +
                        $"Bucket={lmdFileUploadRequest.BucketName}," +
                        $"SourceFilePath={lmdFileUploadRequest.FilePath}");

                    return response;
                }
                else
                {
                    throw new FileUploadException($"S3 service returned error while uploading file to S3. " +
                        $"HttpCode={response.ResponseCode}" +
                        $"S3Key={lmdFileUploadRequest.S3KeyForFile}," +
                        $"Bucket={lmdFileUploadRequest.BucketName}," +
                        $"SubPath={lmdFileUploadRequest.S3Path}," +
                        $"SourceFilePath={lmdFileUploadRequest.FilePath}");
                }
            }
            catch (Exception ex)
            {
                throw new FileUploadException($"Error occurred while uploading file to S3. " +
                    $"S3Key={lmdFileUploadRequest.S3KeyForFile}," +
                    $"Bucket={lmdFileUploadRequest.BucketName}," +
                    $"SubPath={lmdFileUploadRequest.S3Path}," +
                    $"SourceFilePath={lmdFileUploadRequest.FilePath}", ex);
            }
        }
        protected virtual string CreateNewS3Key(string docId, string filePath)
        {
            //try to find the extension 
            var extension = Path.GetExtension(filePath) ?? "";
            return $"{docId}.{extension.Trim('.')}";
        }
        #endregion
        public abstract Task<object> Run(object input);
        public IConfigurationRoot Configuration { get; }
        public ILogger<T> Logger { get; }
        public IAmazonS3 AmazonS3Client { get; }
        public IAmazonSimpleNotificationService SnsClient { get; }
    }
}
