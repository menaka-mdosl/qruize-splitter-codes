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
using Amazon.SQS;
using Amazon.SQS.Model;

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
    private readonly IAmazonSQS _sqsClient;
    private static readonly int MAX_RETRY_COUNT = 3;


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
        _sqsClient = new AmazonSQSClient();
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

    public async Task FunctionHandler(ILambdaContext context)
    {
        // Ensure the _sqsClient is instantiated
        if (_sqsClient == null)
        {
            context.Logger.LogError("SQS client is not initialized.");
            return;
        }


        try
        {
            var receiveMessageRequest = new ReceiveMessageRequest
            {
                //  QueueUrl = "https://sqs.us-east-2.amazonaws.com/752606545687/mdo-ngeneric2uat-qruize-splitter-filein",
                QueueUrl =configuration.GetSQSURL(),//"https://sqs.us-west-1.amazonaws.com/752606545687/mdo-qruizereplacement-generic-prod-bulk-filein-dryrun",
               
                MaxNumberOfMessages = configuration.GetMaxNumberOfMessages(),  // Fetch up to 10 messages at a time
                WaitTimeSeconds = 5,       // Long polling to reduce unnecessary invocations
                VisibilityTimeout = 900     // 15 minutes to prevent duplicate processing
            };

           // Console.WriteLine($"QueueUrl = {configuration.GetSQSURL()}");

            var receiveMessageResponse = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest);

            if (receiveMessageResponse.Messages.Count > 0)
            {
                foreach (var message in receiveMessageResponse.Messages)
                {
                    await ProcessMessage(message, context);
                }
            }
            else
            {
                context.Logger.LogInformation("No new messages in SQS.");
            }
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error polling SQS: {ex.Message}");
        }
    }
    private async Task ProcessMessage(Message message, ILambdaContext context)
    {
        try
        {
            int retryCount = GetRetryCount(message);

            if (retryCount >= 3)
            {
                await MoveToDeadLetterQueue(message, context);
                return;
            }

            var eventData = JsonConvert.DeserializeObject<SfInputMessegeData>(message.Body);
            if (eventData == null)
            {
                context.Logger.LogError("Failed to deserialize SQS message.");
                return;
            }

            eventData.fileExtension = eventData.fileExtension.ToLower();
            eventData.receiptHandle = message.ReceiptHandle;

            context.Logger.LogInformation($"Processing file: {eventData.fileExtension}");

            // Extend visibility timeout to prevent reprocessing while Step Function runs
            await ExtendVisibilityTimeout(message.ReceiptHandle, context);

            // Send Queue Monitor message
            EventBody qmsMessageEvent = CreateFileQueueMonitor(eventData);
            await SendQMSMessage(qmsMessageEvent);


            // Get supported file formats
            string fileFormats = configuration.GetFileformats();
            bool isFileFormatSupported = fileFormats.Split('|')
                .Any(fmt => fmt.ToLower().Replace(".", "").Equals(eventData.fileExtension));

            if (isFileFormatSupported)
            {
                context.Logger.LogInformation($"Splitter available for file extension: {eventData.fileExtension}");

               
                // Invoke Step Function
                await InvokeStepFunctionAsync(JsonConvert.SerializeObject(eventData), eventData);
            }
            else
            {
                EventBody qmsMessageEventx = CreateUnsupportedFileFormatEvent(eventData);
                await SendQMSMessage(qmsMessageEventx);
                context.Logger.LogWarning($"Unsupported file format: {eventData.fileExtension}");
            }



        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error processing message: {ex.Message}");
            await IncrementRetryCount(message, context);
        }
    }
    private int GetRetryCount(Message message)
    {
        if (message.Attributes.TryGetValue("ApproximateReceiveCount", out string count))
        {
            return int.Parse(count);
        }
        return 0;
    }

    private async Task MoveToDeadLetterQueue(Message message, ILambdaContext context)
    {
        try
        {
            var sendMessageRequest = new SendMessageRequest
            {
                // QueueUrl = "https://sqs.us-east-2.amazonaws.com/752606545687/mdo-ngeneric2uat-qruize-dlq",
                QueueUrl = configuration.GetDLQSQSURL(),
                //"https://sqs.us-west-1.amazonaws.com/752606545687/mdo-qruizereplacement-generic-prod-bulk-filein-dryrun-dlq",
                MessageBody = message.Body
            };

            await _sqsClient.SendMessageAsync(sendMessageRequest);
            context.Logger.LogInformation("Message moved to DLQ.");

            // Delete message from the main queue after moving it to DLQ
            // await _sqsClient.DeleteMessageAsync("https://sqs.us-east-2.amazonaws.com/752606545687/mdo-ngeneric2uat-qruize-splitter-filein", message.ReceiptHandle);
            await _sqsClient.DeleteMessageAsync(configuration.GetSQSURL(), message.ReceiptHandle);

        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Failed to move message to DLQ: {ex.Message}");
        }
    }

    private async Task IncrementRetryCount(Message message, ILambdaContext context)
{
    int retryCount = GetRetryCount(message) + 1;

    var sendMessageRequest = new SendMessageRequest
    {
        //  QueueUrl = "https://sqs.us-east-2.amazonaws.com/752606545687/mdo-ngeneric2uat-qruize-splitter-filein",
        QueueUrl = configuration.GetSQSURL(),//"https://sqs.us-west-1.amazonaws.com/752606545687/mdo-qruizereplacement-generic-prod-bulk-filein-dryrun",
        MessageBody = message.Body,
        MessageAttributes = new Dictionary<string, MessageAttributeValue>
        {
            {
                "RetryCount", new MessageAttributeValue
                {
                    StringValue = retryCount.ToString(),
                    DataType = "Number"
                }
            }
        }
    };

    await _sqsClient.SendMessageAsync(sendMessageRequest);
        // await _sqsClient.DeleteMessageAsync("https://sqs.us-east-2.amazonaws.com/752606545687/mdo-ngeneric2uat-qruize-splitter-filein", message.ReceiptHandle);
        await _sqsClient.DeleteMessageAsync(configuration.GetSQSURL(), message.ReceiptHandle);
        context.Logger.LogInformation($"Message requeued with retry count: {retryCount}");
}


    private async Task ExtendVisibilityTimeout(string receiptHandle, ILambdaContext context)
    {
        try
        {
            var changeVisibilityRequest = new ChangeMessageVisibilityRequest
            {
                //QueueUrl = "https://sqs.us-east-2.amazonaws.com/752606545687/mdo-ngeneric2uat-qruize-splitter-filein",
                QueueUrl = configuration.GetSQSURL(),//"https://sqs.us-west-1.amazonaws.com/752606545687/mdo-qruizereplacement-generic-prod-bulk-filein-dryrun",
                ReceiptHandle = receiptHandle,
                VisibilityTimeout = 900 // Extend visibility timeout to prevent duplicate processing
            };

            await _sqsClient.ChangeMessageVisibilityAsync(changeVisibilityRequest);
            context.Logger.LogInformation($"Visibility timeout extended for message: {receiptHandle}");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error extending visibility timeout: {ex.Message}");
        }
    }

    //public async Task FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    //{


    //     // Event pooolong

    //    if (sqsEvent?.Records?.Count > 0)
    //    {
    //        foreach (var record in sqsEvent.Records)
    //        {
    //            var message = record.Body;
    //            var receiptHandle = record.ReceiptHandle;  // Extract Receipt Handle

    //            // Log the receipt handle
    //            context.Logger.LogInformation($"Received message with Receipt Handle: {receiptHandle}");


    //            string fileformats = configuration.GetFileformats();

    //            if (string.IsNullOrEmpty(fileformats))
    //            {
    //                context.Logger.LogError("File formats configuration is missing or empty.");
    //                continue;  // Skip this record if the file formats are not configured
    //            }

    //            try
    //            {
    //                // Deserialize the message
    //                var eventData = JsonConvert.DeserializeObject<SfInputMessegeData>(message);

    //                if (eventData == null)
    //                {
    //                    context.Logger.LogError("Failed to deserialize the message into SfInputMessegeData.");
    //                    continue;  // Skip this record if deserialization fails
    //                }

    //                // Set the file extension and pass receipt handle
    //                string extn = eventData.fileExtension;
    //                eventData.fileExtension = extn.ToLower();
    //                eventData.receiptHandle = receiptHandle;  // Pass receipt handle to eventData




    //                // Log the message details for processing
    //                context.Logger.LogInformation($"Processing message with file extension: {eventData.fileExtension}");

    //                // Check for supported file formats
    //                bool isFileFormatSupported = false;
    //                foreach (var fmt in fileformats.Split('|').ToList())
    //                {
    //                    if (fmt.ToLower().Replace(".", "").Equals(eventData.fileExtension.ToLower()))
    //                    {
    //                        isFileFormatSupported = true;
    //                        context.Logger.LogInformation($"Splitter available for this file extension. File Extension - {eventData.fileExtension.ToLower()}");

    //                        // Change visibility timeout for the SQS message
    //                        var sqsClient = new AmazonSQSClient();
    //                        var changeVisibilityRequest = new ChangeMessageVisibilityRequest
    //                        {
    //                            QueueUrl = "https://sqs.us-east-2.amazonaws.com/752606545687/mdo-ngeneric2uat-qruize-splitter-filein",
    //                            ReceiptHandle = record.ReceiptHandle,
    //                            VisibilityTimeout = 900 // Adjust based on your Step Function processing time
    //                        };
    //                        await sqsClient.ChangeMessageVisibilityAsync(changeVisibilityRequest);
    //                        context.Logger.LogLine($"Visibility timeout extended for message: {record.MessageId}");

    //                        // Send Queue Monitor message
    //                        EventBody qmsMessageEvent = CreateFileQueueMonitor(eventData);
    //                        await SendQMSMessage(qmsMessageEvent);

    //                        // Invoke Step Function with the message payload
    //                        await InvokeStepFunctionAsync(JsonConvert.SerializeObject(eventData), eventData);
    //                        break;  // Exit loop after successful processing
    //                    }
    //                }

    //                if (!isFileFormatSupported)
    //                {
    //                    context.Logger.LogWarning($"File format not supported: {eventData.fileExtension.ToLower()}");
    //                }
    //            }
    //            catch (JsonException jsonEx)
    //            {
    //                context.Logger.LogError($"JSON deserialization error: {jsonEx.Message}");
    //                // Send error details to QMS
    //                var errorData = new QmsErrorEventData
    //                {
    //                    ErrorCode = "300",
    //                    ErrorMessage = "Queue monitoring error - JSON deserialization failure",
    //                    Exception = jsonEx,
    //                    FailedAt = "DESERIALIZATION"
    //                };
    //                EventBody qmsMessageEvent = CreateFileProcessingErrorEvent(null, errorData);
    //                await SendQMSMessage(qmsMessageEvent);
    //            }
    //            catch (Exception ex)
    //            {
    //                context.Logger.LogError($"Unexpected error processing message {record.MessageId}: {ex.Message}");
    //                // Send general error details to QMS
    //                var errorData = new QmsErrorEventData
    //                {
    //                    ErrorCode = "300",
    //                    ErrorMessage = "Queue monitoring error",
    //                    Exception = ex,
    //                    FailedAt = "STARTING"
    //                };
    //                EventBody qmsMessageEvent = CreateFileProcessingErrorEvent(null, errorData);
    //                await SendQMSMessage(qmsMessageEvent);
    //            }
    //        }
    //    }
    //    else
    //    {
    //        context.Logger.LogWarning("No records found in the received SQS event.");
    //    }
    //}


    //public async Task FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    //{


    //   // Event pooolong

    //    if (sqsEvent.Records.Count > 0)
    //    {
    //        foreach (var record in sqsEvent.Records)
    //        {
    //            var message = record.Body;
    //            var receiptHandle = record.ReceiptHandle;  // Extract Receipt Handle
    //            string fileformats = configuration.GetFileformats();
    //            var EventData = JsonConvert.DeserializeObject<SfInputMessegeData>(message.ToString());
    //            try
    //            {
    //                string extn = EventData.fileExtension;
    //                EventData.fileExtension = extn.ToLower();
    //                // Add receipt handle to eventData
    //                EventData.receiptHandle = receiptHandle;

    //                //send queue monitor qms message
    //                EventBody qmsMessageEvent = CreateFileQueueMonitor(EventData);
    //                await SendQMSMessage(qmsMessageEvent);

    //                bool isFileFormatSupported = false;
    //                foreach (var fmt in fileformats.Split('|').ToList())
    //                {
    //                    if (fmt.ToLower().Replace(".", "").Equals(EventData.fileExtension.ToLower()))
    //                    {
    //                        isFileFormatSupported = true;
    //                        logger?.LogInformation($"Splitter available for this file extention. File Extension - {EventData.fileExtension.ToLower()}");


    //                        // Log the incoming message
    //                        context.Logger.LogLine($"Processing message: {record.Body}");

    //                        // Change visibility timeout for the SQS message
    //                        var sqsClient = new AmazonSQSClient();
    //                        var changeVisibilityRequest = new ChangeMessageVisibilityRequest
    //                        {
    //                            QueueUrl = "<Your SQS URL>",
    //                            ReceiptHandle = record.ReceiptHandle,
    //                            VisibilityTimeout = 900 // Adjust this based on your Step Function processing time
    //                        };
    //                        await sqsClient.ChangeMessageVisibilityAsync(changeVisibilityRequest);
    //                        context.Logger.LogLine($"Visibility timeout extended for message: {record.MessageId}");
    //                        //....

    //                        message = JsonConvert.SerializeObject(EventData);
    //                        // Invoke Step Function with the message payload
    //                        await InvokeStepFunctionAsync(message, EventData);
    //                        break;
    //                    }
    //                    else
    //                    {

    //                    }
    //                }
    //                if (!isFileFormatSupported)
    //                {
    //                    //QC Event
    //                    //send qms qc status message
    //                    qmsMessageEvent = CreateUnsupportedFileFormatEvent(EventData);
    //                    //await SendQMSMessage(qmsMessageEvent);

    //                    logger?.LogWarning($"Bulk splitters are not support for this file format. File Extension - {EventData.fileExtension.ToLower()}");
    //                }

    //            }
    //            catch (Exception ex)
    //            {

    //                //send qms status message
    //                var errorData = new QmsErrorEventData()
    //                {
    //                    ErrorCode = "300",
    //                    ErrorMessage = "Queue monitoring error",
    //                    Exception = ex,
    //                    FailedAt = "STARTING"
    //                };
    //                EventBody qmsMessageEvent = CreateFileProcessingErrorEvent(EventData, errorData);
    //                await SendQMSMessage(qmsMessageEvent);


    //                logger?.LogError(ex, $"Error occurred during s3 event process. " +
    //                    $"S3Key={EventData?.s3Key}," +
    //                    $"DocId ={EventData?.docId}");
    //            }

    //        }
    //    }
    //    else
    //    {
    //        logger?.LogWarning("No recods found with received SQS.");
    //    }

    //}

    public async Task SendTaskSuccess(string taskToken, object result)
    {
        var stepFunctionsClient = new AmazonStepFunctionsClient();
        var request = new SendTaskSuccessRequest
        {
            TaskToken = taskToken,
            Output = JsonConvert.SerializeObject(result)
        };

        await stepFunctionsClient.SendTaskSuccessAsync(request);
    }




    private async Task InvokeStepFunctionAsync(string message, SfInputMessegeData msgData)
    {
        try
        {
            if (msgData == null)
            {
                logger?.LogError("InvokeStepFunctionAsync: msgData is null. Cannot proceed.");
                return;
            }

            // Deserialize the existing message to ensure it has the receiptHandle
            var messageData = JsonConvert.DeserializeObject<Dictionary<string, object>>(message);

            if (messageData == null)
            {
                logger?.LogError("InvokeStepFunctionAsync: Message deserialization failed.");
                return;
            }

            // Add receipt handle to the message if it's missing
            if (!messageData.ContainsKey("receiptHandle") && msgData?.receiptHandle != null)
            {
                messageData["receiptHandle"] = msgData.receiptHandle;  // Add receipt handle to message
            }

            // Prepare the Step Function execution request
            var startExecutionRequest = new StartExecutionRequest
            {
                StateMachineArn = configuration.GetStepFunctionARN(),
                Input = JsonConvert.SerializeObject(messageData)  // Pass the updated message with receiptHandle
            };

            // Start execution of the Step Function
            await _stepFunctionsClient.StartExecutionAsync(startExecutionRequest);
            logger?.LogInformation($"Successfully invoked Step Function for S3Key={msgData?.s3Key}, DocId={msgData?.docId}");
        }
        catch (Exception ex)
        {
            // Send QMS status message in case of failure
            var errorData = new QmsErrorEventData()
            {
                ErrorCode = "301",
                ErrorMessage = "Queue monitoring error",
                Exception = ex,
                FailedAt = "STARTING"
            };
            EventBody qmsMessageEvent = CreateFileProcessingErrorEvent(msgData, errorData);
            await SendQMSMessage(qmsMessageEvent);

            // Log error details to CloudWatch
            logger?.LogError(ex, $"Error invoking Step Function. " +
                $"S3Key={msgData?.s3Key}, DocId={msgData?.docId}, Message: {message}");
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
