using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QruizeMetadataHandler.Core.App
{

	[Serializable]
	public class FileUploadException : Exception
	{
		public FileUploadException() { }
		public FileUploadException(string message) : base(message) { }
		public FileUploadException(string message, Exception inner) : base(message, inner) { }
		protected FileUploadException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
