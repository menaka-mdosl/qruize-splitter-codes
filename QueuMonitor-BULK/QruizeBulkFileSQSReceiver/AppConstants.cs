using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QruizeBulkFileSQSReceiver
{
    public static class AppConstants
    {
        //--- config keys ---
        public const string CFG_KEY_FILE_FORMATS = "FILE_FORMATS";
        public const string CFG_KEY_ARN_STEP_FUNCTION = "ARN_STEP_FUNCTION";
        public const string CFG_KEY_QMS_EVENT_SOURCE_NAME = "QMS_EVENT_SOURCE_NAME";
        public const string CFG_KEY_QMS_EVENT_SOURCE_ARN = "QMS_EVENT_SOURCE_ARN";

    }
}
