using System;

namespace MDO2.Core.Util
{
    public static class ExceptionExtensions
    {
        private static string ConcatMessage(Exception ex, string message, string seperator)
        {
            if (ex.InnerException == null)
                return message;
            else
            {
                if (message == "")
                    return ConcatMessage(ex.InnerException, $"{message}", seperator);
                else
                    return ConcatMessage(ex.InnerException, $"{seperator}{message}", seperator);
            }
        }

        public static string ConcatMessge(this Exception ex, string seperator = "-->")
        {
            return ConcatMessage(ex, string.Empty, seperator);
        }
    }
}
