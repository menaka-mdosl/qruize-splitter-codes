using Amazon.S3;
using Amazon.SimpleNotificationService;
using QruizeBatchReportHandler.Core.App.Model;
using QruizeBatchReportHandler.Core.Data;
using MDO2.Core.Model.Metadata;
using MDO2.Core.QMS.Model.Message;
using MDO2.Core.QMS.Model.Message.EventData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MDO2.Core.LMD.S3.Model;
using QruizeBatchReportHandler.Core.Splitting.Model;
using Path = System.IO.Path;
using Aspose.OCR;
using Aspose.Pdf;
using static System.Collections.Specialized.BitVector32;
using Aspose.Pdf.Operators;
using Aspose.Pdf.Devices;
using Aspose.Drawing;
using Amazon.S3.Model;
using QruizeBatchReportHandler.Core.Data.Model;
using Aspose.Pdf.Text;
using Aspose.Pdf.Drawing;
using System.Text.RegularExpressions;
using Document = Aspose.Pdf.Document;
using SharpCompress.Common;
using static System.Net.Mime.MediaTypeNames;
using System.Text;
using System.Xml.Linq;

namespace QruizeBatchReportHandler.Core.App
{
    internal abstract class BaseAppEntryPoint<T> : IAppEntryPoint
    {

        #region Static objects
        //-- application processing stages

        public const string STAGE_START = "Strating";
        public const string STAGE_S3_DOWNLOADING = "DownloadingStage";
        public const string STAGE_FILE_SPLITING = "SpittingStage";
        public const string STAGE_FILE_INDEXING = "IndexingStage";
        public const string STAGE_S3_UPLOAD = "UploadingStage";
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

        protected virtual EventBody ReportNameSubstitutedAsMissing(MetadataFile metadataFile, List<ReportMatchedDetails> matchedReport)
        {
            var eventSrc = Configuration.GetQmsEventSourceName();
            var evb = EventBodyFactory.Create<ConverterQcEventData>(EventLevel.WARN, eventSrc);
            var evd = (ConverterQcEventData)evb.Data;
            evd.QcType = ConverterQcEventData.QC_TYPE_FORMAT_SUBSTITUE_REPORT_NAME;
            evd.DocId = metadataFile?.DocID;
            evd.ChainId = metadataFile?.ChainID;
            evd.ParentDocId = metadataFile?.ParentDocID;
            evd.FileType = "BULK";
            evd.BulkType = "BATCH";
            foreach(var page in matchedReport) {
                if (page.ReportName.Equals("MISSING"))
                {
                    evd.ReportList.Add(new FailuresReportDataElements
                    {
                        Page = page.page,
                        IndexName = "Report Name",
                        SubstitutedValue = "MISSING"
                    });
                }
            }
            
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
        }
        protected virtual EventBody CreateFileProcessedEvent(MetadataFile metadataFile, List<IndexedDocumentDetails> indexedDocs)
        {
            var eventSrc = Configuration.GetQmsEventSourceName();
            var evb = EventBodyFactory.Create<ConverterFileProcessedEventData>(EventLevel.INFO, eventSrc);
            var evd = (ConverterFileProcessedEventData)evb.Data;
            evd.DocId = metadataFile?.DocID;
            evd.ChainId = metadataFile?.ChainID;
            evd.ParentDocId = metadataFile?.ParentDocID;
            foreach (var indexedDoc in indexedDocs)
            {
                evd.ReportList.Add(new IndexedReportDataElements
                {
                    DocId = indexedDoc.NewDocId,
                    ReportName = indexedDoc.ReportName,
                    BusinessDate = metadataFile?.GetIndex(MetadataIndexName.BusinessDate)?.IndexValue ?? "",
                });
            }
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
            catch (Exception ex)
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
                fileProcessingResult.ProcessEvent = fileProcessingResult.ProcessEvent;
                s3FileProcessingResult.Failed.Add(fileProcessingResult);
            }

