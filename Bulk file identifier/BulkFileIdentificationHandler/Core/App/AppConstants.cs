namespace BulkFileIdentificationHandler.Core.App
{
    public static class AppConstants
    {
        //--- config keys ---
        //temporary path to do the file operations
        public const string CFG_KEY_FILE_TEMP_PATH = "FILE_TEMP_PATH";
        //metadata file extension default is .metadata
        public const string CFG_KEY_S3_METADATA_EXT = "S3_METADATA_EXT";
        //sub directory path where files need to be uploaded in S3 bucket, default is empty string
        public const string CFG_KEY_S3_KEY_SUB_PATH = "S3_KEY_SUB_PATH";
        //qms sns topic arn
        public const string CFG_KEY_QMS_SNS_ARN = "QMS_SNS_ARN";
        //qms event source name for this app
        public const string CFG_KEY_QMS_EVENT_SOURCE_NAME = "QMS_EVENT_SOURCE_NAME";
        //database connection string
        public const string CFG_KEY_DB_CONNECTION_STRING = "DB_CONNECTION_STRING";
        //database name
        public const string CFG_KEY_DB_DATABASE_NAME = "DB_DATABASE_NAME";
        //database collection name for indexing
        public const string CFG_KEY_DB_COLLECTION_NAME_INDEXING = "DB_COLLECTION_NAME_INDEXING";
        //database collection name for splitting
        public const string CFG_KEY_DB_COLLECTION_NAME_SPLITTING = "DB_COLLECTION_NAME_SPLITTING";
        //bucket name of raw files
        public const string CFG_KEY_RAW_S3_BUCKET = "RAW_S3_BUCKET";
        //path to raw files
        public const string CFG_KEY_RAW_S3_PATH = "RAW_S3_PATH";
        //bucket name to converted files
        public const string CFG_KEY_CONVERTED_S3_BUCKET = "CONVERTED_S3_BUCKET";
        //path to converted files
        public const string CFG_KEY_CONVERTED_S3_PATH = "CONVERTED_S3_PATH";
        //metadata extention
        public const string CFG_KEY_CONVERTED_METADATA_EXTENSION = "CONVERTED_METADATA_EXTENSION";


    }
}
