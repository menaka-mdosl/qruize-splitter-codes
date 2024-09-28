using System;

namespace MDO2.Core.Util
{
    public static class DateTimeExtensions
    {
        public static DateTimeOffset? FromUnixTimestampInMilliseconds(this long? timestamp)
        {
            if (timestamp.HasValue)
            {
                try
                {
                    return DateTimeOffset.FromUnixTimeMilliseconds(timestamp.Value);
                }
                catch (Exception)
                {
                    //return null if not successful
                }
            }

            return null;
        }
        public static DateTimeOffset? FromUnixTimestampInMilliseconds(this long timestamp)
        {
            if (timestamp > 0)
            {
                try
                {
                    return DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
                }
                catch (Exception)
                {
                    //return null if not successful
                }
            }

            return null;
        }
        public static DateTimeOffset? FromUnixTimestampInSeconds(this long? timestamp)
        {
            if (timestamp.HasValue)
            {
                try
                {
                    return DateTimeOffset.FromUnixTimeSeconds(timestamp.Value);
                }
                catch (Exception)
                {
                    //return null if not successful
                }
            }

            return null;
        }
        public static DateTimeOffset? FromUnixTimestampInSeconds(this long timestamp)
        {
            if (timestamp > 0)
            {
                try
                {
                    return DateTimeOffset.FromUnixTimeSeconds(timestamp);
                }
                catch (Exception)
                {
                    //return null if not successful
                }
            }

            return null;
        }

        public static string FormatTo(this DateTime? dateTime, string format, DateTime defaultValue)
        {
            return dateTime.HasValue ? dateTime.Value.ToString(format) : defaultValue.ToString(format);
        }
    }
}
