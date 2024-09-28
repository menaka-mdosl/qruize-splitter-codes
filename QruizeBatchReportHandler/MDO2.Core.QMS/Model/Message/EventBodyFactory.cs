using MDO2.Core.QMS.Model.Message.EventData;
using System;

namespace MDO2.Core.QMS.Model.Message
{
    public static class EventBodyFactory
    {
        public static EventBody Create<T>(EventLevel eventLevel, string eventSource, string eventType = null) where T : IEventData
        {
            if (typeof(T) == typeof(EventDataFileAcceptance))
            {
                var evb = new EventBody()
                {
                    EventLevel = eventLevel,
                    EventSource = eventSource,
                    EventTime = DateTime.UtcNow,
                    EventType = eventType ?? "FILE_ACCEPTANCE",
                    Data = (EventDataFileAcceptance)Activator.CreateInstance(typeof(EventDataFileAcceptance))
                };
                return evb;
            }
            else if (typeof(T) == typeof(EventDataSystemError))
            {
                var evb = new EventBody()
                {
                    EventLevel = eventLevel,
                    EventSource = eventSource,
                    EventTime = DateTime.UtcNow,
                    EventType = eventType ?? "SYSTEM_ERROR",
                    Data = (EventDataSystemError)Activator.CreateInstance(typeof(EventDataSystemError))
                };
                return evb;
            }
            else if (typeof(T) == typeof(ConverterFileInEventData))
            {
                var evb = new EventBody()
                {
                    EventLevel = eventLevel,
                    EventSource = eventSource,
                    EventTime = DateTime.UtcNow,
                    EventType = eventType ?? "CONV_FILE_IN",
                    Data = (ConverterFileInEventData)Activator.CreateInstance(typeof(ConverterFileInEventData))
                };
                return evb;
            }
            else if (typeof(T) == typeof(ConverterFileProcessedEventData))
            {
                var evb = new EventBody()
                {
                    EventLevel = eventLevel,
                    EventSource = eventSource,
                    EventTime = DateTime.UtcNow,
                    EventType = eventType ?? "BULK_FILE_PROCESSED",
                    Data = (ConverterFileProcessedEventData)Activator.CreateInstance(typeof(ConverterFileProcessedEventData))
                };
                return evb;
            }
            else if (typeof(T) == typeof(ConverterFileProcessingErrorEventData))
            {
                var evb = new EventBody()
                {
                    EventLevel = eventLevel,
                    EventSource = eventSource,
                    EventTime = DateTime.UtcNow,
                    EventType = eventType ?? "BULK_FILE_PROCESSING_ERROR",
                    Data = (ConverterFileProcessingErrorEventData)Activator.CreateInstance(typeof(ConverterFileProcessingErrorEventData))
                };
                return evb;
            }
            else if(typeof(T) == typeof(ConverterFileUploadEventData))
            {
                var evb = new EventBody()
                {
                    EventLevel = eventLevel,
                    EventSource = eventSource,
                    EventTime = DateTime.UtcNow,
                    EventType = eventType ?? "CONV_FILE_PROCESSED_UPLOAD",
                    Data = (ConverterFileUploadEventData)Activator.CreateInstance(typeof(ConverterFileUploadEventData))
                };
                return evb;
            }
            else if (typeof(T) == typeof(ConverterQcEventData))
            {
                var evb = new EventBody()
                {
                    EventLevel = eventLevel,
                    EventSource = eventSource,
                    EventTime = DateTime.UtcNow,
                    EventType = eventType ?? "QC_EVENT",
                    Data = (ConverterQcEventData)Activator.CreateInstance(typeof(ConverterQcEventData))
                };
                return evb;
            }
            else
            {
                var evb = new EventBody()
                {
                    EventLevel = eventLevel,
                    EventSource = eventSource,
                    EventTime = DateTime.UtcNow,
                    EventType = eventType ?? "UNKNOWN_EVENT_TYPE",
                    Data = (T)Activator.CreateInstance(typeof(T))
                };
                return evb;
            }
        }
    }
}
