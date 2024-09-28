namespace QruizeMetadataHandler.Core.App
{
    public static class AppConstants
    {
        //--- config keys ---
        //temporary path to do the file operations
        public const string CFG_KEY_FILE_TEMP_PATH = "CFG_KEY_FILE_TEMP_PATH";
        //metadata file extension default is .metadata
        public const string CFG_KEY_S3_METADATA_EXT = "S3_METADATA_EXT";
        //sub directory path where files need to be uploaded in S3 bucket, default is empty string
        public const string CFG_KEY_S3_KEY_SUB_PATH = "S3_KEY_SUB_PATH";
        //qms sns topic arn
        public const string CFG_KEY_QMS_SNS_ARN = "QMS_SNS_ARN";
        //qms event source name for this app
        public const string CFG_KEY_QMS_EVENT_SOURCE_NAME = "QMS_EVENT_SOURCE_NAME";
        //bucket name of raw files
        public const string RAW_S3_BUCKET = "RAW_S3_BUCKET";
        //path to raw files
        public const string RAW_S3_PATH = "RAW_S3_PATH";
        //bucket name to converted files
        public const string CONVERTED_S3_BUCKET_XPS = "CONVERTED_S3_BUCKET_XPS";
        //path to converted files
        public const string CONVERTED_S3_PATH = "CONVERTED_S3_PATH";

        public const string CONVERTED_S3_BUCKET = "CONVERTED_S3_BUCKET";
        //path to converted files
        public const string CONVERTED_S3_PATH_XPS = "CONVERTED_S3_PATH_XPS";
        //metadata extention
        public const string CONVERTED_S3_BUCKET_RTF = "CONVERTED_S3_BUCKET_RTF";

        public const string CONVERTED_S3_PATH_RTF = "CONVERTED_S3_PATH_RTF";
        //metadata extention
        public const string CONVERTED_S3_BUCKET_OXPS = "CONVERTED_S3_BUCKET_OXPS";

        public const string CONVERTED_S3_PATH_OXPS = "CONVERTED_S3_PATH_OXPS";
        //metadata extention
        public const string CONVERTED_S3_BUCKET_DOC = "CONVERTED_S3_BUCKET_DOC";

        public const string CONVERTED_S3_PATH_DOC = "CONVERTED_S3_PATH_DOC";
        //metadata extention
        public const string CONVERTED_S3_BUCKET_DOCX = "CONVERTED_S3_BUCKET_DOCX";

        public const string CONVERTED_S3_PATH_DOCX = "CONVERTED_S3_PATH_DOCX";

        public const string CONVERTED_S3_BUCKET_TXT = "CONVERTED_S3_BUCKET_TXT";

        public const string CONVERTED_S3_PATH_TXT = "CONVERTED_S3_PATH_TXT";
        //metadata extention
        public const string CONVERTED_METADATA_EXTENSION = "CONVERTED_METADATA_EXTENSION";

        public const string META_GENERATOR_API_BASE_URL = "META_GENERATOR_API_BASE_URL";

        public const string META_GENERATOR_API_HEADER = "META_GENERATOR_API_HEADER";

        public const string S3_REGION = "S3_REGION";
    }
}
