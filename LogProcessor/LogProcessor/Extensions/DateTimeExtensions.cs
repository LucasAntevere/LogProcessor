using System;
using System.Globalization;

namespace LogProcessor.Extensions
{
    public static class DateTimeExtensions
    {
        private const int YEAR_UNIX_BASE = 1970;
        private const int MONTH_UNIX_BASE = 1;
        private const int DAY_UNIX_BASE = 1;
        private const int MINUTE_UNIX_BASE = 0;
        private const int SECOND_UNIX_BASE = 0;
        private const int MILLISECOND_UNIX_BASE = 0;

        public static DateTime ToDateFromUnixTimestamp(this string unixTimestamp)
        {
            return new DateTime(YEAR_UNIX_BASE, MONTH_UNIX_BASE, DAY_UNIX_BASE, MINUTE_UNIX_BASE, 
                                SECOND_UNIX_BASE, MILLISECOND_UNIX_BASE, DateTimeKind.Utc).AddSeconds(Double.Parse(unixTimestamp, new CultureInfo("en-US")));
        }
    }
}
