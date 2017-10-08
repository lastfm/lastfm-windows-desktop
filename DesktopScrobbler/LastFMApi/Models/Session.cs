using Newtonsoft.Json;

namespace LastFM.ApiClient.Models
{
    // A Last.fm API model for an authenticated session
    public class Session
    {
        // The session token
        [JsonProperty("session")]
        public SessionToken SessionToken { get; set; }
    }
}
