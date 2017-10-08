using Newtonsoft.Json;

namespace LastFM.ApiClient.Models    
{
    // A Last.fm API mode representing a user
    public class User
    {
        // The details of the user
        [JsonProperty("user")]
        public UserInfo UserDetail { get; set; }
    }
}
