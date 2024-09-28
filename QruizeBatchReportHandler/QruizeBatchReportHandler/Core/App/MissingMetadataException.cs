using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QruizeBatchReportHandler.Core.App
{

	[Serializable]
	public class MissingMetadataException : Exception
	{
		public MissingMetadataException() { }
		public MissingMetadataException(string message) : base(message) { }
		public MissingMetadataException(string message, Exception inner) : base(message, inner) { }
		protected MissingMetadataException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
