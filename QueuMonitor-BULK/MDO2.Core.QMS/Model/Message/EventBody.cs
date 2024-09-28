using MDO2.Core.QMS.Model.Message.EventData;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace MDO2.Core.QMS.Model.Message
{
    public class EventBody
    {
        [JsonProperty("eventSource")]
        public string EventSource { get; set; }

        [JsonProperty("eventLevel")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EventLevel EventLevel { get; set; }

        [JsonProperty("eventTime")]
        public DateTime EventTime { get; set; }

        [JsonProperty("eventType")]
        public string EventType { get; set; }

        [JsonProperty("eventData")]
        public IEventData Data { get; set; }

        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
