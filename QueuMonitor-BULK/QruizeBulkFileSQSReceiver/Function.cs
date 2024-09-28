using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.Runtime.Internal.Util;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MDO2.Core.QMS.Model.Message;
using MDO2.Core.QMS.Model.Message.EventData;
using MDO2.Core.QMS;
using Amazon.SimpleNotificationService;
using Amazon.Lambda.Logging.AspNetCore;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace QruizeBulkFileSQSReceiver;

public class Function
{

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    /// 
    private readonly IAmazonStepFunctions _stepFunctionsClient;
    //build configuration
    IConfigurationRoot configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
        .AddEnvironmentVariables()
        .Build();
    IAmazonSimpleNotificationService snsClient;
    private readonly ILogger<Function> logger;
    public Function()
    {
        _stepFunctionsClient = new AmazonStepFunctionsClient();

        // Create a LoggerFactory and configure logging directly
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddLambdaLogger(new LambdaLoggerOptions
            {
                IncludeEventId = false
            });
            builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
        });

        // Create a logger
        logger = loggerFactory.CreateLogger<Function>();
        snsClient= new AmazonSimpleNotificationServiceClient();
    }

    public async Task FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        if (sqsEvent.Records.Count > 0)
        {
            foreach (var record in sqsEvent.Records)
            {
                var message = record.Body;
                string fileformats = configuration.GetFileformats();
                var EventData = JsonConvert.DeserializeObject<SfInputMessegeData>(message.ToString());
                try
                {
                    string extn = EventData.fileExtension;
                    EventData.fileExtension = extn.ToLower();
                    //send queue monitor qms message
                    EventBody qmsMessageEvent = CreateFileQueueMonitor(EventData);
                    await SendQMSMessage(qmsMessageEvent);

                    bool isFileFormatSupported = false;
                    foreach (var fmt in fileformats.Split('|').ToList())
                    {
                        if (fmt.ToLower().Replace(".", "").Equals(EventData.fileExtension.ToLower()))
                        {
                            isFileFormatSupported = true;
                            logger?.LogInformation($"Splitter available for this file extention. File Extension - {EventData.fileExtension.ToLower()}");

                            message = JsonConvert.SerializeObject(EventData);
                            // Invoke Step Function with the message payload
                            await InvokeStepFunctionAsync(message, EventData);
                            break;
                        }
                        else
                        {

                        }
                    }
                    if (!isFileFormatSupported)
                    {
                        //QC Event
                        //send qms qc status message
                        qmsMessageEvent = CreateUnsupportedFileFormatEvent(EventData);
                        //await SendQMSMessage(qmsMessageEvent);

                        logger?.LogWarning($"Bulk splitters are not support for this file format. File Extension - {EventData.fileExtension.ToLower()}");
                    }

                }
                catch (Exception ex)
                {

                    //send qms status message
                    var errorData = new QmsErrorEventData()
                    {
                        ErrorCode = "300",
                        ErrorMessage = "Queue monitoring error",
                        Exception = ex,
                        FailedAt = "STARTING"
                    };
                    EventBody qmsMessageEvent = CreateFileProcessingErrorEvent(EventData, errorData);
                    await SendQMSMessage(qmsMessageEvent);


                    logger?.LogError(ex, $"Error occurred during s3 event process. " +
                        $"S3Key={EventData?.s3Key}," +
                        $"DocId ={EventData?.docId}");
                }

            }
        }
        else
        {
            logger?.LogWarning("No recods found with received SQS.");
        }

    }
    private async Task InvokeStepFunctionAsync(string message, SfInputMessegeData msgData)
    {
        try
        {
            var startExecutionRequest = new StartExecutionRequest
            {
                StateMachineArn = configuration.GetStepFunctionARN(),
                Input = message
            };

            await _stepFunctionsClient.StartExecutionAsync(startExecutionRequest);
        }
        catch (Exception ex)
        {
            //send qms status message
            var errorData = new QmsErrorEventData()
            {
                ErrorCode = "301",
                ErrorMessage = "Queue monitoring error",
                Exception = ex,
                FailedAt = "STARTING"
            };
            EventBody qmsMessageEvent = CreateFileProcessingErrorEvent(msgData, errorData);
            await SendQMSMessage(qmsMessageEvent);


            logger?.LogError(ex, $"Error invoking Step Function. " +
                $"S3Key={msgData?.s3Key}," +
                $"DocId ={msgData?.docId}");
        }
    }



    #region QMS
    protected virtual EventBody CreateFileQueueMonitor(SfInputMessegeData inputData)
    {
        var eventSrc = configuration.GetQMSEventSource();
        var evb = EventBodyFactory.Create<ConverterFileInEventData>(EventLevel.INFO, eventSrc);
        var evd = (ConverterFileInEventData)evb.Data;
        evd.DocId = inputData?.docId;
        evd.ChainId = inputData?.chainId;
        evd.S3BucketName = inputData?.s3BucketName;
        evd.S3KeyMedatada = inputData?.s3KeyMedatada;
        evd.S3Key = inputData?.s3Key;

        return evb;
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
            var arn = configuration.GetQMSARN();
            Console.WriteLine(arn);
            if (!string.IsNullOrWhiteSpace(arn))
            {
                var client = new SnsQmsClient(snsClient);
                var response = await client.SendEventAsync(arn, eventBody);
                if (!response.Success)
                {
                    logger?.LogError($"Failed to send qms update. " +
                        $"EventLevel={eventBody.EventLevel}," +
                        $"EventType={eventBody.EventType}," +
                        $"Error={response.Exception?.ToString()}");
                }
                else
                {
                    logger?.LogDebug($"QMS update sent. " +
                        $"EventLevel={eventBody.EventLevel}," +
                        $"EventType={eventBody.EventType}");
                }
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, $"Error occurred while sending qms update. " +
                $"EventLevel={eventBody.EventLevel}," +
                $"EventType={eventBody.EventType}");
        }
    }

    protected virtual EventBody CreateUnsupportedFileFormatEvent(SfInputMessegeData inputData)
    {
        var eventSrc = configuration.GetQMSEventSource();
        var evb = EventBodyFactory.Create<ConverterQcEventData>(EventLevel.WARN, eventSrc);
        var evd = (ConverterQcEventData)evb.Data;
        evd.DocId = inputData?.docId;
        evd.ChainId = inputData?.chainId;
        evd.ParentDocId = "";
        evd.FileExtension = inputData?.fileExtension;
        evd.QcType = ConverterQcEventData.QC_TYPE_FORMAT_NOT_SUPPORTED;
        return evb;
    }

    protected virtual EventBody CreateFileProcessingErrorEvent(SfInputMessegeData inputData, QmsErrorEventData errorData)
    {
        var eventSrc = configuration.GetQMSEventSource();
        var evb = EventBodyFactory.Create<ConverterFileProcessingErrorEventData>(EventLevel.ERROR, eventSrc);
        var evd = (ConverterFileProcessingErrorEventData)evb.Data;
        evd.DocId = inputData?.docId;
        evd.ChainId = inputData?.chainId;
        evd.ParentDocId = "";
        evd.FailedAt = errorData.FailedAt;
        evd.ErrorMessage = errorData.ErrorMessage;
        evd.ExceptionData = errorData.Exception?.ToString() ?? "";
        evd.ErrorCode = errorData.ErrorCode;
        return evb;
    }
    #endregion
}