            return fileProcessingResult;

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
        protected virtual List<ReportMatchedDetails> DoSplit(string binaryFilePath, string tempPath, List<SplitDocumentTypeEntity> splittingDocuments)
        {
            // Set the license files
            SetLicense();

            try
            {
                // Load the PDF document
                Aspose.Pdf.Document pdfDocument = new Aspose.Pdf.Document(binaryFilePath);

                Logger.LogDebug("Check if the file is readable or not. " + $"BinaryFilePath={binaryFilePath}");
                //Check file is Readable or non-readable
                bool isFileReadable = IsFIleReadable(binaryFilePath);

                List<SplitterResult> splitterResults = new List<SplitterResult>();
                SplitterResult currentSplitterResult = new SplitterResult();
                var pageReportMatch = new List<ReportMatchedDetails>();

                Logger.LogDebug("Starting data extraction from PDF file pages for bulk split. " + $"BinaryFilePath={binaryFilePath}");
                if (splittingDocuments.Count > 0)
                {
                    if (splittingDocuments[0].SplittingTypeOrder.ToLower().Equals("ReportNameCompare".ToLower()))
                    {

                        int pdfWordCount = 25; //default word count
                        int pdfLineCount = 4;
                        bool ifNeedLine = false;

                        Logger.LogInformation("Default Word Count - " + pdfWordCount);
                        Logger.LogInformation("Default Line Count - " + pdfLineCount);

                        int pageNumber = 0;
                        bool indexedNomatchedReports = true;

                        foreach (Page page in pdfDocument.Pages)
                        {

                            string extractedPortion = string.Empty.ToString();

                            if (!splittingDocuments[0].WordCount.ToLower().Equals("n/a") && !splittingDocuments[0].WordCount.Equals(null))
                            {
                                pdfWordCount = Convert.ToInt32(splittingDocuments[0].WordCount);
                                Logger.LogInformation("Word count from DB - " + pdfWordCount);
                            }
                            else
                            {
                                ifNeedLine = true;
                            }
                            if (ifNeedLine)
                            {
                                if (!splittingDocuments[0].LineCount.ToLower().Equals("n/a") && !splittingDocuments[0].LineCount.Equals(null))
                                {

                                    pdfLineCount = Convert.ToInt32(splittingDocuments[0].LineCount);
                                    Logger.LogInformation("Line count from DB - " + pdfLineCount);
                                }
                            }

                            //if (!splittingDocuments[0].LineCount.ToLower().Equals("n/a") | !splittingDocuments[0].LineCount.Equals(null))
                            //{
                            //    pdfLineCount = Convert.ToInt32(splittingDocuments[0].LineCount);
                            //    Logger.LogInformation("Line count from DB - " + pdfLineCount);
                            //}

                            if (ifNeedLine)
                            {
                                extractedPortion = ExtractGivenLinesFromPDFPage(page, pdfLineCount, isFileReadable);
                                Logger.LogInformation("Successfully extracted lines from page");
                            }
                            else
                            {
                                extractedPortion = ExtractGivenWordsFromPDFPage(page, pdfWordCount, isFileReadable);
                                Logger.LogInformation("Successfully extracted word from page");
                            }

                            if (!extractedPortion.Equals(string.Empty) | !string.IsNullOrWhiteSpace(extractedPortion))
                            {
                                if (indexedNomatchedReports)
                                {
                                    bool isMatched = false;
                                    List<string> macthedRptName = new List<string>();
                                    // Sort the list from longest to shortest
                                    var sortedReportNames = splittingDocuments[0].ReportNames.OrderByDescending(name => name.Length).ToList();

                                    foreach (var reportName in sortedReportNames)
                                    {
                                        if (Regex.IsMatch(extractedPortion, $"{reportName}(\r\n|\r|\n|  )", RegexOptions.IgnoreCase))
                                        {
                                            pageReportMatch.Add(new ReportMatchedDetails { page = pageNumber + 1, ReportName = reportName, PageData = page, PageExreactedData = extractedPortion });
                                            isMatched = true;
                                            break;
                                        }
                                        else if (Regex.IsMatch(extractedPortion, $"{reportName}(\r\n|\r|\n| )", RegexOptions.IgnoreCase))
                                        {
                                            pageReportMatch.Add(new ReportMatchedDetails { page = pageNumber + 1, ReportName = reportName, PageData = page, PageExreactedData = extractedPortion });
                                            isMatched = true;
                                            break;
                                        }
                                        else if (Regex.IsMatch(extractedPortion, $"{reportName}", RegexOptions.IgnoreCase))
                                        {
                                            pageReportMatch.Add(new ReportMatchedDetails { page = pageNumber + 1, ReportName = reportName, PageData = page, PageExreactedData = extractedPortion });
                                            isMatched = true;
                                            break;
                                        }
                                    }



                                    //foreach (var reportName in splittingDocuments[0].ReportNames)
                                    //{
                                    //    if (Regex.IsMatch(extractedPortion, $"{reportName}(\r\n|\r|\n|  )", RegexOptions.IgnoreCase))
                                    //    {
                                    //        macthedRptName.Add(reportName);
                                    //    }
                                    //    else if (Regex.IsMatch(extractedPortion, $@"(?<!\S)\b{Regex.Escape(reportName)}\b(?!\s*\w)", RegexOptions.IgnoreCase))
                                    //    {
                                    //        macthedRptName.Add(reportName);
                                    //    }
 

                                    //    //bool isReportNameInAnotherReport = false;
                                    //    //foreach (var rptNme in splittingDocuments[0].ReportNames)
                                    //    //{
                                    //    //    if (!reportName.Equals(rptNme))
                                    //    //    {
                                    //    //        if (Regex.IsMatch(rptNme, reportName, RegexOptions.IgnoreCase))
                                    //    //        {
                                    //    //            if (Regex.IsMatch(extractedPortion, $"{rptNme}(\r\n|\r|\n|  )" , RegexOptions.IgnoreCase))
                                    //    //            {
                                    //    //                pageReportMatch.Add(new ReportMatchedDetails { page = pageNumber + 1, ReportName = rptNme, PageData = page, PageExreactedData = extractedPortion });
                                    //    //                isMatched = true;
                                    //    //                isReportNameInAnotherReport = true;
                                    //    //                break;
                                    //    //            }
                                    //    //            else
                                    //    //            {
                                    //    //                if (Regex.IsMatch(extractedPortion, $"{rptNme}(\r\n|\r|\n| )", RegexOptions.IgnoreCase))
                                    //    //                {
                                    //    //                    pageReportMatch.Add(new ReportMatchedDetails { page = pageNumber + 1, ReportName = rptNme, PageData = page, PageExreactedData = extractedPortion });
                                    //    //                    isMatched = true;
                                    //    //                    isReportNameInAnotherReport = true;
                                    //    //                    break;
                                    //    //                }
                                    //    //            }

                                    //    //        }
                                    //    //    }

                                    //    //}

                                    //    //if (!isMatched)
                                    //    //{
                                    //    //    if (!isReportNameInAnotherReport)
                                    //    //    {
                                    //    //        //$"{reportName}(\r\n|\r|\n|  )"   \b{reportName}(\r\n|\r|\n|[ ]{2,})
                                    //    //        if (Regex.IsMatch(extractedPortion, $"{reportName}(\r\n|\r|\n|  )", RegexOptions.IgnoreCase))
                                    //    //        {
                                    //    //            pageReportMatch.Add(new ReportMatchedDetails { page = pageNumber + 1, ReportName = reportName, PageData = page, PageExreactedData = extractedPortion });
                                    //    //            isMatched = true;
                                    //    //            break;
                                    //    //        }
                                    //    //        else
                                    //    //        {
                                    //    //            if (Regex.IsMatch(extractedPortion, $"{reportName}", RegexOptions.IgnoreCase))
                                    //    //            {
                                    //    //                pageReportMatch.Add(new ReportMatchedDetails { page = pageNumber + 1, ReportName = reportName, PageData = page, PageExreactedData = extractedPortion });
                                    //    //                isMatched = true;
                                    //    //                break;
                                    //    //            }
                                    //    //        }
                                    //    //    }
                                    //    //}
                                    //}

                                    //if (macthedRptName.Count > 1)
                                    //{
                                    //    string machedRept = string.Empty;
                                    //    int length = 0;
                                    //    int max = 0;
                                    //    foreach (var nme in macthedRptName)
                                    //    {
                                    //        length = nme.Length;
                                    //        if (length > max)
                                    //        {
                                    //            max = length;
                                    //            machedRept = nme;
                                    //        }
                                    //    }

                                    //    pageReportMatch.Add(new ReportMatchedDetails { page = pageNumber + 1, ReportName = machedRept, PageData = page, PageExreactedData = extractedPortion });
                                    //    isMatched = true;

                                    //}
                                    //if (macthedRptName.Count.Equals(1))
                                    //{
                                    //    pageReportMatch.Add(new ReportMatchedDetails { page = pageNumber + 1, ReportName = macthedRptName[0], PageData = page, PageExreactedData = extractedPortion });
                                    //    isMatched = true;
                                    //}
                                    if (!isMatched)
                                    {
                                        pageReportMatch.Add(new ReportMatchedDetails { page = pageNumber + 1, ReportName = "MISSING", PageData = page, PageExreactedData = extractedPortion });
                                    }
                                }
                                {
                                    //TO DO:
                                }

                                //dataMappingResult.MappedDocuments.Where(x => x.Success).ToList()

                            }
                            else
                            {
                                Logger.LogInformation($"No any content or (IsNullOrWhiteSpace) in mentioned line or word count - {splittingDocuments[0].SplittingTypeOrder} " +
                      $"BinaryFilePath={binaryFilePath}");
                            }
                            pageNumber++;
                        }

                    }
                    else
                    {
                        Logger.LogInformation($"This type is not supported. Type - {splittingDocuments[0].SplittingTypeOrder} " +
                      $"BinaryFilePath={binaryFilePath}");
                        //TO DO:
                    }

                }
                else
                {
                    Logger.LogWarning($"No DB entry for split this file. " +
                       $"BinaryFilePath={binaryFilePath}");
                }
                return pageReportMatch;
            }
            catch (Exception ex)
            {
                Logger.LogError("Error occurred while splitting file. " + $"BinaryFilePath={binaryFilePath} , Error - {ex}");
                throw;
            }
        }

