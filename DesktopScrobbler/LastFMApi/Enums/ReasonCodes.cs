namespace LastFM.ApiClient.Enums
{
    // Last.fm code to represent various status codes globally used by the Last.fm API
    public static class ReasonCodes
    {
        // The reason (or not) a media item was ignored whilst being scrobbled
        public enum IgnoredReason
        {
            AllOk = 0,
            ArtistIgnored = 1,
            TrackIgnored = 2,
            TimestampTooOld = 3,
            TimestampTooNew = 4,
            ScrobbleLimitExceeded = 5
        }

        // The reason for an API call failing
        public enum ErrorCode
        {
            ErrorUnknown = 0,
            InvalidService = 2,
            InvalidMethod = 3,
            AuthenticationFailed = 4,
            InvalidFormat = 5,
            InvalidParameters = 6,
            InvalidResourceSpecified = 7,
            OperationFailed = 8,
            InvalidSessionKey = 9,
            InvalidAPIKey = 10,
            ServiceOffline = 11,
            InvalidMethodSignature = 13,
            TemporaryError = 16,
            SuspendedAPI = 26,
            RateLimitExceeded = 29
        }
    }

}
