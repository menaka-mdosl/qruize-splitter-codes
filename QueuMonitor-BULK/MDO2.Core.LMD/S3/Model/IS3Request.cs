namespace MDO2.Core.LMD.S3.Model
{
    public interface IS3Request
    {
        string BucketName { get; set; }
        string S3KeyForFile { get; set; }
        string S3Path { get; set; }
    }
}
