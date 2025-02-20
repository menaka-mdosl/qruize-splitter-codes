using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDO2.Core.QMS.Model.Message.EventData
{
    public class FileidentifiedEventData : ConverterEventDataBase
    {

        public FileidentifiedEventData()
        {
            Hotel = "";
            MgmtGroup = "";
            BusinessDate = "";
        }

        [JsonProperty("hotel")]
        public string Hotel { get; set; }
        [JsonProperty("mgmtGroup")]
        public string MgmtGroup { get; set; }
        [JsonProperty("businessDate")]
        public string BusinessDate { get; set; }


        [JsonProperty("fileType")]
        public string FileType { get; set; }

        [JsonProperty("bulkType")]
        public string BulkType { get; set; }




    }
}
