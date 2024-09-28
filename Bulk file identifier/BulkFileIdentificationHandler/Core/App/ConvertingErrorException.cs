using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkFileIdentificationHandler.Core.App
{

	[Serializable]
	public class ConvertingErrorException : Exception
	{
		public ConvertingErrorException() { }
		public ConvertingErrorException(string message) : base(message) { }
		public ConvertingErrorException(string message, Exception inner) : base(message, inner) { }
		protected ConvertingErrorException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
