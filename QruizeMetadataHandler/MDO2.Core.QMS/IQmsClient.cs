using MDO2.Core.QMS.Model;
using System.Threading.Tasks;

namespace MDO2.Core.QMS
{
    public interface IQmsClient
    {
        Task<SendEventResponse> SendEventAsync(SendEventRequest request);
    }
}