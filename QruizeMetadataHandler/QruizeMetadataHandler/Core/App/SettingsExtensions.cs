using Microsoft.Extensions.Configuration;

namespace QruizeMetadataHandler.Core.App
{
    internal static class SettingsExtensions
    {

        internal static string GetSmApiUrl(this IConfigurationRoot configuration)
        {
            var smBaseUrl = configuration.GetValue<string>("SM_API_BASE_URL");
            if (string.IsNullOrWhiteSpace(smBaseUrl))
                return "";
            else
            {
                return smBaseUrl;
            }
        }
        internal static string GetSmApiToken(this IConfigurationRoot configuration)
        {
            var smToken = configuration.GetValue<string>("SM_API_TOKEN");
            if (string.IsNullOrWhiteSpace(smToken))
                return "";
            else
            {
                return smToken;
            }
        }
        internal static string GetMetaGeneratorApiUrl(this IConfigurationRoot configuration)
        {
            var smBaseUrl = configuration.GetValue<string>(AppConstants.META_GENERATOR_API_BASE_URL);
            if (string.IsNullOrWhiteSpace(smBaseUrl))
                return "";
            else
            {
                return smBaseUrl;
            }
        }
        internal static string GetMetaGeneratorApiHeader(this IConfigurationRoot configuration)
        {
            var smToken = configuration.GetValue<string>(AppConstants.META_GENERATOR_API_HEADER);
            if (string.IsNullOrWhiteSpace(smToken))
                return "";
            else
            {
                return smToken;
            }
        }
        internal static string GetS3Region(this IConfigurationRoot configuration)
        {
            var smToken = configuration.GetValue<string>(AppConstants.S3_REGION);
            if (string.IsNullOrWhiteSpace(smToken))
                return "";
            else
            {
                return smToken;
            }
        }
        public static string GetUploadS3MetadataExtension(this IConfigurationRoot configuration)
        {
            var configValue = configuration.GetValue<string>(AppConstants.CONVERTED_METADATA_EXTENSION);
            if (string.IsNullOrWhiteSpace(configValue))
                return "";
            else
            {
                return configValue;
            }
        }
        public static string GetTempPath(this IConfigurationRoot configuration)
        {
            var tempPath = configuration.GetValue<string>(AppConstants.CFG_KEY_FILE_TEMP_PATH);
            if (string.IsNullOrWhiteSpace(tempPath))
            {
                return Path.GetTempPath();
            }
            else
            {
                return tempPath;
            }
        }

        public static string GetRawS3Bucket(this IConfigurationRoot configuration)
        {
            var configValue = configuration.GetValue<string>(AppConstants.RAW_S3_BUCKET);
            if (string.IsNullOrWhiteSpace(configValue))
                return "";
            else
            {
                return configValue;
            }
        }
        public static string GetRawS3Path(this IConfigurationRoot configuration)
        {
            var configValue = configuration.GetValue<string>(AppConstants.RAW_S3_PATH);
            if (string.IsNullOrWhiteSpace(configValue))
                return "";
            else
            {
                return configValue;
            }
        }

        public static string GetConvertedS3BucketXPS(this IConfigurationRoot configuration)
        {
            var configValue = configuration.GetValue<string>(AppConstants.CONVERTED_S3_BUCKET_XPS);
            if (string.IsNullOrWhiteSpace(configValue))
                return "";
            else
            {
                return configValue;
            }
        }
        public static string GetConvertedS3PathXPS(this IConfigurationRoot configuration)
        {
            var configValue = configuration.GetValue<string>(AppConstants.CONVERTED_S3_PATH_XPS);
            if (string.IsNullOrWhiteSpace(configValue))
                return "";
            else
            {
                return configValue;
            }
        }
        public static string GetConvertedS3Bucket(this IConfigurationRoot configuration)
        {
            var configValue = configuration.GetValue<string>(AppConstants.CONVERTED_S3_BUCKET);
            if (string.IsNullOrWhiteSpace(configValue))
                return "";
            else
            {
                return configValue;
            }
        }
        public static string GetConvertedS3Path(this IConfigurationRoot configuration)
        {
            var configValue = configuration.GetValue<string>(AppConstants.CONVERTED_S3_PATH);
            if (string.IsNullOrWhiteSpace(configValue))
                return "";
            else
            {
                return configValue;
            }
        }
    
        public static string GetQmsArn(this IConfigurationRoot configuration)
        {
            var configValue = configuration.GetValue<string>(AppConstants.CFG_KEY_QMS_SNS_ARN);
            if (string.IsNullOrWhiteSpace(configValue))
                return "";
            else
            {
                return configValue;
            }
        }
        public static string GetQmsEventSourceName(this IConfigurationRoot configuration)
        {
            var configValue = configuration.GetValue<string>(AppConstants.CFG_KEY_QMS_EVENT_SOURCE_NAME);
            if (string.IsNullOrWhiteSpace(configValue))
                return "";
            else
            {
                return configValue;
            }
        }
    }
}
