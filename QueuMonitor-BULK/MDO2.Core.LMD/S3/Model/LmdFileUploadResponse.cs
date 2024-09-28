using System.Net;

namespace MDO2.Core.LMD.S3.Model
{
    public class LmdFileUploadResponse : IS3UploadResponse
    {
        public string BinaryFileKey { get; set; }
        public string MetadataFileKey { get; set; }
        public HttpStatusCode ResponseCode { get; set; }
    }
}
