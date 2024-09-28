using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using MDO2.Core.LMD.S3.Model;
using MDO2.Core.Util;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDO2.Core.LMD.S3
{
    public class S3Client : IDisposable
    {
        #region static functions
        public static string CreateObjectUrl(RegionEndpoint regionEndpoint,
                                     string bucketName,
                                     string fileKey,
                                     string amazonHost = "amazonaws.com")
        {
            if (regionEndpoint is null)
            {
                throw new ArgumentNullException(nameof(regionEndpoint));
            }
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentException(nameof(bucketName));
            }
            if (string.IsNullOrEmpty(fileKey))
            {
                throw new ArgumentException(nameof(fileKey));
            }
            if (string.IsNullOrEmpty(amazonHost))
            {
                throw new ArgumentException(nameof(amazonHost));
            }


            return $"https://{bucketName}.s3-{regionEndpoint.SystemName}.{amazonHost}/{fileKey}";
        }
        #endregion

        private readonly IAmazonS3 s3Client;
        private readonly bool externalClient = false;
        private bool disposedValue;

        public S3Client()
        {
            s3Client = new AmazonS3Client();
        }
        public S3Client(IAmazonS3 s3Client)
        {
            this.s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
            externalClient = true;
        }
        public S3Client(string accessKey, string secretKey)
        {
            if (string.IsNullOrWhiteSpace(accessKey))
            {
                throw new ArgumentException($"'{nameof(accessKey)}' cannot be null or whitespace", nameof(accessKey));

            }
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new ArgumentException($"'{nameof(secretKey)}' cannot be null or whitespace", nameof(secretKey));
            }

            var aWSCredentials = new BasicAWSCredentials(accessKey, secretKey);
            s3Client = new AmazonS3Client(aWSCredentials);
        }
        public S3Client(string accessKey, string secretKey, RegionEndpoint regionEndpoint)
        {
            if (string.IsNullOrWhiteSpace(accessKey))
            {
                throw new ArgumentException($"'{nameof(accessKey)}' cannot be null or whitespace", nameof(accessKey));

            }
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new ArgumentException($"'{nameof(secretKey)}' cannot be null or whitespace", nameof(secretKey));
            }

            if (regionEndpoint is null)
            {
                throw new ArgumentNullException(nameof(regionEndpoint));
            }

            var aWSCredentials = new BasicAWSCredentials(accessKey, secretKey);
            s3Client = new AmazonS3Client(aWSCredentials, regionEndpoint);
        }
        public S3Client(BasicAWSCredentials aWSCredentials)
        {
            var creds = aWSCredentials ?? throw new ArgumentNullException(nameof(aWSCredentials));
            s3Client = new AmazonS3Client(creds);
        }
        public S3Client(BasicAWSCredentials aWSCredentials, RegionEndpoint regionEndpoint)
        {
            var creds = aWSCredentials ?? throw new ArgumentNullException(nameof(aWSCredentials));
            var region = regionEndpoint ?? throw new ArgumentNullException(nameof(regionEndpoint));
            s3Client = new AmazonS3Client(creds, region);
        }

        private string CreateS3Key(string path, string key)
        {
            if (path.IsNullOrWhiteSpace())
                return key;
            else
                return $"{path.Trim('/', '\\')}/{key}";
        }
        private async Task<LmdFileUploadResponse> UploadLmdFilePrivateAsync(LmdFileUploadRequest request)
        {
            try
            {
                //full s3 key with logical path
                var s3FullKey = CreateS3Key(request.S3Path, request.S3KeyForFile);

                var bfPutRequest = new PutObjectRequest()
                {
                    BucketName = request.BucketName,
                    FilePath = request.FilePath,
                    Key = s3FullKey
                };

                //upload binary file
                var bfPutResponse = await s3Client.PutObjectAsync(bfPutRequest);
                if (bfPutResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new BinaryFileUploadException($"Failed to upload binary file, " +
                        $"S3 returned {bfPutResponse.HttpStatusCode}");
                }

                var objectUrl = CreateObjectUrl(s3Client.Config.RegionEndpoint, request.BucketName, bfPutRequest.Key);

                //convert metadata to in-memory stream
                //var metaJson = Newtonsoft.Json.JsonConvert.SerializeObject(request.MetadataInfo);
                var metaStream = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(request.MetadataInfo));
                var metaKey = $"{bfPutRequest.Key}{request.MetadataExtention}";

                //delete existing metadata file if enabled
                if (request.DeleteMetadataFileIfExits)
                {
                    try
                    {
                        var exitsMeta = await Exists(s3Client, request.BucketName, metaKey);
                        if (exitsMeta)
                        {
                            await s3Client.DeleteObjectAsync(request.BucketName, metaKey);
                        }
                    }
                    catch (AmazonS3Exception ex)
                    {
                        throw new MetadataFileUploadException($"Failed delete existing metadata file with key {metaKey}, " +
                           $"S3 returned {ex.StatusCode}");
                    }
                    catch (Exception ex)
                    {
                        throw new MetadataFileUploadException($"Failed delete existing metadata file with key {metaKey}, " +
                            $"Exception={ex}");
                    }
                }

                var metPutRequest = new PutObjectRequest()
                {
                    BucketName = request.BucketName,
                    InputStream = metaStream,
                    AutoCloseStream = true,
                    AutoResetStreamPosition = true,
                    Key = metaKey
                };
                await s3Client.PutObjectAsync(metPutRequest);

                return new LmdFileUploadResponse()
                {
                    ResponseCode = bfPutResponse.HttpStatusCode,
                    BinaryFileKey = bfPutRequest.Key,
                    MetadataFileKey = metaKey,
                };

            }
            catch (BinaryFileUploadException) { throw; }
            catch (MetadataFileUploadException) { throw; }
            catch (Exception ex)
            {
                throw new S3ClientException($"Error occurred while trying to upload file {request.FilePath}", ex);
            }
        }

        public async Task<bool> Exists(IAmazonS3 _s3Client, string bucketName, string fileKey)
        {
            try
            {
                await _s3Client.GetObjectMetadataAsync(bucketName, fileKey);
                return true;
            }
            catch (Amazon.S3.AmazonS3Exception)
            {
                return false;
            }
        }
        public Task<LmdFileUploadResponse> UploadLMDFileAsync(LmdFileUploadRequest request)
        {
            //validate request data
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var validationResult = new ValidatorUploadToLmdRequest().Validate(request);
            if (!validationResult.IsValid)
            {
                throw new S3ClientException($"Validation failed for upload request, " +
                    $"{validationResult.Errors.First().ErrorMessage}");
            }

            return UploadLmdFilePrivateAsync(request);
        }
        #region IDisposable implementation 
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue && disposing)
            {
                if (s3Client != null && !externalClient)
                {
                    try
                    {
                        s3Client.Dispose();
                    }
                    catch
                    {
                        //object being disposed
                    }
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