        protected virtual string ExtractBusinessDate(string extractedTxt, string expectedFormat, string outputDateFormat)
        {
            Logger.LogInformation($"Extracting business date from the page, format - {expectedFormat}");
            var dateFormats = new Dictionary<string, string>
            {
                { "dd/MM/yyyy", @"\b\d{2}/\d{2}/\d{4}\b" },
                { "MM/dd/yyyy", @"\b\d{2}/\d{2}/\d{4}\b" },
                { "yyyy/MM/dd", @"\b\d{4}/\d{2}/\d{2}\b" },
                { "dd-MM-yyyy", @"\b\d{2}-\d{2}-\d{4}\b" },
                { "MM-dd-yyyy", @"\b\d{2}-\d{2}-\d{4}\b" },
                { "yyyy-MM-dd", @"\b\d{4}-\d{2}-\d{2}\b" },
                { "dd.MM.yyyy", @"\b\d{2}\.\d{2}\.\d{4}\b" },
                { "MM.dd.yyyy", @"\b\d{2}\.\d{2}\.\d{4}\b" },
                { "yyyy.MM.dd", @"\b\d{4}\.\d{2}\.\d{2}\b" },
                { "dd MMM yyyy", @"\b\d{2} [A-Za-z]{3} \d{4}\b" },
                { "MMM dd, yyyy", @"\b[A-Za-z]{3} \d{2}, \d{4}\b" },
                { "yyyy MMM dd", @"\b\d{4} [A-Za-z]{3} \d{2}\b" },
                { "d/M/yyyy", @"\b\d{1,2}/\d{1,2}/\d{4}\b" },
                { "M/d/yyyy", @"\b\d{1,2}/\d{1,2}/\d{4}\b" },
                { "yyyy/M/d", @"\b\d{4}/\d{1,2}/\d{1,2}\b" },
                { "d-M-yyyy", @"\b\d{1,2}-\d{1,2}-\d{4}\b" },
                { "M-d-yyyy", @"\b\d{1,2}-\d{1,2}-\d{4}\b" },
                { "yyyy-M-d", @"\b\d{4}-\d{1,2}-\d{1,2}\b" },
                { "d.M.yyyy", @"\b\d{1,2}\.\d{1,2}\.\d{4}\b" },
                { "M.d.yyyy", @"\b\d{1,2}\.\d{1,2}\.\d{4}\b" },
                { "yyyy.M.d", @"\b\d{4}\.\d{1,2}\.\d{1,2}\b" },
                { "dd-MMM-yyyy", @"\b\d{2}-[A-Za-z]{3}-\d{4}\b" },
                { "d-MMM-yyyy", @"\b\d{1,2}-[A-Za-z]{3}-\d{4}\b" },
                { "MMM-dd-yyyy", @"\b[A-Za-z]{3}-\d{2}-\d{4}\b" },
                { "MMM d, yyyy", @"\b[A-Za-z]{3} \d{1,2}, \d{4}\b" },
                { "yyyy-MMM-dd", @"\b\d{4}-[A-Za-z]{3}-\d{2}\b" }
            };
            string input = expectedFormat;
            if (expectedFormat.Equals(null) || expectedFormat.Equals(string.Empty))
            {
                Logger.LogInformation($"Given date format is empty or null.");
                input = "dd-MMM-yyyy";
            }
            string pattern = @"\b\d{2}-[A-Za-z]{3}-\d{4}\b";
            bool isMachedAnyPattern = false;
            foreach (var format in dateFormats)
            {
                MatchCollection matches = Regex.Matches(input, format.Key);
                if (matches.Count > 0)
                {
                    isMachedAnyPattern = true;
                    pattern = format.Value;
                    Logger.LogInformation($"Date pattern identified. {pattern}");
                    break;
                }
            }

            if (!isMachedAnyPattern)
            {
                Logger.LogWarning("No any mached pattern for the given format.");

                foreach (var format in dateFormats)
                {
                    MatchCollection matches = Regex.Matches(extractedTxt, format.Value);
                    if (matches.Count > 0)
                    {
                        pattern = format.Value;
                        Logger.LogInformation($"Date pattern identified by using default patterns (Stored in code). {pattern}");
                        break;
                    }
                }

            }

            Match extractedDate = Regex.Match(extractedTxt, pattern);
            if (extractedDate.Success)
            {
                Logger.LogInformation("Matched businessDate: " + extractedDate.Value);
                return extractedDate.Value;
            }
            else
            {
                Logger.LogInformation("No match found for this Bdate.");
                return null;
            }
        }

