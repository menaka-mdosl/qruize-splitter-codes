using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QruizeBatchReportHandler.Core.App
{
   [Serializable]
    public class NoEntryForIndexingException : Exception
    {
        public NoEntryForIndexingException() { }
        public NoEntryForIndexingException(string message) : base(message) { }
        public NoEntryForIndexingException(string message, Exception inner) : base(message, inner) { }
        protected NoEntryForIndexingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}

