using System;

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

        public static long GetUnixTimeStampFromDateTime(DateTime dateTime)
        {
            DateTime epochDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan elapsedTime = dateTime.ToUniversalTime() - epochDate;

            return (long)elapsedTime.TotalSeconds;
        }
    }
}
