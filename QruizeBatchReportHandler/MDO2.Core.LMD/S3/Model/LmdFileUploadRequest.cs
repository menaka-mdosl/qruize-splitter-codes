﻿using MDO2.Core.Model.Metadata;

namespace MDO2.Core.LMD.S3.Model
{
    public class LmdFileUploadRequest : IS3UploadRequest
    {
        public LmdFileUploadRequest()
        {
            CheckKeyExits = false;
        }

        public string BucketName { get; set; }
        public string FilePath { get; set; }
        public string S3KeyForFile { get; set; }
        public string S3Path { get; set; }
        public bool CheckKeyExits { get; set; }
        public bool DeleteMetadataFileIfExits { get; set; }
    }
}
