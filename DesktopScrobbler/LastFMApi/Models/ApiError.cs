using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LastFM.ApiClient.Models
{
    public class ApiError
    {
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
            RateLimitExceeded=29
        }
    }
}
