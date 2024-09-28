using System.Net;

namespace MDO2.Core.LMD.S3.Model
{
    public class LmdFileUploadResponse : IS3UploadResponse
    {
        public string BinaryFileKey { get; set; }
        public HttpStatusCode ResponseCode { get; set; }
    }
}
