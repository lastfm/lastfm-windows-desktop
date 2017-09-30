using Newtonsoft.Json;

namespace LastFM.ApiClient.Models
{
    public class SessionToken
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("subscriber")]
        public string Subscriber { get; set; }
    }
}
