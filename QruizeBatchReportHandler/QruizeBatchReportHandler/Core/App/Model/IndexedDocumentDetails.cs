using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QruizeBatchReportHandler.Core.App.Model
{
    public class IndexedDocumentDetails
    {
        public string ReportName { get; set; }
        public string S3ObjectName { get; set; }
        public string NewDocId { get; set; }
        public string TempFileLocation { get; set; }
        public string BusinessDate { get; set; }


    }
}