        protected virtual List<IndexedDocumentDetails> ReportIndexing(List<ReportMatchedDetails> splittedPages, List<IndexDocumentTypeEntity> indexingDocuments, string tempPath, string binaryFilePath)
        {
            try
            {
                var indexedReportList = new List<ReportMatchedDetails>();
                var indexedReports = new List<IndexedDocumentDetails>();

                bool isFileReadable = IsFIleReadable(binaryFilePath);

                if (indexingDocuments.Count > 0)
                {
                    var substititionWords = indexingDocuments[0].SubstitutionWords;
                    var indexReportNames = indexingDocuments[0].ReportNames;
                    string expectedDateFormat = indexingDocuments[0].ExpectedDateFormat;
                    string outputDateFormat = indexingDocuments[0].OutputDateFormat;
                    int extractLineCountIndex = 4; //Default Line count
                    try
                    {
                        extractLineCountIndex = Convert.ToInt32(indexingDocuments[0].FetchNLines);
                    }
                    catch (Exception e)
                    {
                        Logger.LogInformation("Error occured during FetchNLines, set default line count as '4' .");
                    }

                    var groupedData = splittedPages.GroupBy(p => p.ReportName);//Grouped with report name

                    foreach (var group in groupedData)
                    {
                        string newDocId = Guid.NewGuid().ToString();
                        string indexedReportName = "MISSING";
                        bool isMachedSubstitition = false;

                        //substititionWord matching
                        if (substititionWords.Count > 0)
                        {
                            foreach (var substititionWord in substititionWords)
                            {
                                if (group.Key.Equals(substititionWord.Key))
                                {
                                    foreach (var g in group)
                                    {
                                        indexedReportName = substititionWord.Value;
                                        indexedReportList.Add(new ReportMatchedDetails { page = g.page, ReportName = substititionWord.Value, PageData = g.PageData, NewDocId = newDocId });
                                        isMachedSubstitition = true;
                                        break;
                                    }
                                }
                                if (isMachedSubstitition)
                                {
                                    break;
                                }
                            }
                        }

                        if (!isMachedSubstitition)
                        {
                            bool isMatchedReportName = false;
                            foreach (var reportNme in indexReportNames)
                            {
                                foreach (var g in group)
                                {
                                    if (g.ReportName.Equals(reportNme))
                                    {
                                        indexedReportName = reportNme;
                                        indexedReportList.Add(new ReportMatchedDetails { page = g.page, ReportName = reportNme, PageData = g.PageData, NewDocId = newDocId });
                                        isMatchedReportName = true;
                                        break;
                                    }
                                }
                                if (isMatchedReportName)
                                {
                                    break;
                                }
                            }

                            if (!isMatchedReportName)
                            {
                                foreach (var g in group)
                                {
                                    indexedReportName = g.ReportName;
                                    indexedReportList.Add(new ReportMatchedDetails { page = g.page, ReportName = g.ReportName, PageData = g.PageData, NewDocId = newDocId });
                                    isMatchedReportName = true;
                                    break;
                                }
                            }
                        }
                        try
                        {
                            //Save File in temp file path
                            // Initialize a new PDF document to hold the merged pages
                            Document newTempPdfFile = new Document();

                            // Add each extracted page to the merged PDF document
                            foreach (var page in group)
                            {
                                newTempPdfFile.Pages.Add(page.PageData);
                            }
                            //PDF path
                            string pdfPath = $"{tempPath}/{newDocId}.pdf";

                            // Save the merged PDF document
                            newTempPdfFile.Save(pdfPath);

                            Logger.LogDebug($"PDF pages merged and saved successfully. File path - {pdfPath}");

                            indexedReports.Add(new IndexedDocumentDetails
                            {
                                ReportName = indexedReportName,
                                NewDocId = newDocId,
                                TempFileLocation = pdfPath,
                                S3ObjectName = $"{newDocId}.pdf",
                                //BusinessDate= reportBdate
                            });
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError($"Error occured while saving splitted pdf files. {binaryFilePath} , Error - {ex}");
                            throw;
                        }
                    }
                }
                else
                {
                    try
                    {
                        Logger.LogWarning($"No DB entry for index this file. BinaryPath - {binaryFilePath} , Report Indexed as MISSING without splitting.");
                        string newDocId = Guid.NewGuid().ToString();
                        //Save File in temp file path
                        // Initialize a new PDF document to hold the merged pages
                        Document newTempPdfFile = new Document();

                        // Add each extracted page to the merged PDF document
                        foreach (var page in splittedPages)
                        {
                            newTempPdfFile.Pages.Add(page.PageData);
                        }
                        //PDF path
                        string pdfPath = $"{tempPath}/{newDocId}.pdf";

                        // Save the merged PDF document
                        newTempPdfFile.Save(pdfPath);

                        Logger.LogDebug($"PDF pages merged and saved successfully. File path - {pdfPath}");

                        indexedReports.Add(new IndexedDocumentDetails
                        {
                            ReportName = "MISSING",
                            NewDocId = newDocId,
                            TempFileLocation = pdfPath,
                            S3ObjectName = $"{newDocId}.pdf"
                        });
                    }
                    catch (Exception exp)
                    {
                        Logger.LogError($"Error occured while saving splitted pdf files. {binaryFilePath} , Error - {exp}");
                        throw;
                    }
                }
                return indexedReports;
            }
            catch (Exception ex)
            {
                Logger.LogError("Error occurred while indexing file. " + $"BinaryFilePath={binaryFilePath} , Error - {ex}");
                throw;
            }
        }

