using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LastFM.Common.Helpers
{
    public static class UnixTimeStampHelper
    {
        public static DateTime GetDateTimeFromUnixTimestamp(string unixTimeStamp)
        {
            var convertedDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);

            double timeStampSeconds = 0;

            if (double.TryParse(unixTimeStamp, out timeStampSeconds))
            {
                convertedDate = convertedDate.AddSeconds(timeStampSeconds);
            }

            return convertedDate;
        }

        public static double GetUnixTimeStampFromDateTime(DateTime timeToconvert)
        {
            double secondsSince1970 = timeToconvert.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
            return secondsSince1970;
        }
    }
}
