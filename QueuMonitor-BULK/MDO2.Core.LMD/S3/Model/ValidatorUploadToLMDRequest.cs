using FluentValidation;

namespace MDO2.Core.LMD.S3.Model
{
    public class ValidatorUploadToLmdRequest : AbstractValidator<LmdFileUploadRequest>
    {
        public ValidatorUploadToLmdRequest()
        {
            RuleFor(x => x.BucketName).NotEmpty().MinimumLength(3);
            RuleFor(x => x.FilePath).NotEmpty();
            RuleFor(x => x.MetadataExtention).NotEmpty();
            RuleFor(x => x.S3KeyForFile).NotEmpty();
            RuleFor(x => x.MetadataInfo).NotNull();
        }
    }
}