        protected virtual string OcrNonReadablePage(string ImagePath)
        {

            // Enable automatic noise removal
            Aspose.OCR.Models.PreprocessingFilters.PreprocessingFilter filters = new Aspose.OCR.Models.PreprocessingFilters.PreprocessingFilter();
            filters.Add(Aspose.OCR.Models.PreprocessingFilters.PreprocessingFilter.Median());


            Aspose.OCR.AsposeOcr api = new Aspose.OCR.AsposeOcr();
            Aspose.OCR.OcrInput input = new Aspose.OCR.OcrInput(Aspose.OCR.InputType.SingleImage);
            Aspose.OCR.RecognitionSettings recognitionSettings = new Aspose.OCR.RecognitionSettings();
            recognitionSettings.Language = Aspose.OCR.Language.Eng;

            input.Add(ImagePath);


            List<Aspose.OCR.RecognitionResult> results = api.Recognize(input);


            //Return the recognized text from the first (and only) result
            if (results.Count > 0)
            {
                return results[0].RecognitionText;
            }
            else
            {
                return string.Empty;
            }

        }

        protected virtual string ExtractGivenWordsFromPDFPage(Page pageText, int wordsCount, bool isReadable)
        {
            string text = string.Empty;
            // Counter for the number of words extracted
            int wordCount = 0;
            // Store the first 25 words
            List<string> extractedWords = new List<string>();

            if (isReadable)
            {
                // Assuming you've already loaded the PDF and accessed the desired page
                TextFragmentAbsorber textFragmentAbsorber = new TextFragmentAbsorber();
                TextExtractionOptions extractionOptions = new TextExtractionOptions(TextExtractionOptions.TextFormattingMode.Pure);

                // Set the text extraction options to ensure reading order is preserved
                textFragmentAbsorber.TextSearchOptions = new TextSearchOptions(true);
                textFragmentAbsorber.ExtractionOptions = extractionOptions;
                pageText.Accept(textFragmentAbsorber);  // pageText is the page you're extracting from

                // Create a list to store fragments with their Y-coordinates
                var fragmentsWithPosition = new List<(TextFragment fragment, double yPosition)>();

                // Extract each fragment and get its Y-position
                foreach (TextFragment fragment in textFragmentAbsorber.TextFragments)
                {
                    fragmentsWithPosition.Add((fragment, fragment.Position.YIndent));
                }

                // Sort the fragments by Y-position to ensure they are in proper order
                var sortedFragments = fragmentsWithPosition.OrderByDescending(f => f.yPosition);

                // Create a StringBuilder to accumulate the text
                StringBuilder combinedText = new StringBuilder();

                // Combine all sorted fragments into a single string
                foreach (var sortedFragment in sortedFragments)
                {
                    combinedText.AppendLine(sortedFragment.fragment.Text); // Append the text with a newline
                }

                // Get the final string
                text = combinedText.ToString();
            }
            else
            {
                string tmpImageName = string.Empty;
                // Create a stream to save the image
                using (MemoryStream imageStream = new MemoryStream())
                {
                    // Create a Resolution object
                    Resolution resolution = new Resolution(300);

                    // Create a PNG device with specified attributes
                    PngDevice pngDevice = new PngDevice(resolution);

                    // Convert a particular page and save the image to stream
                    pngDevice.Process(pageText, imageStream);

                    tmpImageName = $"/tmp/image_{Guid.NewGuid().ToString()}.png";
                    // Save the image
                    File.WriteAllBytes(tmpImageName, imageStream.ToArray());
                }
                text = OcrNonReadablePage(tmpImageName);
            }


            //// Convert the extracted words into a single string for pattern matching
            string extractedText = ExtractWordsWithSpaces(text, wordsCount); ;

            return extractedText;
        }

