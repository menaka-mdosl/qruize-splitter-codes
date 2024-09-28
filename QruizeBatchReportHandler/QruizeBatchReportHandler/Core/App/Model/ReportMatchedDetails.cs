using Aspose.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QruizeBatchReportHandler.Core.App.Model
{
    public class ReportMatchedDetails
    {
        public int page { get; set; }
        public string ReportName { get; set; }
        public Page PageData { get; set; }
        public string NewDocId { get; set; }
        public string PageExreactedData { get; set;}
    }
}
