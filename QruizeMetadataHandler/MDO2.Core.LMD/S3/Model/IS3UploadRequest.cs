namespace MDO2.Core.LMD.S3.Model
{
    public interface IS3UploadRequest : IS3Request
    {
        string FilePath { get; set; }
        bool CheckKeyExits { get; set; }
    }
}