        private string ExtractWordsWithSpaces(string text, int wordsCount)
        {
            int wordCounter = 0;
            int charIndex = 0;

            while (wordCounter < wordsCount && charIndex < text.Length)
            {
                // Check if the current character is a space
                if (char.IsWhiteSpace(text[charIndex]))
                {
                    // If it's a space, count it as the end of a word if the previous character was not a space
                    if (charIndex > 0 && !char.IsWhiteSpace(text[charIndex - 1]))
                    {
                        wordCounter++;
                    }
                }
                else if (charIndex == text.Length - 1)
                {
                    // Count the last word if we reach the end of the text
                    wordCounter++;
                }

                // Move to the next character
                charIndex++;
            }

            // Return the substring that contains the first `wordsCount` words
            return text.Substring(0, charIndex).Trim();
        }
        protected virtual string ExtractGivenLinesFromPDFPage(Page pageText, int linesCount, bool isReadable)
        {
            // Counter for the number of words extracted
            int wordCount = 0;
            // Variables for storing results
            List<string> extractedLines = new List<string>();

            if (isReadable)
            {
                //TextFragmentAbsorber textFragmentAbsorber = new TextFragmentAbsorber();
                //textFragmentAbsorber.TextSearchOptions = new TextSearchOptions(true);
                //// Accept the absorber for a specific page or all pages
                //pageText.Accept(textFragmentAbsorber);

                //// Extract text from the page
                //string text = textFragmentAbsorber.Text;

                // Assuming you've already loaded the PDF and accessed the desired page
                TextFragmentAbsorber textFragmentAbsorber = new TextFragmentAbsorber();
                TextExtractionOptions extractionOptions = new TextExtractionOptions(TextExtractionOptions.TextFormattingMode.Pure);

                // Set the text extraction options to ensure reading order is preserved
                textFragmentAbsorber.TextSearchOptions = new TextSearchOptions(true);
                textFragmentAbsorber.ExtractionOptions = extractionOptions;
                pageText.Accept(textFragmentAbsorber);  // pageText is the page you're extracting from

                // Get all text fragments
                var textFragments = textFragmentAbsorber.TextFragments;

                // Convert the collection to a list and group fragments by Y-coordinate (to detect lines)
                var fragmentsWithPosition = textFragments.Cast<TextFragment>()
                    .GroupBy(f => f.Position.YIndent)
                    .OrderByDescending(g => g.Key)  // Sort lines by Y position (descending, top to bottom)
                    .ToList();

                // Create a StringBuilder to accumulate the text
                StringBuilder preservedTextBuilder = new StringBuilder();

                // Combine the fragments within each line in their original order
                foreach (var lineGroup in fragmentsWithPosition)
                {
                    // Sort the fragments within the line by X position (left to right)
                    var sortedFragments = lineGroup.OrderBy(f => f.Position.XIndent);

                    // Combine the sorted fragments for this line
                    string lineText = string.Join(" ", sortedFragments.Select(f => f.Text));

                    // Append the line to the final result, followed by a line break
                    preservedTextBuilder.AppendLine(lineText);
                }

                // Get the final text with original line and word order preserved
                string text = preservedTextBuilder.ToString();

                //// Create a list to store fragments with their Y-coordinates
                //var fragmentsWithPosition = new List<(TextFragment fragment, double yPosition)>();

                //// Extract each fragment and get its Y-position
                //foreach (TextFragment fragment in textFragmentAbsorber.TextFragments)
                //{
                //    fragmentsWithPosition.Add((fragment, fragment.Position.YIndent));
                //}

                //// Sort the fragments by Y-position to ensure they are in proper order
                //var sortedFragments = fragmentsWithPosition.OrderByDescending(f => f.yPosition);

                //// Create a StringBuilder to accumulate the text
                //StringBuilder combinedText = new StringBuilder();

                //// Combine all sorted fragments into a single string
                //foreach (var sortedFragment in sortedFragments)
                //{
                //    combinedText.AppendLine(sortedFragment.fragment.Text); // Append the text with a newline
                //}

                //// Get the final string
                // string text = combinedText.ToString();

                // Split text into lines
                string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                // Collect the first 4 lines
                foreach (string line in lines)
                {
                    if (extractedLines.Count <= linesCount)
                    {
                        extractedLines.Add(line.Trim());
                    }
                    else
                    {
                        break;
                    }

                }


            }
            else
            {
                string tmpImageName = string.Empty;
                // Create a stream to save the image
                using (MemoryStream imageStream = new MemoryStream())
                {
                    // Create a Resolution object
                    Resolution resolution = new Resolution(300);

                    // Create a PNG device with specified attributes
                    PngDevice pngDevice = new PngDevice(resolution);

                    // Convert a particular page and save the image to stream
                    pngDevice.Process(pageText, imageStream);

                    tmpImageName = $"/tmp/image_{Guid.NewGuid().ToString()}.png";
                    // Save the image
                    File.WriteAllBytes(tmpImageName, imageStream.ToArray());

                }
                string text = OcrNonReadablePage(tmpImageName);
                // Split text into lines
                string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                // Collect the first 4 lines
                foreach (string line in lines)
                {
                    if (extractedLines.Count <= linesCount)
                    {
                        extractedLines.Add(line.Trim());
                    }
                    else
                    {
                        break;
                    }

                }

            }

            // Convert the extracted lines into a single string for pattern matching
            string extractedTextLines = string.Join(Environment.NewLine, extractedLines);
            return extractedTextLines;
        }


