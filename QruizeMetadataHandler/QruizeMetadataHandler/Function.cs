using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService;
using QruizeMetadataHandler.Core.App;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QMS = MDO2.Core.QMS;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using QruizeMetadataHandler.Core.App.Model;
using Newtonsoft.Json.Linq;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]


namespace QruizeMetadataHandler
{
    public class Function
    {
        private readonly ILogger logger;
        private readonly IAmazonSimpleNotificationService snsService;
        private readonly IConfigurationRoot configurationRoot;

        public Function()
        {
            Startup.Setup();
            logger = Startup.ServiceProvider.GetRequiredService<ILogger<Function>>();
            snsService = Startup.ServiceProvider.GetService<IAmazonSimpleNotificationService>();
            configurationRoot = Startup.ServiceProvider.GetService<IConfigurationRoot>();
        }

        private void SendQmsSystemError(string errorMessage, string failedAt, Exception exception)
        {
            if (snsService != null && configurationRoot != null)
            {
                var arn = configurationRoot.GetValue<string>(AppConstants.CFG_KEY_QMS_SNS_ARN);
                if (string.IsNullOrWhiteSpace(arn))
                {
                    var evetSrc = configurationRoot.GetValue<string>(AppConstants.CFG_KEY_QMS_EVENT_SOURCE_NAME);
                    var evb = QMS.Model.Message.EventBodyFactory
                        .Create<QMS.Model.Message.EventData.EventDataSystemError>(QMS.Model.Message.EventLevel.ERROR, evetSrc);
                    var evd = (QMS.Model.Message.EventData.EventDataSystemError)evb.Data;
                    evd.ExceptionData = exception?.ToString();
                    evd.ErrorMessage = errorMessage;
                    evd.FailedAt = failedAt;

                    var client = new QMS.SnsQmsClient(snsService);
                    var task = Task.Factory.StartNew(() => client.SendEventAsync(arn, evb));
                    task.Wait();
                }
            }
            else
            {
                logger?.LogWarning($"Sns service or configuration service not available. " +
                    $"SnsAvailable={snsService != null},ConfigAvailable={configurationRoot != null}");
            }
        }

        /// <summary>
        /// SNS event based function handler
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        
        public async Task <JsonArray> FunctionHandler(JsonArray jsonEvnt, ILambdaContext context)
        {
            
            var EventData = JsonConvert.DeserializeObject<List<MessegeSettings>>(jsonEvnt.ToString());// deserialize the json object
            var jsonObjectList = new List<JsonObject>(); // List to hold JsonObject instances

            logger?.LogInformation($"Received Step Function trigger. Entries={EventData}");
            var app = Startup.ServiceProvider.GetRequiredService<SFAppEntryPoint>();
            var result = await app.Run(EventData);//run the main event
            string json = JsonConvert.SerializeObject(result);//serialize the final data

            var jsonObject = JsonObject.Parse(json);// Parse JSON string to JsonObject
            //JArray jsonArray = JArray.Parse(json);

            return (JsonArray)jsonObject;
            //foreach (var e in EventData) {

            //    //logger?.LogInformation($"Received Step Function trigger. Entries={e}");

            //    //var app = Startup.ServiceProvider.GetRequiredService<SFAppEntryPoint>();
            //    //var result = await app.Run(e);//run the main event

            //    string json = JsonConvert.SerializeObject(result);//serialize the final data

            //    var jsonNode = JsonNode.Parse(json); // Parse JSON string to JsonNode

            //    if (jsonNode is JsonObject jsonObject) // Check if JsonNode is a JsonObject
            //    {
            //        jsonObjectList.Add(jsonObject); // Add the JsonObject to the list
            //    }
            //    else
            //    {
            //        logger?.LogError("The parsed JSON is not a JsonObject.");
            //        throw new InvalidOperationException("Parsed JSON is not a JsonObject.");
            //    }

            //}
            //return jsonObjectList;
        }
    }
}