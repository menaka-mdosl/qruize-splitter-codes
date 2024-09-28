using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QruizeBulkFileSQSReceiver
{
    public sealed class QmsErrorEventData
    {
        public string? ErrorMessage { get; set; }
        public string? FailedAt { get; set; }
        public Exception? Exception { get; set; }
        public string? ErrorCode { get; set; }
    }
}
