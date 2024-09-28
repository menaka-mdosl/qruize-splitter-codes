using Amazon.S3;
using Amazon.SimpleNotificationService;
using BulkFileIdentificationHandler.Core.App.Model;
using BulkFileIdentificationHandler.Core.Data;
using MDO2.Core.Model.Metadata;
using MDO2.Core.QMS.Model.Message;
using MDO2.Core.QMS.Model.Message.EventData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MDO2.Core.LMD.S3.Model;
using BulkFileIdentificationHandler.Core.Splitting.Model;
using Path = System.IO.Path;
using Aspose.Pdf;
using static System.Collections.Specialized.BitVector32;
using Aspose.Pdf.Operators;
using Aspose.BarCode.BarCodeRecognition;
using Aspose.Pdf.Devices;
using Aspose.Drawing;
using Amazon.S3.Model;
using Document = Aspose.Pdf.Document;
using Aspose.Pdf.Text;
using Aspose.Pdf.Facades;

namespace BulkFileIdentificationHandler.Core.App
{
    internal abstract class BaseAppEntryPoint<T> : IAppEntryPoint
    {

        #region Static objects
        //-- application processing stages
        public const string STAGE_STARTING = "STARTING";
        public const string STAGE_S3_DOWNLOAD = "S3_DOWNLOAD";
        public const string STAGE_FILE_CONVERT = "FILE_CONVERT";
        public const string STAGE_S3_UPLOAD = "S3_UPLOAD";
        //--
        #endregion


        protected BaseAppEntryPoint(IConfigurationRoot configuration,
                                    ILogger<T> logger,
                                    IAmazonS3 amazonS3Client,
                                    IAmazonSimpleNotificationService snsClient,
                                    IDataService dataService)
        {
            Configuration = configuration;
            Logger = logger;
            AmazonS3Client = amazonS3Client;
            SnsClient = snsClient;
            DataService = dataService;
        }
        

