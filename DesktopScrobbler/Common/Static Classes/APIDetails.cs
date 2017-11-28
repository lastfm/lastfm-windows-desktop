
namespace LastFM.Common.Static_Classes
{
    // A static place for putting the Last.fm related credentials and URL endpoints.
    // You MUST modify these based on your own application's credentials
    public static class APIDetails
    {
        // The Url endpoint for the scrobbling API
        public const string EndPointUrl = "https://ws.audioscrobbler.com";

        // The Url endpoint for the authentication / authorization API
        public const string UserAuthorizationEndPointUrl = "https://www.last.fm/api/auth/?api_key={0}&token={1}";

        // The name of your application (as defined when created on the Last.fm website)
        public const string ApplicationName = "Last.fm Desktop Scrobbler for Windows";

        // The key given to you by the Last.fm website
        public const string Key = @"";

        // The shared secret given to you by the Last.fm website
        public const string SharedSecret = @"";
    }
}
