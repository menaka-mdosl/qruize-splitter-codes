using Aspose.Pdf;
using BulkFileIdentificationHandler.Core.App.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkFileIdentificationHandler.Core.App.Model
{
    internal class SplitterResult
    {
        public Document SplittedDocument { get; set; }
        public string DocId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string reportName { get; set; }
        public string S3Key { get; set; }
        public List<string> BarcodeData { get; set; }

        public SplitterResult()
        {
            SplittedDocument = new Document();
            BarcodeData = new List<string>();
        }
    }
}