        protected async Task<ProcessedFileEntry> ProcessFile(string s3Bucket, string metadataS3Key)
        {

            Logger.LogInformation($"Starting spliiting The barcode File. ");
            string currentProcessingStage = STAGE_START;//track processing stage

            ProcessedFileEntry result = new ProcessedFileEntry();
            List<MessegeSettings> indexedDataList = new List<MessegeSettings>();
            ConvertedDocument ConvertedDocument = new ConvertedDocument();
            MetadataFile metadataFile = null;
            string binaryFilePath = String.Empty;
            bool errorOccurred = false;

            try
            {

                currentProcessingStage = STAGE_S3_DOWNLOADING; //set stage - S3 downloading

                metadataFile = await ReadMetadataFile(s3Bucket, metadataS3Key);//download and read metadata file

                ////var qmsMessageEvent = CreateFileInEvent(metadataFile);//send File In qms message
                //await SendQMSMessage(qmsMessageEvent);
                ////try{Console.WriteLine(qmsMessageEvent.ToJson().ToString());}catch (Exception){}

                //  - connect with the mongo database
                var indexingDocuments = await DataService.GetIndexDocumentTypeAsync(metadataFile);
                var splittingDocuments = await DataService.GetSplitDocumentTypeAsync(metadataFile);

                var tempPath = Configuration.GetTempPath();

                binaryFilePath = await ReadBinaryFile(s3Bucket, metadataFile.S3Key, tempPath);//download XPS file

                currentProcessingStage = STAGE_FILE_SPLITING;
                var splittedDocument = DoSplit(binaryFilePath, tempPath, splittingDocuments);

                //Missing QMS
                EventBody evnt = ReportNameSubstitutedAsMissing(metadataFile, splittedDocument);
                await SendQMSMessage(evnt);

                currentProcessingStage = STAGE_FILE_INDEXING;
                var indexedReports = ReportIndexing(splittedDocument, indexingDocuments, tempPath, binaryFilePath);

                // - save in the s3 bucket
                var failedItems = new List<UploadFailure>();

                currentProcessingStage = STAGE_S3_UPLOAD;
                foreach (var indexedDoc in indexedReports)
                {
                    
                    try
                    {
                        string newReportName = indexedDoc.ReportName;
                        if (indexedReports.Count.Equals(1))
                        {
                            if (indexedDoc.ReportName.Equals("MISSING"))
                            {
                                newReportName = metadataFile.GetIndex(MetadataIndexName.ReportName)?.IndexValue ?? "";
                            }
                        }
                        string newS3KeyPath = "";
                        if (Configuration.GetConvertedS3Path().Equals("") || Configuration.GetConvertedS3Path().Equals(null))
                        {
                            newS3KeyPath = indexedDoc.S3ObjectName;
                        }
                        else
                        {
                            newS3KeyPath = $"{Configuration.GetConvertedS3Path()}/{indexedDoc.S3ObjectName}";
                        }
                        var converterFileUploadRequest = new LmdFileUploadRequest()
                        {
                            BucketName = Configuration.GetConvertedS3Bucket(),
                            S3Path = Configuration.GetConvertedS3Path(),
                            FilePath = indexedDoc.TempFileLocation,
                            S3KeyForFile = indexedDoc.S3ObjectName
                        };
                        var converterUplaodResult = await UploadToS3(converterFileUploadRequest);

                        indexedDataList.Add(new MessegeSettings
                        {
                            s3BucketName = s3Bucket,
                            s3KeyMedatada = metadataS3Key,
                            docId = metadataFile.DocID,
                            chainId = metadataFile.ChainID,
                            fileExtension = metadataFile.GetIndex(MetadataIndexName.FileExtension)?.IndexValue ?? "",
                            s3Key = metadataFile.S3Key,
                            s3Url = metadataFile.S3Url,
                            hotel = metadataFile.GetIndex(MetadataIndexName.Hotels)?.IndexValue ?? "",
                            hmg = metadataFile.GetIndex(MetadataIndexName.ManagementGroup)?.IndexValue ?? "",
                            reportName = newReportName,
                            connector = metadataFile.GetIndex(MetadataIndexName.ConnectorID)?.IndexValue ?? "",
                            entityId = metadataFile.EntityID,
                            projectId = metadataFile.ProjectID,
                            newDocId = indexedDoc.NewDocId,
                            newS3Key = newS3KeyPath,
                            newS3Bucket = Configuration.GetConvertedS3Bucket(),
                            processed = true,
                        });

                        //Clean up temp splitted files
                        CleanUp(indexedDoc.TempFileLocation);
                    }
                    catch (Exception ex)
                    {
                        //failedItems.Add(new UploadFailure()
                        //{
                        //    Document = ",
                        //    Error = ex
                        //});
                        continue;
                    }

                }

                //result.ProcessEvent = indexedDataList;
                //result.Processed = true;


                //QMS Processed Event
                var qmsMessageEvent = CreateFileProcessedEvent(metadataFile, indexedReports);
                await SendQMSMessage(qmsMessageEvent);


                if (failedItems.Count != 0)
                {
                    Logger.LogWarning($"Following reports are not uploaded to the S3 bucket FailedCount={failedItems.Count}");

                    foreach (var item in failedItems)
                    {
                        Logger.LogError($"Document={item.Document?.NewFileName},Error={item.Error}");
                    }
                }
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
                    FailedAt = currentProcessingStage
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
                    FailedAt = currentProcessingStage
                };

                var processedQmsMessageEvent = CreateFileProcessingErrorEvent(metadataFile, errrorData);//send File In qms message
                await SendQMSMessage(processedQmsMessageEvent);
                try
                {
                    Console.WriteLine(processedQmsMessageEvent.ToJson().ToString());
                }
                catch (Exception) { }

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
                    FailedAt = currentProcessingStage
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
                    result.ProcessEvent = indexedDataList;
                    result.ConvertedDocument = ConvertedDocument;
                }

