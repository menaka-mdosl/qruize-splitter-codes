using Microsoft.Extensions.Configuration;

namespace QruizeBatchReportHandler.Core.App
{
    internal static class SettingsExtensions
    {
        internal static string GetDbConnectionString(this IConfigurationRoot configurationRoot)
        {
            return configurationRoot.GetValue<string>(AppConstants.CFG_KEY_DB_CONNECTION_STRING);
        }
        internal static string GetDbDatabaseName(this IConfigurationRoot configurationRoot)
        {
            return configurationRoot.GetValue<string>(AppConstants.CFG_KEY_DB_DATABASE_NAME);
        }
        internal static string GetDbSplitDocumentsIndexingCollectionName(this IConfigurationRoot configurationRoot)
        {
            return configurationRoot.GetValue<string>(AppConstants.CFG_KEY_DB_COLLECTION_NAME_INDEXING);
        }
        internal static string GetDbSplitDocumentsSplittingCollectionName(this IConfigurationRoot configurationRoot)
        {
            return configurationRoot.GetValue<string>(AppConstants.CFG_KEY_DB_COLLECTION_NAME_SPLITTING);
        }
        internal static int GetIndexingNLineCount(this IConfigurationRoot configuration)
        {
            var nLinesCount = configuration.GetValue<string>("INDEXING_N_Lines_Count");
            if (int.TryParse(nLinesCount, out int nline))
            {
                return nline;
            }
            else
            {
                return 7;
            }
        }

        //internal static string GetSmApiUrl(this IConfigurationRoot configuration)
        //{
        //    var smBaseUrl = configuration.GetValue<string>(AppConstants.SM_API_BASE_URL);
        //    if (string.IsNullOrWhiteSpace(smBaseUrl))
        //        return "";
        //    else
        //    {
        //        return smBaseUrl;
        //    }
        //}
        //internal static string GetSmApiToken(this IConfigurationRoot configuration)
        //{
        //    var smToken = configuration.GetValue<string>("SM_API_TOKEN");
        //    if (string.IsNullOrWhiteSpace(smToken))
        //        return "";
        //    else
        //    {
        //        return smToken;
        //    }
        //}
        //public static string GetUploadS3MetadataExtension(this IConfigurationRoot configuration)
        //{
        //    var configValue = configuration.GetValue<string>("CONVERTED_METADATA_EXTENSION");
        //    if (string.IsNullOrWhiteSpace(configValue))
        //        return "";
        //    else
        //    {
        //        return configValue;
        //    }
        //}
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
        //public static string GetUploadS3Path(this IConfigurationRoot configuration)
        //{
        //    var configValue = configuration.GetValue<string>(AppConstants.UPLOAD_S3_PATH);
        //    if (string.IsNullOrWhiteSpace(configValue))
        //        return "";
        //    else
        //    {
        //        return configValue;
        //    }
        //}

        //public static string GetRawS3Bucket(this IConfigurationRoot configuration)
        //{
        //    var configValue = configuration.GetValue<string>("RAW_S3_BUCKET");
        //    if (string.IsNullOrWhiteSpace(configValue))
        //        return "";
        //    else
        //    {
        //        return configValue;
        //    }
        //}
        //public static string GetRawS3Path(this IConfigurationRoot configuration)
        //{
        //    var configValue = configuration.GetValue<string>("RAW_S3_PATH");
        //    if (string.IsNullOrWhiteSpace(configValue))
        //        return "";
        //    else
        //    {
        //        return configValue;
        //    }
        //}

        public static string GetConvertedS3Bucket(this IConfigurationRoot configuration)
        {
            var configValue = configuration.GetValue<string>(AppConstants.CFG_KEY_CONVERTED_S3_BUCKET);
            if (string.IsNullOrWhiteSpace(configValue))
                return "";
            else
            {
                return configValue;
            }
        }
        public static string GetConvertedS3Path(this IConfigurationRoot configuration)
        {
            var configValue = configuration.GetValue<string>(AppConstants.CFG_KEY_CONVERTED_S3_PATH);
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
