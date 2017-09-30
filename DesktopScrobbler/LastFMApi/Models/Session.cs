using Newtonsoft.Json;

namespace LastFM.ApiClient.Models
{
    public class Session
    {
        [JsonProperty("session")]
        public SessionToken SessionToken { get; set; }
    }
}