        #region QMS event helpers
        protected virtual EventBody EmptyDocQC(MetadataFile metadataFile)
        {
            var eventSrc = Configuration.GetQmsEventSourceName();
            var evb = EventBodyFactory.Create<ConverterQcEventData>(EventLevel.ERROR, eventSrc);
            var evd = (ConverterQcEventData)evb.Data;
            evd.QcType = ConverterQcEventData.QC_TYPE_FORMAT_EMPTY_DOCUMENT;
            evd.DocId = metadataFile?.DocID;
            evd.ChainId = metadataFile?.ChainID;
            evd.ParentDocId = metadataFile?.ParentDocID;
            evd.FileSize = metadataFile?.GetIndex(MetadataIndexName.FileSizeInBytes)?.IndexValue;
            return evb;
        }
        protected virtual EventBody CorruptedDocQC(MetadataFile metadataFile)
        {
            var eventSrc = Configuration.GetQmsEventSourceName();
            var evb = EventBodyFactory.Create<ConverterQcEventData>(EventLevel.ERROR, eventSrc);
            var evd = (ConverterQcEventData)evb.Data;
            evd.QcType = ConverterQcEventData.QC_TYPE_FORMAT_CORRUPTED_FILE;
            evd.DocId = metadataFile?.DocID;
            evd.ChainId = metadataFile?.ChainID;
            evd.ParentDocId = metadataFile?.ParentDocID;
            evd.FileExtension = metadataFile?.GetIndex(MetadataIndexName.FileExtension)?.IndexValue;
            evd.FileSize = metadataFile?.GetIndex(MetadataIndexName.FileSizeInBytes)?.IndexValue;
            return evb;
        }
        protected virtual EventBody ProtectedDocQC(MetadataFile metadataFile)
        {
            var eventSrc = Configuration.GetQmsEventSourceName();
            var evb = EventBodyFactory.Create<ConverterQcEventData>(EventLevel.ERROR, eventSrc);
            var evd = (ConverterQcEventData)evb.Data;
            evd.QcType = ConverterQcEventData.QC_TYPE_FORMAT_PROTECTED_FILE;
            evd.DocId = metadataFile?.DocID;
            evd.ChainId = metadataFile?.ChainID;
            evd.ParentDocId = metadataFile?.ParentDocID;
            evd.FileExtension = metadataFile?.GetIndex(MetadataIndexName.FileExtension)?.IndexValue; 
            evd.protectionTypeDesc = String.Empty;
            evd.canContinue = false;
            return evb;
        }
        protected virtual EventBody CreateFileInEvent(MetadataFile metadataFile)
        {
            var eventSrc = Configuration.GetQmsEventSourceName();
            var evb = EventBodyFactory.Create<ConverterFileInEventData>(EventLevel.INFO, eventSrc);
            var evd = (ConverterFileInEventData)evb.Data;
            evd.DocId = metadataFile?.DocID;
            evd.ChainId = metadataFile?.ChainID;
            evd.ParentDocId = metadataFile?.ParentDocID;
            evd.Hotel = metadataFile?.GetIndex(MetadataIndexName.Hotels)?.IndexValue;
            evd.MgmtGroup = metadataFile?.GetIndex(MetadataIndexName.ManagementGroup)?.IndexValue;
            evd.BusinessDate = metadataFile?.GetIndex(MetadataIndexName.BusinessDate)?.IndexValue;
            return evb;
        } protected virtual EventBody CreateBulkFileTypeEvent(MetadataFile metadataFile,string bulkType,string fileType)
        {
            
            var eventSrc = Configuration.GetQmsEventSourceName();
            var evb = EventBodyFactory.Create<BulkFileTypeEvent>(EventLevel.INFO, eventSrc);
            var evd = (BulkFileTypeEvent)evb.Data;
            evd.DocId = metadataFile?.DocID;
            evd.ChainId = metadataFile?.ChainID;
            evd.ParentDocId = metadataFile?.ParentDocID;
            evd.Hotel = metadataFile?.GetIndex(MetadataIndexName.Hotels)?.IndexValue;
            evd.MgmtGroup = metadataFile?.GetIndex(MetadataIndexName.ManagementGroup)?.IndexValue;
            evd.BusinessDate = metadataFile?.GetIndex(MetadataIndexName.BusinessDate)?.IndexValue;
            evd.BulkType = bulkType;
            evd.FileType=fileType;
            return evb;
        }
        protected virtual EventBody CreateFileProcessedEvent(MetadataFile metadataFile, string convertedDocId)
        {
            var eventSrc = Configuration.GetQmsEventSourceName();
            var evb = EventBodyFactory.Create<ConverterFileProcessedEventData>(EventLevel.INFO, eventSrc);
            var evd = (ConverterFileProcessedEventData)evb.Data;
            evd.DocId = metadataFile?.DocID;
            evd.ChainId = metadataFile?.ChainID;
            evd.ParentDocId = metadataFile?.ParentDocID;
            evd.ConvertedDocId = convertedDocId;
            return evb;
        }
        protected virtual EventBody CreateFileProcessingErrorEvent(MetadataFile metadataFile, QmsErrorEventData errorData)
        {
            var eventSrc = Configuration.GetQmsEventSourceName();
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
        internal virtual EventBody CreateSystemErrorAocEvent(MetadataFile metadataFile, QmsErrorEventData errorData)
        {
            var eventSrc = Configuration.GetQmsEventSourceName();
            var evb = EventBodyFactory.Create<QruizeXpsToPdfSystemErrorEventData>(EventLevel.ERROR, eventSrc);
            var evd = (QruizeXpsToPdfSystemErrorEventData)evb.Data;
            evd.DocId = metadataFile?.DocID;
            evd.ChainId = metadataFile?.ChainID;
            evd.ParentDocId = metadataFile?.ParentDocID;
            evd.FailedAt = errorData.FailedAt;
            evd.ErrorMessage = errorData.ErrorMessage;
            evd.ExceptionData = errorData.Exception?.ToString() ?? "";
            evd.ErrorCode = errorData.ErrorCode;
            return evb;
        }
        internal virtual async Task SendSystemErrorAoc(MetadataFile metadataFile, QmsErrorEventData evntData)
        {
            var evb = CreateSystemErrorAocEvent(metadataFile, evntData);
            await SendQMSMessage(evb);
        }
        protected virtual async Task SendQMSMessage(EventBody eventBody)
        {
            try
            {
                Console.WriteLine(eventBody.ToJson().ToString());
            }
            catch (Exception Ex)
            {

            }
            try
            {
                var arn = Configuration.GetQmsArn();
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
        protected async Task<ProcessedFileEntry> ProcessMessage(MessegeSettings EventData)
        {
            FileProcessingResult s3FileProcessingResult = new FileProcessingResult();

           
                var bucket = EventData.s3BucketName;
                var s3Key = EventData.s3KeyMedatada;

                var fileProcessingResult = await ProcessFile(bucket, s3Key);
                if (fileProcessingResult != null && !fileProcessingResult.Processed)
                {
                    fileProcessingResult.ProcessEvent = EventData;
                    s3FileProcessingResult.Failed.Add(fileProcessingResult);
                }

            return fileProcessingResult;

        }
        protected virtual async Task<MetadataFile> ReadMetadataFile(string bucket, string s3Key)
        {

            Logger.LogInformation($"Downloading metadata file. " +
                $"Bucket={bucket}," +
                $"Key={s3Key}");

            var getObjectResponse = await AmazonS3Client.GetObjectAsync(bucket,  s3Key);
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
        protected virtual async Task<string> ReadBinaryFile(string bucket, string s3Key, string tempPath)
        {
            Logger.LogInformation($"Downloading Barcode file. " +
                   $"Bucket={bucket}," +
                   $"Key={s3Key}");

            var getObjectResponse = await AmazonS3Client.GetObjectAsync(bucket, s3Key);
            if (getObjectResponse?.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                Logger.LogDebug($"Reading Barcode file. " +
                    $"Bucket={bucket}," +
                    $"Key={s3Key}");

                Directory.CreateDirectory(tempPath);//create temp directory

                var finalFullPath = Path.Combine(tempPath, Path.GetFileName(s3Key));//create final full path

                using (var outStream = File.OpenWrite(finalFullPath))
                {
                    getObjectResponse.ResponseStream.CopyTo(outStream);
                }

                Logger.LogInformation($"XPS file reading successful. " +
                    $"Bucket={bucket}," +
                    $"Key={s3Key}," +
                    $"LocalPath={finalFullPath}");

                return finalFullPath;
            }
            else
            {
                throw new S3FileDownloadException($"Failed to download XPS file. " +
                    $"Bucket={bucket}," +
                    $"Key={s3Key}," +
                    $"Error=HTTP status {getObjectResponse?.HttpStatusCode}");
            }

        }

        public bool ContainsBarcodeInPDF(string pdfPath)
        {
            SetLicense();
            // Load the PDF document
            Document pdfDocument = new Document(pdfPath);
            int count = 0;
            // Iterate through all the pages of the PDF
            foreach (var page in pdfDocument.Pages)
            {
                count++;
                // Create a stream to save the extracted image
                using (MemoryStream imageStream = new MemoryStream())
                {
                    // Create a Resolution object (300 DPI recommended for barcode detection)
                    Resolution resolution = new Resolution(150);

                    // Create a PNG device to convert the PDF page to an image
                    PngDevice pngDevice = new PngDevice(resolution);

                    // Convert the page and save the image to the stream
                    pngDevice.Process(page, imageStream);
                    imageStream.Position = 0;

                    // Check for barcode in the image stream
                    using (BarCodeReader reader = new BarCodeReader(imageStream, DecodeType.AllSupportedTypes))
                    {
                        var barcodes = reader.ReadBarCodes();
                        if (barcodes.Count() > 0)
                        {
                            return true;
                        }
                      
                    }
                }
                if (count == 10)
                {
                    break;
                }
            }
            // If no barcode is found, return false
            return false;
        }
        protected virtual bool IsFIleReadable(string filePath)
        {
            // Load the PDF document
            Document pdfDocument = new Document(filePath);

            bool isReadable = false;

            // Iterate through all the pages
            foreach (Page page in pdfDocument.Pages)
            {
                TextAbsorber textAbsorber = new TextAbsorber();
                page.Accept(textAbsorber);

                // Get the extracted text
                string extractedText = textAbsorber.Text;

                // Check if the extracted text is not empty
                if (!string.IsNullOrWhiteSpace(extractedText))
                {
                    isReadable = true;
                    break;
                }
            }

            if (isReadable)
            {
                Logger.LogInformation($"The PDF file is readable. File Path = {filePath}");
            }
            else
            {
                Logger.LogInformation($"The PDF file is non-readable. File Path = {filePath}");
            }
            return isReadable;
        }

        internal bool CheckIfPdfContainsTextOrImages(string binaryPath)
        {
            // Instantiate a memoryStream object to hold the extracted text from Document
            MemoryStream ms = new MemoryStream();
            // Instantiate PdfExtractor object
            PdfExtractor extractor = new PdfExtractor();

            // Bind the input PDF document to extractor
            extractor.BindPdf(binaryPath);
            // Extract text from the input PDF document
            extractor.ExtractText();
            // Save the extracted text to a text file
            extractor.GetText(ms);
            // Check if the MemoryStream length is greater than or equal to 1

            bool containsText = ms.Length >= 1;

            // Extract images from the input PDF document
            extractor.ExtractImage();

            // Calling HasNextImage method in while loop. When images will finish, loop will exit
            bool containsImage = extractor.HasNextImage();

            // Now find out whether this PDF is text only or image only

            if (containsText && !containsImage)
                Console.WriteLine("PDF contains text only");
            else if (!containsText && containsImage)
                Console.WriteLine("PDF contains image only");
            else if (containsText && containsImage)
                Console.WriteLine("PDF contains both text and image");
            else if (!containsText && !containsImage)
                Console.WriteLine("PDF contains neither text or nor image");

            return containsImage;
        }

        protected async Task<ProcessedFileEntry> ProcessFile(string s3Bucket, string metadataS3Key)
        {
            
            Logger.LogInformation($"Starting spliiting The barcode File. ");
            string currentProcessingStage = STAGE_STARTING;//track processing stage

           

            ProcessedFileEntry result = new ProcessedFileEntry();
            ConvertedDocument ConvertedDocument = new ConvertedDocument();
            MetadataFile metadataFile = null;
            string binaryFilePath = String.Empty;
            bool errorOccurred = false;
            
            try
            {

                currentProcessingStage = STAGE_S3_DOWNLOAD; //set stage - S3 downloading
                metadataFile = await ReadMetadataFile(s3Bucket, metadataS3Key);//download and read metadata file

                //create bulkfile in event
                var evnt=CreateFileInEvent(metadataFile);
                Logger.LogInformation($"Create bulkfile in event. ");
                await SendQMSMessage(evnt);   

                var tempPath = Configuration.GetTempPath();

                binaryFilePath = await ReadBinaryFile(s3Bucket, metadataFile.S3Key, tempPath);//download XPS file

                string bulkType = "UNIDENTIFIED";
                Logger.LogDebug("Check if the file is readable or not. " + $"BinaryFilePath={binaryFilePath}");

                // Set the license files
                SetLicense();

                bool isContainImages=CheckIfPdfContainsTextOrImages(binaryFilePath);
                //Check file is Readable or non-readable
                //bool isFileReadable = IsFIleReadable(binaryFilePath);

                if (!isContainImages)
                {
                    bulkType = "BATCH";
                }
                if (isContainImages)
                {
                    bool iscontainbarcode = ContainsBarcodeInPDF(binaryFilePath);
                    Logger.LogDebug($"Starting to identifing bulkFile" +
                           $"s3Bucket={s3Bucket}," +
                           $"metadataS3Key={metadataS3Key}");

                    if (iscontainbarcode)
                    {
                        bulkType = "BARCODE";
                    }
                }
                if (bulkType.Equals("UNIDENTIFIED"))
                {
                    Logger.LogWarning("File is not identified as a batch or barcode." + $"BinaryFilePath={binaryFilePath}");
                }

                //Logger.LogDebug($"Starting to identifing bulkFile" +
                //       $"s3Bucket={s3Bucket}," +
                //       $"BulkType={bulkType}," +
                //       $"metadataS3Key={metadataS3Key}");

                ConvertedDocument.bulkType = bulkType;


                //QMS for bulk type
                evnt = CreateBulkFileTypeEvent(metadataFile, bulkType, "BULK");
                await SendQMSMessage(evnt);

            }

            catch (S3FileDownloadException ex)
            {
                errorOccurred = true;

                Logger?.LogError($"S3 file download error occurred. " +
                    $"S3Key={metadataS3Key}," +
                    $"S3Bucket={s3Bucket}," +
                    $"Error={ex}");

                //send qms aoc
                var errrorData = new QmsErrorEventData()
                {
                    ErrorCode = "500",
                    ErrorMessage = "S3 file download error occurred. ",
                    Exception = ex,
                    FailedAt = "S3UPLOADING"
                };
                await SendSystemErrorAoc(metadataFile, errrorData);
            }
            catch (ConvertingErrorException ex)
            {
                errorOccurred = true;

                Logger?.LogError($"Error Occured When Converting." +
                 $"S3Key={metadataS3Key}," +
                 $"S3Bucket={s3Bucket}," +
                 $"EntityId={metadataFile?.EntityID}," +
                 $"ProjectId={metadataFile?.ProjectID}," +
                 $"Error={ex}");

                //send qms aoc
                var errrorData = new QmsErrorEventData()
                {
                    ErrorCode = "500",
                    ErrorMessage = "Error Occured When Converting.",
                    Exception = ex,
                    FailedAt = "CONVERTING"
                };

                var processedQmsMessageEvent = CreateFileProcessingErrorEvent(metadataFile, errrorData);//send File In qms message
                await SendQMSMessage(processedQmsMessageEvent);
                try
                {
                    Console.WriteLine(processedQmsMessageEvent.ToJson().ToString());
                }
                catch (Exception){}

            }
            catch (AppProcessingException ex)
            {
                errorOccurred = true;

                Logger?.LogError($"App Processing Error occurred. " +
                   $"S3Key={metadataS3Key}," +
                   $"S3Bucket={s3Bucket}," +
                   $"EntityId={metadataFile?.EntityID}," +
                   $"ProjectId={metadataFile?.ProjectID}," +
                   $"Error={ex}");

                //send qms aoc
                var errrorData = new QmsErrorEventData()
                {
                    ErrorCode = "500",
                    ErrorMessage = "App Processing Error occurred.  ",
                    Exception = ex,
                    FailedAt = "APP PROCESSING"
                };
                await SendSystemErrorAoc(metadataFile, errrorData);

            }
            catch (MissingMetadataException ex)
            {
                errorOccurred = true;

                Logger?.LogError($"Cannot find Metadata files. " +
                    $"S3Key={metadataS3Key}," +
                    $"S3Bucket={s3Bucket}," +
                    $"EntityId={metadataFile?.EntityID}," +
                    $"ProjectId={metadataFile?.ProjectID}," +
                    $"Error={ex}");

                //send qms aoc
                var errrorData = new QmsErrorEventData()
                {
                    ErrorCode = "500",
                    ErrorMessage = "Cannot find Metadata files. ",
                    Exception = ex,
                    FailedAt = "METADATA PROCESSING"
                };
                await SendSystemErrorAoc(metadataFile, errrorData);
            }
           
            catch (Exception ex)
            {
                errorOccurred = true;

                Logger?.LogError($"Unidentified Exception Occured. " +
                   $"S3Key={metadataS3Key}," +
                   $"S3Bucket={s3Bucket}," +
                   $"EntityId={metadataFile?.EntityID}," +
                   $"ProjectId={metadataFile?.ProjectID}," +
                   $"Error={ex}");

                //send qms aoc
                var errrorData = new QmsErrorEventData()
                {
                    ErrorCode = "500",
                    ErrorMessage = "Unidentified Exception Occured.",
                    Exception = ex,
                    FailedAt = ""
                };
                await SendSystemErrorAoc(metadataFile, errrorData);
            }
            finally
            {
                if (errorOccurred)
                {
                    result.Processed = false;
                    result.FailedAt = currentProcessingStage;

                    Logger?.LogInformation($"Currant Process is failed. " +
                       $"S3Key={metadataS3Key}," +
                       $"S3Bucket={s3Bucket}," +
                       $"EntityId={metadataFile?.EntityID}," +
                       $"ProjectId={metadataFile?.ProjectID}");
                }
                else
                {
                    result.Processed = true;
                    result.ConvertedDocument= ConvertedDocument;
                }

                //clean up tempory files
                CleanUp(binaryFilePath);
                Logger?.LogInformation($"cleaned all temporary files. " +
                       $"binaryFilePath={binaryFilePath}," + 
                       $"S3Bucket={s3Bucket}," +
                       $"EntityId={metadataFile?.EntityID}," +
                       $"ProjectId={metadataFile?.ProjectID}");

            }

            return result;
        }

        public void SetLicense()
        {
            try
            {
                var barcodeLicense = new Aspose.BarCode.License();
                barcodeLicense.SetLicense("Aspose.TotalProductFamily.lic");

                var pdfLicense = new Aspose.Pdf.License();
                pdfLicense.SetLicense("Aspose.TotalProductFamily.lic");
            }
            catch (Exception ex)
            {
                Logger?.LogInformation($"Error occured duiring ASpose license Activating process . Ex - {ex}");
                throw;
            }
           

            //var wordsLicense = new Aspose.Words.License();
            //wordsLicense.SetLicense("Aspose.TotalProductFamily.lic");

            //var drawingLicense = new Aspose.Drawing.License();
            //drawingLicense.SetLicense("Aspose.TotalProductFamily.lic");

            //var ocrLicense = new Aspose.OCR.License();
            //ocrLicense.SetLicense("Aspose.TotalProductFamily.lic");
        }

        #endregion

        #region S3 Upload
        #endregion
        public abstract Task<object> Run(object input);
        public IConfigurationRoot Configuration { get; }
        public ILogger<T> Logger { get; }
        public IAmazonS3 AmazonS3Client { get; }
        public IDataService DataService { get; }
        public IAmazonSimpleNotificationService SnsClient { get; }
    }
}
