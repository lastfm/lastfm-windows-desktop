using System;

namespace LastFMApiClient.Helpers
{
    // Helper function to convert unix timestamps to C# DateTime, and vice versa
    public static class UnixTimeStampHelper
    {
        // Convert from a unix timestamp to a C# DateTime
        public static DateTime GetDateTimeFromUnixTimestamp(string unixTimeStamp)
        {
            // Create a new date instance from the 1st January 1970
            var convertedDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            double timeStampSeconds = 0;

            if (double.TryParse(unixTimeStamp, out timeStampSeconds))
            {
                // A unix timestamp is the number of seconds that has elapsed from 1st January 1970 until now
                convertedDate = convertedDate.AddSeconds(timeStampSeconds);
            }

            return convertedDate;
        }

        // Convert from a c# DateTime to a unix timestamp
        public static long GetUnixTimeStampFromDateTime(DateTime dateTime)
        {
            // Get a date instance based on the 1st January 1970
            DateTime epochDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            // Establish how long has elapsed between now, and the 1970 date
            TimeSpan elapsedTime = dateTime.ToUniversalTime() - epochDate;

            // The unixtimestamp date is the number of seconds that has elapsed since 1st January 1970
            return (long)elapsedTime.TotalSeconds;
        }
    }
}
