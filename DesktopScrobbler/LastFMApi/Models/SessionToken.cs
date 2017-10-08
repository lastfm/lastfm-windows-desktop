using Newtonsoft.Json;

namespace LastFM.ApiClient.Models
{
    // A Last.fm API model for a session token
    public class SessionToken
    {
        // The name of the user (username)
        [JsonProperty("name")]
        public string Name { get; set; }

        // The session key
        [JsonProperty("key")]
        public string Key { get; set; }

        // Whether the user is a subscriber
        [JsonProperty("subscriber")]
        public string Subscriber { get; set; }
    }
}
