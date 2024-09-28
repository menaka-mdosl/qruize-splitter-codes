using Amazon;
using Amazon.SimpleNotificationService;
using MDO2.Core.QMS.Model;
using MDO2.Core.QMS.Model.Message;
using System;
using System.Threading.Tasks;

namespace MDO2.Core.QMS
{
    public class SnsQmsClient : IQmsClient, IDisposable
    {
        private readonly bool externalClient = false;
        private readonly IAmazonSimpleNotificationService notificationService;
        private bool disposedValue;

        public SnsQmsClient()
        {
            notificationService = new AmazonSimpleNotificationServiceClient();
        }
        public SnsQmsClient(IAmazonSimpleNotificationService notificationService)
        {
            if (notificationService is null)
            {
                throw new ArgumentNullException(nameof(notificationService));
            }

            externalClient = true;
            this.notificationService = notificationService;
        }
        public SnsQmsClient(string awsAccessKey, string awsSecretKey, RegionEndpoint regionEndpoint)
        {
            if (string.IsNullOrEmpty(awsSecretKey))
            {
                throw new ArgumentException(nameof(awsSecretKey));
            }

            if (string.IsNullOrEmpty(awsAccessKey))
            {
                throw new ArgumentException(nameof(awsAccessKey));
            }

            if (regionEndpoint is null)
            {
                throw new ArgumentNullException(nameof(regionEndpoint));
            }

            var basicAuth = new Amazon.Runtime.BasicAWSCredentials(awsAccessKey, awsSecretKey);
            notificationService = new AmazonSimpleNotificationServiceClient(basicAuth, regionEndpoint);
        }

        private async Task<SendEventResponse> SendEventInternalAsync(SendEventRequest request)
        {
            var returnObject = new SendEventResponse();
            try
            {
                await notificationService.PublishAsync(request.Destination, request.Body.ToJson());

                returnObject.Success = true;
                return returnObject;
            }
            catch (Exception ex)
            {
                returnObject.Success = false;
                returnObject.Exception = ex;
                return returnObject;
            }
        }

        public Task<SendEventResponse> SendEventAsync(SendEventRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            return SendEventInternalAsync(request);
        }
        public Task<SendEventResponse> SendEventAsync(string destinationTopicArn, EventBody eventBody)
        {
            if (destinationTopicArn is null)
            {
                throw new ArgumentNullException(nameof(destinationTopicArn));
            }

            if (eventBody is null)
            {
                throw new ArgumentNullException(nameof(eventBody));
            }

            return SendEventInternalAsync(new SendEventRequest() { Body = eventBody, Destination = destinationTopicArn });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue && disposing)
            {
                if (!externalClient && notificationService != null)
                {
                    notificationService.Dispose();
                }

                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
