using Newtonsoft.Json;

namespace LastFM.ApiClient.Models
{
    public class User
    {
        [JsonProperty("user")]
        public UserInfo UserDetail { get; set; }
    }
}
