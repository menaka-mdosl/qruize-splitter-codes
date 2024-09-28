namespace BulkFileIdentificationHandler.Core.App
{
    public interface IAppEntryPoint
    {
        Task<object> Run(object input);
    }
}