                //clean up tempory file
                CleanUp(binaryFilePath);

                //Clean up temp splitted files


                Logger?.LogInformation($"cleaned all temporary files. " +
                       $"binaryFilePath={binaryFilePath}," +
                       $"S3Bucket={s3Bucket}," +
                       $"EntityId={metadataFile?.EntityID}," +
                       $"ProjectId={metadataFile?.ProjectID}");

            }

            return result;
        }

        public virtual void SetLicense()
        {
            try
            {
                string licensePath = "Aspose.Total.Product.Family.lic";

                var pdfLicense = new Aspose.Pdf.License();
                pdfLicense.SetLicense(licensePath);

                var drawingLicense = new Aspose.Drawing.License();
                drawingLicense.SetLicense(licensePath);

                var ocrLicense = new Aspose.OCR.License();
                ocrLicense.SetLicense(licensePath);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error occurred while setting up the Aspose license. Error - {ex} ");
                throw;
            }

        }

        #endregion

        #region S3 Upload
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
                    throw new FileUploadException($"S3 service returned error while uploading file to Tempory S3 path. " +
                        $"HttpCode={response.ResponseCode}" +
                        $"S3Key={lmdFileUploadRequest.S3KeyForFile}," +
                        $"Bucket={lmdFileUploadRequest.BucketName}," +
                        $"SubPath={lmdFileUploadRequest.S3Path}," +
                        $"SourceFilePath={lmdFileUploadRequest.FilePath}");
                }
            }
            catch (Exception ex)
            {
                throw new FileUploadException($"Error occurred while uploading file to Tempory S3 path. " +
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
            var NewFilePath = Configuration.GetConvertedS3Path();
            return $"{NewFilePath}{docId}.{extension.Trim('.')}";
        }
        #endregion
        public abstract Task<object> Run(object input);
        public IConfigurationRoot Configuration { get; }
        public ILogger<T> Logger { get; }
        public IAmazonS3 AmazonS3Client { get; }
        public IDataService DataService { get; }
        public IAmazonSimpleNotificationService SnsClient { get; }
    }
}
