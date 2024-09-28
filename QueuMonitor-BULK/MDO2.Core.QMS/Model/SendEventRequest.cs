using MDO2.Core.QMS.Model.Message;

namespace MDO2.Core.QMS.Model
{
    public class SendEventRequest
    {
        public string Destination { get; set; }
        public EventBody Body { get; set; }
    }
}
