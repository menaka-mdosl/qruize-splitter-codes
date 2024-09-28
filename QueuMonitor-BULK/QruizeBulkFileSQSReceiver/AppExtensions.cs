using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QruizeBulkFileSQSReceiver
{
    public static class AppExtensions
    {
        public static string GetFileformats(this IConfiguration configuration)
        {
            var fileFormats = configuration.GetValue<string>(AppConstants.CFG_KEY_FILE_FORMATS);
            if (string.IsNullOrWhiteSpace(fileFormats))
                return "";
            else
            {
                return fileFormats;
            }
        }

        public static string GetStepFunctionARN(this IConfiguration configuration)
        {
            var arn = configuration.GetValue<string>(AppConstants.CFG_KEY_ARN_STEP_FUNCTION);
            if (string.IsNullOrWhiteSpace(arn))
                return "";
            else
            {
                return arn;
            }
        }
        public static string GetQMSARN(this IConfiguration configuration)
        {
            var arn = configuration.GetValue<string>(AppConstants.CFG_KEY_QMS_EVENT_SOURCE_ARN);
            if (string.IsNullOrWhiteSpace(arn))
                return "";
            else
            {
                return arn;
            }
        }

        public static string GetQMSEventSource(this IConfiguration configuration)
        {
            var entSource = configuration.GetValue<string>(AppConstants.CFG_KEY_QMS_EVENT_SOURCE_NAME);
            if (string.IsNullOrWhiteSpace(entSource))
                return "";
            else
            {
                return entSource;
            }
        }
    }
}
